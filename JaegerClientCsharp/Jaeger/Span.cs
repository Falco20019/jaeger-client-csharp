using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Jaeger.Samplers;
using OpenTracing;

namespace Jaeger
{
    /// <summary>
    /// MockSpans are created via <see cref="Tracer.BuildSpan"/>, but they are also returned via calls to
    /// <see cref="Tracer.FinishedSpans"/>. They provide accessors to all Span state.
    /// </summary>
    /// <seealso cref="Tracer.FinishedSpans"/>
    public sealed class Span : ISpan
    {
        private static readonly Random Random = new Random();
        private static readonly byte[] Buffer = new byte[8];
        private static readonly int MSB = BitConverter.IsLittleEndian ? 7 : 0;

        private static long UniqueId()
        {
            long result;
            do
            {
                Random.NextBytes(Buffer);
                result = BitConverter.ToInt64(Buffer, 0);
            } while (result == 0);

            return result;
        }

        private readonly object _lock = new object();
        private SpanContext _context;
        private DateTimeOffset _finishTimestamp;
        private bool _finished;
        private readonly Dictionary<string, object> _tags;
        private readonly List<Reference> _references;
        private readonly List<LogEntry> _logEntries = new List<LogEntry>();
        private readonly List<Exception> _errors = new List<Exception>();

        public DateTimeOffset StartTimestamp { get; }

        public TimeSpan Duration => FinishTimestamp.Subtract(StartTimestamp);

        public Tracer Tracer { get; }

        /// <summary>
        /// The finish time of the Span; only valid after a call to <see cref="Finish()"/>.
        /// </summary>
        public DateTimeOffset FinishTimestamp
        {
            get
            {
                if (_finishTimestamp == DateTimeOffset.MinValue)
                    throw new InvalidOperationException("Must call Finish() before FinishTimestamp");

                return _finishTimestamp;
            }
        }

        public string OperationName { get; private set; }
        public Metrics.IMetrics Metrics { get; private set; }

        /// <summary>
        /// A copy of all tags set on this span.
        /// </summary>
        public IReadOnlyDictionary<string, object> Tags => _tags.ToImmutableDictionary();

        /// <summary>
        /// A copy of all log entries added to this span.
        /// </summary>
        public IList<LogEntry> LogEntries => _logEntries.ToImmutableList();

        /// <summary>
        /// A copy of exceptions thrown by this class (e.g. adding a tag after span is finished).
        /// </summary>
        public IList<Exception> GeneratedErrors => _errors.ToImmutableList();

        public IList<Reference> References => _references.ToImmutableList();

        public SpanContext Context
        {
            // C# doesn't have "return type covariance" so we use the trick with the explicit interface implementation
            // and this separate property.
            get
            {
                lock (_lock)
                {
                    return _context;
                }
            }
        }

        ISpanContext ISpan.Context => Context;

        public Span(Tracer tracer,
            string operationName,
            Metrics.IMetrics metrics,
            DateTimeOffset startTimestamp,
            Dictionary<string, object> initialTags,
            List<Reference> references)
        {
            Tracer = tracer;
            OperationName = operationName;
            Metrics = metrics;
            StartTimestamp = startTimestamp;

            _tags = initialTags == null
                ? new Dictionary<string, object>()
                : new Dictionary<string, object>(initialTags);

            _references = references == null
                ? new List<Reference>()
                : references.ToList();

            var parentContext = FindPreferredParentRef(_references);

            if (parentContext == null)
            {
                // we are a root span
                string debugId = _references.FirstOrDefault()?.Context.DebugId;
                _context = CreateNewRootContext(operationName, debugId);
            }
            else
            {
                // we are a child span
                _context = CreateNewChildContext(parentContext);
            }
        }

        private SpanContext CreateNewRootContext(string operationName, string debugId)
        {
            long id = UniqueId();
            SpanContext.ContextFlags flags = SpanContext.ContextFlags.None;
            if (debugId != null)
            {
                flags = SpanContext.ContextFlags.Sampled | SpanContext.ContextFlags.Debug;
                SetTag(Constants.DEBUG_ID_HEADER_KEY, debugId);
                Metrics.TraceStartedSampled.Inc(1);
            }
            else
            {
                SamplingStatus samplingStatus = Tracer.Sampler.Sample(operationName, id);
                if (samplingStatus.IsSampled)
                {
                    flags |= SpanContext.ContextFlags.Sampled;
                    foreach (var tag in samplingStatus.Tags)
                    {
                        _tags.Add(tag.Key, tag.Value);
                    }
                    Metrics.TraceStartedSampled.Inc(1);
                }
                else
                {
                    Metrics.TraceStartedNotSampled.Inc(1);
                }
            }

            return new SpanContext(id, id, 0, flags, new Dictionary<string, string>());
        }

        private SpanContext CreateNewChildContext(SpanContext parentContext)
        {
            if (parentContext.IsSampled)
            {
                Metrics.TracesJoinedSampled.Inc(1);
            }
            else
            {
                Metrics.TracesJoinedNotSampled.Inc(1);
            }

            return new SpanContext(parentContext.TraceId, UniqueId(), parentContext.SpanId, parentContext.Flags, MergeBaggages(_references));
        }

        public ISpan SetOperationName(string operationName)
        {
            CheckForFinished("Setting operationName [{0}] on already finished span", operationName);
            OperationName = operationName;
            return this;
        }

        public ISpan SetTag(string key, bool value)
        {
            return SetObjectTag(key, value);
        }

        public ISpan SetTag(string key, double value)
        {
            return SetObjectTag(key, value);
        }

        public ISpan SetTag(string key, int value)
        {
            return SetObjectTag(key, value);
        }

        public ISpan SetTag(string key, string value)
        {
            return SetObjectTag(key, value);
        }

        private ISpan SetObjectTag(string key, object value)
        {
            lock (_lock)
            {
                CheckForFinished("Setting tag [{0}:{1}] on already finished span", key, value);

                if (OpenTracing.Tag.Tags.SamplingPriority.Key.Equals(key) && value is IConvertible convertible)
                {
                    int priority = convertible.ToInt32(CultureInfo.CurrentCulture);
                
                    SpanContext.ContextFlags newFlags;
                    if (priority > 0)
                    {
                        newFlags = Context.Flags | SpanContext.ContextFlags.Sampled | SpanContext.ContextFlags.Debug;
                    }
                    else
                    {
                        newFlags = Context.Flags & ~SpanContext.ContextFlags.Sampled;
                    }

                    _context = _context.WithFlags(newFlags);
                }

                if (Context.IsSampled)
                {
                    _tags[key] = value;
                }

                return this;
            }
        }

        public ISpan Log(IDictionary<string, object> fields)
        {
            return Log(DateTimeOffset.UtcNow, fields);
        }

        public ISpan Log(DateTimeOffset timestamp, IDictionary<string, object> fields)
        {
            lock (_lock)
            {
                CheckForFinished("Adding logs {0} at {1} to already finished span.", fields, timestamp);

                if (Context.IsSampled)
                {
                    _logEntries.Add(new LogEntry(timestamp, fields));
                }
                
                return this;
            }
        }

        public ISpan Log(string @event)
        {
            return Log(DateTimeOffset.UtcNow, @event);
        }

        public ISpan Log(DateTimeOffset timestamp, string @event)
        {
            return Log(timestamp, new Dictionary<string, object> { { "event", @event } });
        }

        public ISpan SetBaggageItem(string key, string value)
        {
            lock (_lock)
            {
                CheckForFinished("Adding baggage [{0}:{1}] to already finished span.", key, value);
                _context = _context.WithBaggageItem(key, value);
                return this;
            }
        }

        public string GetBaggageItem(string key)
        {
            lock (_lock)
            {
                return _context.GetBaggageItem(key);
            }
        }

        public void Finish()
        {
            Finish(DateTimeOffset.UtcNow);
        }

        public void Finish(DateTimeOffset finishTimestamp)
        {
            lock (_lock)
            {
                CheckForFinished("Tried to finish already finished span");
                _finishTimestamp = finishTimestamp;
                Tracer.AppendFinishedSpan(this);
                _finished = true;
            }
        }

        private static SpanContext FindPreferredParentRef(IList<Reference> references)
        {
            if (!references.Any())
                return null;

            // return the context of the parent, if applicable
            foreach (var reference in references)
            {
                if (OpenTracing.References.ChildOf.Equals(reference.ReferenceType))
                    return reference.Context;
            }

            // otherwise, return the context of the first reference
            return references.First().Context;
        }

        private static Dictionary<string, string> MergeBaggages(IList<Reference> references)
        {
            var baggage = new Dictionary<string, string>();
            foreach (var reference in references)
            {
                if (reference.Context.GetBaggageItems() != null)
                {
                    foreach (var bagItem in reference.Context.GetBaggageItems())
                    {
                        baggage[bagItem.Key] = bagItem.Value;
                    }
                }
            }

            return baggage;
        }

        private void CheckForFinished(string format, params object[] args)
        {
            if (_finished)
            {
                var ex = new InvalidOperationException(string.Format(format, args));
                _errors.Add(ex);
                throw ex;
            }
        }

        public override string ToString()
        {
            return $"TraceId: {_context.TraceId}, SpanId: {_context.SpanId}, ParentId: {_context.ParentId}, OperationName: {OperationName}, IsSampled: {_context.IsSampled}";
        }

        public sealed class LogEntry
        {
            public DateTimeOffset Timestamp { get; }

            public IReadOnlyDictionary<string, object> Fields { get; }

            public LogEntry(DateTimeOffset timestamp, IDictionary<string, object> fields)
            {
                Timestamp = timestamp;
                Fields = new ReadOnlyDictionary<string, object>(fields);
            }

            public override string ToString()
            {
                return $"Timestamp: {Timestamp}, Fields: " + string.Join("; ", this.Fields.Select(e => $"{e.Key} = {e.Value}"));
            }
        }

        public sealed class Reference : IEquatable<Reference>
        {
            public SpanContext Context { get; }

            /// <summary>
            /// See <see cref="OpenTracing.References"/>.
            /// </summary>
            public string ReferenceType { get; }

            public Reference(SpanContext context, string referenceType)
            {
                Context = context ?? throw new ArgumentNullException(nameof(context));
                ReferenceType = referenceType ?? throw new ArgumentNullException(nameof(referenceType));
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as Reference);
            }

            public bool Equals(Reference other)
            {
                return other != null &&
                       EqualityComparer<SpanContext>.Default.Equals(Context, other.Context) &&
                       ReferenceType == other.ReferenceType;
            }

            public override int GetHashCode()
            {
                var hashCode = 2083322454;
                hashCode = hashCode * -1521134295 + EqualityComparer<SpanContext>.Default.GetHashCode(Context);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ReferenceType);
                return hashCode;
            }
        }
    }
}
