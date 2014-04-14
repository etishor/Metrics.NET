
using Metrics.Core;
namespace Metrics.Samples
{
    public class HealthChecksSample
    {
        public static void RegisterHealthChecks()
        {
            HealthChecks.RegisterHealthCheck("DatabaseConnected", () =>
            {
                CheckDbIsConnected();
                return "Database Connection OK";
            });

            HealthChecks.RegisterHealthCheck("DiskSpace", () =>
            {
                int freeDiskSpace = GetFreeDiskSpace();

                if (freeDiskSpace <= 512)
                {
                    return HealthCheckResult.Unhealthy("DiskSpace", "Not enough disk space: {0}", freeDiskSpace);
                }
                else
                {
                    return HealthCheckResult.Unhealthy("DiskSpace", "Disk space ok: {0}", freeDiskSpace);
                }
            });
        }

        public static void CheckDbIsConnected()
        {
        }

        public static int GetFreeDiskSpace()
        {
            return 100;
        }

    }
}
