// src/Web.Api/Middleware/ExceptionHandlingMiddleware.cs
using CleanEfApi.Application.Responses;
using System.Net;
using System.Text.Json;
using Microsoft.Data.SqlClient; // For SqlException
using Microsoft.EntityFrameworkCore.Storage; // For RetryLimitExceededException

namespace CleanEfApi.Web.Api.Middleware;

// IWebHostEnvironment is correctly part of the primary constructor
public class ExceptionHandlingMiddleware(RequestDelegate _next, ILogger<ExceptionHandlingMiddleware> _logger, IWebHostEnvironment _env)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex) // Catch all unhandled exceptions at this point
        {
            // Log the top-level exception first
            _logger.LogError(ex, "Middleware: Caught unhandled exception. Type: {ExceptionType}, Message: {Message}", ex.GetType().FullName, ex.Message);

            // Determine if it's a database connectivity issue using the helper method
            if (IsDatabaseConnectivityException(ex))
            {
                await HandleDatabaseConnectivityExceptionAsync(context, ex);
            }
            else
            {
                await HandleGenericInternalServerErrorAsync(context, ex);
            }
        }
    }

    /// <summary>
    /// Determines if an exception (or any of its inner exceptions) is related to database connectivity issues.
    /// </summary>
    /// <param name="ex">The exception to check.</param>
    /// <returns>True if it's a known database connectivity exception, false otherwise.</returns>
    private bool IsDatabaseConnectivityException(Exception ex)
    {
        // Direct checks for common root database exceptions
        if (ex is SqlException || ex is TimeoutException)
        {
            return true;
        }

        // Check for InvalidOperationException related to connection string initialization
        if (ex is InvalidOperationException invalidOpEx && invalidOpEx.Message.Contains("ConnectionString property has not been initialized"))
        {
            return true;
        }

        // Check if it's an EF Core RetryLimitExceededException wrapping a DB issue
        if (ex is RetryLimitExceededException) // No need to check inner here, recursion will do it
        {
            // If the outer exception is RetryLimitExceededException, we want to try to find its root cause
            // by drilling into its inner exception recursively.
            if (ex.InnerException != null)
            {
                return IsDatabaseConnectivityException(ex.InnerException);
            }
            return false; // Should ideally not happen for RetryLimitExceededException if it's the root cause of connectivity issue
        }

        // Recursively check InnerException for wrapped exceptions
        // This handles cases where a DbUpdateException or other EF Core exceptions might wrap a SqlException, etc.
        if (ex.InnerException != null)
        {
            return IsDatabaseConnectivityException(ex.InnerException);
        }

        return false;
    }

    private async Task HandleDatabaseConnectivityExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable; // 503
        context.Response.ContentType = "application/json";

        var errorDetailMessage = _env.IsDevelopment()
            ? $"Original Error: {ex.Message.Substring(0, Math.Min(ex.Message.Length, 200))}... (Type: {ex.GetType().FullName})"
            : "A temporary issue prevents connection to the database.";

        var errorResponse = ApiResponse.Error(
            "The database is currently unavailable. Please try again later.",
            new List<ApiError>
            {
                new ApiError
                {
                    Code = "DB_UNAVAILABLE",
                    Message = errorDetailMessage
                }
            }
        );

        await context.Response.WriteAsJsonAsync(errorResponse, typeof(ApiResponse), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    private async Task HandleGenericInternalServerErrorAsync(HttpContext context, Exception ex)
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError; // 500
        context.Response.ContentType = "application/json";

        var errorResponse = ApiResponse.Error(
            "An unexpected internal server error occurred.",
            new List<ApiError>
            {
                new ApiError
                {
                    Code = "INTERNAL_SERVER_ERROR",
                    Message = "Please try again later. If the problem persists, contact support." +
                              (_env.IsDevelopment() ? $" (Type: {ex.GetType().FullName}, Message: {ex.Message.Substring(0, Math.Min(ex.Message.Length, 200))}...)" : "")
                }
            }
        );

        await context.Response.WriteAsJsonAsync(errorResponse, typeof(ApiResponse), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }
}