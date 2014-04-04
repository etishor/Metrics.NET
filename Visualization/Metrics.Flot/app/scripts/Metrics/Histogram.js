(function ($, _, metrics) {
    'use strict';

    function Histogram(name) {
        var count = new metrics.ValueSeries(name + '.Count'),
            max = new metrics.ValueSeries(name + '.Histogram.Max'),
            mean = new metrics.ValueSeries(name + '.Histogram.Mean'),
            min = new metrics.ValueSeries(name + '.Histogram.Min'),
            stdDev = new metrics.ValueSeries(name + '.Histogram.StdDev'),
            median = new metrics.ValueSeries(name + '.Histogram.Median'),
            percentile75 = new metrics.ValueSeries(name + '.Histogram.P75%'),
            percentile95 = new metrics.ValueSeries(name + '.Histogram.P95%'),
            percentile98 = new metrics.ValueSeries(name + '.Histogram.P98%'),
            percentile99 = new metrics.ValueSeries(name + '.Histogram.P99%'),
            percentile999 = new metrics.ValueSeries(name + '.Histogram.P99,9%');

        this.getNames = function () {
            return [name];
        };

        this.update = function (value) {
            count.update(value.count);
            max.update(value.max);
            mean.update(value.mean);
            min.update(value.min);
            stdDev.update(value.stdDev);
            median.update(value.median);
            percentile75.update(value.percentile75);
            percentile95.update(value.percentile95);
            percentile98.update(value.percentile98);
            percentile99.update(value.percentile99);
            percentile999.update(value.percentile999);
        };

        this.getSeries = function () {
            return _.union(
                max.getSeries(),
                mean.getSeries(),
                min.getSeries(),
                stdDev.getSeries(),
                median.getSeries(),
                percentile75.getSeries(),
                percentile95.getSeries(),
                percentile98.getSeries(),
                percentile99.getSeries(),
                percentile999.getSeries());
        };
    }

    $.extend(true, this, { metrics: { Histogram: Histogram } });

}).call(this, this.jQuery, this._, this.metrics);