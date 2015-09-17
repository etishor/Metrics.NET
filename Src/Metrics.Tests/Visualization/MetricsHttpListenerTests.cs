using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Metrics.Visualization;
using Xunit;

namespace Metrics.Tests.Visualization
{
    public class MetricsHttpListenerTests
    {
        private static string Endpoint(string test)
        {
            return "http://localhost:58888/metricstest/" + test + "/";
        }
        private static Task<MetricsHttpListener> StartListener(string endpoint)
        {
            var context = new TestContext();
            return MetricsHttpListener.StartHttpListenerAsync(Endpoint(endpoint), context.DataProvider, () => new HealthStatus(), CancellationToken.None);
        }

        [Fact]
        public async Task MetricsHttpListener_CanBeDisposed()
        {
            var listener = await StartListener("HttpEndpointCanBeDisposed");
            listener.Dispose();
        }
        
        [Fact]
        public async Task MetricsHttpListener_CanBeDoubleDisposed()
        {
            var listener = await StartListener("HttpEndpointCanBeDisposed");
            listener.Dispose();
            listener.Dispose();
        }

        [Fact]
        public async Task MetricsHttpListener_DoesNotThrowIfPortIsOccupied()
        {
            const string endpoint = "OccupiedPort";
            using (var listener = new HttpListener())
            {
                listener.Prefixes.Add(Endpoint(endpoint));
                listener.Start();
                
                var result = await StartListener(endpoint);
                result.Should().BeNull();
            }
        }

        [Fact]
        public async Task MetricsHttpListener_LogsAnErrorIfPortIsOccupied()
        {
            const string endpoint = "OccupiedPort";
            using (var listener = new HttpListener())
            {
                listener.Prefixes.Add(Endpoint(endpoint));
                listener.Start();

                var loggedAnError = false;
                using (var config = Metric.Config)
                {
                    config.WithErrorHandler((exception, s) => { loggedAnError = true; }, true);
                    config.WithErrorHandler((exception) => { loggedAnError = true; }, true);

                    await StartListener(endpoint);
                }
                Assert.True(loggedAnError);
                listener.Close();
            }
        }


        [Fact]
        public void MetricsHttpListener_MetricsConfig_SecondCallToWithHttpEndportDoesNotThrow()
        {
            using (var config = Metric.Config.WithHttpEndpoint("http://localhost:58888/metricstest/HttpListenerTests/sameendpoint/"))
            {
                config.WithHttpEndpoint("http://localhost:58888/metricstest/HttpListenerTests/sameendpoint/");
            }
        }
    }
}
