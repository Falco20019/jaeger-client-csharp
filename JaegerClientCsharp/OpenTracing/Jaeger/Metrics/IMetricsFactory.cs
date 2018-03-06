namespace OpenTracing.Jaeger.Metrics
{
    public interface IMetricsFactory
    {
        IMetrics CreateMetrics();
    }
}
