using Metrics;
using System;
using System.Collections.Generic;

namespace Owin.Metrics
{
    /// <summary>
    /// Helper class to register OWIN Metrics
    /// </summary>
    public static class OwinMetrics
    {
        /// <summary>
        /// Add Metrics Middleware to the Owin pipeline.
        /// Sample: Metric.Config.WithOwin( m => app.Use(m)) 
        /// Where app is the IAppBuilder
        /// </summary>
        /// <param name="config">Chainable configuration object.</param>
        /// <param name="middlewareRegistration">Action used to register middleware. This should generally be app.Use(middleware)</param>
        /// <returns>Chainable configuration object.</returns>
        public static MetricsConfig WithOwin(this MetricsConfig config, Action<object> middlewareRegistration)
        {
            return config.WithOwin(middlewareRegistration, owin =>
                                owin.WithRequestMetricsConfig()
                                    .WithMetricsEndpoint());
        }

        /// <summary>
        /// Add Metrics Middleware to the Owin pipeline.
        /// Sample: Metric.Config.WithOwin( m => app.Use(m)) 
        /// Where app is the IAppBuilder
        /// </summary>
        /// <param name="config">Chainable configuration object.</param>
        /// <param name="middlewareRegistration">Action used to register middleware. This should generally be app.Use(middleware)</param>
        /// <param name="owinConfig">Action used to configure Owin metrics.</param>
        /// <returns>Chainable configuration object.</returns>
        public static MetricsConfig WithOwin(this MetricsConfig config, Action<object> middlewareRegistration, Action<OwinMetricsConfig> owinConfig)
        {
            var owin = config.WithConfigExtension((ctx, hs) => new OwinMetricsConfig(middlewareRegistration, ctx, hs));
            owinConfig(owin);
            return config;
        }

        /// <summary>
        /// Add Metrics Middleware to the Owin pipeline.
        /// Sample: Metric.Config.WithOwin( m =&gt; app.Use(m))
        /// Where app is the IAppBuilder
        /// </summary>
        /// <param name="config">Chainable configuration object.</param>
        /// <param name="middlewareRegistration">Action used to register middleware. This should generally be app.Use(middleware)</param>
        /// <param name="owinConfig">Action used to configure Owin metrics.</param>
        /// <param name="metricNameResolver">
        /// The metric name resolver callback. 
        /// If not provided a metric for each OWIN request will be created for each route and their route parameters. 
        /// These route parameters need to be determined using the underlying web framework.
        /// e.g.
        /// - /sample/1
        /// - /sample/2
        /// </param>
        /// <returns>Chainable configuration object.</returns>
        public static MetricsConfig WithOwin(this MetricsConfig config, Action<object> middlewareRegistration,
            Action<OwinMetricsConfig> owinConfig, Func<IDictionary<string, object>, string> metricNameResolver)
        {
            var owin = config.WithConfigExtension((ctx, hs) => new OwinMetricsConfig(middlewareRegistration, ctx, hs));
            owinConfig(owin);
            return config;
        }
    }
}
