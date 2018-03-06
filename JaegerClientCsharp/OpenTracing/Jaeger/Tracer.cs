using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using OpenTracing.Jaeger.Metrics;
using OpenTracing.Jaeger.Propagation;
using OpenTracing.Jaeger.Reporters;
using OpenTracing.Jaeger.Samplers;
using OpenTracing.Propagation;
using OpenTracing.Util;

namespace OpenTracing.Jaeger
{
    /// <summary>
    /// <see cref="Tracer"/> makes it easy to test the semantics of OpenTracing instrumentation.
    /// <para/>
    /// By using a <see cref="Tracer"/> as an <see cref="ITracer"/> implementation for unittests, a developer can assert that Span
    /// properties and relationships with other Spans are defined as expected by instrumentation code.
    /// <para/>
    /// The MockTracerTests class has simple usage examples.
    /// </summary>
    public class Tracer : ITracer, IDisposable
    {
        private readonly object _lock = new object();

        private readonly List<Span> _finishedSpans = new List<Span>();
        private readonly string _serviceName;
        private readonly Dictionary<string, object> _tags;
        private readonly IScopeManager _scopeManager;
        private readonly IPropagator _propagator;
        private readonly ISampler _sampler;
        private readonly IReporter _reporter;
        private readonly IMetrics _metrics;
        private readonly string _version;

        public string ServiceName => _serviceName;
        public IScopeManager ScopeManager => _scopeManager;
        public IPropagator Propagator => _propagator;
        public ISampler Sampler => _sampler;
        public IReporter Reporter => _reporter;
        public IMetrics Metrics => _metrics;

        public ISpan ActiveSpan => ScopeManager?.Active?.Span;

        /// <summary>
        /// A copy of all tags set on this tracer.
        /// </summary>
        public IReadOnlyDictionary<string, object> Tags => _tags.ToImmutableDictionary();

        private Tracer(string serviceName, Dictionary<string, object> tags, IScopeManager scopeManager, IPropagator propagator, ISampler sampler, IReporter reporter, IMetrics metrics)
        {
            _serviceName = serviceName;
            _tags = tags;
            _scopeManager = scopeManager;
            _propagator = propagator;
            _sampler = sampler;
            _reporter = reporter;
            _metrics = metrics;

            _version = GetVersion();
            tags.Add(Constants.JAEGER_CLIENT_VERSION_TAG_KEY, _version);

            String hostname = System.Net.Dns.GetHostName();
            if (hostname != null)
            {
                tags.Add(Constants.TRACER_HOSTNAME_TAG_KEY, hostname);

                try
                {
                    tags.Add(Constants.TRACER_IP_TAG_KEY, System.Net.Dns.GetHostAddresses(hostname).First(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToString());
                }
                catch
                {
                }
            }
        }

        private static string GetVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            return $"CSharp-{version}";
        }

        public void Dispose()
        {
            _sampler.Dispose();
            _reporter.Dispose();
        }

        /// <summary>
        /// Clear the <see cref="FinishedSpans"/> queue.
        /// <para/>
        /// Note that this does *not* have any effect on Spans created by MockTracer that have not Finish()ed yet; those
        /// will still be enqueued in <see cref="FinishedSpans"/> when they Finish().
        /// </summary>
        public void Reset()
        {
            lock (_lock)
            {
                _finishedSpans.Clear();
            }
        }

        /// <summary>
        /// Returns a copy of all Finish()ed MockSpans started by this MockTracer (since construction or the last call to
        /// <see cref="Tracer.Reset"/>).
        /// </summary>
        /// <seealso cref="Tracer.Reset"/>
        public List<Span> FinishedSpans()
        {
            lock (_lock)
            {
                return new List<Span>(_finishedSpans);
            }
        }

        /// <summary>
        /// Noop method called on <see cref="ISpan.Finish()"/>.
        /// </summary>
        protected virtual void OnSpanFinished(Span span)
        {
            if (span.Context.IsSampled)
            {
                _reporter.Report(span);
                _metrics.SpansFinished.Inc(1);
            }
        }

        public ISpanBuilder BuildSpan(string operationName)
        {
            return new SpanBuilder(this, operationName, _metrics);
        }

        public void Inject<TCarrier>(ISpanContext spanContext, IFormat<TCarrier> format, TCarrier carrier)
        {
            _propagator.Inject((SpanContext)spanContext, format, carrier);
        }

        public ISpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier)
        {
            return _propagator.Extract(format, carrier);
        }

        internal void AppendFinishedSpan(Span span)
        {
            lock (_lock)
            {
                _finishedSpans.Add(span);
                OnSpanFinished(span);
            }
        }

        public sealed class Builder
        {
            private readonly string _serviceName;
            private readonly Dictionary<string, object> _initialTags = new Dictionary<string, object>();
            private IScopeManager _scopeManager;
            private IPropagator _propagator;
            private ISampler _sampler;
            private IReporter _reporter;
            private IMetrics _metrics;

            public Builder(String serviceName)
            {
                this._serviceName = checkValidServiceName(serviceName);
            }

            public Builder WithPropagator(IPropagator propagator)
            {
                this._propagator = propagator;
                return this;
            }

            public Builder WithReporter(IReporter reporter)
            {
                this._reporter = reporter;
                return this;
            }

            public Builder WithSampler(ISampler sampler)
            {
                this._sampler = sampler;
                return this;
            }

            public Builder WithMetrics(IMetrics metrics)
            {
                this._metrics = metrics;
                return this;
            }

            public Builder WithMetricsFactory(IMetricsFactory factory)
            {
                this._metrics = factory.CreateMetrics();
                return this;
            }

            public Builder WithScopeManager(IScopeManager scopeManager)
            {
                this._scopeManager = scopeManager;
                return this;
            }

            public Builder WithTag(string key, bool value)
            {
                this._initialTags[key] = value;
                return this;
            }

            public Builder WithTag(string key, double value)
            {
                this._initialTags[key] = value;
                return this;
            }

            public Builder WithTag(string key, int value)
            {
                this._initialTags[key] = value;
                return this;
            }

            public Builder WithTag(string key, string value)
            {
                this._initialTags[key] = value;
                return this;
            }

            public Tracer Build()
            {
                if (_metrics == null)
                {
                    _metrics = NoopMetricsFactory.Instance.CreateMetrics();
                }
                if (_reporter == null)
                {
                    _reporter = new RemoteReporter.Builder()
                        .WithMetrics(_metrics)
                        .Build();
                }
                if (_sampler == null)
                {
                    // TODO: RemoteControlledSampler still missing!
                    _sampler = new ConstSampler(true);
                    //_sampler = new RemoteControlledSampler.Builder(_serviceName)
                    //    .withMetrics(metrics)
                    //    .build();
                }
                if (_scopeManager == null)
                {
                    _scopeManager = new AsyncLocalScopeManager();
                }
                if (_propagator == null)
                {
                    _propagator = Propagators.TextMap;
                }

                return new Tracer(_serviceName, _initialTags, _scopeManager, _propagator, _sampler, _reporter, _metrics);
            }

            public static String checkValidServiceName(String serviceName)
            {
                if (string.IsNullOrEmpty(serviceName.Trim()))
                {
                    throw new ArgumentException("Service name must not be null or empty", nameof(serviceName));
                }

                return serviceName;
            }
        }
    }
}
