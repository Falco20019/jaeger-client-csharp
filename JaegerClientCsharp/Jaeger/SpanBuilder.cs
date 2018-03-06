using System;
using System.Collections.Generic;
using System.Linq;
using OpenTracing;

namespace Jaeger
{
    public sealed class SpanBuilder : ISpanBuilder
    {
        private readonly Tracer _tracer;
        private readonly string _operationName;
        private readonly Metrics.IMetrics _metrics;
        private DateTimeOffset _startTimestamp = DateTimeOffset.MinValue;
        private readonly List<Span.Reference> _references = new List<Span.Reference>();
        private readonly Dictionary<string, object> _initialTags = new Dictionary<string, object>();
        private bool _ignoreActiveSpan;

        public SpanBuilder(Tracer tracer, string operationName, Metrics.IMetrics metrics)
        {
            _tracer = tracer;
            _operationName = operationName;
            _metrics = metrics;
        }

        public ISpanBuilder AsChildOf(ISpanContext parent)
        {
            if (parent == null)
                return this;

            return AddReference(References.ChildOf, parent);
        }

        public ISpanBuilder AsChildOf(ISpan parent)
        {
            if (parent == null)
                return this;

            return AddReference(References.ChildOf, parent.Context);
        }

        public ISpanBuilder AddReference(string referenceType, ISpanContext referencedContext)
        {
            if (referencedContext != null)
            {
                _references.Add(new Span.Reference((SpanContext)referencedContext, referenceType));
            }

            return this;
        }

        public ISpanBuilder IgnoreActiveSpan()
        {
            _ignoreActiveSpan = true;
            return this;
        }

        public ISpanBuilder WithTag(string key, bool value)
        {
            _initialTags[key] = value;
            return this;
        }

        public ISpanBuilder WithTag(string key, double value)
        {
            _initialTags[key] = value;
            return this;
        }

        public ISpanBuilder WithTag(string key, int value)
        {
            _initialTags[key] = value;
            return this;
        }

        public ISpanBuilder WithTag(string key, string value)
        {
            _initialTags[key] = value;
            return this;
        }

        public ISpanBuilder WithStartTimestamp(DateTimeOffset startTimestamp)
        {
            _startTimestamp = startTimestamp;
            return this;
        }

        public IScope StartActive(bool finishSpanOnDispose)
        {
            ISpan span = Start();
            return _tracer.ScopeManager.Activate(span, finishSpanOnDispose);
        }

        public ISpan Start()
        {
            if (_startTimestamp == DateTimeOffset.MinValue) // value was not set by builder
            {
                _startTimestamp = DateTimeOffset.UtcNow;
            }

            ISpanContext activeSpanContext = _tracer.ActiveSpan?.Context;

            if (!_references.Any() && !_ignoreActiveSpan && activeSpanContext != null)
            {
                _references.Add(new Span.Reference((SpanContext)activeSpanContext, References.ChildOf));
            }

            var span = new Span(_tracer, _operationName, _metrics, _startTimestamp, _initialTags, _references);
            if (span.Context.IsSampled)
            {
                _metrics.SpansStartedSampled.Inc(1);
            }
            else
            {
                _metrics.SpansStartedNotSampled.Inc(1);
            }
            return span;
        }
    }
}
