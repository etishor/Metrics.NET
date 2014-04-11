(function ($, _, metrics) {
    'use strict';

    function StringGauge(name) {
        this.value = null;
        this.name = name;

        this.update = function (value) {
            this.value = value;
        };
    }

    function MetricsRegistry() {
        var stringGauges = new metrics.MetricSet(StringGauge),
            gauges = new metrics.MetricSet(metrics.ValueSeries),
            counters = new metrics.MetricSet(metrics.ValueSeries),
            meters = new metrics.MetricSet(metrics.Meter),
            histograms = new metrics.MetricSet(metrics.Histogram),
            timers = new metrics.MetricSet(metrics.Timer);

        this.getSeriesNames = function () {

            return _.union(gauges.getNames(),
                counters.getNames(),
                meters.getNames(),
                histograms.getNames(),
                timers.getNames());
        };

        this.getSeries = function (name) {
            var series = _.union(gauges.getSeries(name),
                counters.getSeries(name),
                meters.getSeries(name),
                histograms.getSeries(name),
                timers.getSeries(name));

            return _(series).map(function (s) {
                return s.series;
            }).flatten().value();
        };

        this.update = function (metricsData) {
            _(metricsData.Gauges).each(function (value, name) {
                var numericValue = parseFloat(value);
                if (isNaN(value)) {
                    stringGauges.get(name).update(value);
                } else {
                    gauges.get(name).update(numericValue);
                }
            });

            _(metricsData.Counters).each(function (value, name) {
                counters.get(name).update(value);
            });

            _(metricsData.Meters).each(function (value, name) {
                meters.get(name).update(value);
            });

            _(metricsData.Histograms).each(function (value, name) {
                histograms.get(name).update(value);
            });

            _(metricsData.Timers).each(function (value, name) {
                timers.get(name).update(value);
            });
        };
    }

    $.extend(true, this, { metrics: { MetricsRegistry: MetricsRegistry } });


}).call(this, this.jQuery, this._, this.metrics);