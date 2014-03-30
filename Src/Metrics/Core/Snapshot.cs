using System;
using System.Collections.Generic;
using System.Linq;

namespace Metrics.Core
{
    public class Snapshot
    {
        private readonly List<long> values;

        public Snapshot(IEnumerable<long> values)
        {
            this.values = new List<long>(values);
            this.values.Sort();
        }

        public int Size { get { return this.values.Count; } }

        public long Max { get { return this.values.LastOrDefault(); } }
        public long Min { get { return this.values.FirstOrDefault(); } }
        public double Mean { get { return Size == 0 ? 0.0 : this.values.Average(); } }

        public double StdDev
        {
            get
            {
                if (this.Size <= 1)
                {
                    return 0;
                }

                double avg = values.Average();
                double sum = values.Sum(d => Math.Pow(d - avg, 2));
                return Math.Sqrt((sum) / (values.Count() - 1));
            }
        }

        public double Median { get { return GetValue(0.5d); } }
        public double Percentile75 { get { return GetValue(0.75d); } }
        public double Percentile95 { get { return GetValue(0.95d); } }
        public double Percentile98 { get { return GetValue(0.98d); } }
        public double Percentile99 { get { return GetValue(0.99d); } }
        public double Percentile999 { get { return GetValue(0.999d); } }

        public IEnumerable<long> Values { get { return this.values.AsEnumerable(); } }

        public double GetValue(double quantile)
        {
            if (quantile < 0.0 || quantile > 1.0)
            {
                throw new ArgumentException(string.Format("{0} is not in [0..1]", quantile));
            }

            if (this.Size == 0)
            {
                return 0;
            }

            double pos = quantile * (values.Count + 1);

            if (pos < 1)
            {
                return values[0];
            }

            if (pos >= values.Count)
            {
                return values[values.Count - 1];
            }

            double lower = values[(int)pos - 1];
            double upper = values[(int)pos];
            return lower + (pos - Math.Floor(pos)) * (upper - lower);
        }
    }
}
