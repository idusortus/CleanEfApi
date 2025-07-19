using CleanEfApi.Application.DTOs;
using CleanEfApi.Application.Interfaces;
using CleanEfApi.Application.Responses;
using CleanEfApi.Application.Results;
using Microsoft.Extensions.Logging;

namespace CleanEfApi.Application.Services;

public class QuoteService(IQuoteRepository _quoteRepository, ILogger<QuoteService> _logger) : IQuoteService
{
    public Task<Result<QuoteResponse>> CreateQuoteAsync(QuoteCreateRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<Result> DeleteQuoteAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<Result<List<QuoteResponse>>> GetAllQuotesAsync(int pageNumber = 1, int pageSize = 10, string? category = null, string? sortBy = null, string? sortOrder = "asc")
    {
        throw new NotImplementedException();
    }

    public async Task<Result<QuoteResponse>> GetQuoteByIdAsync(int id)
    {
        // Parameter validation (still here as it's a business rule for input to this service)
        if (id <= 0)
        {
            _logger.LogWarning("QuoteService: Invalid ID for GetQuoteByIdAsync: {Id}", id);
            return Result<QuoteResponse>.Failure(new ApiError { Field = "id", Message = "ID must be greater than 0.", Code = "INVALID_ID_FORMAT" });
        }

        _logger.LogInformation("QuoteService: Retrieving quote with ID: {Id}", id);

        // Leverage IQuoteRepository for data access
        var quote = await _quoteRepository.GetByIdAsync(id);

        if (quote == null)
        {
            _logger.LogInformation("QuoteService: Quote with ID: {Id} not found.", id);
            return Result<QuoteResponse>.Failure(new ApiError { Code = "QUOTE_NOT_FOUND", Message = $"Quote with ID '{id}' does not exist." });
        }

        // Manual mapping from Quote entity to QuoteResponse DTO (as no AutoMapper)
        var quoteResponse = new QuoteResponse
        {
            QuoteId = quote.QuoteId,
            Author = quote.Author,
            Content = quote.Content,
            Category = quote.Category,
            Likes = quote.Likes,
        };

        _logger.LogInformation("QuoteService: Successfully retrieved quote with ID: {Id}", id);
        return Result<QuoteResponse>.Success(quoteResponse);
    }

    public Task<Result<QuoteResponse>> UpdateQuoteAsync(int id, QuoteUpdateRequest request)
    {
        throw new NotImplementedException();
    }
}
