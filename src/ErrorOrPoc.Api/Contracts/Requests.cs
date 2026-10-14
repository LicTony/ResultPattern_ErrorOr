namespace ErrorOrPoc.Api.Contracts;

public sealed record CreateUserRequest(string Name, string Email, string Plan);

public sealed record UpgradePlanRequest(string NewPlan);
