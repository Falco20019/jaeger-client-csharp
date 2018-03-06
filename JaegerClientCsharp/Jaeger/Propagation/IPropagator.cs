using OpenTracing.Propagation;

namespace Jaeger.Propagation
{
    /// <summary>
    /// Allows the developer to inject into the <see cref="Tracer.Inject"/> and <see cref="Tracer.Extract"/> calls.
    /// </summary>
    public interface IPropagator
    {
        void Inject<TCarrier>(SpanContext context, IFormat<TCarrier> format, TCarrier carrier);

        SpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier);
    }
}