using System;
using System.Net;
using System.Net.Sockets;
using Metrics.Logging;
using Metrics.Utils;

namespace Metrics.Graphite
{
    public sealed class PickleGraphiteSender : GraphiteSender
    {
        private static readonly ILog log = LogProvider.GetCurrentClassLogger();

        public const int DefaultPickleJarSize = 100;

        private readonly string host;
        private readonly int port;

        private readonly int pickleJarSize;

        private TcpClient client;
        private PickleJar jar = new PickleJar();


        public PickleGraphiteSender(string host, int port, int batchSize = DefaultPickleJarSize)
        {
            this.host = host;
            this.port = port;
            this.pickleJarSize = batchSize;
        }

        public override void Send(string name, string value, string timestamp)
        {
            this.jar.Append(name, value, timestamp);

            if (jar.Size >= this.pickleJarSize)
            {
                WriteCurrentJar();
                this.jar = new PickleJar();
            }
        }

        private void WriteCurrentJar()
        {
            try
            {
                if (this.client == null)
                {
                    this.client = InitClient(this.host, this.port);
                }

                this.jar.WritePickleData(this.client.GetStream());
            }
            catch (Exception x)
            {
                using (this.client) { }
                this.client = null;
                MetricsErrorHandler.Handle(x, "Error sending Pickled data to graphite endpoint " + host + ":" + port.ToString());
            }
        }

        protected override void SendData(string data)
        { }

        public override void Flush()
        {
            try
            {
                WriteCurrentJar();
                if (this.client != null)
                {
                    this.client.GetStream().Flush();
                }
            }
            catch (Exception x)
            {
                using (this.client) { }
                this.client = null;
                MetricsErrorHandler.Handle(x, "Error sending Pickled data to graphite endpoint " + host + ":" + port.ToString());
            }
        }

        private static TcpClient InitClient(string host, int port)
        {
            var endpoint = new IPEndPoint(HostResolver.Resolve(host), port);
            var client = new TcpClient();
            client.Connect(endpoint);
            log.Debug(() => "Picked client for graphite initialized for " + host + ":" + port.ToString());
            return client;
        }


        protected override void Dispose(bool disposing)
        {
            Flush();
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
