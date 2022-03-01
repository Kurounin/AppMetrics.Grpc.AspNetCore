using App.Metrics;
using App.Metrics.AspNetCore.Endpoints;
using System;

namespace AppMetrics.Grpc.AspNetCore.HostedServices
{
    public class MetricsServerOptions
    {
        //
        // Summary:
        //     Gets or sets the port to host available endpoints provided by App Metrics.
        //
        // Value:
        //     The App Metrics available endpoint's port.
        public int Port { get; set; } = 5501;

        //
        // Summary:
        //     Gets or sets the environment info endpoint, defaults to /env.
        //
        // Value:
        //     The environment info endpoint.
        public string EnvironmentInfoEndpoint { get; set; } = "/env";

        //
        // Summary:
        //     Gets or sets the metrics endpoint, defaults to /metrics.
        //
        // Value:
        //     The metrics endpoint.
        public string MetricsEndpoint { get; set; } = "/metrics";

        //
        // Summary:
        //     Gets or sets the metrics text endpoint, defaults to metrics-text.
        //
        // Value:
        //     The metrics text endpoint.
        public string MetricsTextEndpoint { get; set; } = "/metrics-text";

        /// <summary>
        /// 
        /// </summary>
        public IMetricsRoot? Metrics { get; set; }

        //
        // Summary:
        //     Gets or sets System.Action`1 to configure the provided App.Metrics.AspNetCore.Endpoints.MetricEndpointsOptions.
        public Action<MetricEndpointsOptions> EndpointOptions { get; set; } = o => { };
    }
}