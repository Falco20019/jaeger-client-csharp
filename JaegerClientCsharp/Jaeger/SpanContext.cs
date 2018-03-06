using System;
using System.Collections.Generic;
using OpenTracing;

namespace Jaeger
{
    /// <summary>
    /// <see cref="SpanContext"/> implements a Dapper-like <see cref="ISpanContext"/> with a trace-id and span-id.
    /// <para/>
    /// Note that parent ids are part of the <see cref="Span"/>, not the <see cref="SpanContext"/>
    /// (since they do not need to propagate between processes).
    /// </summary>
    public sealed class SpanContext : ISpanContext
    {
        [Flags]
        public enum ContextFlags
        {
            None    = 0b00,
            Sampled = 0b01,
            Debug   = 0b10,
        }

        private readonly IDictionary<string, string> _baggage;

        public long TraceId { get; }

        public long SpanId { get; }

        /// <summary>
        /// The spanId of the Span's first <see cref="References.ChildOf"/> reference, or the first reference of any type,
        /// or 0 if no reference exists.
        /// </summary>
        /// <seealso cref="SpanContext.SpanId"/>
        /// <seealso cref="Span.References"/>
        public long ParentId { get; }

        public ContextFlags Flags { get; }
        public bool IsSampled => Flags.HasFlag(ContextFlags.Sampled);
        public bool IsDebug => Flags.HasFlag(ContextFlags.Debug);

        public string DebugId { get; }

        /// <summary>
        /// Returns true when the instance of the context is only used to return the debug/correlation ID
        /// from Extract() method. This happens in the situation when "jaeger-debug-id" header is passed in
        /// the carrier to the Extract() method, but the request otherwise has no span context in it.
        /// Previously this would've returned null from the extract method, but now it returns a dummy
        /// context with only debugId filled in.
        /// </summary>
        /// <see cref="Constants.DEBUG_ID_HEADER_KEY"/>
        public bool IsDebugIdContainerOnly => TraceId == 0 && DebugId != null;

        /// <summary>
        /// An internal constructor to create a new <see cref="SpanContext"/>.
        /// This should only be called by <see cref="Span"/> and/or <see cref="Tracer"/>.
        /// </summary>
        /// <param name="traceId">The id of the trace</param>
        /// <param name="spanId">The id of the span</param>
        /// <param name="parentId">The id of the parent</param>
        /// <param name="flags">TODO</param>
        /// <param name="baggage">The SpanContext takes ownership of the baggage parameter.</param>
        /// <param name="debugId">TODO</param>
        /// <seealso cref="SpanContext.WithBaggageItem(string, string)"/>
        internal SpanContext(long traceId, long spanId, long parentId, ContextFlags flags, IDictionary<string, string> baggage, string debugId = null)
        {
            TraceId = traceId;
            SpanId = spanId;
            ParentId = parentId;
            Flags = flags;
            _baggage = baggage;
            DebugId = debugId;
        }

        public IEnumerable<KeyValuePair<string, string>> GetBaggageItems()
        {
            return _baggage;
        }

        public string GetBaggageItem(string key)
        {
            if (_baggage.ContainsKey(key))
                return _baggage[key];

            return null;
        }

        /// <summary>
        /// Create and return a new (immutable) SpanContext with the added baggage item.
        /// </summary>
        public SpanContext WithBaggageItem(string key, string val)
        {
            var newBaggage = new Dictionary<string, string>(_baggage);

            newBaggage[key] = val;

            return new SpanContext(TraceId, SpanId, ParentId, Flags, newBaggage, DebugId);
        }

        /// <summary>
        /// Create and return a new (immutable) SpanContext with the added flags.
        /// </summary>
        public SpanContext WithFlags(ContextFlags flags)
        {
            return new SpanContext(TraceId, SpanId, ParentId, flags, _baggage, DebugId);
        }
        
        /// <summary>
        /// Create a new dummy SpanContext as a container for debugId string. This is used when
        /// "jaeger-debug-id" header is passed in the request headers and forces the trace to be sampled as
        /// debug trace, and the value of header recorded as a span tag to serve as a searchable
        /// correlation ID.
        /// </summary>
        /// <param name="debugId">Arbitrary string used as correlation ID</param>
        /// <returns>New dummy SpanContext that serves as a container for debugId only.</returns>
        /// <see cref="Constants.DEBUG_ID_HEADER_KEY"/>
        public static SpanContext WithDebugId(string debugId)
        {
            return new SpanContext(0, 0, 0, 0, new Dictionary<string, string>(), debugId);
        }
    }
}
