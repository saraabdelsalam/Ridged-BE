# Ridged-BE Authentication System

## Overview
This project implements a comprehensive authentication system using **Clean Architecture**, **CQRS pattern**, **Vertical Slicing**, and **JWT authentication** with refresh tokens.

## Architecture

### Project Structure
- **Ridged.Domain**: Domain entities, enums, and common types (Result with HTTP status codes)
- **Ridged.Application**: Application layer with CQRS handlers, validators, DTOs, and interfaces
- **Ridged.Infrastructure**: Data access, repositories, JWT service, and Identity configuration
- **RidgedApi**: API layer with minimal API endpoints and middleware

### Feature Organization (Vertical Slicing)
Each feature is organized in a dedicated folder with all related files:
```
Features/Auth/Register/
├── DTOs/
│   ├── RegisterRequest.cs      # Request DTO
│   └── RegisterResponse.cs     # Response DTO
├── RegisterCommand.cs           # MediatR command
├── RegisterCommandHandler.cs   # Business logic
└── RegisterCommandValidator.cs # FluentValidation rules
```

**Benefits:**
- ✅ **Cohesive**: All related code in one place
- ✅ **Scalable**: Easy to add new features without affecting existing ones
- ✅ **Maintainable**: Clear separation between features
- ✅ **Testable**: Each feature can be tested independently

### Key Patterns
- **CQRS**: Commands and Queries separated with MediatR
- **Vertical Slicing**: Features organized by use case (Register, Login, ForgotPassword, etc.)
- **Repository Pattern**: UserRepository for data access abstraction
- **Result Pattern**: Consistent error handling across all operations

## Features Implemented

### 1. User Registration
- **Endpoint**: `POST /api/auth/register`
- Email-based registration with password validation
- Generates email verification token
- Password hashing using ASP.NET Core Identity
- Returns verification token (in development; use email service in production)

**Request:**
```json
{
  "email": "user@example.com",
  "password": "SecurePass123!",
  "confirmPassword": "SecurePass123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "message": "Registration successful. Please check your email to verify your account.",
  "statusCode": 200,
  "data": {
    "userId": 1,
    "email": "user@example.com",
    "message": "Account created successfully. Please verify your email using the verification token. Token: xxx"
  }
}
```

### 2. Email Verification
- **Endpoint**: `POST /api/auth/verify-email`
- Verifies email address using token
- Token expires after 24 hours

**Request:**
```json
{
  "token": "verification_token_here"
}
```

### 3. User Login
- **Endpoint**: `POST /api/auth/login`
- Email and password authentication
- Requires verified email
- Returns JWT access token and refresh token
- Access token expires in 15 minutes
- Refresh token expires in 7 days

**Request:**
```json
{
  "email": "user@example.com",
  "password": "SecurePass123!"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Login successful",
  "statusCode": 200,
  "data": {
    "accessToken": "eyJhbGc...",
    "refreshToken": "refresh_token_here",
    "userId": 1,
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "role": "Customer"
  }
}
```

### 4. Refresh Token
- **Endpoint**: `POST /api/auth/refresh-token`
- Generates new access and refresh tokens
- Validates expired access token and active refresh token

**Request:**
```json
{
  "accessToken": "expired_access_token",
  "refreshToken": "current_refresh_token"
}
```

### 5. Forgot Password
- **Endpoint**: `POST /api/auth/forgot-password`
- Generates password reset token
- Token expires after 1 hour
- Does not reveal if email exists (security best practice)

**Request:**
```json
{
  "email": "user@example.com"
}
```

### 6. Reset Password
- **Endpoint**: `POST /api/auth/reset-password`
- Resets password using valid token
- Invalidates all refresh tokens for security

**Request:**
```json
{
  "token": "reset_token_here",
  "newPassword": "NewSecurePass123!",
  "confirmPassword": "NewSecurePass123!"
}
```

## Error Handling

### Centralized Error Management
- **HTTP Status Codes**: Standard REST API status codes (200, 400, 401, 403, 404, 409, 500)
- **Result Pattern**: Consistent return type with success/failure states
- **Global Exception Handler**: Middleware to catch and format all exceptions
- **Validation Pipeline**: FluentValidation integrated with MediatR

### HTTP Status Codes Used

| Status Code | Name | Usage |
|-------------|------|-------|
| 200 | OK | Successful operations |
| 400 | Bad Request | Validation errors, invalid tokens |
| 401 | Unauthorized | Invalid credentials, expired tokens |
| 403 | Forbidden | Unverified account, account locked |
| 404 | Not Found | User not found |
| 409 | Conflict | Email already exists |
| 500 | Internal Server Error | Database errors, unexpected failures |

### Error Response Format
```json
{
  "success": false,
  "message": "User-friendly error message",
  "statusCode": 401,
  "errors": {
    "Email": ["Email is required"],
    "Password": ["Password must be at least 8 characters"]
  }
}
```

### Success Response Format
```json
{
  "success": true,
  "message": "Operation completed successfully",
  "statusCode": 200,
  "data": {
    "userId": 1,
    "email": "user@example.com"
  }
}
```

### Status Code Examples

**Registration with Existing Email (409 Conflict):**
```json
{
  "success": false,
  "message": "An account with this email already exists",
  "statusCode": 409
}
```

**Login with Invalid Credentials (401 Unauthorized):**
```json
{
  "success": false,
  "message": "Invalid email or password",
  "statusCode": 401
}
```

**Login with Unverified Account (403 Forbidden):**
```json
{
  "success": false,
  "message": "Please verify your email before logging in",
  "statusCode": 403
}
```

**Validation Errors (400 Bad Request):**
```json
{
  "success": false,
  "message": "Validation failed",
  "statusCode": 400,
  "errors": {
    "Email": ["Email is required"],
    "Password": ["Password must be at least 8 characters"]
  }
}
```

## Security Features

### Password Requirements
- Minimum 8 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one number
- At least one special character

### Token Security
- JWT with HMAC-SHA256 signing
- Refresh tokens are cryptographically random
- Tokens stored hashed in database
- Refresh tokens invalidated on password reset
- Token expiry validation with zero clock skew

### Additional Security
- Password hashing using Identity's PasswordHasher
- Email verification required before login
- Account lockout after failed attempts (configured in Identity)
- Secure token generation using RNG

## Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "AppConnection": "Server=localhost;Database=RidgedDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "RidgedApi",
    "Audience": "RidgedClient",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  }
}
```

### Important Notes
⚠️ **Before deploying to production:**
1. Change the JWT SecretKey to a strong, unique value
2. Store secrets in Azure Key Vault or environment variables
3. Implement email service for verification and password reset
4. Configure CORS appropriately
5. Enable HTTPS only
6. Set `SignIn.RequireConfirmedEmail = true` in Identity configuration

## Database Migration

Run the migration to create/update the database:
```bash
dotnet ef database update --project Ridged.Infrastructure --startup-project RidgedApi
```

## Running the Application

1. Update connection string in `appsettings.json`
2. Run migrations:
   ```bash
   dotnet ef database update --project Ridged.Infrastructure --startup-project RidgedApi
   ```
3. Build and run:
   ```bash
   dotnet run --project RidgedApi
   ```
4. Access Swagger UI: `https://localhost:5001/swagger`

## Testing the API

### 1. Register a User
```bash
POST https://localhost:5001/api/auth/register
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "Test123!@#",
  "confirmPassword": "Test123!@#",
  "firstName": "Test",
  "lastName": "User"
}
```

### 2. Verify Email (use token from registration response)
```bash
POST https://localhost:5001/api/auth/verify-email
Content-Type: application/json

{
  "token": "verification_token_from_registration"
}
```

### 3. Login
```bash
POST https://localhost:5001/api/auth/login
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "Test123!@#"
}
```

## Extending the System

### Adding New Features
1. Create feature folder in `Ridged.Application/Features/[FeatureName]`
2. Add Command/Query, Handler, Validator, Request/Response DTOs
3. Create endpoint in `RidgedApi/Endpoints/[Feature]Endpoints.cs`
4. Map endpoint in `Program.cs`

### Adding Authorization
Use the `[Authorize]` attribute or `.RequireAuthorization()` on endpoints:
```csharp
group.MapGet("/profile", GetProfile)
    .RequireAuthorization();
```

Access user claims in handlers:
```csharp
var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
```

## Dependencies

### Application Layer
- MediatR
- FluentValidation
- FluentValidation.DependencyInjectionExtensions

### Infrastructure Layer
- Microsoft.AspNetCore.Identity.EntityFrameworkCore
- Microsoft.EntityFrameworkCore.SqlServer
- System.IdentityModel.Tokens.Jwt

### API Layer
- Microsoft.AspNetCore.Authentication.JwtBearer
- Swashbuckle.AspNetCore

## Best Practices Applied

✅ **Vertical Slicing** - Features organized by business capability with dedicated DTO folders
✅ **CQRS** - Clear separation of commands and queries
✅ **Repository Pattern** - Data access abstraction
✅ **Dependency Injection** - Loose coupling
✅ **FluentValidation** - Declarative validation rules
✅ **Result Pattern** - Consistent error handling with HTTP status codes
✅ **Global Exception Handling** - Centralized error responses
✅ **Standard HTTP Status Codes** - REST API best practices (200, 400, 401, 403, 404, 409, 500)
✅ **JWT + Refresh Tokens** - Secure authentication
✅ **Password Hashing** - ASP.NET Core Identity
✅ **Email Verification** - Account security
✅ **Clean Architecture** - Clear separation of concerns
✅ **DTO Organization** - Dedicated folders for Request/Response models

## Architecture Highlights

### Result Pattern with HTTP Status Codes
The application uses a Result pattern that includes standard HTTP status codes:

```csharp
public class Result
{
    public bool IsSuccess { get; }
    public string Message { get; }
    public int StatusCode { get; }  // Standard HTTP status code
    
    public static Result Success(string message = "Operation completed successfully")
    public static Result Failure(string message, int statusCode = 400)
}
```

### API Response Structure
All API endpoints return a consistent response format:

```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public int StatusCode { get; set; }
    public T? Data { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
}
```

### Handler Status Codes
Each handler returns appropriate HTTP status codes:

| Operation | Scenario | Status Code |
|-----------|----------|-------------|
| Register | Email exists | 409 Conflict |
| Register | Creation failed | 500 Internal Server Error |
| Login | Invalid credentials | 401 Unauthorized |
| Login | Unverified account | 403 Forbidden |
| Login | Account locked | 403 Forbidden |
| VerifyEmail | Invalid token | 400 Bad Request |
| RefreshToken | Invalid/expired token | 401 Unauthorized |
| RefreshToken | User not found | 404 Not Found |
| ResetPassword | Invalid token | 400 Bad Request |

### Client-Side Error Handling

**Using HTTP Status Codes:**
```javascript
fetch('/api/auth/login', {
  method: 'POST',
  body: JSON.stringify({ email, password })
})
.then(async response => {
  const data = await response.json();
  
  if (response.status === 401) {
    showError("Invalid credentials");
  } else if (response.status === 403) {
    showError("Please verify your email");
  } else if (response.ok) {
    handleSuccessfulLogin(data.data);
  }
});
```

**Using Response Body:**
```javascript
const response = await login(email, password);

if (!response.success) {
  switch (response.statusCode) {
    case 401:
      showError("Invalid credentials");
      break;
    case 403:
      showError("Please verify your email");
      break;
    case 409:
      showError("Email already exists");
      break;
    default:
      showError(response.message);
  }
}

## Future Enhancements

- [ ] Email service integration (SendGrid, AWS SES, etc.)
- [ ] Two-factor authentication (2FA)
- [ ] Social login (Google, Facebook, etc.)
- [ ] Rate limiting on authentication endpoints
- [ ] Account management features (change password, update profile)
- [ ] Audit logging for security events
- [ ] Redis for token blacklisting