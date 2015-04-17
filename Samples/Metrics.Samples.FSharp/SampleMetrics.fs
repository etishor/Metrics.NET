namespace Metrics.Samples.FSharp

open Metrics
open Metrics.Core
open System
open System.Collections.Generic
open System.Threading

type SampleMetrics () =
    /// <summary>
    /// keep the total count of the requests
    /// </summary>
    let totalRequestsCounter = Metric.Counter("Requests", Unit.Requests)
    
    /// <summary>
    /// count the current concurrent requests
    /// </summary>
    let concurrentRequestsCounter = Metric.Counter("SampleMetrics.ConcurrentRequests", Unit.Requests)

    /// <summary>
    /// keep a histogram of the input data of our requet method 
    /// </summary>
    let histogramOfData = Metric.Histogram("ResultsExample", Unit.Items, SamplingType.LongTerm)

    /// <summary>
    /// measure the rate at which requests come in
    /// </summary>
    let meter = Metric.Meter("Requests", Unit.Requests)

    /// <summary>
    /// measure the time rate and duration of requests
    /// </summary>
    let timer = Metric.Timer("Requests", Unit.Requests, SamplingType.FavourRecent, TimeUnit.Seconds, TimeUnit.Milliseconds)

    let mutable someValue : double = 1.0;

//    do
//        // define a simple gauge that will provide the instant value of this.someValue when requested
//        let valProvider = fun () -> someValue
//        ignore <| Metric.Gauge ("SampleMetrics.DataValue", valProvider , new Unit("$"))

    member this.Request (i : int) : unit =
        use ctxt = timer.NewContext() // measure until disposed
        someValue <- someValue * (double)(i + 1) // will be reflected in the gauge 

        concurrentRequestsCounter.Increment() // increment concurrent requests counter

        totalRequestsCounter.Increment() // increment total requests counter 

        meter.Mark() // signal a new request to the meter
        let random = System.Random()
        histogramOfData.Update( (int64(random.Next()))) // update the histogram with the input data

        // simulate doing some work
        let ms = Math.Abs(random.Next())
        Thread.Sleep ms

        concurrentRequestsCounter.Decrement() // decrement number of concurrent requests

    static member RunSomeRequests () =
        let test = new SampleMetrics()
        let tasks = new List<Thread>()
        for i = 0 to 10 do
            let j = i;
            tasks.Add(new Thread(fun () -> test.Request(j)))

        tasks.ForEach(fun t -> t.Start())
        tasks.ForEach(fun t -> t.Join())