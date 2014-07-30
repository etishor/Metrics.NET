###0.1.10 / 2014-07-30
* fix json formating (thanks to Evgeniy Kucheruk @kpoxa)

###0.1.9 / 2014-07-04
* make reporting more extensible

###0.1.8
* remove support for .NET 4.0

###0.1.6
* for histograms also store last value
* refactor configuration ( use Metric.Config.With...() )
* add option to completely disable metrics Metric.Config.CompletelyDisableMetrics() (useful for measuring metrics impact)
* simplify health checks