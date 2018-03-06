using System;
using Jaeger.Thrift.Agent;

namespace Jaeger.Samplers
{
    interface ISamplingManager
    {
        SamplingStrategyResponse GetSamplingStrategy(String serviceName);
    }
}
