using System.Collections.Generic;
using System.Collections.Immutable;

namespace Jaeger.Reporters
{
    public class InMemoryReporter : IReporter
    {
        private readonly List<Span> _spans = new List<Span>();

        public IList<Span> Spans => _spans.ToImmutableList();

        public void Dispose()
        {
            // Nothings to do
        }

        public void Report(Span span)
        {
            lock (this)
            {
                _spans.Add(span);
            }
        }

        public void Clear()
        {
            lock (this)
            {
                _spans.Clear();
            }
        }
    }
}
