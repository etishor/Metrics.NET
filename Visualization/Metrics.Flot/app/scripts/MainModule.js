(function (angular, metrics) {
    'use strict';

    this.MainModule = angular.module('MainModule', ['ngRoute', 'angular-flot'])
        .value('metricsEndpoint', window.metricsEndpoint || '/json')
        .service('MetricsService', [metrics.MetricsRegistry])
        .controller('DashboardController', ['$scope', '$interval', '$http', 'metricsEndpoint', 'MetricsService', metrics.DashboardController])
        .config(['$routeProvider', function ($routeProvider) {
            $routeProvider
                .when('/', { templateUrl: 'templates/Dashboard.tmpl.html', controller: 'DashboardController' });
        }]);

}).call(this, this.angular, this.metrics);