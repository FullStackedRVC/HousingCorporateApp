using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace HouseCom.Error
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly IHostEnvironment _environment;
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(
            IHostEnvironment environment,
            ILogger<GlobalExceptionHandler> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            // Log error asynchronously
            await LogErrorAsync(exception, cancellationToken);

            // Create error response
            var errorResponse = await CreateErrorResponseAsync(context, exception, cancellationToken);

            // Add development details if needed
            await AddDevelopmentDetailsAsync(errorResponse, exception, cancellationToken);

            // Send response
            await SendErrorResponseAsync(context, errorResponse, cancellationToken);

            return true;
        }

        private async Task LogErrorAsync(Exception exception, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                _logger.LogError(
                    exception,
                    "An error occurred at {Time}: {Message}",
                    DateTimeOffset.UtcNow,
                    exception.Message
                );
            }, cancellationToken);
        }

        private async Task<ErrorResponse> CreateErrorResponseAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                var errorResponse = new ErrorResponse
                {
                    TraceId = context.TraceIdentifier,
                    Timestamp = DateTimeOffset.UtcNow
                };

                // Map exceptions to responses
                (errorResponse.StatusCode, errorResponse.Message) = exception switch
                {
                    // Create operation exceptions
                    InvalidOperationException => (
                        HttpStatusCode.BadRequest,
                        "Invalid operation attempted"),
                    ArgumentException => (
                        HttpStatusCode.BadRequest,
                        "Invalid input provided"),

                    // Read operation exceptions
                    KeyNotFoundException => (
                        HttpStatusCode.NotFound,
                        "Requested resource not found"),
                    FileNotFoundException => (
                        HttpStatusCode.NotFound,
                        "Requested file not found"),

                    // Update operation exceptions
                    ConcurrencyException => (
                        HttpStatusCode.Conflict,
                        "The resource was modified by another user"),
                    ValidationException => (
                        HttpStatusCode.UnprocessableEntity,
                        "Validation failed for the request"),

                    // Delete operation exceptions
                    DeleteNotAllowedException => (
                        HttpStatusCode.MethodNotAllowed,
                        "Delete operation not allowed for this resource"),
                    DeleteConstraintException => (
                        HttpStatusCode.Conflict,
                        "Cannot delete due to existing dependencies"),

                    // Authentication/Authorization exceptions
                    UnauthorizedAccessException => (
                        HttpStatusCode.Unauthorized,
                        "Authentication required"),
                    ForbiddenException => (
                        HttpStatusCode.Forbidden,
                        "Access denied to the requested resource"),

                    // Default case for unhandled exceptions
                    _ => (
                        HttpStatusCode.InternalServerError,
                        "An unexpected error occurred")
                };

                return errorResponse;
            }, cancellationToken);
        }

        private async Task AddDevelopmentDetailsAsync(
            ErrorResponse errorResponse,
            Exception exception,
            CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                if (_environment.IsDevelopment())
                {
                    errorResponse.DeveloperMessage = new DeveloperMessage
                    {
                        Exception = exception.GetType().Name,
                        StackTrace = exception.StackTrace,
                        InnerException = exception.InnerException?.Message,
                        Source = exception.Source,
                        TargetSite = exception.TargetSite?.Name
                    };
                }
            }, cancellationToken);
        }

        private async Task SendErrorResponseAsync(
            HttpContext context,
            ErrorResponse errorResponse,
            CancellationToken cancellationToken)
        {
            context.Response.StatusCode = (int)errorResponse.StatusCode;
            context.Response.ContentType = "application/json";

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = _environment.IsDevelopment()
            };

            await context.Response.WriteAsJsonAsync(errorResponse, options, cancellationToken);
        }
    }

    // Custom async-friendly exceptions
    public class ConcurrencyException : Exception
    {
        public ConcurrencyException(string message) : base(message) { }
        public ConcurrencyException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    public class DeleteNotAllowedException : Exception
    {
        public DeleteNotAllowedException(string message) : base(message) { }
        public DeleteNotAllowedException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    public class DeleteConstraintException : Exception
    {
        public DeleteConstraintException(string message) : base(message) { }
        public DeleteConstraintException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
        public ValidationException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message) : base(message) { }
        public ForbiddenException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    // Enhanced response models
    public class ErrorResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }
        public string TraceId { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public DeveloperMessage DeveloperMessage { get; set; }
    }

    public class DeveloperMessage
    {
        public string Exception { get; set; }
        public string StackTrace { get; set; }
        public string InnerException { get; set; }
        public string Source { get; set; }
        public string TargetSite { get; set; }
    }
}