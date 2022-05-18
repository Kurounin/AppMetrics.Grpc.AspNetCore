using App.Metrics;
using App.Metrics.AspNetCore.Tracking;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AppMetrics.Grpc.AspNetCore.Interceptors {

    public class MetricsInterceptor : Interceptor
    {
        private static Dictionary<string, bool> _routesIgnoreStatus = new Dictionary<string, bool>();
        private readonly IReadOnlyList<Regex> _ignoredRoutesRegex;
        internal const string MetricsOriginalStatusCode = "AppMetrics.Grpc.AspNetCore_OriginalStatusCode";

        public MetricsInterceptor(IOptions<MetricsWebTrackingOptions> trackingMiddlewareOptionsAccessor, IMetrics metrics)
        {
            _ignoredRoutesRegex = trackingMiddlewareOptionsAccessor.Value.IgnoredRoutesRegex;
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            AddMetricsCurrentRouteName(context);

            try
            {
                return await base.UnaryServerHandler(request, context, continuation);
            }
            catch (Exception ex)
            {
                UpdateResponseStatusCode(context, ex);

                throw;
            }
        }

        public override async Task ServerStreamingServerHandler<TRequest, TResponse>(TRequest request, IServerStreamWriter<TResponse> responseStream, ServerCallContext context, ServerStreamingServerMethod<TRequest, TResponse> continuation)
        {
            AddMetricsCurrentRouteName(context);

            try
            {
                await base.ServerStreamingServerHandler(request, responseStream, context, continuation);
            }
            catch (Exception ex)
            {
                UpdateResponseStatusCode(context, ex);

                throw;
            }
        }

        public override async Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, ServerCallContext context, ClientStreamingServerMethod<TRequest, TResponse> continuation)
        {
            AddMetricsCurrentRouteName(context);

            try
            {
                return await base.ClientStreamingServerHandler(requestStream, context, continuation);
            }
            catch (Exception ex)
            {
                UpdateResponseStatusCode(context, ex);

                throw;
            }
        }

        public override async Task DuplexStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, IServerStreamWriter<TResponse> responseStream, ServerCallContext context, DuplexStreamingServerMethod<TRequest, TResponse> continuation)
        {
            AddMetricsCurrentRouteName(context);

            try
            {
                await base.DuplexStreamingServerHandler(requestStream, responseStream, context, continuation);
            }
            catch (Exception ex)
            {
                UpdateResponseStatusCode(context, ex);

                throw;
            }
        }

        private void AddMetricsCurrentRouteName(ServerCallContext context)
        {
            var templateRoute = (context.GetHttpContext().Request.Path.Value ?? "").ToLowerInvariant();

            if (!IsNotAnIgnoredRoute(templateRoute))
            {
                context.GetHttpContext().AddMetricsCurrentRouteName(templateRoute);
            }
        }

        private bool IsNotAnIgnoredRoute(string url)
        {
            if (_routesIgnoreStatus.TryGetValue(url, out var isIgnored))
            {
                return isIgnored;
            }

            if (_ignoredRoutesRegex.Any() && !string.IsNullOrEmpty(url))
            {
                var isRouteIgnored = _ignoredRoutesRegex.Any(ignorePattern => ignorePattern.IsMatch(url));

                var routesIgnoreStatus = new Dictionary<string, bool>(_routesIgnoreStatus);
                routesIgnoreStatus[url] = isRouteIgnored;

                Interlocked.Exchange(ref _routesIgnoreStatus, routesIgnoreStatus);

                return isRouteIgnored;
            }

            return false;
        }

        private void UpdateResponseStatusCode(ServerCallContext context, Exception exception)
        {
            var statusCode = StatusCodes.Status500InternalServerError;

            if (exception is RpcException rpcException)
            {
                // use a matching status code, similar with https://github.com/grpc/grpc/blob/master/doc/http-grpc-status-mapping.md
                switch (rpcException.StatusCode)
                {
                    case StatusCode.OK:
                        statusCode = StatusCodes.Status200OK;
                        break;
                    case StatusCode.Cancelled:
                        statusCode = StatusCodes.Status400BadRequest;
                        break;
                    case StatusCode.Unknown:
                        statusCode = StatusCodes.Status500InternalServerError;
                        break;
                    case StatusCode.InvalidArgument:
                        statusCode = StatusCodes.Status406NotAcceptable;
                        break;
                    case StatusCode.DeadlineExceeded:
                        statusCode = StatusCodes.Status504GatewayTimeout;
                        break;
                    case StatusCode.NotFound:
                        statusCode = StatusCodes.Status404NotFound;
                        break;
                    case StatusCode.AlreadyExists:
                        statusCode = StatusCodes.Status409Conflict;
                        break;
                    case StatusCode.PermissionDenied:
                        statusCode = StatusCodes.Status403Forbidden;
                        break;
                    case StatusCode.ResourceExhausted:
                        statusCode = StatusCodes.Status429TooManyRequests;
                        break;
                    case StatusCode.FailedPrecondition:
                        statusCode = StatusCodes.Status412PreconditionFailed;
                        break;
                    case StatusCode.Aborted:
                        statusCode = StatusCodes.Status503ServiceUnavailable;
                        break;
                    case StatusCode.OutOfRange:
                        statusCode = StatusCodes.Status416RangeNotSatisfiable;
                        break;
                    case StatusCode.Unimplemented:
                        statusCode = StatusCodes.Status501NotImplemented;
                        break;
                    case StatusCode.Internal:
                        statusCode = StatusCodes.Status500InternalServerError;
                        break;
                    case StatusCode.Unavailable:
                        statusCode = StatusCodes.Status503ServiceUnavailable;
                        break;
                    case StatusCode.DataLoss:
                        statusCode = StatusCodes.Status400BadRequest;
                        break;
                    case StatusCode.Unauthenticated:
                        statusCode = StatusCodes.Status401Unauthorized;
                        break;
                }
            }

            var httpContext = context.GetHttpContext();
            
            httpContext.Items[MetricsOriginalStatusCode] = httpContext.Response.StatusCode;
            httpContext.Response.StatusCode = statusCode;
        }
    }
}