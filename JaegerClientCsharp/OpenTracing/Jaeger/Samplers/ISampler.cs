using System;

namespace OpenTracing.Jaeger.Samplers
{
    /// <summary>
    /// Sampler is responsible for deciding if a new trace should be sampled and captured for storage.
    /// </summary>
    public interface ISampler : IDisposable
    {
        /// <param name="operation">The operation name set on the span.</param>
        /// <param name="traceId">The traceId on the span</param>
        /// <returns>Whether or not the new trace should be sampled</returns>
        SamplingStatus Sample(String operation, long traceId);
    }
}
