using System.Dynamic;
using CleanEfApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanEfApi.Infrastructure.Database;

public class QuoteDbContext : DbContext
{
    public QuoteDbContext(DbContextOptions<QuoteDbContext> options)
    : base(options)
    {
    }

    public DbSet<Quote> Quotes => Set<Quote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Quote>().HasKey(k => k.QuoteId);
    }
}