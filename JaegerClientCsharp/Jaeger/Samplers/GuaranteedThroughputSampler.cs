using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Jaeger.Samplers
{
    class GuaranteedThroughputSampler : ISampler
    {
        public const string TYPE = "lowerbound";

        public IDictionary<string, object> Tags => tags.ToImmutableDictionary();

        private ProbabilisticSampler probabilisticSampler;
        private RateLimitingSampler lowerBoundSampler;
        private readonly IDictionary<string, object> tags;

        public GuaranteedThroughputSampler(double samplingRate, double lowerBound)
        {
            tags =
                new Dictionary<string, object>
                {
                    {Constants.SAMPLER_TYPE_TAG_KEY, TYPE},
                    {Constants.SAMPLER_PARAM_TAG_KEY, samplingRate}
                };

            probabilisticSampler = new ProbabilisticSampler(samplingRate);
            lowerBoundSampler = new RateLimitingSampler(lowerBound);
        }
        
        /// <summary>
        /// Updates the probabilistic and lowerBound samplers
        /// </summary>
        /// <param name="samplingRate">The sampling rate for probabilistic sampling</param>
        /// <param name="lowerBound">The lower bound limit for lower bound sampling</param>
        /// <returns>true iff any samplers were updated</returns>
        public bool Update(double samplingRate, double lowerBound)
        {
            lock (this)
            {
                bool isUpdated = false;
                if (Math.Abs(samplingRate - probabilisticSampler.SamplingRate) > double.Epsilon)
                {
                    probabilisticSampler = new ProbabilisticSampler(samplingRate);
                    tags[Constants.SAMPLER_PARAM_TAG_KEY] = samplingRate;
                    isUpdated = true;
                }

                if (Math.Abs(lowerBound - lowerBoundSampler.MaxTracesPerSecond) > double.Epsilon)
                {
                    lowerBoundSampler = new RateLimitingSampler(lowerBound);
                    isUpdated = true;
                }

                return isUpdated;
            }
        }

        /// <summary>
        /// Only implemented to satisfy the sampler interface.
        /// </summary>
        public void Dispose()
        {
            probabilisticSampler.Dispose();
            lowerBoundSampler.Dispose();
        }

        /// <summary>
        /// Calls <see cref="ISampler.Sample(string,long)"/> on both samplers, returning true for
        /// <see cref="SamplingStatus.IsSampled"/> if either samplers set #isSampled to true.
        /// The tags corresponding to the sampler that returned true are set on <see cref="SamplingStatus.Tags"/>
        /// If both samplers return true, tags for {@link ProbabilisticSampler} is given priority.
        /// </summary>
        /// <param name="operation">The operation name, which is ignored by this sampler</param>
        /// <param name="traceId">The traceId on the span</param>
        /// <returns>A boolean that says wheather to sample.</returns>
        public SamplingStatus Sample(string operation, long traceId)
        {
            SamplingStatus probabilisticSamplingStatus = probabilisticSampler.Sample(operation, traceId);
            SamplingStatus lowerBoundSamplingStatus = lowerBoundSampler.Sample(operation, traceId);

            if (probabilisticSamplingStatus.IsSampled)
            {
                return probabilisticSamplingStatus;
            }

            return SamplingStatus.Of(lowerBoundSamplingStatus.IsSampled, tags);
        }
    }
}
