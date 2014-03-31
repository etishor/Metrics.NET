
using System;
using System.Collections.Generic;
using System.Threading;
using Metrics.Utils;

namespace Metrics.Samples
{
    public class SampleMetrics
    {
        /// <summary>
        /// keep the total count of the requests
        /// </summary>
        private readonly Counter totalRequestsCounter = Metric.Counter<SampleMetrics>("Requests", Unit.Requests);

        /// <summary>
        /// count the current concurrent requests
        /// </summary>
        private readonly Counter concurrentRequestsCounter = Metric.Counter("SampleMetrics.ConcurrentRequests", Unit.Requests);

        /// <summary>
        /// keep a histogram of the number of concurrent requests
        /// </summary>
        private readonly Histogram histogramOfConcurrentRequests = Metric.Histogram("SampleMetrics.ConcurrentRequests", Unit.Requests, SamplingType.FavourRecent);

        /// <summary>
        /// keep a histogram of the input data of our requet method 
        /// </summary>
        private readonly Histogram histogramOfData = Metric.Histogram<SampleMetrics>("ResultsExample", Unit.Items, SamplingType.LongTerm);

        /// <summary>
        /// measure the rate at which requests come in
        /// </summary>
        private readonly Meter meter = Metric.Meter<SampleMetrics>("Requests", Unit.Requests);

        /// <summary>
        /// measure the time rate and duration of requests
        /// </summary>
        private readonly Timer timer = Metric.Timer<SampleMetrics>("Requests", Unit.Requests, SamplingType.FavourRecent, TimeUnit.Seconds, TimeUnit.Milliseconds);

        private double someValue = 1;

        public SampleMetrics()
        {
            // define a simple gauge that will provide the instant value of this.someValue when requested
            Metric.Gauge("SampleMetrics.DataValue", () => this.someValue.ToString("F"), new Unit("$"));
        }

        public void Request(int i)
        {
            using (this.timer.NewContext()) // measure until disposed
            {
                someValue *= (i + 1); // will be reflected in the gauge 

                this.concurrentRequestsCounter.Increment(); // increment concurrent requests counter
                this.histogramOfConcurrentRequests.Update(concurrentRequestsCounter.Value);// update the histogram with the concurrent requests counter 

                this.totalRequestsCounter.Increment(); // increment total requests counter 

                this.meter.Mark(); // signal a new request to the meter

                this.histogramOfData.Update(i); // update the histogram with the input data


                // simulate doing some work
                int ms = Math.Abs((int)(ThreadLocalRandom.NextLong() % 1000L));
                Thread.Sleep(ms);

                this.concurrentRequestsCounter.Decrement(); // decrement number of concurrent requests
            }
        }


        public static void RunSomeRequests()
        {
            SampleMetrics test = new SampleMetrics();
            List<Thread> tasks = new List<Thread>();
            for (int i = 0; i < 10; i++)
            {
                int j = i;
                tasks.Add(new Thread(() => test.Request(j)));
            }

            tasks.ForEach(t => t.Start());
            tasks.ForEach(t => t.Join());
        }
    }
}
