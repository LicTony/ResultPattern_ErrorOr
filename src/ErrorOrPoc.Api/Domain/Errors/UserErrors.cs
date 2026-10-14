using ErrorOr;

namespace ErrorOrPoc.Api.Domain.Errors;

public static class UserErrors
{
    public static readonly Error NameTooShort = Error.Validation(
        code: "User.NameTooShort",
        description: "The user name must have at least 3 characters.");

    public static readonly Error InvalidEmail = Error.Validation(
        code: "User.InvalidEmail",
        description: "The provided email format is invalid.");

    public static readonly Error InvalidPlan = Error.Validation(
        code: "User.InvalidPlan",
        description: "Plan must be either 'Free' or 'Pro'.");

    public static readonly Error DuplicateEmail = Error.Conflict(
        code: "User.DuplicateEmail",
        description: "A user with the specified email already exists.");

    public static readonly Error NotFound = Error.NotFound(
        code: "User.NotFound",
        description: "The requested user was not found.");
}
