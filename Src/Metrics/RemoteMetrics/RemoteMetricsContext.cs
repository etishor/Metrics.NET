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
        private readonly Scheduler scheduler;
        private JsonMetricsContext data = null;

        public RemoteMetricsContext(Uri remoteUri, TimeSpan updateInterval, Func<string, JsonMetricsContext> deserializer)
            : this(new ActionScheduler(), remoteUri, updateInterval, deserializer)
        { }

        public RemoteMetricsContext(Scheduler scheduler, Uri remoteUri, TimeSpan updateInterval, Func<string, JsonMetricsContext> deserializer)
        {
            this.scheduler = scheduler;
            this.scheduler.Start(updateInterval, (c) => UpdateMetrics(remoteUri, deserializer, c));
        }

        private async Task UpdateMetrics(Uri remoteUri, Func<string, JsonMetricsContext> deserializer, CancellationToken token)
        {
            try
            {
                var remoteContext = await HttpRemoteMetrics.FetchRemoteMetrics(remoteUri, deserializer, token);
                remoteContext.Environment.Add("RemoteUri", remoteUri.ToString());
                remoteContext.Environment.Add("RemoteVersion", remoteContext.Version);
                remoteContext.Environment.Add("RemoteTimestamp", remoteContext.Timestamp);

                this.data = remoteContext;
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
