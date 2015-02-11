
using System;
using Metrics.Utils;
namespace Metrics
{
    public struct Unit : IHideObjectMembers
    {
        public static readonly Unit None = new Unit(string.Empty);
        public static readonly Unit Requests = new Unit("Requests");
        public static readonly Unit Commands = new Unit("Commands");
        public static readonly Unit Calls = new Unit("Calls");
        public static readonly Unit Events = new Unit("Events");
        public static readonly Unit Errors = new Unit("Errors");
        public static readonly Unit Results = new Unit("Results");
        public static readonly Unit Items = new Unit("Items");
        public static readonly Unit MegaBytes = new Unit("Mb");
        public static readonly Unit KiloBytes = new Unit("Kb");
        public static readonly Unit Bytes = new Unit("bytes");
        public static readonly Unit Percent = new Unit("%");
        public static readonly Unit Threads = new Unit("Threads");

        public static Unit Custom(string name)
        {
            return new Unit(name);
        }

        public static implicit operator Unit(string name)
        {
            return Unit.Custom(name);
        }

        public readonly string Name;

        public Unit(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            this.Name = name;
        }

        public override string ToString()
        {
            return this.Name;
        }

        public string FormatCount(long value)
        {
            if (!string.IsNullOrEmpty(this.Name))
            {
                return string.Format("{0} {1}", value, this.Name);
            }
            return value.ToString();
        }

        public string FormatValue(double value)
        {
            if (!string.IsNullOrEmpty(this.Name))
            {
                return string.Format("{0:F2} {1}", value, this.Name);
            }
            return value.ToString("F2");
        }

        public string FormatRate(double value, TimeUnit timeUnit)
        {
            return string.Format("{0:F2} {1}/{2}", value, this.Name, timeUnit.Unit());
        }

        public string FormatDuration(double value, TimeUnit? timeUnit)
        {
            return string.Format("{0:F2} {1}", value, timeUnit.HasValue ? timeUnit.Value.Unit() : this.Name);
        }
    }
}
