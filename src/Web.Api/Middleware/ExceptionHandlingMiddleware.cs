// src/Web.Api/Middleware/ExceptionHandlingMiddleware.cs
using CleanEfApi.Application.Responses; // Your custom API Response DTOs
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net; // For HttpStatusCode
using System.Text.Json; // For JsonSerializer

namespace CleanEfApi.Web.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate _next, ILogger<ExceptionHandlingMiddleware> _logger)
{
    // InvokeAsync: This is the method that the middleware pipeline will call
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Call the next middleware in the pipeline
            await _next(context);
        }
        catch (Exception ex)
        {
            // An unhandled exception occurred further down the pipeline
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);

            // Set the response status code to 500 Internal Server Error
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            // Create your standardized error response
            var errorResponse = ApiResponse.Error(
                "An unexpected internal server error occurred.",
                new List<ApiError>
                {
                    new ApiError
                    {
                        Code = "INTERNAL_SERVER_ERROR",
                        Message = "Please try again later. If the problem persists, contact support."
                        // In development, you might add 'ex.Message' or 'ex.StackTrace' here for debugging,
                        // but NEVER in production.
                    }
                }
            );

            // Write the error response to the HTTP response body as JSON
            // Use System.Text.Json for serialization as it's the default in .NET Core
            await context.Response.WriteAsJsonAsync(errorResponse, typeof(ApiResponse), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
    }
}