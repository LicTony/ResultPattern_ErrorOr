namespace ErrorOrPoc.Api.Domain.Models;

public sealed record User(Guid Id, string Name, string Email, string Plan);
