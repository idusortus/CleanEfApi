using CleanEfApi.Application.DTOs;
using CleanEfApi.Application.Responses;
using CleanEfApi.Application.Services;
using CleanEfApi.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CleanEfApi.Web.Api.Controllers;

[ApiController]
[Route("/api/quotes")]
public class QuotesController(
    IQuoteService _quoteService,
    ILogger<QuotesController> _logger)
    : ControllerBase
{
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<QuoteResponse>))] 
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))] 
    public async Task<ActionResult<ApiResponse<QuoteResponse>>> GetQuoteById(int id)
    {
        // 1. Explicit Input Validation for Path Parameters (not handled by ValidationActionFilter)
        if (id <= 0) // Return 400 Bad Request with a structured error response
        {
            _logger.LogWarning("GetQuote: Invalid quote ID received: {QuoteId}", id);
            
            return BadRequest(ApiResponse.Error("Quote ID must be a positive integer.",
                new List<ApiError> { new ApiError { Field = "id", Message = "ID must be greater than 0.", Code = "INVALID_ID_FORMAT" } }));
        }
        _logger.LogInformation("GetQuote: Attempting to retrieve quote with ID: {QuoteId}", id);

        // No try-catch here; letting the global ExceptionHandlingMiddleware catch unhandled exceptions (e.g., DB connection issues).
        var quote = await _quoteService.GetQuoteByIdAsync(id);

        if (quote == null) // 3. Resource Not Found Handling 404
        {
            _logger.LogInformation("GetQuote: Quote with ID: {QuoteId} not found.", id);
            return NotFound(ApiResponse.Error($"Quote with ID '{id}' not found.",
                new List<ApiError> { new ApiError { Code = "QUOTE_NOT_FOUND", Message = $"Quote with ID '{id}' does not exist." } }));
        }

        // 4. DTO Projection for the Response Payload
        var quoteResponse = new QuoteResponse
        {
            QuoteId = quote.QuoteId, 
            Author = quote.Author,
            Content = quote.Content,
            Category = quote.Category,
            Likes = quote.Likes
        };

        _logger.LogInformation("GetQuote: Successfully retrieved quote with ID: {QuoteId}", id);
        return Ok(ApiResponse.Ok(quoteResponse, "Quote retrieved successfully."));
    }

    // [HttpPost]
    // [Authorize]
    // [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ApiResponse<QuoteResponse>))]
    // [ProducesResponseType(StatusCodes.Status400BadRequest, Type=typeof(ApiResponse))]
    // public async Task<ActionResult<QuoteResponse>> PostQuote([FromBody] QuoteCreateRequest req)
    // {
    //     _logger.LogInformation("Controller: PostQuote request recieved for author: {Author}", req.Author);
    //     // translate req to Quote
    //     var quote = new Quote
    //     {
    //         Author = req.Author,
    //         Content = req.Content,
    //         Category = req.Category,
    //         Created = DateTimeOffset.UtcNow,
    //         LastModified = DateTimeOffset.UtcNow
    //     };

    //     _context.Quotes.Add(quote);
    //     await _context.SaveChangesAsync();

        

    //     // translate Quote to QuoteResponse
    //     var qr = new QuoteResponse
    //     {
    //         QuoteId = quote.QuoteId,
    //         Author = quote.Author,
    //         Content = quote.Content,
    //         Likes = quote.Likes
    //     };

    //     // Return a link to the newly created quote
    //     return CreatedAtAction(nameof(GetQuote), new { id = qr.QuoteId }, qr);
    // }
}