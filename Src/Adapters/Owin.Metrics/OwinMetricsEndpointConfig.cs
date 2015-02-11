
namespace Owin.Metrics
{
    public class OwinMetricsEndpointConfig
    {
        public OwinMetricsEndpointConfig()
        {
            MetricsEndpoint();
            MetricsJsonEndpoint();
            MetricsHealthEndpoint();
            MetricsTextEndpoint();
            MetricsPingEndpoint();
        }

        /// <summary>
        /// Gets or sets a value indicating whether [metrics endpoint enabled].
        /// </summary>
        /// <value>
        /// <c>true</c> if [metrics endpoint enabled]; otherwise, <c>false</c>.
        /// </value>
        internal bool MetricsEndpointEnabled { get; set; }

        /// <summary>
        /// Gets or sets the name of the metrics endpoint.
        /// </summary>
        /// <value>
        /// The name of the metrics endpoint.
        /// </value>
        internal string MetricsEndpointName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [metrics json endpoint enabled].
        /// </summary>
        /// <value>
        /// <c>true</c> if [metrics json endpoint enabled]; otherwise, <c>false</c>.
        /// </value>
        internal bool MetricsJsonEndpointEnabled { get; set; }

        /// <summary>
        /// Gets or sets the name of the metrics json endpoint.
        /// </summary>
        /// <value>
        /// The name of the metrics json endpoint.
        /// </value>
        internal string MetricsJsonEndpointName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [metrics text endpoint enabled].
        /// </summary>
        /// <value>
        /// <c>true</c> if [metrics text endpoint enabled]; otherwise, <c>false</c>.
        /// </value>
        internal bool MetricsTextEndpointEnabled { get; set; }

        /// <summary>
        /// Gets or sets the name of the metrics text endpoint.
        /// </summary>
        /// <value>
        /// The name of the metrics text endpoint.
        /// </value>
        internal string MetricsTextEndpointName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [metrics health endpoint enabled].
        /// </summary>
        /// <value>
        /// <c>true</c> if [metrics health endpoint enabled]; otherwise, <c>false</c>.
        /// </value>
        internal bool MetricsHealthEndpointEnabled { get; set; }

        /// <summary>
        /// Gets or sets the name of the metrics health endpoint.
        /// </summary>
        /// <value>
        /// The name of the metrics health endpoint.
        /// </value>
        internal string MetricsHealthEndpointName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [metrics ping endpoint enabled].
        /// </summary>
        /// <value>
        /// <c>true</c> if [metrics ping endpoint enabled]; otherwise, <c>false</c>.
        /// </value>
        internal bool MetricsPingEndpointEnabled { get; set; }

        /// <summary>
        /// Gets or sets the name of the metrics ping endpoint.
        /// </summary>
        /// <value>
        /// The name of the metrics ping endpoint.
        /// </value>
        internal string MetricsPingEndpointName { get; set; }

        /// <summary>
        /// Configures the metrics endpoint
        /// </summary>
        /// <param name="endpoint">The endpoint e.g http://api.com/{endpoint} </param>
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        /// <returns></returns>
        public OwinMetricsEndpointConfig MetricsEndpoint(string endpoint = "metrics", bool enabled = true)
        {
            MetricsEndpointEnabled = enabled;
            MetricsEndpointName = endpoint;
            return this;
        }

        /// <summary>
        /// Configures the json endpoint which returns metrics as a json result
        /// </summary>
        /// <param name="endpoint">The endpoint e.g http://api.com/{endpoint} </param>        
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        /// <returns></returns>
        public OwinMetricsEndpointConfig MetricsJsonEndpoint(string endpoint = "json", bool enabled = true)
        {
            MetricsJsonEndpointEnabled = enabled;
            MetricsJsonEndpointName = endpoint;
            return this;
        }

        /// <summary>
        /// Configures the text endpoint which returns metrics as human readable text
        /// </summary>
        /// <param name="endpoint">The endpoint e.g http://api.com/{endpoint} </param>        
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        /// <returns></returns>
        public OwinMetricsEndpointConfig MetricsTextEndpoint(string endpoint = "text", bool enabled = true)
        {
            MetricsTextEndpointEnabled = enabled;
            MetricsTextEndpointName = endpoint;
            return this;
        }

        /// <summary>
        /// Configures the health endpoint which returns health status's in json format.
        /// </summary>
        /// <param name="endpoint">The endpoint e.g http://api.com/{endpoint} </param>        
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        /// <returns></returns>
        public OwinMetricsEndpointConfig MetricsHealthEndpoint(string endpoint = "health", bool enabled = true)
        {
            MetricsHealthEndpointEnabled = enabled;
            MetricsHealthEndpointName = endpoint;
            return this;
        }

        /// <summary>
        /// Configures the ping endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint e.g http://api.com/{endpoint} </param>        
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        /// <returns></returns>
        public OwinMetricsEndpointConfig MetricsPingEndpoint(string endpoint = "ping", bool enabled = true)
        {
            MetricsPingEndpointEnabled = enabled;
            MetricsPingEndpointName = endpoint;
            return this;
        }
    }
}
