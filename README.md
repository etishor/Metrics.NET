Metrics.NET
===========

[![Build status](https://ci.appveyor.com/api/projects/status/m6ng7uml4wqm3ni2)](https://ci.appveyor.com/project/etishor/metrics-net)
[![Mono Build Status](https://api.travis-ci.org/etishor/Metrics.NET.svg)](https://travis-ci.org/etishor/Metrics.NET)

###Intro

The Metrics.NET library provides a way of instrumenting applications with custom metrics (timers, histograms, counters etc) that can be reported in various ways and can provide insights on what is happening inside a running application. 

This library is a .NET Port of the awesome Java [metrics library](https://github.com/dropwizard/metrics) by Coda Hale.

For a very good introduction on why metrics are necesary I highly recommand Coda Hale's [talk about metrics](https://www.youtube.com/watch?v=czes-oa0yik) and [slides](https://dl.dropboxusercontent.com/u/2744222/2011-04-09-Metrics-Metrics-Everywhere.pdf).

The library is [published on NuGet](https://www.nuget.org/packages/Metrics.NET/) can be installed with the following command:

    Install-Package Metrics.NET

Supported runtimes: .NET 4.5.1, .NET 4.5, .NET 4.0 (on a separate branch) & Mono 3.8.0 ( tested on OsX )

The API of the library might change until a 1.X version will be made available.

###Visualization Demo

A demo of the visualization app is available [here](http://www.erata.net/Metrics.NET/demo/). This demo uses fake, generated values for the metrics.

The visualization app is also avaliable on github : [Metrics.NET.FlotVisualization](https://github.com/etishor/Metrics.NET.FlotVisualization). 

###Documentation

* [Available Metrics](https://github.com/etishor/Metrics.NET/wiki/Available-Metrics)
* [Health Checks](https://github.com/etishor/Metrics.NET/wiki/Health-Checks)
* [Metric Visualization](https://github.com/etishor/Metrics.NET/wiki/Metrics-Visualization)
* [Documentation Wiki](https://github.com/etishor/Metrics.NET/wiki/)

###Quick Usage Sample

```csharp
public class SampleMetrics
{
    private readonly Timer timer = Metric.Timer("Requests", SamplingType.FavourRecent, Unit.Requests);
    private readonly Counter counter = Metric.Counter("ConcurrentRequests", Unit.Requests);

    public void Request(int i)
    {
        this.counter.Increment();
        using (this.timer.NewContext()) // measure until disposed
        {
            // do some work
        }
        this.counter.Decrement();
    }
}
```

###Adapters for other applications

Adapters integrate Metrics.NET with other libraries & frameworks.

* [Nancy.Metrics](https://github.com/etishor/Metrics.NET/wiki/NancyFX-Metrics-Adapter)
* [Owin.Metrics](https://github.com/etishor/Metrics.NET/wiki/OWIN-Metrics-Adapter) - thanks to [@alhardy - Allan Hardy](https://github.com/alhardy)

###TODO
A live list of my future plan

* Enhance the visualization app to support contexts, tags, user values and set metrics
* Push metrics out of process, to dedicated metrics service
* Investigate the possibility of using zeromq to delegate metrics to another process - for across cluster metrics
* Adapter for graphite and other existing solutions for aggregating metrics
* Investigate the possibility of using Redis as an off-process metrics container (the collections behind the metrics seem to map to redis data types)
* Investigate the possibility for backend to receive metrics from client js app (not sure it makes sense to capture metrics from js apps - maybe from SPAs)

###License
.NET Port of the awesome [Java metrics library by Coda Hale](https://github.com/dropwizard/metrics)

This port is also inspired and contains some code from [Daniel Crenna's port](https://github.com/danielcrenna/metrics-net) of the same library.

I have decided to write another .NET port of the same library since Daniel does not actively maintain metrics-net. 
I've also wanted to better understand the internals of the library and try to provide a better API, more suitable for the .NET world.

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

