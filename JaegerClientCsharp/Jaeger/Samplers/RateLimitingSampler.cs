using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Jaeger.Util;

namespace Jaeger.Samplers
{
    public class RateLimitingSampler : ISampler
    {
        public const string TYPE = "ratelimiting";

        public RateLimiter RateLimiter { get; }
        public double MaxTracesPerSecond { get; }
        public IDictionary<string, object> Tags { get; }

        public RateLimitingSampler(double maxTracesPerSecond)
        {
            this.MaxTracesPerSecond = maxTracesPerSecond;
            double maxBalance = maxTracesPerSecond < 1.0 ? 1.0 : maxTracesPerSecond;
            this.RateLimiter = new RateLimiter(maxTracesPerSecond, maxBalance);

            Dictionary<string, object> tags =
                new Dictionary<string, object>
                {
                    {Constants.SAMPLER_TYPE_TAG_KEY, TYPE},
                    {Constants.SAMPLER_PARAM_TAG_KEY, maxTracesPerSecond}
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
        /// This sampler returns the decision based maxTracesPerSecond.
        /// </summary>
        /// <param name="operation">Not used by this sampler.</param>
        /// <param name="traceId">A long that represents the traceId used to make a sampling decision the command line arguments.</param>
        /// <returns>A boolean that says wheather to sample.</returns>
        public SamplingStatus Sample(string operation, long traceId)
        {
            return SamplingStatus.Of(this.RateLimiter.CheckCredit(1.0), Tags);
        }

        protected bool Equals(RateLimitingSampler other)
        {
            return Math.Abs(MaxTracesPerSecond - other.MaxTracesPerSecond) < double.Epsilon;
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
            return MaxTracesPerSecond.GetHashCode();
        }
    }
}
