using CleanEfApi.Application.DTOs;
using CleanEfApi.Domain.Entities;
using CleanEfApi.Infrastructure.Database;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CleanEfApi.Web.Api.Controllers;

[ApiController]
[Route("/api/quotes")]
public class QuotesController(QuoteDbContext _context) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<ActionResult<QuoteResponse>> GetQuote(int id)
    {
        var quote = await _context.Quotes.FindAsync(id);
        if (quote == null) return NotFound();
        var qr = new QuoteResponse
        {
            Author = quote.Author,
            Content = quote.Content,
            Category = quote.Category,
            Likes = quote.Likes
        };

        return Ok(qr);
    }

    [HttpPost()]
    public async Task<ActionResult<QuoteResponse>> PostQuote([FromBody] QuoteCreateRequest req)
    {
        // translate req to Quote
        var quote = new Quote
        {
            Author = req.Author,
            Content = req.Content,
            Category = req.Category,
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        };

        _context.Quotes.Add(quote);
        await _context.SaveChangesAsync();

        // translate Quote to QuoteResponse
        var qr = new QuoteResponse
        {
            QuoteId = quote.QuoteId,
            Author = quote.Author,
            Content = quote.Content,
            Likes = quote.Likes
        };

        // Return a link to the newly created quote
        return CreatedAtAction(nameof(GetQuote), new { id = qr.QuoteId }, qr);
    }
}