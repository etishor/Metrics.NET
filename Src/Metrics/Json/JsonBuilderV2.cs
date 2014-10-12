using System.Globalization;
using System.Linq;
using Metrics.MetricData;
using Metrics.Utils;

namespace Metrics.Json
{
    public sealed class JsonBuilderV2
    {
        public const int Version = 2;
        public const string MetricsMimeType = "application/vnd.metrics.net.v2.metrics+json";

#if !DEBUG
        private const bool DefaultIndented = false;
#else
        private const bool DefaultIndented = true;
#endif
        public static string BuildJson(MetricsData data) { return BuildJson(data, Clock.Default, indented: DefaultIndented); }

        public static string BuildJson(MetricsData data, Clock clock, bool indented = DefaultIndented)
        {
            var version = Version.ToString(CultureInfo.InvariantCulture);
            var timestamp = clock.UTCDateTime.ToString("yyyy-MM-ddTHH:mm:ss.ffffK", CultureInfo.InvariantCulture);

            return JsonMetricsContext.FromContext(data, version, timestamp, AppEnvironment.Current.ToArray())
                .ToJsonObject()
                .AsJson(indented);
        }
    }
}
