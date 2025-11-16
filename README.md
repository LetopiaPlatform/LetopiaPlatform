# BokraPlatform-Backend

A comprehensive backend platform for Bokra, built with .NET 8.0 and following clean architecture principles.

## Project Structure

```
Bokra.sln
│
├── Bokra.API/
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
├── Bokra.Core/
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
├── Bokra.Infrastructure/
│   ├── Data/
│   │   ├── Configurations/
│   │   └── Migrations/
│   ├── Repositories/
│   ├── Identity/
│   ├── External/
│   └── Caching/
│
└── Bokra.Tests/
    ├── Bokra.UnitTests/
    └── Bokra.IntegrationTests/
```

### Layer Descriptions

- **Bokra.API**: Presentation layer containing controllers, middleware, filters, DTOs, and validators
- **Bokra.Core**: Domain layer with business logic, entities, services, and interfaces
- **Bokra.Infrastructure**: Infrastructure layer handling data access, repositories, identity, external services, and caching
- **Bokra.Tests**: Test projects including unit and integration tests

