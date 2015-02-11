using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Metrics;
using System.Net.NetworkInformation;
using System.Collections;

namespace Metrics.Tests.MetricsConfigTests
{
    public class HttpEndpointTests
    {
        private int GetUsedPort()
        {
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();
            IEnumerator myEnum = tcpConnInfoArray.GetEnumerator();

            while (myEnum.MoveNext())
            {
                TcpConnectionInformation TCPInfo = (TcpConnectionInformation)myEnum.Current;
                return TCPInfo.LocalEndPoint.Port;
            }
            throw new Exception("No used port found.");
        }

        [Fact]
        public void HttpEndpointCanBeDisposed()
        {
            var config = Metric.Config.WithHttpEndpoint("http://localhost/metricstest/HttpListenerTests/HttpEndpointCanBeDisposed/");
            config.Dispose();
        }

        [Fact]
        public void HttpEndpointDoesNotThrowIfPortIsOccupied()
        {
            var usedPort = GetUsedPort();
            Metric.Config.WithHttpEndpoint(String.Format("http://localhost:{0}/", usedPort));
        }

        [Fact]
        public void HttpEndportLogsAnErrorIfPortIsOccupied()
        {
            var loggedAnError = false;
            var config = Metric.Config;
            config.WithErrorHandler((exception, s) => { loggedAnError = true; }, true);
            config.WithErrorHandler((exception) => { loggedAnError = true; }, true);

            var usedPort = GetUsedPort();
            config.WithHttpEndpoint(String.Format("http://localhost:{0}/", usedPort));
            Assert.True(loggedAnError);
        }

        [Fact]
        public void SecondCallToWithHttpEndportDoesNotThrow()
        {
            var config = Metric.Config.WithHttpEndpoint("http://localhost/metricstest/HttpListenerTests/sameendpoint/");
            config.WithHttpEndpoint("http://localhost/metricstest/HttpListenerTests/sameendpoint/");
        }

        [Fact]
        public void HttpEndpointDoesNotThrowOnDispose()
        {
            var usedPort = GetUsedPort();

            var config = Metric.Config.WithHttpEndpoint(String.Format("http://localhost:{0}/", usedPort));
            config.Dispose();
        }

    }
}
