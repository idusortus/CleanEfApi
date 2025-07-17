# Milan's somewhat simplified structure

Web.Api -> Infrastructure -> App -> Domain
```bash
├───Application
│   ├───Abstractions
│   │   ├───Authentication
│   │   ├───Behaviors
│   │   ├───Data
│   │   └───Messaging
│   ├───Todos
│   │   ├───Complete
│   │   ├───Create
│   │   ├───Delete
│   │   ├───Get
│   │   ├───GetById
│   │   └───Update
│   └───Users
│       ├───GetByEmail
│       ├───GetById
│       ├───Login
│       └───Register
│
├───Domain
│   ├───Todos
│   └───Users
│
├───Infrastructure
│   ├───Authentication
│   ├───Authorization
│   ├───Database
│   │   └───Migrations
│   ├───DomainEvents
│   ├───Time
│   ├───Todos
│   └───Users
├───SharedKernel
│
└───Web.Api
    ├───Endpoints
    │   ├───Todos
    │   └───Users
    ├───Extensions
    ├───Infrastructure
    └───Middleware
```  
## Ex:
```

#### Web.Api -> Application
dotnet add Web.Api/Web.Api.csproj reference Application/Application.csproj

#### Web.Api -> Infrastructure
dotnet add Web.Api/Web.Api.csproj reference Infrastructure/Infrastructure.csproj

#### Infra -> Application
dotnet add Infrastructure/Infrastructure.csproj reference Application/Application.csproj

#### Application -> Domain
dotnet add Application/Application.csproj reference Domain/Domain.csproj
```

# EF Notes
# General EF Core (cli usage) Notes

- Update to latest version of cli tool
```dotnet tool update --global dotnet-ef```

- To install LocalDb without installing the full version of Visual Studio, download SqlServerExpress [Link](https://go.microsoft.com/fwlink/p/?linkid=2216019&clcid=0x409&culture=en-us&country=us) and
  choose a custom configuration. Buried toward the end of the custom options is a [ ] to include LocalDB installation.
- If working with LocalDb, and having issues connecting to the local instance, 
```bash
sqllocaldb stop MSSQLLocalDB
sqllocaldb delete MSSQLLocalDB
sqllocaldb delete MSSQLLocalDB
sqllocaldb start MSSQLLocalDB
```  

## Basic Process 
- Create an entity
- Create something similar to ApplicationDbContext
```csharp
namespace EFApi.Web.Api.Entities;

public class Quote : BaseEntity
{
    public int Id { get; set; }
    public string? Author { get; set; }
    public string? Content { get; set; }
    public string? Category { get; set; } 
    public int Rating { get; set; } = 1;
}

namespace EFApi.Web.Api.Entities;

public abstract class BaseEntity
{
    public DateTimeOffset Created { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset LastModified { get; set; }

    public string? LastModifiedBy { get; set; }
}

using EFApi.Web.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace EFApi.Web.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : base(options)  {  }
    
    public DbSet<Quote> Quotes { get; set; }
}
```
- Install EFcore packages
```
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.18">
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.18" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.18">
```
- Add a connection string to appsettings.Development.json (remember, anything in this config will overwrite what is present in appsettings.json *when running in development mode*)
```json
{
  "ConnectionStrings": {
        "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=QuotesDB;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```
- Add the appropriate efcore package to Web.Api 
```dotnet add package Microsoft.EntityFrameworkCore.Design```  

- Initialize the migration and setup the DB
```
dotnet ef migrations add InitialCreate
dotnet ef database update
```

- If following something similar to a Clean solution structure: (from root containing /src and /tests)
```
dotnet ef migrations add InitialCreate --project src/Infra/ --startup-project src/Web.Api/

dotnet ef database update --project src/Infra/ --startup-project src/Web.Api/


### AddValidatorsFromAssemblyContaining<TValidator>()
You've observed the dependencies correctly! Let's break down why you need both `using` statements and elaborate on `AddValidatorsFromAssemblyContaining<TValidator>()`.

---

### Why Both `using FluentValidation.AspNetCore;` and `using FluentValidation;`?

This comes down to how .NET extension methods and namespaces are organized within the FluentValidation ecosystem.

1.  **`using FluentValidation.AspNetCore;`**
    *   **Provides:** The `AddFluentValidationAutoValidation()` extension method (and `AddFluentValidationClientsideAdapters()`, etc.).
    *   **Purpose:** This namespace contains the code specifically designed to integrate FluentValidation with ASP.NET Core's MVC and API pipeline. `AddFluentValidationAutoValidation()` lives here because it hooks into the `IServiceCollection` to modify how MVC handles model validation.

2.  **`using FluentValidation;`**
    *   **Provides:** The `AddValidatorsFromAssemblyContaining<TValidator>()` extension method.
    *   **Purpose:** While it might seem counter-intuitive that a method for ASP.NET Core integration (`AddValidatorsFromAssemblyContaining`) isn't in `FluentValidation.AspNetCore`, this specific extension method actually lives in the **`FluentValidation.DependencyInjectionExtensions`** package.
        *   **Crucial Point:** `FluentValidation.AspNetCore` has a **dependency** on `FluentValidation.DependencyInjectionExtensions`. When you install `FluentValidation.AspNetCore`, it automatically brings `FluentValidation.DependencyInjectionExtensions` along.
        *   The extension methods in `FluentValidation.DependencyInjectionExtensions` (including `AddValidatorsFromAssemblyContaining`) are exposed via the `FluentValidation` namespace (the core library's namespace).

**In short:**

*   You need `using FluentValidation.AspNetCore;` for the ASP.NET Core-specific auto-validation setup (`AddFluentValidationAutoValidation`).
*   You need `using FluentValidation;` (which is the namespace where `AddValidatorsFromAssemblyContaining` is exposed, thanks to the transitive dependency on `FluentValidation.DependencyInjectionExtensions`) for the assembly scanning feature.

It's a common pattern in .NET libraries where core extension methods for DI might reside in a "base" namespace even if they're conceptually part of a framework-specific integration package.

---

### Elaborating on `AddValidatorsFromAssemblyContaining<TValidator>()`

Let's assume your validator (`QuoteCreateRequestValidator`) is located in `src/Application/Validators/`.

The `AddValidatorsFromAssemblyContaining<TValidator>()` method is designed for **assembly scanning**.

*   **What `TValidator` (e.g., `QuoteCreateRequestValidator`) signifies:** You provide it with *any type* (`TValidator`) that exists within the **assembly** where your FluentValidation validators are defined. It doesn't matter what type `TValidator` is, as long as it's from the correct assembly. Using one of your validator types is merely a convenient way to point to that specific assembly.

*   **What it scans:** It will scan the **entire assembly** (in your case, the assembly compiled from your `src/Application/` project) for any public, non-abstract classes that inherit from `AbstractValidator<T>`.

*   **What it registers:** For every `AbstractValidator<T>` it finds in that assembly, it automatically registers it in your DI container as `IValidator<T>`.

It picks up any `AbstractValidator<T>` defined in your `src/Application/` class library project (or whatever project contains `QuoteCreateRequestValidator`), regardless of what sub-folder they are in.

This assembly scanning is very powerful as it means you don't have to manually register each validator. You just add new validator files to your `Application` project, and they'll automatically be picked up by the DI container when the application starts.