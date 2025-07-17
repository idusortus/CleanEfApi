namespace CleanEfApi.Application.DTOs;

public class QuoteCreateRequest
{
    public string? Author { get; set; }
    public string? Content { get; set; }
    public string? Category { get; set; }
}