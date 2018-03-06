using System.Collections.Generic;

namespace OpenTracing.Jaeger.Reporters
{
    public class CompositeReporter : IReporter
    {
        private readonly List<IReporter> _reporters;

        public CompositeReporter(params IReporter[] reporters)
        {
            _reporters = new List<IReporter>(reporters);
        }

        public void Dispose()
        {
            _reporters.ForEach(r => r.Dispose());
        }

        public void Report(Span span)
        {
            _reporters.ForEach(r => r.Report(span));
        }
    }
}
