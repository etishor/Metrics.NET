using System.Net.Sockets;
using System.Text;

namespace Metrics.Graphite
{
    public sealed class TcpGraphiteSender : GraphiteSender
    {
        private readonly TcpClient client;

        public TcpGraphiteSender(string host, int port)
        {
            this.client = new TcpClient();
            this.client.Connect(host, port);
        }

        protected override void SendData(string data)
        {
            var bytes = Encoding.UTF8.GetBytes(data);

            this.client.GetStream().Write(bytes, 0, bytes.Length);
            this.client.GetStream().Flush();
        }

        protected override void Dispose(bool disposing)
        {
            using (this.client)
            {
                this.client.Close();
            }
            base.Dispose(disposing);
        }
    }
}
