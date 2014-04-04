(function ($, _) {
    'use strict';

    function MetricSet(InstanceType) {
        var metrics = {};

        this.get = function (name) {
            var result = metrics[name];
            if (!result) {
                result = new InstanceType(name);
                metrics[name] = result;
            }
            return result;
        };

        this.getSeries = function (names) {
            return _(metrics).map(function (m) {
                return m.getSeries();
            }).flatten()
            .where(function (s) {
                return !names || names.length === 0 || matchName(s, names);
            }).value();
        };

        this.getNames = function () {
            return _(metrics).map(function (m) {
                return m.getNames();
            }).flatten().value();
        };

        function matchName(series, names) {
            if (typeof (names) === typeof ('string')) {
                return series.name.indexOf(names) === 0;
            }
            return _(names).any(function (n) {
                return series.name.indexOf(n) === 0;
            });
        }
    }

    $.extend(true, this, { metrics: { MetricSet: MetricSet } });

}).call(this, this.jQuery, this._);