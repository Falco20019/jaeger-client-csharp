using System;
using System.Collections.Generic;
using OpenTracing.Propagation;

namespace OpenTracing.Jaeger.Propagation
{
    /// <summary>
    /// <see cref="IPropagator"/> implementation that uses <see cref="ITextMap"/> internally.
    /// </summary>
    public sealed class TextMapPropagator : IPropagator
    {
        public const string SpanIdKey = "spanid";
        public const string TraceIdKey = "traceid";
        public const string ParentIdKey = "parentid";
        public const string FlagsKey = "flags";
        public const string BaggageKeyPrefix = "baggage-";

        public void Inject<TCarrier>(SpanContext context, IFormat<TCarrier> format, TCarrier carrier)
        {
            if (carrier is ITextMap text)
            {
                foreach (var entry in context.GetBaggageItems())
                {
                    text.Set(BaggageKeyPrefix + entry.Key, entry.Value);
                }

                text.Set(SpanIdKey, context.SpanId.ToString());
                text.Set(TraceIdKey, context.TraceId.ToString());
                text.Set(ParentIdKey, context.ParentId.ToString());
                text.Set(FlagsKey, ((int)context.Flags).ToString());
            }
            else
            {
                throw new InvalidOperationException($"Unknown carrier [{carrier.GetType()}]");
            }
        }

        public SpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier)
        {
            long? traceId = null;
            long? spanId = null;
            long? parentId = null;
            SpanContext.ContextFlags? flags = null;
            Dictionary<string, string> baggage = new Dictionary<string, string>();

            if (carrier is ITextMap text)
            {
                foreach (var entry in text)
                {
                    if (TraceIdKey.Equals(entry.Key))
                    {
                        traceId = Convert.ToInt64(entry.Value);
                    }
                    else if (SpanIdKey.Equals(entry.Key))
                    {
                        spanId = Convert.ToInt64(entry.Value);
                    }
                    else if (ParentIdKey.Equals(entry.Key))
                    {
                        parentId = Convert.ToInt64(entry.Value);
                    }
                    else if (FlagsKey.Equals(entry.Key))
                    {
                        flags = (SpanContext.ContextFlags)Convert.ToInt32(entry.Value);
                    }
                    else if (entry.Key.StartsWith(BaggageKeyPrefix))
                    {
                        var key = entry.Key.Substring(BaggageKeyPrefix.Length);
                        baggage[key] = entry.Value;
                    }
                }
            }
            else
            {
                throw new InvalidOperationException($"Unknown carrier [{carrier.GetType()}]");
            }

            if (traceId.HasValue && spanId.HasValue && parentId.HasValue && flags.HasValue)
            {
                return new SpanContext(traceId.Value, spanId.Value, parentId.Value, flags.Value, baggage);
            }

            return null;
        }
    }
}