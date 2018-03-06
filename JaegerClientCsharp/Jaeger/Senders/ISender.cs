using System;
using System.Threading;
using System.Threading.Tasks;

namespace Jaeger.Senders
{
    public interface ISender : IDisposable
    {
        Task<int> AppendAsync(Span span, CancellationToken cancellationToken);

        Task<int> FlushAsync(CancellationToken cancellationToken);

        Task<int> CloseAsync(CancellationToken cancellationToken);
    }
}
