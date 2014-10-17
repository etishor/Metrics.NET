using System;
using System.Net;
using Metrics.Json;

namespace Metrics.RemoteMetrics
{
    public static class HttpRemoteMetrics
    {
        private class CustomClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri address)
            {
                HttpWebRequest request = base.GetWebRequest(address) as HttpWebRequest;
                request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                return request;
            }
        }

        public static JsonMetricsContext FetchRemoteMetrics(Uri remoteUri, Func<string, JsonMetricsContext> deserializer)
        {
            using (CustomClient client = new CustomClient())
            {
                client.Headers.Add("Accept-Encoding", "gizp");
                var json = client.DownloadString(remoteUri);
                return deserializer(json);
            }
        }
    }
}
