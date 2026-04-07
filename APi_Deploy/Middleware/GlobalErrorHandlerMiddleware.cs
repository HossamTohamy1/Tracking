using APi_Presentation.ViewModel;
using Domain.Enums;
using Domain.Exceptions;
using Serilog;
using Serilog.Context;
using System.Text.Json;

namespace APi_Presentation.Middleware
{
    public class GlobalErrorHandlerMiddleware : IMiddleware
    {
        private readonly IWebHostEnvironment _environment;
        private static readonly Serilog.ILogger Logger = Log.ForContext<GlobalErrorHandlerMiddleware>();

        public GlobalErrorHandlerMiddleware(IWebHostEnvironment environment)
        {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var traceId = context.TraceIdentifier;
            var correlationId = Guid.NewGuid().ToString("N")[..8];

            using (LogContext.PushProperty("CorrelationId", correlationId))
            using (LogContext.PushProperty("TraceId", traceId))
            using (LogContext.PushProperty("RequestPath", context.Request.Path))
            using (LogContext.PushProperty("RequestMethod", context.Request.Method))
            {
                try
                {
                    context.Response.Headers.Append("X-Correlation-Id", correlationId);
                    await next(context);
                }
                catch (BaseApplicationException appEx)
                {
                    await HandleApplicationExceptionAsync(context, appEx, traceId, correlationId);
                }
                catch (UnauthorizedAccessException unauthorizedEx)
                {
                    await HandleUnauthorizedExceptionAsync(context, unauthorizedEx, traceId, correlationId);
                }
                catch (Exception ex)
                {
                    await HandleUnexpectedExceptionAsync(context, ex, traceId, correlationId);
                }
            }
        }

        private async Task HandleApplicationExceptionAsync(
            HttpContext context,
            BaseApplicationException exception,
            string traceId,
            string correlationId)
        {
            using (LogContext.PushProperty("Module", exception.Module))
            using (LogContext.PushProperty("ErrorCode", exception.ErrorCode))
            {
                if (exception is BusinessLogicException)
                    Logger.Warning(exception, "Business logic error: {Message}", exception.Message);
                else if (exception is ValidationException validationEx)
                    Logger.Warning(exception, "Validation error: {Message}. Errors: {@ValidationErrors}",
                        exception.Message, validationEx.ValidationErrors);
                else
                    Logger.Error(exception, "Application error: {Message}", exception.Message);
            }

            object response;

            if (exception is ValidationException ve && ve.ValidationErrors.Any())
            {
                response = ResponseViewModel<object>.ValidationError(
                    ve.Message,
                    ve.ValidationErrors,
                    _environment.IsDevelopment() ? traceId : null);
            }
            else
            {
                response = ResponseViewModel<bool>.Error(
                    exception.Message,
                    exception.ErrorCode,
                    _environment.IsDevelopment() ? traceId : null);
            }

            await WriteResponseAsync(context, response, exception.HttpStatusCode, traceId, correlationId);
        }

        private async Task HandleUnauthorizedExceptionAsync(
            HttpContext context,
            UnauthorizedAccessException exception,
            string traceId,
            string correlationId)
        {
            Logger.Warning(exception, "Unauthorized access: {Message}", exception.Message);

            var response = ResponseViewModel<bool>.Error(
                "Access denied.",
                AppErrorCode.UnauthorizedAccess,
                _environment.IsDevelopment() ? traceId : null);

            await WriteResponseAsync(context, response, StatusCodes.Status403Forbidden, traceId, correlationId);
        }

        private async Task HandleUnexpectedExceptionAsync(
            HttpContext context,
            Exception exception,
            string traceId,
            string correlationId)
        {
            Logger.Error(exception, "Unexpected error occurred");

            var response = ResponseViewModel<bool>.Error(
                _environment.IsDevelopment()
                    ? $"Unexpected error. TraceId: {traceId}"
                    : "An unexpected error occurred. Please try again later.",
                AppErrorCode.InternalServerError,
                _environment.IsDevelopment() ? traceId : null);

            await WriteResponseAsync(context, response, StatusCodes.Status500InternalServerError, traceId, correlationId);
        }

        private async Task WriteResponseAsync(
            HttpContext context,
            object response,
            int statusCode,
            string traceId,
            string correlationId)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            context.Response.Headers.Append("X-Trace-Id", traceId);

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = _environment.IsDevelopment()
            };

            await context.Response.WriteAsJsonAsync(response, jsonOptions);
        }
    }
}