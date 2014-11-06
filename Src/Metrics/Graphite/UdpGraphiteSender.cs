using System.Net.Sockets;
using System.Text;

namespace Metrics.Graphite
{
    public sealed class UdpGraphiteSender : GraphiteSender
    {
        private readonly UdpClient client;

        public UdpGraphiteSender(string host, int port)
        {
            this.client = new UdpClient(host, port);
        }

        protected override void SendData(string data)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            this.client.Send(bytes, bytes.Length);
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
