using CleanEfApi.Domain.Common;

namespace CleanEfApi.Domain.Entities;

public class Quote : BaseEntity
{
    public int QuoteId { get; set; }
    public string? Author { get; set; }
    public string? Content { get; set; }
    public string? Category { get; set; }
    public int Likes { get; set; } = 0;
}