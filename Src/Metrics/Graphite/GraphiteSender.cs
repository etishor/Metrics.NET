using System;
using System.Globalization;
using Metrics.Utils;

namespace Metrics.Graphite
{
    public abstract class GraphiteSender : IDisposable
    {
        public void Send(string name, long value, DateTime timestamp)
        {
            Send(name, value.ToString("D", CultureInfo.InvariantCulture), timestamp);
        }

        public void Send(string name, double value, DateTime timestamp)
        {
            Send(name, value.ToString("F", CultureInfo.InvariantCulture), timestamp);
        }

        public void Send(string name, string value, DateTime timestamp)
        {
            var data = string.Concat(name, " ", value, " ", timestamp.ToUnixTime().ToString("D", CultureInfo.InvariantCulture), "\n");
            SendData(data);
        }
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
