using System;

namespace OpenTracing.Jaeger.Exceptions
{
    public class SenderException : Exception
    {
        public int DroppedSpans { get; }

        public SenderException(string message, int droppedSpans) : this(message, null, droppedSpans)
        {
            DroppedSpans = droppedSpans;
        }

        public SenderException(string message, Exception innerException, int droppedSpans) : base(message, innerException)
        {
            DroppedSpans = droppedSpans;
        }
    }
}
