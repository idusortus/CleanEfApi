using System.Dynamic;
using CleanEfApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace CleanEfApi.Infrastructure.Persistence;

public class QuoteDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
{
    public QuoteDbContext(DbContextOptions<QuoteDbContext> options)
    : base(options)
    {
    }

    public DbSet<Quote> Quotes => Set<Quote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Quote>().HasKey(k => k.QuoteId);
    }
}