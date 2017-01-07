using System;
using Polly;

namespace Metrics.NET.InfluxDB
{
    internal class Rate
    {
        internal int Events { get; private set; }

        internal TimeSpan Period { get; private set; }

        internal Rate (int events, TimeSpan period)
        {
            Events = events;
            Period = period;
        }

        /// <summary>
        /// Rate specification in the form of: <code>events / timeframe</code>
        /// </summary>
        internal Rate (string specification)
        {
            var parts = specification.Split ('/');
            Events = int.Parse (parts [0].Trim ());
            Period = TimeSpan.Parse (parts [1].Trim ());
        }

        internal Policy AsPolicy ()
        {
            return Policy.Handle<Exception> ().CircuitBreaker (Events, Period);
        }
    }
}
