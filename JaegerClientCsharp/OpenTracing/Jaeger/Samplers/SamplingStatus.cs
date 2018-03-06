using System.Collections.Generic;

namespace OpenTracing.Jaeger.Samplers
{
    public class SamplingStatus
    {
        public bool IsSampled { get; }
        public IDictionary<string, object> Tags { get; }

        private SamplingStatus(bool isSampled, IDictionary<string, object> tags)
        {
            IsSampled = isSampled;
            Tags = tags;
        }

        public static SamplingStatus Of(bool isSampled, IDictionary<string, object> tags)
        {
            return new SamplingStatus(isSampled, tags);
        }
    }
}