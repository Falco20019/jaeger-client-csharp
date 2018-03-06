using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jaeger.Thrift;
using Thrift.Protocols;
using Thrift.Transports.Client;

namespace Jaeger.Senders
{
    public class HttpSender : ThriftSender
    {
        private const string HTTP_COLLECTOR_JAEGER_THRIFT_FORMAT_PARAM = "format=jaeger.thrift";

        private readonly THttpClientTransport httpTransport;
        private readonly TProtocol protocol;

        /// <param name="uri">Uri</param>
        public HttpSender(Uri uri) : base(new TBinaryProtocol.Factory())
        {
            Uri collectorUri = new UriBuilder(uri)
            {
                Query = HTTP_COLLECTOR_JAEGER_THRIFT_FORMAT_PARAM
            }.Uri;
            httpTransport = new THttpClientTransport(collectorUri, null);
            protocol = protocolFactory.GetProtocol(httpTransport);
        }

        protected override async Task SendAsync(Process process, List<Thrift.Span> spans, CancellationToken cancellationToken)
        {
            var batch = new Batch(process, spans);
            await batch.WriteAsync(protocol, cancellationToken);
            await protocol.Transport.FlushAsync(cancellationToken);
        }

        public override Task<int> CloseAsync(CancellationToken cancellationToken)
        {
            try
            {
                return base.CloseAsync(cancellationToken);
            }
            finally
            {
                httpTransport.Close();
            }
        }
    }
}
