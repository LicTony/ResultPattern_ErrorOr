# Result Pattern with ErrorOr in .NET 10

A lightweight Proof of Concept (PoC) demonstrating how to implement the **Result Pattern** in ASP.NET Core **Minimal APIs** using [.NET 10](https://dotnet.microsoft.com/) and the [ErrorOr](https://github.com/amantinband/error-or) library.

---

## Why the Result Pattern?

Traditional error handling in .NET often relies on throwing and catching exceptions for domain validation or business rule violations. This introduces several drawbacks:

- **Performance overhead**: Exception stack trace unwinding is expensive.
- **Obscured control flow**: Exceptions bypass type signatures and create hidden exit points.
- **Leaky abstractions**: Higher layers must guess which exceptions lower layers might throw.

The **Result Pattern** replaces exceptions for expected domain failures by returning an explicit discriminated union type (`ErrorOr<T>`), enforcing type safety, predictable control flow, and clean HTTP mapping.

---

## Architecture Overview

```
HTTP Request
     │
     ▼
┌──────────────────────────────────────────────┐
│ Presentation (Minimal APIs)                  │
│ Map endpoints & translate ErrorOr<T>         │
│ to RFC 7807/9457 ProblemDetails via Match()  │
└──────────────────────┬───────────────────────┘
                       │
                       ▼
┌──────────────────────────────────────────────┐
│ Application / Domain Service                 │
│ Execute business rules & validations         │
│ Return ErrorOr<User> or Error list           │
└──────────────────────┬───────────────────────┘
                       │
                       ▼
┌──────────────────────────────────────────────┐
│ Domain                                       │
│ User model & static typed UserErrors         │
│ (Validation, Conflict, NotFound)             │
└──────────────────────────────────────────────┘
```

---

## Quick Start

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (SDK `10.0.100` or higher)

### Build and Run

```bash
# Clone the repository
git clone https://github.com/LicTony/ResultPattern_ErrorOr.git
cd ResultPattern_ErrorOr

# Build the solution
dotnet build

# Run unit tests
dotnet test

# Run the API
dotnet run --project src/ErrorOrPoc.Api
```

The API will start listening locally (e.g. `http://localhost:5000` or HTTPS port configured in `launchSettings.json`).

---

## Key Patterns Demonstrated

### 1. Typed Domain Errors
Domain errors are defined as static, strongly-typed `Error` instances rather than custom exception classes:

```csharp
public static class UserErrors
{
    public static readonly Error NameTooShort = Error.Validation(
        code: "User.NameTooShort",
        description: "The user name must have at least 3 characters.");

    public static readonly Error DuplicateEmail = Error.Conflict(
        code: "User.DuplicateEmail",
        description: "A user with the specified email already exists.");

    public static readonly Error NotFound = Error.NotFound(
        code: "User.NotFound",
        description: "The requested user was not found.");
}
```

### 2. Multi-Error Accumulation
Instead of failing on the first error, domain validation can collect all validation issues and return them in a single `List<Error>`:

```csharp
List<Error> errors = [];
if (name.Length < 3) errors.Add(UserErrors.NameTooShort);
if (!IsValidEmail(email)) errors.Add(UserErrors.InvalidEmail);

if (errors.Count > 0)
    return errors; // Implicit conversion to ErrorOr<User>
```

### 3. Fluent Chaining with `.Then()`
Operations can be composed cleanly without nested conditional checks:

```csharp
public ErrorOr<User> UpgradePlan(Guid id, string newPlan)
{
    return GetUser(id)
        .Then(user =>
        {
            if (newPlan != "Pro" && newPlan != "Enterprise")
                return (ErrorOr<User>)UserErrors.InvalidPlan;

            var updated = user with { Plan = newPlan };
            _users[id] = updated;
            return (ErrorOr<User>)updated;
        });
}
```

### 4. Direct Translation to `ProblemDetails`
An extension method maps `ErrorOr<T>` directly to HTTP `IResult`:

- Single `NotFound` error $\rightarrow$ `404 Not Found` with `ProblemDetails`
- Single `Conflict` error $\rightarrow$ `409 Conflict` with `ProblemDetails`
- Multiple `Validation` errors $\rightarrow$ `400 Bad Request` with `HttpValidationProblemDetails`

```csharp
userGroup.MapPost("", (CreateUserRequest request, IUserService service) =>
{
    var result = service.CreateUser(request.Name, request.Email, request.Plan);
    return result.MatchResult(user => Results.Created($"/api/users/{user.Id}", user));
});
```

---

## API Endpoints & Sample Requests

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/users` | Register a new user |
| `GET` | `/api/users` | Retrieve all users |
| `GET` | `/api/users/{id}` | Retrieve user by ID |
| `PUT` | `/api/users/{id}/plan` | Upgrade user subscription plan |

### Sample: Create User (Success)

```bash
curl -X POST http://localhost:5000/api/users \
  -H "Content-Type: application/json" \
  -d '{"name": "Tony Stark", "email": "tony@stark.com", "plan": "Pro"}'
```

**Response (`201 Created`):**
```json
{
  "id": "e4f3a3d2-7c8b-4a5e-9f1d-6b8c9d0e1f2a",
  "name": "Tony Stark",
  "email": "tony@stark.com",
  "plan": "Pro"
}
```

### Sample: Validation Failure (Accumulated Errors)

```bash
curl -X POST http://localhost:5000/api/users \
  -H "Content-Type: application/json" \
  -d '{"name": "Al", "email": "invalid-email", "plan": "Ultimate"}'
```

**Response (`400 Bad Request`):**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "User.NameTooShort": ["The user name must have at least 3 characters."],
    "User.InvalidEmail": ["The provided email format is invalid."],
    "User.InvalidPlan": ["Plan must be either 'Free' or 'Pro'."]
  }
}
```

### Sample: Resource Not Found

```bash
curl -X GET http://localhost:5000/api/users/00000000-0000-0000-0000-000000000000
```

**Response (`404 Not Found`):**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "User.NotFound",
  "status": 404,
  "detail": "The requested user was not found."
}
```

---

## Testing

The test suite in [`tests/ErrorOrPoc.Tests`](tests/ErrorOrPoc.Tests) covers:
- Validation errors accumulation.
- Conflict detection.
- Not found behavior.
- Fluent `.Then()` chaining.
- HTTP `ProblemHttpResult` and `HttpValidationProblemDetails` mapping.

Run tests via CLI:
```bash
dotnet test
```

---

## License
MIT
