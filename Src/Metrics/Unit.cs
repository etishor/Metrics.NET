
using System;
using Metrics.Utils;
namespace Metrics
{
    public class Unit : IHideObjectMembers
    {
        public static readonly Unit None = new Unit();
        public static readonly Unit Requests = new Unit("Requests");
        public static readonly Unit Errors = new Unit("Errors");
        public static readonly Unit Results = new Unit("Results");
        public static readonly Unit Calls = new Unit("Calls");
        public static readonly Unit Items = new Unit("Items");
        public static readonly Unit MegaBytes = new Unit("Mb");
        public static readonly Unit KiloBytes = new Unit("Kb");
        public static readonly Unit Bytes = new Unit("bytes");

        public static Unit Custom(string name)
        {
            return new Unit(name);
        }

        private Unit()
        {
            this.Name = string.Empty;
        }

        public Unit(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("name must not be null", "name");
            }

            this.Name = name;
        }

        public string Name { get; private set; }

        public virtual string FormatCount(long value)
        {
            if (!string.IsNullOrEmpty(this.Name))
            {
                return string.Format("{0} {1}", value, this.Name);
            }
            return value.ToString();
        }

        public virtual string FormatValue(double value)
        {
            if (!string.IsNullOrEmpty(this.Name))
            {
                return string.Format("{0:F2} {1}", value, this.Name);
            }
            return value.ToString("F2");
        }

        public virtual string FormatValue(string value)
        {
            if (!string.IsNullOrEmpty(this.Name))
            {
                return string.Format("{0} {1}", value, this.Name);
            }
            return value;
        }

        public virtual string FormatRate(double value, TimeUnit timeUnit)
        {
            return string.Format("{0:F2} {1}/{2}", value, this.Name, timeUnit.Unit());
        }

        public virtual string FormatDuration(double value, TimeUnit? timeUnit)
        {
            return string.Format("{0:F2} {1}", value, timeUnit.HasValue ? timeUnit.Value.Unit() : this.Name);
        }
    }
}
