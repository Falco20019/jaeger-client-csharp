using System;

namespace Jaeger.Reporters
{
    /// <summary>
    /// Reporter is the interface Tracer uses to report finished span to something that collects those
    /// spans. Default implementation is remote reporter that sends spans out of process.
    /// </summary>
    public interface IReporter : IDisposable
    {
        void Report(Span span);
    }
}
