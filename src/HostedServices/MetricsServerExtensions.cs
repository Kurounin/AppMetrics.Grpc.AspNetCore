using System;
using App.Metrics;
using App.Metrics.AspNetCore.Endpoints;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AppMetrics.Grpc.AspNetCore.HostedServices
{
    public static class MetricsServerExtensions
    {
        public static IHostBuilder AddMetricsServer(this IHostBuilder hostBuilder, IMetricsRoot metrics, Action<MetricEndpointsOptions> optionsDelegate)
        {
            hostBuilder.ConfigureServices(delegate (HostBuilderContext context, IServiceCollection services)
            {
                var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
                var metricsServerSection = configuration.GetSection(nameof(MetricsServerOptions));

                services.AddHostedService<MetricsServer>();

                services.Configure<MetricsServerOptions>(options =>
                {
                    metricsServerSection.Bind(options);

                    options.Metrics = metrics;
                    options.EndpointOptions += optionsDelegate;
                });
            });

            return hostBuilder;
        }

        public static IWebHostBuilder AddMetricsServer(this IWebHostBuilder hostBuilder, IMetricsRoot metrics, Action<MetricEndpointsOptions> optionsDelegate)
        {
            hostBuilder.ConfigureServices(delegate (WebHostBuilderContext context, IServiceCollection services)
            {
                var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
                var metricsServerSection = configuration.GetSection(nameof(MetricsServerOptions));

                services.AddHostedService<MetricsServer>();

                services.Configure<MetricsServerOptions>(options =>
                {
                    metricsServerSection.Bind(options);

                    options.Metrics = metrics;
                    options.EndpointOptions += optionsDelegate;
                });
            });

            return hostBuilder;
        }
    }
}
