using System;
using Metrics.Core;
using Metrics.Json;
using Metrics.MetricData;
using Metrics.Utils;

namespace Metrics.RemoteMetrics
{
    public sealed class RemoteMetricsContext : ReadOnlyMetricsContext, MetricsDataProvider
    {
        private readonly Scheduler scheduler;

        private MetricsData currentData = MetricsData.Empty;

        public RemoteMetricsContext(Uri remoteUri, TimeSpan updateInterval, Func<string, JsonMetricsContext> deserializer)
            : this(new ActionScheduler(), remoteUri, updateInterval, deserializer)
        { }

        public RemoteMetricsContext(Scheduler scheduler, Uri remoteUri, TimeSpan updateInterval, Func<string, JsonMetricsContext> deserializer)
        {
            this.scheduler = scheduler;
            this.scheduler.Start(updateInterval, () => UpdateMetrics(remoteUri, deserializer));
        }

        private void UpdateMetrics(Uri remoteUri, Func<string, JsonMetricsContext> deserializer)
        {
            try
            {
                var remoteContext = HttpRemoteMetrics.FetchRemoteMetrics(remoteUri, deserializer);
                remoteContext.Environment.Add("RemoteUri", remoteUri.ToString());
                remoteContext.Environment.Add("RemoteVersion", remoteContext.Version);
                remoteContext.Environment.Add("RemoteTimestamp", remoteContext.Timestamp);

                this.currentData = remoteContext.ToMetricsData();
            }
            catch
            {
                this.currentData = MetricsData.Empty;
            }
        }

        public override MetricsDataProvider DataProvider { get { return this; } }
        public MetricsData CurrentMetricsData { get { return this.currentData; } }

        public override void Dispose(bool disposing)
        {
            using (this.scheduler) { }
            base.Dispose(disposing);
        }
    }
}
