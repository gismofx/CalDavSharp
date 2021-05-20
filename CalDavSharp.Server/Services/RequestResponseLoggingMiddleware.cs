using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IO;

namespace CalDavSharp.Server.Middleware
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<RequestResponseLoggingMiddleware>();
            _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
        }

        public async Task Invoke(HttpContext context)
        {
            await LogRequest(context);
            await LogResponse(context);
        }

        private async Task LogRequest(HttpContext context)
        {
            context.Request.EnableBuffering();

            string headers = String.Empty;
            foreach (var key in context.Request.Headers.Keys)
                headers += key + "=" + context.Request.Headers[key] + Environment.NewLine;

            await using var requestStream = _recyclableMemoryStreamManager.GetStream();
            await context.Request.Body.CopyToAsync(requestStream);
            _logger.LogInformation($"{Environment.NewLine}{Environment.NewLine}Http Request Information:{Environment.NewLine}" +
                                   $"From Client: {context.Request.Headers["User-Agent"].FirstOrDefault()}{Environment.NewLine}" +
                                   $"Referrer: {context.Request.Headers["Referer"].FirstOrDefault()}{Environment.NewLine}" +
                                   $"Schema: {context.Request.Scheme}{Environment.NewLine}" +
                                   $"Host: {context.Request.Host}{Environment.NewLine}" +
                                   $"Path: {context.Request.Path}{Environment.NewLine}" +
                                   $"Method: {context.Request.Method}{Environment.NewLine}" +
                                   $"QueryString: {context.Request.QueryString}{Environment.NewLine}" +
                                   $"Raw Header: {headers}{Environment.NewLine}" +
                                   $"Request Body: {ReadStreamInChunks(requestStream)}");
            context.Request.Body.Position = 0;
        }

        private async Task LogResponse(HttpContext context)
        {
            var originalBodyStream = context.Response.Body;

            await using var responseBody = _recyclableMemoryStreamManager.GetStream();
            context.Response.Body = responseBody;

            await _next(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var text = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            //bug UserNameclaims
            var u1 = context.User?.Identity?.Name??"null";
            var u2 = context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "null";


            var thing = context.User.FindFirst(ClaimTypes.Name);
            var claimsCount = context.User.Claims.Count();
            var user = context.User.ToString();
            string userName = thing?.Value ?? @"[No Logged In User(Yet)]"; //(context.User.Claims.Count() > 0) ? context.User.Claims?.Where(c => c.Type.EndsWith(@"identity/claims/name", StringComparison.OrdinalIgnoreCase)).FirstOrDefault().Value : @"[No Logged In User(Yet)]"; ;


            _logger.LogInformation($"{Environment.NewLine}Http Response Information:{Environment.NewLine}" +
                                   $"Schema: {context.Request.Scheme}{Environment.NewLine}" +
                                   $"Host: {context.Request.Host}{Environment.NewLine}" +
                                   $"Path: {context.Request.Path}{Environment.NewLine}" +
                                   $"Authenticated Username: {userName}{Environment.NewLine}" +
                                   $"ClaimCount: {u1}-{u2}-{claimsCount}{Environment.NewLine}" +
                                   $"QueryString: {context.Request.QueryString}{Environment.NewLine}" +
                                   $"Response Body: {text}");

            await responseBody.CopyToAsync(originalBodyStream);
        }

        private static string ReadStreamInChunks(Stream stream)
        {
            const int readChunkBufferLength = 4096;

            stream.Seek(0, SeekOrigin.Begin);

            using var textWriter = new StringWriter();
            using var reader = new StreamReader(stream);

            var readChunk = new char[readChunkBufferLength];
            int readChunkLength;

            do
            {
                readChunkLength = reader.ReadBlock(readChunk, 0, readChunkBufferLength);
                textWriter.Write(readChunk, 0, readChunkLength);
            } while (readChunkLength > 0);

            return textWriter.ToString();
        }
    }
}