using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jaeger.Reporters.Protocols;
using Jaeger.Thrift;
using Jaeger.Thrift.Agent;
using Thrift.Protocols;

namespace Jaeger.Senders
{
    public class UdpSender : ThriftSender
    {
        public const string DEFAULT_AGENT_UDP_HOST = "localhost";
        public const int DEFAULT_AGENT_UDP_COMPACT_PORT = 6831;

        private readonly Agent.Client agentClient;
        private readonly ThriftUdpClientTransport udpTransport;
        
        /// <summary>
        /// This constructor expects Jaeger running running on <value>DEFAULT_AGENT_UDP_HOST</value>
        /// and port <value>DEFAULT_AGENT_UDP_COMPACT_PORT</value>
        /// </summary>
        public UdpSender() : this(DEFAULT_AGENT_UDP_HOST, DEFAULT_AGENT_UDP_COMPACT_PORT)
        {
        }
        
        /// <param name="host">Host</param>
        /// <param name="port">Port</param>
        public UdpSender(String host, int port) : base(new TCompactProtocol.Factory())
        {
            if (string.IsNullOrEmpty(host))
            {
                host = DEFAULT_AGENT_UDP_HOST;
            }

            if (port == 0)
            {
                port = DEFAULT_AGENT_UDP_COMPACT_PORT;
            }

            udpTransport = new ThriftUdpClientTransport(host, port);
            agentClient = new Agent.Client(protocolFactory.GetProtocol(udpTransport));
        }

        protected override Task SendAsync(Process process, List<Thrift.Span> spans, CancellationToken cancellationToken)
        {
            return agentClient.emitBatchAsync(new Batch(process, spans), cancellationToken);
        }

        public override Task<int> CloseAsync(CancellationToken cancellationToken)
        {
            try
            {
                return base.CloseAsync(cancellationToken);
            }
            finally
            {
                udpTransport.Close();
            }
        }
    }
}
