using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using OpenTracing.Jaeger.Exceptions;
using OpenTracing.Jaeger.Metrics;
using OpenTracing.Jaeger.Senders;

namespace OpenTracing.Jaeger.Reporters
{
    public class RemoteReporter : IReporter
    {
        public static readonly TimeSpan DEFAULT_FLUSH_INTERVAL_MS = TimeSpan.FromMilliseconds(100);
        public const int DEFAULT_MAX_QUEUE_SIZE = 100;

        private readonly BufferBlock<Func<CancellationToken, Task>> _commandQueue;
        private readonly Task _queueProcessorTask;
        private readonly Task _flushTask;
        private readonly ISender _sender;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly IMetrics _metrics;

        private RemoteReporter(ISender sender, TimeSpan flushInterval, int maxQueueSize, IMetrics metrics)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _sender = sender;
            _metrics = metrics;
            _commandQueue = new BufferBlock<Func<CancellationToken, Task>>(new DataflowBlockOptions {BoundedCapacity = maxQueueSize});
            _queueProcessorTask = ThreadWorker(_cancellationTokenSource.Token);
            _flushTask = FlushTimer(flushInterval, _cancellationTokenSource.Token);
        }

        private async Task ThreadWorker(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (!await _commandQueue.OutputAvailableAsync(cancellationToken))
                        return;

                    var command = await _commandQueue.ReceiveAsync(cancellationToken);

                    try
                    {
                        await command.Invoke(cancellationToken);
                    }
                    catch (SenderException e)
                    {
                        _metrics.ReporterFailure.Inc(e.DroppedSpans);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("QueueProcessor error: {0}", e);
                    // Do nothing, and try again on next span.
                }
            }
        }

        private async Task FlushTimer(TimeSpan interval, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    Flush();
                    await Task.Delay(interval, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    return;
                }
            }
        }

        public void Dispose()
        {
            try
            {
                bool added = _commandQueue.Post(cancellationToken =>
                {
                    _commandQueue.Complete();
                    return Task.CompletedTask;
                });
                if (added)
                {
                    // best-effort: if we can't add CloseCommand in this time then it probably will never happen
                    if (!_queueProcessorTask.Wait(10000))
                    {
                        _cancellationTokenSource.Cancel();
                        if (!Task.WaitAll(new Task[] {_queueProcessorTask, _flushTask}, 1000))
                            throw new Exception("Timeout");
                    }
                }
                else
                {
                    // TODO: log.warn not error
                    _cancellationTokenSource.Cancel();
                    Console.Error.WriteLine("Unable to cleanly close RemoteReporter, command queue is full - probably the"
                                            + " sender is stuck");
                }
                _cancellationTokenSource.Cancel();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Interrupted: {0}", e);
            }

            try
            {
                int n = _sender.CloseAsync(_cancellationTokenSource.Token).Result;
                _metrics.ReporterSuccess.Inc(n);
            }
            catch (SenderException e)
            {
                _metrics.ReporterFailure.Inc(e.DroppedSpans);
            }
        }

        public void Report(Span span)
        {
            // Its better to drop spans, than to block here
            bool added = _commandQueue.Post(async cancellationToken => await _sender.AppendAsync(span, cancellationToken));

            if (!added)
            {
                _metrics.ReporterDropped.Inc(1);
            }
        }

        private void Flush()
        {
            // to reduce the number of updateGauge stats, we only emit queue length on flush
            _metrics.ReporterQueueLength.Update(_commandQueue.Count);

            // We can safely drop FlushCommand when the queue is full - sender should take care of flushing
            // in such case
            _commandQueue.Post(async cancellationToken =>
            {
                int n = await _sender.FlushAsync(cancellationToken);
                _metrics.ReporterSuccess.Inc(n);
            });
        }

        public class Builder
        {
            private ISender sender;
            private TimeSpan flushInterval = DEFAULT_FLUSH_INTERVAL_MS;
            private int maxQueueSize = DEFAULT_MAX_QUEUE_SIZE;

            private IMetrics metrics;

            public Builder WithFlushInterval(TimeSpan flushInterval)
            {
                this.flushInterval = flushInterval;
                return this;
            }

            public Builder WithMaxQueueSize(int maxQueueSize)
            {
                this.maxQueueSize = maxQueueSize;
                return this;
            }

            public Builder WithMetrics(IMetrics metrics)
            {
                this.metrics = metrics;
                return this;
            }

            public Builder WithSender(ISender sender)
            {
                this.sender = sender;
                return this;
            }

            public RemoteReporter Build()
            {
                if (sender == null)
                {
                    sender = new UdpSender();
                }
                if (metrics == null)
                {
                    metrics = NoopMetricsFactory.Instance.CreateMetrics();
                }
                return new RemoteReporter(sender, flushInterval, maxQueueSize, metrics);
            }
        }
    }
}
