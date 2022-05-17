using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace AppMetrics.Grpc.AspNetCore.Interceptors
{
    /// <summary>
    /// Will revert the response StatusCode at the end of the pipeline to the original value.
    /// </summary>
    public class StatusMiddleware
    {
        private readonly RequestDelegate _next;

        public StatusMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            finally
            {
                if (context.Response.StatusCode != StatusCodes.Status200OK &&
                    context.Items.TryGetValue(MetricsInterceptor.MetricsOriginalStatusCode, out var statusCode) &&
                    statusCode is int statusCodeValue)
                {
                    context.Response.StatusCode = statusCodeValue;
                }
            }
        }
    }
}