# Feature: ErrorOr Minimal APIs PoC (.NET 10)

## Objective
Build a clean Proof of Concept (PoC) in .NET 10 demonstrating the Result Pattern using the `ErrorOr` library in an ASP.NET Core Minimal API.

## Problem & Why
Traditional exception-based control flow for domain validation and business errors is slow, obscures intent, and complicates API error handling. The Result Pattern provides explicit, type-safe error handling without exceptions. `ErrorOr` offers a lightweight, fluent discriminated union (`ErrorOr<T>`) that cleanly translates to standard RFC 7807/9457 `ProblemDetails` in HTTP responses.

## Scope & Constraints
- **Framework**: .NET 10 (`net10.0`).
- **Architecture**: Minimal APIs with clean separation: Domain/Result -> Application/Service -> Presentation (Endpoints + ProblemDetails extension).
- **Packages**: `ErrorOr`.
- **Delivery Strategy**: `ask-on-risk` (Forecast: ~250-350 lines).

## Tasks
- [x] `TASK-001`: Initialize .NET 10 solution, Minimal API project (`src/ErrorOrPoc.Api`), and install `ErrorOr` package.
  - Route: inline
  - Checks: `dotnet build` passes on `net10.0` (Observed: 0 warnings, 0 errors).
- [x] `TASK-002`: Implement Domain models, typed domain errors, and a domain service returning `ErrorOr<User>`.
  - Route: inline
  - Checks: Domain returns typed errors (`Validation`, `Conflict`, `NotFound`) and successful result (Observed: compiled cleanly with 0 warnings/errors).
- [x] `TASK-003`: Create Minimal API endpoints and HTTP ProblemDetails mapping extension (`ToProblemHttpResult`).
  - Route: inline
  - Checks: Endpoints map `ErrorOr<T>` cleanly to HTTP 200, 400, 404, 409 `ProblemDetails` (Observed: compiled cleanly with 0 warnings/errors).
- [x] `TASK-004`: Add automated unit/integration tests covering success and error paths.
  - Route: inline
  - Checks: `dotnet test` runs and passes green (Observed: 8 passed, 0 failed, 0 skipped).

## Progress & Verification Evidence
- Branch: `feature/erroror-minimal-api-poc`
- Commits:
  - `039e4b8`: `chore: initialize .net 10 solution and minimal api project with erroror`
  - `eb995b4`: `feat(domain): implement user domain models, typed erroror errors, and service`
  - `6f06e02`: `feat(api): implement minimal api endpoints and erroror to problemdetails mapping`
  - Pending: `test(api): add tests for user service and erroror mapping`
