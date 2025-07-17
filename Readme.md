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