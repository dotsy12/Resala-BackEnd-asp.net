using BackEnd.Application.Common.ResponseFormat;
using BackEnd.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace BackEnd.Api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "💥 Unexpected error occurred in {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);

                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";

            HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
            string message = "حدث خطأ داخلي في الخادم. يرجى المحاولة لاحقاً.";

            if (ex is DomainException domainEx)
            {
                statusCode = HttpStatusCode.BadRequest;

                message = domainEx.Message;

                _logger.LogWarning(ex,
                    "Domain error occurred: {Message}",
                    domainEx.Message);
            }
            else
            {
                _logger.LogError(ex,
                    "Unexpected error occurred in {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);
            }

            context.Response.StatusCode = (int)statusCode;

            var response = ApiResponse<object>.Fail(
                message,
                statusCode
            );

            var json = JsonSerializer.Serialize(response,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

            await context.Response.WriteAsync(json);
        }
    }
}