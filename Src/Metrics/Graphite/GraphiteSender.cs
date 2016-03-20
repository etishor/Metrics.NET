using System;

namespace Metrics.Graphite
{
    public abstract class GraphiteSender : IDisposable
    {
        protected string MetricNamePrefix;
        public virtual void Send(string name, string value, string timestamp)
        {
            var metricName = string.IsNullOrEmpty(MetricNamePrefix) ? name : string.Format("{0}.{1}", MetricNamePrefix, name);
            var data = string.Concat(metricName, " ", value, " ", timestamp, "\n");
            SendData(data);
        }

        public abstract void Flush();

        protected abstract void SendData(string data);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        { }
    }
}
