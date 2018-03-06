using System.Collections.Generic;
using System.Collections.Immutable;

namespace Jaeger.Samplers
{
    class ConstSampler : ISampler
    {
        public const string TYPE = "const";

        public bool Decision { get; }
        public IDictionary<string, object> Tags { get; }

        public ConstSampler(bool decision)
        {
            this.Decision = decision;
            Dictionary<string, object> tags =
                new Dictionary<string, object>
                {
                    {Constants.SAMPLER_TYPE_TAG_KEY, TYPE},
                    {Constants.SAMPLER_PARAM_TAG_KEY, decision}
                };
            this.Tags = tags.ToImmutableDictionary();
        }

        /// <summary>
        /// Only implemented to satisfy the sampler interface.
        /// </summary>
        public void Dispose()
        {
            // Nothing to do
        }

        /// <summary>
        /// This sampler just returns the constant decision defined on it's creation.
        /// </summary>
        /// <param name="operation">Not used by this sampler.</param>
        /// <param name="traceId">A long that represents the traceId used to make a sampling decision the command line arguments.</param>
        /// <returns>A boolean that says wheather to sample.</returns>
        public SamplingStatus Sample(string operation, long traceId)
        {
            return SamplingStatus.Of(Decision, Tags);
        }

        protected bool Equals(ConstSampler other)
        {
            return Decision == other.Decision;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ConstSampler)obj);
        }

        public override int GetHashCode()
        {
            return Decision.GetHashCode();
        }
    }
}
