using System;
using System.Runtime.Serialization;

namespace Jaeger.Exceptions
{
    class SamplingStrategyErrorException : Exception
    {
        public SamplingStrategyErrorException()
        {
        }

        protected SamplingStrategyErrorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public SamplingStrategyErrorException(string message) : base(message)
        {
        }

        public SamplingStrategyErrorException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
