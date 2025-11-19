# Project Structure

```
Ridged-BE/
│
├── Ridged.Domain/                          # Domain Layer - Entities and Business Rules
│   ├── Common/
│   │   ├── ErrorCode.cs                   # Centralized error code enumeration
│   │   └── Result.cs                      # Result pattern for error handling
│   ├── Entities/
│   │   └── User.cs                        # User entity with Identity integration
│   └── Enums/
│       └── UserRole.cs                    # User role enumeration
│
├── Ridged.Application/                     # Application Layer - Business Logic
│   ├── Common/
│   │   ├── Behaviors/
│   │   │   └── ValidationBehavior.cs      # MediatR validation pipeline
│   │   ├── Exceptions/
│   │   │   └── AppException.cs            # Custom application exception
│   │   ├── Interfaces/
│   │   │   ├── IJwtService.cs             # JWT service interface
│   │   │   └── IUserRepository.cs         # User repository interface
│   │   ├── Models/
│   │   │   └── ApiResponse.cs             # Standard API response wrapper
│   │   └── Settings/
│   │       └── JwtSettings.cs             # JWT configuration settings
│   │
│   ├── Features/                           # Vertical Slices - Feature folders
│   │   └── Auth/
│   │       ├── Register/
│   │       │   ├── RegisterCommand.cs             # Command
│   │       │   ├── RegisterCommandHandler.cs      # Command handler
│   │       │   ├── RegisterCommandValidator.cs    # FluentValidation rules
│   │       │   ├── RegisterRequest.cs             # Request DTO
│   │       │   └── RegisterResponse.cs            # Response DTO
│   │       │
│   │       ├── VerifyEmail/
│   │       │   ├── VerifyEmailCommand.cs
│   │       │   └── VerifyEmailCommandHandler.cs
│   │       │
│   │       ├── Login/
│   │       │   ├── LoginCommand.cs
│   │       │   ├── LoginCommandHandler.cs
│   │       │   ├── LoginCommandValidator.cs
│   │       │   ├── LoginRequest.cs
│   │       │   └── LoginResponse.cs
│   │       │
│   │       ├── RefreshToken/
│   │       │   ├── RefreshTokenCommand.cs
│   │       │   ├── RefreshTokenCommandHandler.cs
│   │       │   ├── RefreshTokenRequest.cs
│   │       │   └── RefreshTokenResponse.cs
│   │       │
│   │       ├── ForgotPassword/
│   │       │   ├── ForgotPasswordCommand.cs
│   │       │   ├── ForgotPasswordCommandHandler.cs
│   │       │   ├── ForgotPasswordCommandValidator.cs
│   │       │   └── ForgotPasswordRequest.cs
│   │       │
│   │       └── ResetPassword/
│   │           ├── ResetPasswordCommand.cs
│   │           ├── ResetPasswordCommandHandler.cs
│   │           ├── ResetPasswordCommandValidator.cs
│   │           └── ResetPasswordRequest.cs
│   │
│   ├── Setup.cs                           # Application layer DI registration
│   └── AssemblyReference.cs               # Assembly marker for MediatR
│
├── Ridged.Infrastructure/                  # Infrastructure Layer - Data & External Services
│   ├── Configuration/                      # Entity Framework configurations
│   │   ├── RoleClaimConfiguration.cs
│   │   ├── RoleConfiguration.cs
│   │   ├── UserClaimConfiguration.cs
│   │   ├── UserLoginConfiguration.cs
│   │   ├── UserRoleConfiguration.cs
│   │   ├── usersConfigurations.cs
│   │   └── UserTokenConfiguration.cs
│   │
│   ├── Context/
│   │   └── ApplicationDbContext.cs        # EF Core DbContext with Identity
│   │
│   ├── Extensions/
│   │   └── ServiceCollectionExtensions.cs # Infrastructure service registration
│   │
│   ├── Migrations/                         # EF Core migrations
│   │   ├── 20251119141650_InitialCreate.cs
│   │   ├── 20251119175747_AddAuthenticationFields.cs
│   │   └── ApplicationDbContextModelSnapshot.cs
│   │
│   ├── Repository/
│   │   ├── IRepository.cs                 # Generic repository interface
│   │   ├── Repository.cs                  # Generic repository implementation
│   │   └── UserRepository.cs              # User-specific repository
│   │
│   ├── Services/
│   │   └── JwtService.cs                  # JWT token generation & validation
│   │
│   └── Setup.cs                           # Infrastructure layer DI registration
│
└── RidgedApi/                              # API Layer - HTTP Endpoints
    ├── Endpoints/
    │   └── AuthEndpoints.cs               # Minimal API auth endpoints
    │
    ├── Middleware/
    │   └── GlobalExceptionHandlerMiddleware.cs  # Global error handling
    │
    ├── Properties/
    │   └── launchSettings.json            # Launch configuration
    │
    ├── appsettings.json                   # Configuration settings
    ├── appsettings.Development.json       # Development settings
    ├── Program.cs                         # Application entry point
    ├── AuthEndpoints.http                 # HTTP test file
    └── RidgedApi.http                     # HTTP test file
```

## Architecture Layers

### 1. Domain Layer (`Ridged.Domain`)
**Purpose**: Core business entities and domain logic
- No dependencies on other layers
- Contains entities, value objects, enums
- Defines domain-specific result types and error codes

**Key Components**:
- `User`: Domain entity with Identity integration
- `UserRole`: Enumeration for user roles
- `ErrorCode`: Centralized error codes
- `Result<T>`: Result pattern for operation outcomes

### 2. Application Layer (`Ridged.Application`)
**Purpose**: Application business logic and orchestration
- Depends only on Domain layer
- Contains CQRS commands/queries
- Feature-based organization (vertical slicing)
- Validation rules with FluentValidation
- MediatR pipeline behaviors

**Key Components**:
- **Features**: Organized by use case (Register, Login, etc.)
- **Interfaces**: Abstractions for infrastructure concerns
- **Behaviors**: Cross-cutting concerns (validation, logging)
- **DTOs**: Request/Response models

**Pattern**: Each feature contains:
- Command/Query
- Handler
- Validator
- Request DTO
- Response DTO

### 3. Infrastructure Layer (`Ridged.Infrastructure`)
**Purpose**: Data access and external service implementations
- Depends on Application and Domain layers
- EF Core implementation
- Repository implementations
- JWT service implementation
- Identity configuration

**Key Components**:
- `ApplicationDbContext`: EF Core context with Identity
- `UserRepository`: Data access for users
- `JwtService`: Token generation and validation
- Entity configurations
- Database migrations

### 4. API Layer (`RidgedApi`)
**Purpose**: HTTP API exposure
- Depends on Application and Infrastructure layers
- Minimal API endpoints
- Middleware (error handling, authentication)
- Dependency injection configuration

**Key Components**:
- `Program.cs`: Application configuration and DI setup
- `AuthEndpoints`: Minimal API endpoint definitions
- `GlobalExceptionHandlerMiddleware`: Centralized error handling
- Configuration files

## Design Patterns Used

### 1. CQRS (Command Query Responsibility Segregation)
- **Commands**: Operations that modify state (Register, Login, etc.)
- **Queries**: Operations that read data (future: GetUser, GetProfile)
- **Handlers**: Process commands/queries independently

### 2. Vertical Slicing
- Features organized by business capability
- Each feature is self-contained
- Easy to add new features without affecting existing ones

### 3. Repository Pattern
- Abstraction over data access
- `IUserRepository` interface in Application layer
- Implementation in Infrastructure layer
- Enables testing and swapping data sources

### 4. Result Pattern
- Consistent error handling
- `Result<T>` wraps operation outcomes
- Avoids exceptions for expected failures
- Provides structured error information

### 5. Mediator Pattern
- MediatR library for request/response
- Decouples endpoints from handlers
- Enables pipeline behaviors (validation, logging)

### 6. Dependency Injection
- All dependencies injected via constructor
- Configured in `Setup.cs` files
- Promotes testability and loose coupling

## Data Flow

```
HTTP Request
    ↓
Minimal API Endpoint (AuthEndpoints.cs)
    ↓
MediatR Send Command/Query
    ↓
ValidationBehavior (FluentValidation)
    ↓
Command/Query Handler
    ↓
Repository/Service (Infrastructure)
    ↓
Database/External Service
    ↓
Result<T>
    ↓
ApiResponse<T>
    ↓
HTTP Response (JSON)
```

## Adding New Features

### Step 1: Create Feature Folder
```
Ridged.Application/Features/[FeatureName]/
```

### Step 2: Add Command/Query
```csharp
public record MyCommand(...) : IRequest<Result<MyResponse>>;
```

### Step 3: Add Handler
```csharp
public class MyCommandHandler : IRequestHandler<MyCommand, Result<MyResponse>>
{
    public async Task<Result<MyResponse>> Handle(MyCommand request, CancellationToken cancellationToken)
    {
        // Implementation
    }
}
```

### Step 4: Add Validator (optional)
```csharp
public class MyCommandValidator : AbstractValidator<MyCommand>
{
    public MyCommandValidator()
    {
        RuleFor(x => x.Property).NotEmpty();
    }
}
```

### Step 5: Create Endpoint
```csharp
public static void MapMyEndpoints(this IEndpointRouteBuilder app)
{
    app.MapPost("/api/my-endpoint", async (MyRequest request, IMediator mediator) =>
    {
        var command = new MyCommand(...);
        var result = await mediator.Send(command);
        return result.IsSuccess 
            ? Results.Ok(ApiResponse.SuccessResponse(result.Data))
            : Results.BadRequest(ApiResponse.FailureResponse(result.Message, result.ErrorCode));
    });
}
```

### Step 6: Register in Program.cs
```csharp
app.MapMyEndpoints();
```

## Best Practices

✅ **Single Responsibility**: Each class has one reason to change
✅ **Dependency Inversion**: Depend on abstractions, not implementations
✅ **Separation of Concerns**: Clear layer boundaries
✅ **Testability**: Easy to unit test with mocked dependencies
✅ **Maintainability**: Feature-based organization for easy navigation
✅ **Security**: Password hashing, JWT tokens, validation
✅ **Error Handling**: Structured error responses with error codes
✅ **Validation**: FluentValidation for declarative rules
