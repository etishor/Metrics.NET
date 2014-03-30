Metrics.NET
===========

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

Display Metrics
---------------

Schedule a console report to be runned and displayed every 10 seconds:

    Metric.Reports.PrintConsoleReport(TimeSpan.FromSeconds(10));

Schedule a line to be appended for each metric to a csv file:

    Metric.Reports.StoreCSVReports(@"c:\temp\reports\", TimeSpan.FromSeconds(30));

Schedule a human readable text repot to be appended to a file every 30 seconds

    Metric.Reports.AppendToFile(@"C:\temp\reports\metrics.txt", TimeSpan.FromSeconds(30));

Adapters for other applications
-------------------------------

At the moment there is a NancyFx adapter that collects some metrics about Nancy and can provide metrics via http in json or human readable text. See Samples\NancyFx.Sample for more usage details. 

In the ApplicationStartup method of a Nancy bootstrapper:

    NancyMetrics.RegisterAllMetrics(pipelines);
    NancyMetrics.ExposeMetrics();


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

