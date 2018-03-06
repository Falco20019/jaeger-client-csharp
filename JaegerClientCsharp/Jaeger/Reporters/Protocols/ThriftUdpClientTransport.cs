using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Thrift.Transports;
using Thrift.Transports.Client;

namespace Jaeger.Reporters.Protocols
{
    public class ThriftUdpClientTransport : TMemoryBufferClientTransport
    {
        private readonly UdpClient client;

        public ThriftUdpClientTransport(string host, int port)
        {
            client = new UdpClient();
            client.Connect(host, port);
        }

        public override bool IsOpen => client.Client.Connected;

        public override void Close()
        {
            client.Close();
            base.Close();
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int length, CancellationToken cancellationToken)
        {
            int curDataSize = await base.ReadAsync(buffer, offset, length, cancellationToken);
            if (curDataSize == 0)
            {
                base.Reset();
                UdpReceiveResult result;
                try
                {
                    result = await client.ReceiveAsync();
                }
                catch (IOException e)
                {
                    throw new TTransportException(TTransportException.ExceptionType.Unknown, $"ERROR from underlying socket. {e.Message}");
                }

                await base.WriteAsync(result.Buffer, cancellationToken);
                base.Reset();
            }

            return await base.ReadAsync(buffer, offset, length, cancellationToken);
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            byte[] bytes = base.GetBuffer();

            if (bytes.Length == 0)
                return Task.CompletedTask;

            base.Reset();

            try
            {
                return client.SendAsync(bytes, bytes.Length);
            }
            catch (Exception e)
            {
                throw new TTransportException(TTransportException.ExceptionType.Unknown, $"Cannot flush closed transport. {e.Message}");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                client.Dispose();
            }
        }
    }
}
