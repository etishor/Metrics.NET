(function ($, _, metrics) {
    'use strict';

    function Timer(name) {
        var rate = new metrics.Meter(name + '.Rate'),
            histogram = new metrics.Histogram(name + '.Time');

        this.getNames = function () {
            return [rate.getNames(), histogram.getNames()];
        };

        this.update = function (value) {
            rate.update(value.Rate);
            histogram.update(value.Histogram);
        };

        this.getSeries = function () {
            return _.union(rate.getSeries(), histogram.getSeries());
        };
    }

    $.extend(true, this, { metrics: { Timer: Timer } });

}).call(this, this.jQuery, this._, this.metrics);