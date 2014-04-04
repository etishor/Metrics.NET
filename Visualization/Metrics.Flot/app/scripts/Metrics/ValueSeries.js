(function ($, _) {
    'use strict';

    function ValueSeries(name, cutoffSize) {
        var points = [],
            maxLength = cutoffSize || 50,
            data = [];

        this.getNames = function () {
            return [name];
        };

        this.update = function (value) {

            if (points.length >= maxLength) {
                points = points.slice(1);
            }
            points.push(value);
            data = [];
            for (var i = 0; i < points.length; ++i) {
                data.push([i, points[i]]);
            }
            return data;
        };

        this.getSeries = function () {
            return [{
                name: name,
                series: [{
                    label: _( name.split('.')).last(),
                    data: data
                }]
            }];
        };
    }

    $.extend(true, this, { metrics: { ValueSeries: ValueSeries } });

}).call(this, this.jQuery, this._);