using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Metrics.Json;

namespace Metrics.RemoteMetrics
{
    public static class HttpRemoteMetrics
    {
        public static async Task<JsonMetricsContext> FetchRemoteMetrics(Uri remoteUri, Func<string, JsonMetricsContext> deserializer, CancellationToken token)
        {
            using (WebClient client = new WebClient())
            {
                //client.Headers.Add("Accept-Encoding", "gzip");
                var json = await client.DownloadStringTaskAsync(remoteUri).ConfigureAwait(false);
                return deserializer(json);
            }
        }
    }
}
