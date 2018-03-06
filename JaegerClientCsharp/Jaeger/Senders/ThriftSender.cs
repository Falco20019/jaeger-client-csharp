using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jaeger.Exceptions;
using Jaeger.Reporters.Protocols;
using Jaeger.Thrift;
using Thrift;
using Thrift.Protocols;

namespace Jaeger.Senders
{
    public abstract class ThriftSender : ISender
    {
        private Process process;
        protected readonly ITProtocolFactory protocolFactory;
        private readonly List<Thrift.Span> spanBuffer;

        protected ThriftSender(ITProtocolFactory protocolFactory)
        {
            this.protocolFactory = protocolFactory;
            spanBuffer = new List<Thrift.Span>();
        }

        public void Dispose()
        {
            CloseAsync(CancellationToken.None).Wait();
        }

        protected abstract Task SendAsync(Process process, List<Thrift.Span> spans, CancellationToken cancellationToken);

        public Task<int> AppendAsync(Span span, CancellationToken cancellationToken)
        {
            if (process == null)
            {
                process = new Process(span.Tracer.ServiceName)
                {
                    Tags = JaegerThriftSpanConverter.BuildTags(span.Tracer.Tags)
                };
            }

            Thrift.Span thriftSpan = JaegerThriftSpanConverter.convertSpan(span);
            spanBuffer.Add(thriftSpan);

            // We never directly send them, since we have no ma package size anymore.
            return Task.FromResult(0);
        }

        public async Task<int> FlushAsync(CancellationToken cancellationToken)
        {
            int n = spanBuffer.Count;
            if (n == 0)
            {
                return 0;
            }

            try
            {
                var spanBufferFreeze = new List<Thrift.Span>(spanBuffer);
                spanBuffer.Clear();
                await SendAsync(process, spanBufferFreeze, cancellationToken);
            }
            catch (TException e)
            {
                throw new SenderException("Failed to flush spans.", e, n);
            }
            return n;
        }

        public virtual Task<int> CloseAsync(CancellationToken cancellationToken)
        {
            return FlushAsync(cancellationToken);
        }
    }
}
