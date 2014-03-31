Metrics.NET
===========

[![Build status](https://ci.appveyor.com/api/projects/status/m6ng7uml4wqm3ni2)](https://ci.appveyor.com/project/etishor/metrics-net)

This port is still work in progrss and should not be considered ready for production
------------------------------------------------------------------------------------


.NET Port of the awesome [Java metrics library by codahale](https://github.com/dropwizard/metrics)

This port is also inspired and contains some code from [Daniel Crenna's port](https://github.com/danielcrenna/metrics-net) of the same library.

I have decided to write another .NET port of the same library since Daniel does not actively maintain metrics-net. 
I've also whanted to better understand the internals of the library and try to provide a better API, more suitable for the .NET world.

Intro
-----

The entry point in the Metrics libraty is the [Metric](https://github.com/etishor/Metrics.NET/blob/master/Src/Metrics/Metric.cs) static class. 
Unitll some documentation will be provided that is a good starting point.

The [documentation of the Java Library](http://metrics.codahale.com/manual/core/) is also usefull for understaing the concepts.

The library is published on NuGet as a prerelease library and can be installed with the following command:

    Install-Package Metrics.NET -Pre

Using the library (see Samples for more examples)
-------------------------------------------------

```csharp
    public class SampleMetrics
    {
        private readonly Timer timer = Metric.Timer<SampleMetrics>("Requests", SamplingType.FavourRecent, Unit.Requests);

        public void Request(int i)
        {
            using (this.timer.NewContext()) // measure until disposed
            {
                // do some work
            }
        }
    }
```
Display Metrics
---------------

Schedule a console report to be runned and displayed every 10 seconds:

```csharp
    Metric.Reports.PrintConsoleReport(TimeSpan.FromSeconds(10));
```

Schedule a line to be appended for each metric to a csv file:

```csharp
    Metric.Reports.StoreCSVReports(@"c:\temp\reports\", TimeSpan.FromSeconds(30));
```

Schedule a human readable text repot to be appended to a file every 30 seconds

```csharp
    Metric.Reports.AppendToFile(@"C:\temp\reports\metrics.txt", TimeSpan.FromSeconds(30));
```

Adapters for other applications
-------------------------------

At the moment there is a NancyFx adapter that collects some metrics about Nancy and can provide metrics via http in json or human readable text. See Samples\NancyFx.Sample for more usage details. 

In the ApplicationStartup method of a Nancy bootstrapper:

```csharp
    NancyMetrics.RegisterAllMetrics(pipelines);
    NancyMetrics.ExposeMetrics();
```

TODO
----
A live list of my future plan

* [in progress] Provide a few presets to map performance counters to Gauges ( machine info, process info, CLR stats etc )
* Refactor scheduled report to prevent overlapping
* Find/Implement ConcurrentSkipMap like collection form java - low prio as the performance is good for now
* Provide a way for error reporting (at least for reports that do IO) - maybe a delegate on the Metric class
* Add metrics for NancyFx request/response size
* Provide http endpoint for reporting metrics (based on owin or nancy) together with javascript visualisation solution - the idea is to have out-of-the-box metrics visualization in web apps
* Provide an adapter for hooking into web api for collecting metrics (this might be delayed as I tend to use NancyFx)
* Provide an adapter for hooking into asp.net mvc
* Investigate the possibility of using zeromq to delegate metrics to another process - for accross cluster metrics
* Adapter for graphite and other existing solutions for aggregating metrics
* Mono compatibility
* Investigate the possibility of using Redis as an off-process metrics container (the collections behing the metrics seem to map to redis data types)
* Investigate the possibility for backend to recieve metrics from client js app (not sure it makes sense to capture metrics from js apps - maybe from SPAs)
* Write more tests
* Write more "stupid" benchmarks to be able to keep an eye on how performance changes in time
* Profile & optimize. Also profile existing apps to see the impact of adding metrics

License
-------

This port will always keep the same license as the original Java Metrics library.

The original metrics project is released under this therms (https://github.com/dropwizard/metrics):
Copyright (c) 2010-2013 Coda Hale, Yammer.com
Published under Apache Software License 2.0, see LICENSE

The Daniel Crenna's idiomatic port is relased under this therms (https://github.com/danielcrenna/metrics-net):
The original Metrics project is Copyright (c) 2010-2011 Coda Hale, Yammer.com
This idiomatic port of Metrics to C# and .NET is Copyright (c) 2011 Daniel Crenna
Both works are published under The MIT License, see LICENSE

This port ( Metrics.NET ) is release under Apache 2.0 License ( see LICENSE ) 
Copyright (c) 2014 Iulian Margarintescu

