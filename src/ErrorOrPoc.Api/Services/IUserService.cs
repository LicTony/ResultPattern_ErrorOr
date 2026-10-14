using ErrorOr;
using ErrorOrPoc.Api.Domain.Models;

namespace ErrorOrPoc.Api.Services;

public interface IUserService
{
    ErrorOr<User> CreateUser(string name, string email, string plan);
    ErrorOr<User> GetUser(Guid id);
    ErrorOr<User> UpgradePlan(Guid id, string newPlan);
    IReadOnlyCollection<User> GetAll();
}
