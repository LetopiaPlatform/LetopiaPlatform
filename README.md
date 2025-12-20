# LetopiaPlatform-Backend

A comprehensive backend platform for Letopia, built with .NET 8.0 and following clean architecture principles.

## Project Structure

```
LetopiaPlatform.sln
│
├── LetopiaPlatform.API/
│   ├── Controllers/
│   ├── Middleware/
│   ├── Filters/
│   ├── DTOs/
│   │   ├── Auth/
│   │   ├── User/
│   │   ├── Community/
│   │   ├── Post/
│   │   └── Comment/
│   ├── Validators/
│   ├── Program.cs
│   ├── appsettings.json
│   └── appsettings.Development.json
│
├── LetopiaPlatform.Core/
│   ├── Common/
│   │   ├── BaseEntity.cs
│   │   └── Result.cs
│   ├── Entities/
│   ├── Interfaces/
│   ├── Services/
│   ├── Exceptions/
│   │   └── CustomExceptions.cs
│   └── Enums/
│       ├── UserRole.cs
│       ├── PostType.cs
│       └── VoteType.cs
│
├── LetopiaPlatform.Infrastructure/
│   ├── Data/
│   │   ├── Configurations/
│   │   └── Migrations/
│   ├── Repositories/
│   ├── Identity/
│   ├── External/
│   └── Caching/
│
└── LetopiaPlatform.Tests/
    ├── LetopiaPlatform.UnitTests/
    └── LetopiaPlatform.IntegrationTests/
```

### Layer Descriptions

- **LetopiaPlatform.API**: Presentation layer containing controllers, middleware, filters, DTOs, and validators
- **LetopiaPlatform.Core**: Domain layer with business logic, entities, services, and interfaces
- **LetopiaPlatform.Infrastructure**: Infrastructure layer handling data access, repositories, identity, external services, and caching
- **LetopiaPlatform.Tests**: Test projects including unit and integration tests

