using System.IO;

namespace OpenTracing.Jaeger.Reporters
{
    public class LoggingReporter : IReporter
    {
        private readonly TextWriter _writer;
        private readonly bool _closeOnDispose;

        public LoggingReporter(TextWriter writer, bool closeOnDispose)
        {
            _writer = writer;
            _closeOnDispose = closeOnDispose;
        }

        public void Dispose()
        {
            if (_closeOnDispose)
            {
                lock (_writer)
                {
                    _writer.Close();
                }
            }
        }

        public void Report(Span span)
        {
            lock (_writer)
            {
                _writer.WriteLine($"Span reported: {span}");
            }
        }
    }
}
