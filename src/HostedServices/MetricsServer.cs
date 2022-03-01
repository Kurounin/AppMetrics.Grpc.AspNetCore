using App.Metrics.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;

namespace AppMetrics.Grpc.AspNetCore.HostedServices
{
    public class MetricsServer : IHostedService
    {
        private IHost? _host;
        private readonly ILoggerFactory _loggerFactory;
        private readonly MetricsServerOptions _serverOptions;

        public MetricsServer(ILoggerFactory loggerFactory, IOptions<MetricsServerOptions> options)
        {
            _loggerFactory = loggerFactory;
            _serverOptions = options.Value;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _host = new HostBuilder()
                .ConfigureServices(services =>
                {
                    // Add the logger factory so that logs are configured by the main host
                    services.AddSingleton(_loggerFactory);
                })
                .ConfigureWebHost(webHostBuilder =>
                {
                    webHostBuilder.UseKestrel(options =>
                    {
                        options.ListenAnyIP(_serverOptions.Port);
                    });

                    webHostBuilder
                        .ConfigureAppMetricsHostingConfiguration(options =>
                        {
                            options.MetricsEndpoint = _serverOptions.MetricsEndpoint;
                            options.MetricsTextEndpoint = _serverOptions.MetricsTextEndpoint;
                            options.EnvironmentInfoEndpoint = _serverOptions.EnvironmentInfoEndpoint;
                        })
                        .UseMetrics(options =>
                        {
                            options.EndpointOptions += _serverOptions.EndpointOptions;
                        });

                    webHostBuilder.ConfigureServices(services =>
                    {
                        services.AddRouting();
                        services.AddMetrics(_serverOptions.Metrics);
                    });

                    webHostBuilder.Configure(app =>
                    {
                        app.UseRouting();
                        app.UseMetricsAllEndpoints();
                    });
                })
                .Build();

            return _host.StartAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_host == null)
            {
                return;
            }

            using (_host)
            {
                await _host.StopAsync(cancellationToken);
            }
        }
    }
}