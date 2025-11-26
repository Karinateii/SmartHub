using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace SmartHub.Api.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unhandled exception processing request {Method} {Path}", context.Request.Method, context.Request.Path);
                context.Response.ContentType = "application/json";
                if (ex is InvalidOperationException)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var payload = new { error = ex.Message, traceId = context.TraceIdentifier };
                    await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    var payload = new { error = "Internal Server Error", traceId = context.TraceIdentifier };
                    await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
                }
            }
        }
    }
}
