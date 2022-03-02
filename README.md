# AppMetrics.Grpc.AspNetCore

![Build status](https://github.com/Kurounin/AppMetrics.Grpc.AspNetCore/actions/workflows/BuildAndPack.yml/badge.svg)
[![NuGet](https://img.shields.io/nuget/v/AppMetrics.Grpc.AspNetCore.svg)](https://www.nuget.org/packages/AppMetrics.Grpc.AspNetCore/)
[![License](https://img.shields.io/badge/license-MIT-brightgreen)](https://github.com/Kurounin/AppMetrics.Grpc.AspNetCore/blob/main/LICENSE)


Provides an interceptor that can be used to track [protobuf-net.Grpc.AspNetCore](https://www.nuget.org/packages/protobuf-net.Grpc.AspNetCore) endpoint calls using [App.Metrics.AspNetCore.Tracking](https://www.nuget.org/packages/App.Metrics.AspNetCore.Tracking/) middleware components.

A standalone `MetricsServer` is provided to help expose the metrics on a separate port.

## Installation
Add the package to your application using
```bash
dotnet add package AppMetrics.Grpc.AspNetCore
```

## Usage
Add interceptor when registering code-first services:
```c#
using AppMetrics.Grpc.AspNetCore.Interceptors;

public void ConfigureServices(IServiceCollection services)
{
    services.AddCodeFirstGrpc(config =>
    {
        config.ResponseCompressionLevel = System.IO.Compression.CompressionLevel.Optimal;
        config.Interceptors.Add<MetricsInterceptor>();
    });
}
```

To expose metrics on a different port using the `MetricsServer` call `AddMetricsServer` either on a `IHostBuilder` or a `IWebHostBuilder` and pass the same `IMetricsRoot` instance used in the main host:
```c#
using AppMetrics.Grpc.AspNetCore.HostedServices;
using App.Metrics;
using App.Metrics.AspNetCore;
using App.Metrics.Formatters.Json;
using App.Metrics.Formatters.Prometheus;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Linq;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) {
        var metrics = App.Metrics.AppMetrics.CreateDefaultBuilder()
            .OutputMetrics.AsPrometheusPlainText()
            .Build();

        return Host.CreateDefaultBuilder(args)
            .ConfigureMetrics(metrics)
            .UseMetrics()
            .AddMetricsServer(metrics, endpointsOptions =>
            {
                endpointsOptions.MetricsEndpointOutputFormatter = metrics.OutputMetricsFormatters.OfType<MetricsJsonOutputFormatter>().First();
                endpointsOptions.MetricsTextEndpointOutputFormatter = metrics.OutputMetricsFormatters.OfType<MetricsPrometheusTextOutputFormatter>().First();
                endpointsOptions.EnvironmentInfoEndpointEnabled = false;
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    }
}
```

## Configuration
Optionally add the following configuration to your `appsettings.json`
```json
"MetricsServerOptions": {
  "EnvironmentInfoEndpoint": "/env",
  "MetricsEndpoint": "/metrics",
  "MetricsTextEndpoint": "/metrics-text",
  "Port": 5501
}
```

By default the `MetricsServer` will listen on port **5501** on all interfaces.


## License
AppMetrics.Grpc.AspNetCore is [MIT-licensed](https://github.com/Kurounin/AppMetrics.Grpc.AspNetCore/blob/main/LICENSE).