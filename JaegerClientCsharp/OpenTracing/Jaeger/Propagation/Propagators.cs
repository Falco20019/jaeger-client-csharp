namespace OpenTracing.Jaeger.Propagation
{
    public static class Propagators
    {
        public static readonly IPropagator Console = new ConsolePropagator();

        public static readonly IPropagator TextMap = new TextMapPropagator();
    }
}
