
namespace Owin.Metrics.Middleware
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    public abstract class MetricMiddleware
    {
        private readonly Regex[] ignorePatterns;

        protected MetricMiddleware(Regex[] ignorePatterns)
        {
            this.ignorePatterns = ignorePatterns;
        }

        protected bool PerformMetric(IDictionary<string, object> environment)
        {
            if (ignorePatterns == null)
            {
                return true;
            }

            var requestPath = environment["owin.RequestPath"] as string;

            if (string.IsNullOrWhiteSpace(requestPath)) return false;

            return !this.ignorePatterns.Any(ignorePattern => ignorePattern.IsMatch(requestPath.TrimStart('/')));
        }
    }
}
