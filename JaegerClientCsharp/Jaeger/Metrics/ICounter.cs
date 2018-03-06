﻿namespace Jaeger.Metrics
{
    public interface ICounter : IMetricValue
    {
        long Count { get; }
        void Inc(long delta);
    }
}
