namespace OpenTracing.Jaeger.Metrics
{
    public interface ITimer : IMetricValue
    {
        long MillisecondsTotal { get; }

        void DurationMicros(long time);
    }
}
