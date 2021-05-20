using CalDavSharp.Server.Middleware;
using Microsoft.AspNetCore.Builder;

namespace CalDavSharp.Server.Services
{
    public static class RequestResponseLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestResponseLoggingMiddleware>();
        }
    }
}