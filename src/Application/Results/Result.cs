// src/Application/Results/Result.cs
using CleanEfApi.Application.Responses; // To reference your ApiError class
using System.Collections.Generic;
using System.Linq; // For .Any() and .ToList()

namespace CleanEfApi.Application.Results;

/// <summary>
/// Represents the outcome of an operation without a specific return value on success.
/// </summary>
public class Result
{
    public bool IsSuccess { get; protected set; }
    public List<ApiError>? Errors { get; protected set; } // Nullable, only present on failure

    /// <summary>
    /// Protected constructor to enforce the use of static factory methods.
    /// </summary>
    protected Result() { }

    /// <summary>
    /// Creates a successful Result instance.
    /// </summary>
    public static Result Success()
    {
        return new Result { IsSuccess = true, Errors = null };
    }

    /// <summary>
    /// Creates a failed Result instance with a list of errors.
    /// </summary>
    /// <param name="errors">A list of ApiError objects detailing the failures.</param>
    public static Result Failure(List<ApiError> errors)
    {
        // Ensure errors list is not null or empty if it's a failure
        if (errors == null || !errors.Any())
        {
            errors = new List<ApiError> { new ApiError { Code = "UNKNOWN_FAILURE", Message = "An unknown failure occurred." } };
        }
        return new Result { IsSuccess = false, Errors = errors };
    }

    /// <summary>
    /// Creates a failed Result instance with a single error.
    /// </summary>
    /// <param name="error">An ApiError object detailing the failure.</param>
    public static Result Failure(ApiError error)
    {
        if (error == null)
        {
            error = new ApiError { Code = "UNKNOWN_FAILURE", Message = "An unknown failure occurred." };
        }
        return Failure(new List<ApiError> { error });
    }
}

/// <summary>
/// Represents the outcome of an operation with a specific return value on success.
/// </summary>
/// <typeparam name="T">The type of the successful value.</typeparam>
public class Result<T> : Result
{
    public T? Value { get; private set; } // Nullable, only present on success

    /// <summary>
    /// Private constructor to enforce the use of static factory methods.
    /// </summary>
    private Result() : base() { }

    /// <summary>
    /// Creates a successful Result instance with a value.
    /// </summary>
    /// <param name="value">The value returned by the successful operation.</param>
    public static Result<T> Success(T value)
    {
        return new Result<T> { IsSuccess = true, Value = value, Errors = null };
    }

    /// <summary>
    /// Creates a failed Result instance with a list of errors.
    /// Uses the 'new' keyword to hide the base class's static method for clarity.
    /// </summary>
    /// <param name="errors">A list of ApiError objects detailing the failures.</param>
    public static new Result<T> Failure(List<ApiError> errors)
    {
        // Ensure errors list is not null or empty if it's a failure
        if (errors == null || !errors.Any())
        {
            errors = new List<ApiError> { new ApiError { Code = "UNKNOWN_FAILURE", Message = "An unknown failure occurred." } };
        }
        return new Result<T> { IsSuccess = false, Value = default, Errors = errors }; // default(T) for Value
    }

    /// <summary>
    /// Creates a failed Result instance with a single error.
    /// Uses the 'new' keyword to hide the base class's static method for clarity.
    /// </summary>
    /// <param name="error">An ApiError object detailing the failure.</param>
    public static new Result<T> Failure(ApiError error)
    {
        if (error == null)
        {
            error = new ApiError { Code = "UNKNOWN_FAILURE", Message = "An unknown failure occurred." };
        }
        return Failure(new List<ApiError> { error });
    }
}