using System;
using System.Threading;
using System.Threading.Tasks;
using Metrics.Core;
using Metrics.Json;
using Metrics.MetricData;
using Metrics.Utils;

namespace Metrics.RemoteMetrics
{
    public sealed class RemoteMetricsContext : ReadOnlyMetricsContext
    {
        private readonly Uri remoteUri;
        private readonly TimeSpan updateInterval;
        private readonly Func<string, JsonMetricsContext> deserializer;

        private readonly Scheduler scheduler;

        private JsonMetricsContext data = null;

        public RemoteMetricsContext(Uri remoteUri, TimeSpan updateInterval, Func<string, JsonMetricsContext> deserializer)
            : this(new ActionScheduler(), remoteUri, updateInterval, deserializer)
        { }

        public RemoteMetricsContext(Scheduler scheduler, Uri remoteUri, TimeSpan updateInterval, Func<string, JsonMetricsContext> deserializer)
        {
            this.remoteUri = remoteUri;
            this.updateInterval = updateInterval;
            this.deserializer = deserializer;
            this.scheduler = scheduler;

            this.scheduler.Start(updateInterval, (c) => UpdateMetrics(c));
        }

        private async Task UpdateMetrics(CancellationToken token)
        {
            try
            {
                this.data = await HttpRemoteMetrics.FetchRemoteMetrics(this.remoteUri, this.deserializer, token);
            }
            catch
            {
                this.data = null;
            }
        }

        public override MetricsDataProvider DataProvider
        {
            get
            {
                return new JsonMetricsDataProvider(() => this.data);
            }
        }

        public override void Dispose(bool disposing)
        {
            using (this.scheduler) { }
            base.Dispose(disposing);
        }
    }
}
