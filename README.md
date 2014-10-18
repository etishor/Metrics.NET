#Metrics.NET

[![Build status](https://ci.appveyor.com/api/projects/status/m6ng7uml4wqm3ni2)](https://ci.appveyor.com/project/etishor/metrics-net)
[![Mono Build Status](https://api.travis-ci.org/etishor/Metrics.NET.svg)](https://travis-ci.org/etishor/Metrics.NET)

##What it is
Metrics.NET library is a .NET Port, with lots of additional functionality, of the awesome Java [metrics library](https://github.com/dropwizard/metrics) by Coda Hale.

To paraphrase the Java library description - Metrics is a library which gives you unparalleled insight into what your code does in production. Metrics provides a powerful toolkit of ways to measure the behavior of critical components in your production environment.

##What it does
It can measure things like how long requests take, how often errors occur, how many items are in a cache and how often the cache is hit. It can measure any measurable aspect of the host application.

One of the main goals of the library is to have a minimal impact on the measured application, while at the same time making it easy to capture the desired metrics. A lot of effort has been invested in making the public API as simple and intuitive as possible.

Supported runtimes: .NET 4.5.1, .NET 4.5, .NET 4.0 (on a separate branch) & Mono 3.8.0+ ( tested on OsX ).

##Who is it for
[Developers](https://www.youtube.com/watch?v=8To-6VIJZRE) who need to see what is happening inside their systems at run-time. 

Any application, from a long running service to a web app to a console app can benefit from measuring what is happening while it is running. 

For a very good introduction on why metrics are necesary I highly recommand Coda Hale's [talk about metrics](https://www.youtube.com/watch?v=czes-oa0yik) and [slides](https://dl.dropboxusercontent.com/u/2744222/2011-04-09-Metrics-Metrics-Everywhere.pdf).


##Getting Started
To start using the library, install the [Metrics.NET](https://www.nuget.org/packages/Metrics.NET/) NuGet package, using the package management UI or from the package management console run:

    Install-Package Metrics.NET

Add the following Metrics.NET configuration code somewhere in the initialization section of your application:

```csharp
using Metrics;

Metric.Config
    .WithHttpEndpoint("http://localhost:1234/")
    .WithAllCounters();
```

Run the application and point a web browser to http://localhost:1234/ 

The Metrics Visualization App should already have a number of Gauges that are captured from various performance counters.

You can now start measuring things: 

```csharp
public class SampleMetrics
{
    private readonly Timer timer = Metric.Timer("Requests", Unit.Requests);
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

##Documentation

* [Available Metrics](https://github.com/etishor/Metrics.NET/wiki/Available-Metrics)
* [Health Checks](https://github.com/etishor/Metrics.NET/wiki/Health-Checks)
* [Metric Visualization](https://github.com/etishor/Metrics.NET/wiki/Metrics-Visualization)
* [NancyFx Adapter](https://github.com/etishor/Metrics.NET/wiki/NancyFX-Metrics-Adapter)
* [Owin Adapter](https://github.com/etishor/Metrics.NET/wiki/OWIN-Metrics-Adapter)

The [Samples](https://github.com/etishor/Metrics.NET/tree/master/Samples) folder in the repository contains some usage samples.

##Visualization Demo
The visualization app is also avaliable on github: [Metrics.NET.FlotVisualization](https://github.com/etishor/Metrics.NET.FlotVisualization). 

A demo of the visualization app is available [here](http://www.erata.net/Metrics.NET/demo/). This demo uses fake, generated values for the metrics.

##License
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

