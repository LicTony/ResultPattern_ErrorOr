using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using ErrorOr;
using ErrorOrPoc.Api.Domain.Errors;
using ErrorOrPoc.Api.Domain.Models;

namespace ErrorOrPoc.Api.Services;

public partial class UserService : IUserService
{
    private readonly ConcurrentDictionary<Guid, User> _users = new();

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();

    public ErrorOr<User> CreateUser(string name, string email, string plan)
    {
        var validationErrors = ValidateUserInput(name, email, plan);
        if (validationErrors.Count > 0)
        {
            return validationErrors;
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();
        if (_users.Values.Any(u => u.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase)))
        {
            return UserErrors.DuplicateEmail;
        }

        var user = new User(
            Id: Guid.NewGuid(),
            Name: name.Trim(),
            Email: normalizedEmail,
            Plan: plan.Trim());

        _users[user.Id] = user;
        return user;
    }

    public ErrorOr<User> GetUser(Guid id)
    {
        if (_users.TryGetValue(id, out var user))
        {
            return user;
        }

        return UserErrors.NotFound;
    }

    public ErrorOr<User> UpgradePlan(Guid id, string newPlan)
    {
        return GetUser(id)
            .Then(user =>
            {
                if (newPlan != "Pro" && newPlan != "Enterprise")
                {
                    return (ErrorOr<User>)UserErrors.InvalidPlan;
                }

                var updated = user with { Plan = newPlan };
                _users[id] = updated;
                return (ErrorOr<User>)updated;
            });
    }

    public IReadOnlyCollection<User> GetAll() => _users.Values.ToList().AsReadOnly();

    private static List<Error> ValidateUserInput(string name, string email, string plan)
    {
        List<Error> errors = [];

        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length < 3)
        {
            errors.Add(UserErrors.NameTooShort);
        }

        if (string.IsNullOrWhiteSpace(email) || !EmailRegex().IsMatch(email.Trim()))
        {
            errors.Add(UserErrors.InvalidEmail);
        }

        if (string.IsNullOrWhiteSpace(plan) || (plan != "Free" && plan != "Pro"))
        {
            errors.Add(UserErrors.InvalidPlan);
        }

        return errors;
    }
}
