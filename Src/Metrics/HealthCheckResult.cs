using System;

namespace Metrics
{
    /// <summary>
    /// Result of a health check
    /// </summary>
    public struct HealthCheckResult
    {
        /// <summary>
        /// True if the check was successful, false if the check failed.
        /// </summary>
        public readonly bool IsHealthy;

        /// <summary>
        /// Status message of the check. A status can be provided for both healthy and unhealthy states.
        /// </summary>
        public readonly string Message;

        private HealthCheckResult(bool isHealthy, string message)
        {
            this.IsHealthy = isHealthy;
            this.Message = message;
        }

        /// <summary>
        /// Create a healthy status response.
        /// </summary>
        /// <returns>Healthy status response.</returns>
        public static HealthCheckResult Healthy()
        {
            return Healthy("OK");
        }

        /// <summary>
        /// Create a healthy status response.
        /// </summary>
        /// <param name="message">Status message.</param>
        /// <param name="values">Values to format the status message with.</param>
        /// <returns>Healthy status response.</returns>
        public static HealthCheckResult Healthy(string message, params object[] values)
        {
            var status = string.Format(message, values);
            return new HealthCheckResult(true, string.IsNullOrWhiteSpace(status) ? "OK" : status);
        }

        /// <summary>
        /// Create a unhealthy status response.
        /// </summary>
        /// <returns>Unhealthy status response.</returns>
        public static HealthCheckResult Unhealthy()
        {
            return Unhealthy("FAILED");
        }

        /// <summary>
        /// Create a unhealthy status response.
        /// </summary>
        /// <param name="message">Status message.</param>
        /// <param name="values">Values to format the status message with.</param>
        /// <returns>Unhealthy status response.</returns>
        public static HealthCheckResult Unhealthy(string message, params object[] values)
        {
            var status = string.Format(message, values);
            return new HealthCheckResult(false, string.IsNullOrWhiteSpace(status) ? "FAILED" : status);
        }

        /// <summary>
        /// Create a unhealthy status response.
        /// </summary>
        /// <param name="x">Exception to use for reason.</param>
        /// <returns>Unhealthy status response.</returns>
        public static HealthCheckResult Unhealthy(Exception x)
        {
            var status = string.Format("EXCEPTION: {0} - {1}", x.GetType().Name, x.Message);
            return HealthCheckResult.Unhealthy(status);
        }
    }
}
