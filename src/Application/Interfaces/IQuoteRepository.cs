// src/Application/Interfaces/IQuoteRepository.cs
using CleanEfApi.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CleanEfApi.Application.Interfaces;

/// <summary>
/// Defines the contract for data access operations related to Quote entities.
/// This interface resides in the Application layer and is independent of any
/// specific ORM (like EF Core) or database technology.
/// It represents the persistence boundary for the Quote aggregate.
/// </summary>
public interface IQuoteRepository
{
    /// <summary>
    /// Retrieves a single Quote entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the quote.</param>
    /// <returns>The Quote entity if found, otherwise null.</returns>
    Task<Quote?> GetByIdAsync(int id);

    /// <summary>
    /// Retrieves all Quote entities.
    /// </summary>
    /// <returns>A list of all Quote entities.</returns>
    Task<List<Quote>> GetAllAsync();

    /// <summary>
    /// Adds a new Quote entity to the persistence context.
    /// Changes are not committed to the database until SaveChangesAsync() is called.
    /// </summary>
    /// <param name="quote">The Quote entity to add.</param>
    Task AddAsync(Quote quote); // Returns Task (void-like)

    /// <summary>
    /// Marks an existing Quote entity for update in the persistence context.
    /// Changes are not committed to the database until SaveChangesAsync() is called.
    /// </summary>
    /// <param name="quote">The Quote entity to update.</param>
    Task UpdateAsync(Quote quote); // Returns Task (void-like)

    /// <summary>
    /// Marks an existing Quote entity for deletion in the persistence context.
    /// Changes are not committed to the database until SaveChangesAsync() is called.
    /// </summary>
    /// <param name="quote">The Quote entity to delete.</param>
    Task DeleteAsync(Quote quote); // Returns Task (void-like)

    /// <summary>
    /// Asynchronously saves all changes made in the persistence context to the database.
    /// This represents the "Commit" part of the Unit of Work.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(); // Returns Task<int> (number of changes)
}