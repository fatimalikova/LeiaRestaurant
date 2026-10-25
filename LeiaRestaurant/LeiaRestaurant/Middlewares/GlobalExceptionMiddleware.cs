using Leia.Application.Exceptions;
using Leia.Application.Helpers;
using System.Text.Json;

namespace Leia.Api.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
                await HandleException(context, ex);
            }
        }

        private async Task HandleException(HttpContext context, Exception ex)
        {
            if (context.Response.HasStarted)
                throw ex;

            var statusCode = ex is ApiException apiEx ? apiEx.StatusCode : 500;

            if (statusCode == 500)
                _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);

            var response = statusCode switch
            {
                400 => ResponseModelHelper.BadRequestResult<object>(ex.Message),
                401 => ResponseModelHelper.UnauthorizedResult<object>(ex.Message),
                403 => ResponseModelHelper.ForbiddenResult<object>(ex.Message),
                404 => ResponseModelHelper.NotFoundResult<object>(ex.Message),
                409 => ResponseModelHelper.ConflictResult<object>(ex.Message),
                _ => ResponseModelHelper.ErrorResult<object>("An unexpected error occurred.")
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var json = JsonSerializer.Serialize(response, _jsonOptions);
            await context.Response.WriteAsync(json);
        }
    }
}