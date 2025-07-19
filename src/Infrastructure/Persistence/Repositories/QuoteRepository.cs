// src/Infrastructure/Persistence/Repositories/QuoteRepository.cs
using CleanEfApi.Domain.Entities; // To work with the Quote entity
using CleanEfApi.Application.Interfaces; // To implement IQuoteRepository
using CleanEfApi.Infrastructure.Persistence; // To use QuoteDbContext
using Microsoft.EntityFrameworkCore; // For EF Core methods like FindAsync, ToListAsync, AddAsync

namespace CleanEfApi.Infrastructure.Persistence.Repositories; // Updated namespace

/// <summary>
/// Implements the IQuoteRepository using Entity Framework Core.
/// This concrete repository is responsible for data access logic for Quote entities.
/// It depends on QuoteDbContext, which is part of the Infrastructure layer.
/// </summary>
public class QuoteRepository : IQuoteRepository
{
    private readonly QuoteDbContext _context;

    /// <summary>
    /// Initializes a new instance of the QuoteRepository.
    /// DbContext is injected by the Dependency Injection container.
    /// </summary>
    /// <param name="context">The application's database context.</param>
    public QuoteRepository(QuoteDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task AddAsync(Quote quote)
    {
        await _context.Quotes.AddAsync(quote);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Quote quote)
    {
        _context.Quotes.Remove(quote);
    }

    /// <inheritdoc />
    public async Task<List<Quote>> GetAllAsync()
    {
        return await _context.Quotes.ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Quote?> GetByIdAsync(int id)
    {
        return await _context.Quotes.FindAsync(id);
    }

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync() // Now returns Task<int> to match interface
    {
        return await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Quote quote)
    {
        _context.Quotes.Update(quote);
    }
}