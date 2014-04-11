(function (_, metrics) {
    'use strict';

    function DashboardController($scope, $interval, $http, metricsEndpoint, metricsService) {
        var updatesInterval;

        function startUpdates() {
            updatesInterval = $interval(updateMetrics, 500);
        }

        function stopUpdates() {
            if (updatesInterval) {
                $interval.cancel(updatesInterval);
            }
        }

        function updateMetrics() {
            $http.get(metricsEndpoint).success(function (data) {
                metricsService.update(data);
                updateDashboard();
            });
        }

        function updateDashboard() {
            $scope.metricNames = metricsService.getSeriesNames();

            _($scope.charts).each(function (c) {
                c.update(metricsService);
            });
        }

        $scope.createChart = function () {
            if ($scope.currentChart.hasMetrics()) {
                $scope.charts.splice(0, 0, $scope.currentChart);
                $scope.currentChart = new metrics.Chart();
            }
        };

        $scope.createChartFromMetric = function (name) {
            $scope.charts.splice(0, 0, new metrics.Chart(name, [name]));
        };

        $scope.addToChart = function () {
            $scope.currentChart.addMetric($scope.metricSearch);
            $scope.metricSearch = '';
        };

        $scope.removeChart = function (chart) {
            $scope.charts.splice($scope.charts.indexOf(chart), 1);
        };

        $scope.metricSearch = '';

        $scope.currentChart = new metrics.Chart();

        $scope.charts = [new metrics.Chart('CPU Usage', ['System.CPU Usage'])];

        $scope.$on('$destroy', function () {
            stopUpdates();
        });

        startUpdates();
    }

    $.extend(true, this, { metrics: { DashboardController: DashboardController } });

}).call(this, this._, this.metrics);