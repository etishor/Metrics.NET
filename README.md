Metrics.NET
===========

[![Build status](https://ci.appveyor.com/api/projects/status/m6ng7uml4wqm3ni2)](https://ci.appveyor.com/project/etishor/metrics-net)

This port is still work in progress and should not be considered ready for production
------------------------------------------------------------------------------------


.NET Port of the awesome [Java metrics library by Coda Hale](https://github.com/dropwizard/metrics)

This port is also inspired and contains some code from [Daniel Crenna's port](https://github.com/danielcrenna/metrics-net) of the same library.

I have decided to write another .NET port of the same library since Daniel does not actively maintain metrics-net. 
I've also wanted to better understand the internals of the library and try to provide a better API, more suitable for the .NET world.

Intro
-----

The entry point in the Metrics library is the [Metric](https://github.com/etishor/Metrics.NET/blob/master/Src/Metrics/Metric.cs) static class. 
Until some documentation will be provided that is a good starting point.

The [documentation of the Java Library](http://metrics.codahale.com/manual/core/) is also useful for understating the concepts.

The library is published on NuGet as a pre-release library and can be installed with the following command:

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

Schedule a console report to be run and displayed every 10 seconds:

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

[NancyFx.Metrics](https://github.com/etishor/Metrics.NET/wiki/NancyFX-Metrics-Adapter) - includes visualization support


News
----
A visualization app is now available in the Visualization folder. More details on the [wiki](https://github.com/etishor/Metrics.NET/wiki/Metrics.Flot-Visualization)

![Sample Visualization](https://raw.githubusercontent.com/etishor/Metrics.NET/master/Visualization/Metrics.Flot/sample.png)

TODO
----
A live list of my future plan

* [done] Provide a few presets to map performance counters to Gauges ( machine info, process info, CLR stats etc )
* [done] Provide a way for error reporting (at least for reports that do IO) - maybe a delegate on the Metric class
* [done] Add metrics for NancyFx request/response size
* [done] Re-factor scheduled report to prevent overlapping
* [in-progress]Provide http endpoint for reporting metrics (based on owin or nancy) together with javascript visualization solution - the idea is to have out-of-the-box metrics visualization in web apps
* Find/Implement ConcurrentSkipMap like collection form java - low prio as the performance is good for now
* Provide an adapter for hooking into web api for collecting metrics (this might be delayed as I tend to use NancyFx)
* Provide an adapter for hooking into asp.net mvc
* Investigate the possibility of using zeromq to delegate metrics to another process - for accross cluster metrics
* Adapter for graphite and other existing solutions for aggregating metrics
* Mono compatibility
* Investigate the possibility of using Redis as an off-process metrics container (the collections behind the metrics seem to map to redis data types)
* Investigate the possibility for backend to receive metrics from client js app (not sure it makes sense to capture metrics from js apps - maybe from SPAs)
* Write more tests
* Write more "stupid" benchmarks to be able to keep an eye on how performance changes in time
* Profile & optimize. Also profile existing apps to see the impact of adding metrics

License
-------

This port will always keep the same license as the original Java Metrics library.

The original metrics project is released under this therms (https://github.com/dropwizard/metrics):
Copyright (c) 2010-2013 Coda Hale, Yammer.com
Published under Apache Software License 2.0, see LICENSE

The Daniel Crenna's idiomatic port is released under this therms (https://github.com/danielcrenna/metrics-net):
The original Metrics project is Copyright (c) 2010-2011 Coda Hale, Yammer.com
This idiomatic port of Metrics to C# and .NET is Copyright (c) 2011 Daniel Crenna
Both works are published under The MIT License, see LICENSE

This port ( Metrics.NET ) is release under Apache 2.0 License ( see LICENSE ) 
Copyright (c) 2014 Iulian Margarintescu

