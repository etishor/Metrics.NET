using System;

namespace Metrics.Graphite
{
    public abstract class GraphiteSender : IDisposable
    {
        public virtual void Send(string name, string value, string timestamp)
        {
            var data = string.Concat(name, " ", value, " ", timestamp, "\n");
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
