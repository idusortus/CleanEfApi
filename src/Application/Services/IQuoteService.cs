// src/Application/Services/IQuoteService.cs
using CleanEfApi.Application.DTOs;
using CleanEfApi.Application.Results; // For QuoteResponse, QuoteCreateRequest, QuoteUpdateRequest
// using CleanEfApi.Application.Results; // For the Result and Result<T> pattern

namespace CleanEfApi.Application.Services;

/// <summary>
/// Defines the application-specific business operations for managing quotes.
/// All methods return a Result object to explicitly indicate success or failure.
/// </summary>
public interface IQuoteService
{
    /// <summary>
    /// Retrieves a list of all quotes, with optional filtering, sorting, and pagination.
    /// </summary>
    /// <param name="pageNumber">The page number for pagination (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="category">Optional: Filter quotes by category.</param>
    /// <param name="sortBy">Optional: Field to sort by (e.g., "author", "likes").</param>
    /// <param name="sortOrder">Optional: Sort order ("asc" or "desc").</param>
    /// <returns>A Result containing a list of QuoteResponse DTOs on success, or error details on failure (e.g., invalid pagination parameters).</returns>
    Task<Result<List<QuoteResponse>>> GetAllQuotesAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? category = null,
        string? sortBy = null,
        string? sortOrder = "asc");

    /// <summary>
    /// Retrieves a single quote by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier (QuoteId) of the quote.</param>
    /// <returns>A Result containing a QuoteResponse DTO on success, or error details on failure (e.g., QUOTE_NOT_FOUND, INVALID_ID_FORMAT).</returns>
    Task<Result<QuoteResponse>> GetQuoteByIdAsync(int id);

    /// <summary>
    /// Creates a new quote based on the provided input.
    /// </summary>
    /// <param name="request">The QuoteCreateRequest DTO containing details for the new quote.</param>
    /// <returns>A Result containing the newly created QuoteResponse DTO (including its generated ID) on success, or error details on failure (e.g., business rule violations).</returns>
    Task<Result<QuoteResponse>> CreateQuoteAsync(QuoteCreateRequest request);

    /// <summary>
    /// Updates an existing quote identified by its ID.
    /// </summary>
    /// <param name="id">The unique identifier (QuoteId) of the quote to update.</param>
    /// <param name="request">The QuoteUpdateRequest DTO containing updated details.</param>
    /// <returns>A Result containing the updated QuoteResponse DTO on success, or error details on failure (e.g., QUOTE_NOT_FOUND, CONFLICT_ERROR if specific rules apply).</returns>
    Task<Result<QuoteResponse>> UpdateQuoteAsync(int id, QuoteUpdateRequest request);

    /// <summary>
    /// Deletes a quote identified by its ID.
    /// </summary>
    /// <param name="id">The unique identifier (QuoteId) of the quote to delete.</param>
    /// <returns>A Result indicating success or error details on failure (e.g., QUOTE_NOT_FOUND, DEPENDENCY_EXISTS if the quote cannot be deleted).</returns>
    Task<Result> DeleteQuoteAsync(int id);
}