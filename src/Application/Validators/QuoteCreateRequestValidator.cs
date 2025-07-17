using CleanEfApi.Application.DTOs;
using FluentValidation;

namespace CleanEfApi.Application.Validators;

public class QuoteCreateRequestValidator : AbstractValidator<QuoteCreateRequest>
{
    public QuoteCreateRequestValidator()
    {
        RuleFor(quote => quote.Author)
            .NotEmpty().WithMessage("Author is required.")
            .MaximumLength(100).WithMessage("Author cannot exceed 100 characters.");

        RuleFor(quote => quote.Content)
            .NotEmpty().WithMessage("Content is required.")
            .MaximumLength(500).WithMessage("Content cannot exceed 500 characters.");

        RuleFor(quote => quote.Category)
            .MaximumLength(50).WithMessage("Category cannot exceed 50 characters.");
    }
}