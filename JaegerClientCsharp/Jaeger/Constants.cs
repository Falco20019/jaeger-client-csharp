namespace Jaeger
{
    /// <summary>
    /// The following log fields are recommended for instrumentors who are trying to capture more information about a
    /// logged event. Tracers may expose additional features based on these standardized data points.
    /// <see href="https://github.com/opentracing/specification/blob/master/semantic_conventions.md"/>
    /// </summary>
    public static class Constants
    {
        // TODO these should be configurable
        public const string X_UBER_SOURCE = "x-uber-source";

        /// <summary>
        /// Span tag key to describe the type of sampler used on the root span.
        /// </summary>
        public const string SAMPLER_TYPE_TAG_KEY = "sampler.type";

        /// <summary>
        /// Span tag key to describe the parameter of the sampler used on the root span.
        /// </summary>
        public const string SAMPLER_PARAM_TAG_KEY = "sampler.param";

        /// <summary>
        /// The name of HTTP header or a TextMap carrier key which, if found in the carrier, forces the
        /// trace to be sampled as "debug" trace. The value of the header is recorded as the tag on the
        /// root span, so that the trace can be found in the UI using this value as a correlation ID.
        /// </summary>
        public const string DEBUG_ID_HEADER_KEY = "jaeger-debug-id";

        /// <summary>
        /// The name of the tag used to report client version.
        /// </summary>
        public const string JAEGER_CLIENT_VERSION_TAG_KEY = "jaeger.version";

        /// <summary>
        /// The name used to report host name of the process.
        /// </summary>
        public const string TRACER_HOSTNAME_TAG_KEY = "hostname";

        /// <summary>
        /// The name used to report ip of the process.
        /// </summary>
        public const string TRACER_IP_TAG_KEY = "ip";
    }
}
