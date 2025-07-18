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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using System.Security.Claims;

using Microsoft.AspNetCore.Authentication.Google;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container. 
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationActionFilter>();
})
.ConfigureApiBehaviorOptions(options =>
    options.SuppressModelStateInvalidFilter=true);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi();
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

//// AUTH
// Configure ASP.NET Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Configure Identity password options (adjust for production security)
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.SignIn.RequireConfirmedAccount = false; // Set to true for email confirmation in production
})
.AddEntityFrameworkStores<QuoteDbContext>()
.AddDefaultTokenProviders();
// Configure Authentication Schemes
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // Default for API requests
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;   // Default for unauthenticated requests
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;           // Used by Identity for external logins
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
})
// Add Google External Auth Provider
.AddGoogle(googleOptions =>
{
    googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
    googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
    // Request basic profile and email scopes for user info
    googleOptions.Scope.Add("profile");
    googleOptions.Scope.Add("email");
    googleOptions.SaveTokens = true; // Essential to get access tokens from Google, if you needed them
                                     // for subsequent calls to Google APIs (e.g., Google Drive)
});

builder.Services.AddAuthorization();

// --- SCALAR / OPENAPI CONFIGURATION FOR AUTHENTICATION ---
builder.Services.AddOpenApi(options =>
{
});

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
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();