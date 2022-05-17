# AppMetrics.Grpc.AspNetCore

![Build status](https://github.com/Kurounin/AppMetrics.Grpc.AspNetCore/actions/workflows/BuildAndPack.yml/badge.svg)
[![NuGet](https://img.shields.io/nuget/v/AppMetrics.Grpc.AspNetCore.svg)](https://www.nuget.org/packages/AppMetrics.Grpc.AspNetCore/)
[![License](https://img.shields.io/github/license/Kurounin/AppMetrics.Grpc.AspNetCore.svg)](https://github.com/Kurounin/AppMetrics.Grpc.AspNetCore/blob/main/LICENSE)


Provides an interceptor that can be used to track [Grpc.AspNetCore](https://www.nuget.org/packages/Grpc.AspNetCore) and [protobuf-net.Grpc.AspNetCore](https://www.nuget.org/packages/protobuf-net.Grpc.AspNetCore) endpoint calls using [App.Metrics.AspNetCore.Tracking](https://www.nuget.org/packages/App.Metrics.AspNetCore.Tracking/) middleware components.

A standalone `MetricsServer` is provided to help expose the metrics on a separate port.

## Installation
Add the package to your application using
```bash
dotnet add package AppMetrics.Grpc.AspNetCore
```

## Usage with Minimal APIs
Add grpc metrics before http metrics:
```c#
using AppMetrics.Grpc.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var metrics = App.Metrics.AppMetrics.CreateDefaultBuilder()
    .OutputMetrics.AsPrometheusPlainText()
    .Build();

builder.WebHost
    .ConfigureMetrics(metrics)
    .UseGrpcMetrics()
    .UseMetrics();
```

## Usage without Minimal APIs
Add grpc metrics before http metrics:
```c#
using AppMetrics.Grpc.AspNetCore;

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
            .UseGrpcMetrics()
            .UseMetrics()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    }
}
```

## Usage of standalone MetricsServer
To expose metrics on a different port using the `MetricsServer` call `AddMetricsServer` either on a `IHostBuilder` or a `IWebHostBuilder` and pass the same `IMetricsRoot` instance used in the main host:
```c#
builder.WebHost
    .ConfigureMetrics(metrics)
    .UseGrpcMetrics()
    .UseMetrics()
    .AddMetricsServer(metrics, endpointsOptions =>
    {
        endpointsOptions.MetricsEndpointOutputFormatter = metrics.OutputMetricsFormatters.OfType<MetricsJsonOutputFormatter>().First();
        endpointsOptions.MetricsTextEndpointOutputFormatter = metrics.OutputMetricsFormatters.OfType<MetricsPrometheusTextOutputFormatter>().First();
        endpointsOptions.EnvironmentInfoEndpointEnabled = false;
    });
```

### Configuration
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

## Grafana
The following dashboard can be used to view the exported metrics in Grafana: https://grafana.com/grafana/dashboards/15840

## License
The source code and documentation in this project are released under the [MIT License](https://github.com/Kurounin/AppMetrics.Grpc.AspNetCore/blob/main/LICENSE).