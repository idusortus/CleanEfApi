// src/Application/Responses/ApiResponse.cs
namespace CleanEfApi.Application.Responses;

// This class represents individual error details within an ApiResponse
public class ApiError
{
    public string Code { get; set; } = "API_ERROR";
    public string Message { get; set; } = "An error occurred.";
    public string? Field { get; set; } // For validation errors, which field is invalid
}

// This is the base class for all API responses, handling general status and errors
public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = "Request processed."; // General status message
    public List<ApiError> Errors { get; set; } = []; // Only present when Success is false

    // Private constructor to enforce usage of static factory methods
    // This prevents direct instantiation like 'new ApiResponse()'
    protected ApiResponse() {}

    // Factory method for non-generic success (e.g., for DELETE, or simple acknowledgments)
    // Renamed to 'Ok' for clarity, indicating a successful but potentially no-data response
    public static ApiResponse Ok(string message = "Request successful.")
    {
        return new ApiResponse { Success = true, Message = message, Errors = [] };
    }

    // Factory method for generic success (returns an ApiResponse<T>).
    // This uses a generic type parameter on the *method* to create the generic instance.
    public static ApiResponse<T> Ok<T>(T data, string message = "Request successful.")
    {
        return new ApiResponse<T> { Success = true, Message = message, Data = data, Errors = [] };
    }

    // Factory method for errors (can be used for both generic and non-generic contexts)
    public static ApiResponse Error(string message = "An error occurred.", List<ApiError>? errors = null)
    {
        return new ApiResponse { Success = false, Message = message, Errors = errors ?? new List<ApiError>() };
    }
}

// This is the generic class for API responses that include a data payload
// It inherits from the non-generic ApiResponse and does NOT contain its own static factory methods.
// All creation is handled by the base ApiResponse.Ok<T> method.
public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; set; }

    // Public constructor to allow instantiation from the base class's static factory method.
    public ApiResponse() : base() { }
}