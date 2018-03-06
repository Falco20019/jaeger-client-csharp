namespace Jaeger.Reporters
{
    public class NoopReporter : IReporter
    {
        public void Dispose()
        {
            // Nothing to do
        }

        public void Report(Span span)
        {
            // Nothing to do
        }
    }
}
