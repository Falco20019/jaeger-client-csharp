namespace Jaeger.Metrics
{
    public interface IMetricsFactory
    {
        IMetrics CreateMetrics();
    }
}
