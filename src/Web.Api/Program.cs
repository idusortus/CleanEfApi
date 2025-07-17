using CleanEfApi.Application.DTOs;
using CleanEfApi.Application.Validators;
using CleanEfApi.Infrastructure.Database;
using FluentValidation.AspNetCore;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using CleanEfApi.Web.Api.Filters;
using CleanEfApi.Web.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container. 
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationActionFilter>();
})
.ConfigureApiBehaviorOptions(options =>
    options.SuppressModelStateInvalidFilter=true);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
// Infrastructure
builder.Services.AddDbContext<QuoteDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("localdb"),
    sqlServerOptionsAction: sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null); 
    }));
// Enable automatic validation during model binding
builder.Services.AddFluentValidationAutoValidation();
// //fluent validation (single, scoped)
// builder.Services.AddScoped<IValidator<QuoteCreateRequest>, QuoteCreateRequestValidator>();
// fluent validation (global)
builder.Services.AddValidatorsFromAssemblyContaining<QuoteCreateRequestValidator>();

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // app.UseDeveloperExceptionPage();
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
        .WithTheme(ScalarTheme.Mars)
        .WithDarkModeToggle(true)
        .WithClientButton(true);
    });
}
else
{
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();