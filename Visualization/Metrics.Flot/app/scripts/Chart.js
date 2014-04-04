(function ($, _) {
    'use strict';

    function Chart(name, metrics) {
        var self = this;
        this.name = name;
        this.data = [];
        this.metrics = metrics || [];
        this.options = {
            legend: { margin: [-130, 0] },
            xaxis: { show: false, min: 0, max: 50 }
        };

        function updateName() {
            self.name = self.metrics[0];
        }

        this.addMetric = function (metric) {
            this.metrics.push(metric);
            this.metrics = _(this.metrics).unique().where(function (m) {
                return m && m.length > 0;
            }).value();
            updateName();
        };

        this.update = function (registry) {
            this.data = registry.getSeries(this.metrics);
        };

        this.hasMetrics = function () {
            return _(this.metrics).any();
        };

        updateName();
    }

    $.extend(true, this, { metrics: { Chart: Chart } });

}).call(this, this.jQuery, this._);