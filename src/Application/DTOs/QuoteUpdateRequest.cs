namespace CleanEfApi.Application.DTOs;

public class QuoteUpdateRequest
{
    public string? Author { get; set; }
    public string? Content { get; set; }
    public string? Category { get; set; }
    public int Likes { get; set; }
}