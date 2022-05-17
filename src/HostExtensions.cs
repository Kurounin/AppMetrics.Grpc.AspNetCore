using App.Metrics;
using App.Metrics.AspNetCore.Endpoints;
using AppMetrics.Grpc.AspNetCore.HostedServices;
using AppMetrics.Grpc.AspNetCore.Interceptors;
using Grpc.AspNetCore.Server;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace AppMetrics.Grpc.AspNetCore
{
    public static class HostExtensions
    {
        /// <summary>
        /// Adds App Metrics Grpc middleware to the Microsoft.AspNetCore.Hosting.IHostBuilder.
        /// </summary>
        /// <param name="hostBuilder">The Microsoft.AspNetCore.Hosting.IHostBuilder.</param>
        /// <returns> A reference to this instance after the operation has completed.</returns>
        public static IHostBuilder UseGrpcMetrics(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices(delegate (HostBuilderContext context, IServiceCollection services)
            {
                services.AddSingleton<IStartupFilter>(new StatusStartupFilter());
                services.Configure<GrpcServiceOptions>(config =>
                {
                    config.Interceptors.Add<MetricsInterceptor>();
                });
            });

            return hostBuilder;
        }

        /// <summary>
        /// Adds a hosting service to expose metrics on a different port.
        /// </summary>
        /// <param name="hostBuilder">The Microsoft.AspNetCore.Hosting.IHostBuilder.</param>
        /// <param name="metrics">The App.Metrics.IMetricsRoot.</param>
        /// <param name="optionsDelegate"></param>
        /// <returns></returns>
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
    }
}