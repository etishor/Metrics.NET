
using System;
using Metrics.Core;
namespace Metrics.Samples
{
    public class HealthChecksSample
    {
        public interface IDatabase { void Ping(); }

        public class DatabaseHealthCheck : HealthCheck
        {
            private readonly IDatabase database;
            public DatabaseHealthCheck(IDatabase database)
                : base("DatabaseCheck")
            {
                this.database = database;
                HealthChecks.RegisterHealthCheck(this);
            }

            protected override HealthCheckResult Check()
            {
                // exceptions will be caught and 
                // the result will be unhealthy
                this.database.Ping();
                return HealthCheckResult.Healthy();
            }
        }

        public static void RegisterHealthChecks()
        {
            new DatabaseHealthCheck(null);

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
                    return HealthCheckResult.Unhealthy("Not enough disk space: {0}", freeDiskSpace);
                }
                else
                {
                    return HealthCheckResult.Unhealthy("Disk space ok: {0}", freeDiskSpace);
                }
            });

            HealthChecks.RegisterHealthCheck("SampleOperatoin", () => SampleOperation());
        }

        private static void SampleOperation()
        {
            throw new InvalidOperationException("operation went south");
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
