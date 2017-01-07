using System;
using System.Net.Http;
using Polly;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http.Headers;
using System.Threading;
using Metrics.Logging;

namespace Metrics.NET.InfluxDB
{
    internal class InfluxDbHttpTransport
    {
        private static readonly ILog log = LogProvider.GetCurrentClassLogger();

        private readonly HttpClient _client;
        private readonly Policy _policy;
        private readonly Uri _uri;
        private readonly ConfigOptions _config;

        internal InfluxDbHttpTransport(Uri uri, string username, string password, ConfigOptions config)
        {
            _uri = uri;
            _config = config;

            var byteArray = Encoding.ASCII.GetBytes(string.Format("{0}:{1}", username, password));

            _client = new HttpClient()
            {
                DefaultRequestHeaders =
                {
                    Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray))
                }
            };

            _policy = new Rate(_config.BreakerRate).AsPolicy();
        }

        internal void Send(IEnumerable<InfluxDbRecord> records)
        {
            if (_config.Verbose)
            {
                foreach (var rec in records)
                {
                    log.DebugFormat("posting: {0}", rec.LineProtocol);
                }
            }

            _policy.ExecuteAndCapture(() =>
            {
                var content = string.Join("\n", records.Select(d => d.LineProtocol));

                var cts = new CancellationTokenSource();

                try
                {
                    Metric.Context("Metrics.NET").Meter("influxdb.post.meter", Unit.Events).Mark();

                    var task = _client.PostAsync(_uri, new StringContent(content), cts.Token);

                    if (task.Wait(TimeSpan.FromMilliseconds(_config.HttpTimeoutMillis)))
                    {
                        if ((int)task.Result.StatusCode == 204)
                        {
                            Metric.Context("Metrics.NET").Meter("influxdb.success.meter", Unit.Events).Mark();
                        }
                        else
                        {
                            Metric.Context("Metrics.NET").Meter("influxdb.fail.meter", Unit.Events).Mark();
                            var response = task.Result.Content.ReadAsStringAsync().Result;
                            throw new Exception(string.Format("Error posting to [{0}] {1} {2}", _uri, task.Result.StatusCode, response));
                        }
                    }
                    else
                    {
                        cts.Cancel();
                        Metric.Context("Metrics.NET").Meter("influxdb.timeout.meter", Unit.Events).Mark();
                        throw new TimeoutException(string.Format("Timed out after {0}ms posting to {1}", _config.HttpTimeoutMillis, _uri));
                    }
                }
                catch (Exception e)
                {
                    cts.Cancel();
                    Metric.Context("Metrics.NET").Meter("influxdb.error.meter", Unit.Events).Mark();

                    var agg = e as AggregateException;
                    if (agg != null)
                    {
                        log.ErrorException(agg.InnerException.Message, agg.InnerException);
                    }
                    else
                    {
                        log.ErrorException(e.InnerException.Message, e.InnerException);
                    }

                    throw;
                }
            });
        }
    }
}
