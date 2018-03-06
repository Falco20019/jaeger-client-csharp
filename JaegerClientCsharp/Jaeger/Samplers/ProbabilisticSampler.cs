using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Jaeger.Samplers
{
    public class ProbabilisticSampler : ISampler
    {
        public const double DEFAULT_SAMPLING_PROBABILITY = 0.001;
        public const string TYPE = "probabilistic";

        private readonly long positiveSamplingBoundary;
        private readonly long negativeSamplingBoundary;

        public double SamplingRate { get; }
        public IDictionary<string, object> Tags { get; }

        public ProbabilisticSampler(double samplingRate = DEFAULT_SAMPLING_PROBABILITY)
        {
            if (samplingRate < 0.0 || samplingRate > 1.0)
            {
                throw new ArgumentOutOfRangeException(nameof(samplingRate),
                    "The sampling rate must be greater than 0.0 and less than 1.0");
            }

            this.SamplingRate = samplingRate;
            this.positiveSamplingBoundary = (long)(unchecked((1L << 63) - 1) * samplingRate);
            this.negativeSamplingBoundary = (long)((1L << 63) * samplingRate);

            Dictionary<string, object> tags =
                new Dictionary<string, object>
                {
                    {Constants.SAMPLER_TYPE_TAG_KEY, TYPE},
                    {Constants.SAMPLER_PARAM_TAG_KEY, samplingRate}
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
        /// Uses a trace id to make a sampling decision.
        /// </summary>
        /// <param name="operation">Not used by this sampler.</param>
        /// <param name="traceId">A long that represents the traceId used to make a sampling decision the command line arguments.</param>
        /// <returns>A boolean that says wheather to sample.</returns>
        public SamplingStatus Sample(string operation, long traceId)
        {
            if (traceId > 0)
            {
                return SamplingStatus.Of(traceId <= this.positiveSamplingBoundary, Tags);
            }

            return SamplingStatus.Of(traceId >= this.negativeSamplingBoundary, Tags);
        }

        protected bool Equals(ProbabilisticSampler other)
        {
            return SamplingRate == other.SamplingRate;
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
            return SamplingRate.GetHashCode();
        }
    }
}
