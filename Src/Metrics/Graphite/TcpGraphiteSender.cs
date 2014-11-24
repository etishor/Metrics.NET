using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Metrics.Logging;
using Metrics.Utils;

namespace Metrics.Graphite
{
    public sealed class TcpGraphiteSender : GraphiteSender
    {
        private static readonly ILog log = LogProvider.GetCurrentClassLogger();

        private readonly string host;
        private readonly int port;

        private TcpClient client;

        public TcpGraphiteSender(string host, int port)
        {
            this.host = host;
            this.port = port;
        }

        protected override void SendData(string data)
        {
            try
            {
                if (this.client == null)
                {
                    this.client = InitClient(this.host, this.port);
                }

                var bytes = Encoding.UTF8.GetBytes(data);

                this.client.GetStream().Write(bytes, 0, bytes.Length);
            }
            catch (Exception x)
            {
                using (this.client) { }
                this.client = null;
                MetricsErrorHandler.Handle(x, "Error sending TCP data to graphite endpoint " + host + ":" + port.ToString());
            }
        }

        public override void Flush()
        {
            try
            {
                if (this.client != null)
                {
                    this.client.GetStream().Flush();
                }
            }
            catch (Exception x)
            {
                using (this.client) { }
                this.client = null;
                MetricsErrorHandler.Handle(x, "Error sending TCP data to graphite endpoint " + host + ":" + port.ToString());
            }
        }

        private static TcpClient InitClient(string host, int port)
        {
            var endpoint = new IPEndPoint(HostResolver.Resolve(host), port);
            var client = new TcpClient();
            client.Connect(endpoint);
            log.Debug(() => "TCP client for graphite initialized for " + host + ":" + port.ToString());
            return client;
        }


        protected override void Dispose(bool disposing)
        {
            using (this.client)
            {
                try
                {
                    this.client.Close();
                }
                catch { }
            }
            this.client = null;
            base.Dispose(disposing);
        }
    }
}
