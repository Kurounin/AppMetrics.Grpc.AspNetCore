using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System;

namespace AppMetrics.Grpc.AspNetCore.Interceptors
{
    /// <summary>
    /// Inserts the StatusMiddleware at the beginning of the pipeline.
    /// </summary>
    public class StatusStartupFilter : IStartupFilter
    {
        /// <inheritdoc />
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return AddStatusMiddleware;

            void AddStatusMiddleware(IApplicationBuilder builder)
            {
                builder.UseMiddleware<StatusMiddleware>();

                next(builder);
            }
        }
    }
}