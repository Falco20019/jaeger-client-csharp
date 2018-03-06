namespace Jaeger.Metrics
{
    public interface IMetricValue
    {
        string Name { get; }
        MetricAttribute Attribute { get; }
    }
}
