
using System;
using System.Diagnostics;
using Metrics.ConcurrencyUtilities;

namespace Metrics.Utils
{
    /// <summary>
    /// An exponentially-weighted moving average.
    /// <a href="http://www.teamquest.com/pdfs/whitepaper/ldavg1.pdf">UNIX Load Average Part 1: How It Works</a>
    /// <a href="http://www.teamquest.com/pdfs/whitepaper/ldavg2.pdf">UNIX Load Average Part 2: Not Your Average Average</a>
    /// <a href="http://en.wikipedia.org/wiki/Moving_average#Exponential_moving_average">EMA</a>
    /// </summary>
    public sealed class EWMA
    {
        private const int Interval = 5;
        private const double SecondsPerMinute = 60.0;
        private const int OneMinute = 1;
        private const int FiveMinutes = 5;
        private const int FifteenMinutes = 15;
        private static readonly double M1Alpha = 1 - Math.Exp(-Interval / SecondsPerMinute / OneMinute);
        private static readonly double M5Alpha = 1 - Math.Exp(-Interval / SecondsPerMinute / FiveMinutes);
        private static readonly double M15Alpha = 1 - Math.Exp(-Interval / SecondsPerMinute / FifteenMinutes);

        private volatile bool initialized = false;
        private VolatileDouble rate = new VolatileDouble(0.0);

        private readonly StripedLongAdder uncounted = new StripedLongAdder();
        private readonly double alpha;
        private readonly double interval;

        public static EWMA OneMinuteEWMA()
        {
            return new EWMA(M1Alpha, Interval, TimeUnit.Seconds);
        }

        public static EWMA FiveMinuteEWMA()
        {
            return new EWMA(M5Alpha, Interval, TimeUnit.Seconds);
        }

        public static EWMA FifteenMinuteEWMA()
        {
            return new EWMA(M15Alpha, Interval, TimeUnit.Seconds);
        }

        public EWMA(double alpha, long interval, TimeUnit intervalUnit)
        {
            Debug.Assert(interval > 0);
            this.interval = intervalUnit.ToNanoseconds(interval);
            this.alpha = alpha;
        }

        public void Update(long value)
        {
            this.uncounted.Add(value);
        }

        public void Tick(long externallyCounted = 0L)
        {
            var count = this.uncounted.GetAndReset() + externallyCounted;

            var instantRate = count / this.interval;
            if (this.initialized)
            {
                var doubleRate = this.rate.GetValue();
                this.rate.SetValue(doubleRate + this.alpha * (instantRate - doubleRate));
            }
            else
            {
                this.rate.SetValue(instantRate);
                this.initialized = true;
            }
        }

        public double GetRate(TimeUnit rateUnit)
        {
            return this.rate.GetValue() * rateUnit.ToNanoseconds(1L);
        }

        public void Reset()
        {
            this.uncounted.Reset();
            this.rate.SetValue(0.0);
        }

        public void Merge(EWMA other)
        {
            if (this.initialized)
            {
                while (true)
                {
                    var workingUc = this.uncounted.GetValue();
                    var newUncounted = workingUc + other.uncounted.GetValue();

                    if (other.initialized)
                    {
                        var workingRate = this.rate.GetValue();
                        // We're adding two weighted averages... they should just be added
                        var newRate = workingRate + other.rate.GetValue();

                        // FIXME:  should use CAS, but merging mihgt not be necessary. see commented code below
                        this.uncounted.Reset();
                        this.uncounted.Add(workingUc + newUncounted);
                        this.rate.SetValue(newRate);
                        break;
                        //if (uncounted.CompareAndSet(workingUc, newUncounted))
                        //{
                        //    // very slight potential for a 
                        //    // race condition if another thread gets
                        //    // through Tick(), start to finish, in between
                        //    // the execution of the line above and the
                        //    // line below
                        //    rate.Set(newRate);
                        //    break;
                        //}
                    }
                    else
                    {
                        // FIXME:  should use CAS, but merging mihgt not be necessary. see commented code below
                        this.uncounted.Reset();
                        this.uncounted.Add(workingUc + newUncounted);
                        break;
                        //if (uncounted.CompareAndSet(workingUc, newUncounted))
                        //{
                        //    break;
                        //}
                    }
                }
            }
            else
            {
                this.uncounted.Add(other.uncounted.GetValue());
                if (other.initialized)
                {
                    this.rate.SetValue(other.rate.GetValue());
                }
            }
        }
    }
}
