using ErrorOr;
using ErrorOrPoc.Api.Domain.Errors;
using ErrorOrPoc.Api.Services;
using Xunit;

namespace ErrorOrPoc.Tests.Services;

public class UserServiceTests
{
    private readonly UserService _sut = new();

    [Fact]
    public void CreateUser_WithValidInput_ReturnsSuccessUser()
    {
        // Act
        var result = _sut.CreateUser("Tony Stark", "tony@starkindustries.com", "Pro");

        // Assert
        Assert.False(result.IsError);
        Assert.Equal("Tony Stark", result.Value.Name);
        Assert.Equal("tony@starkindustries.com", result.Value.Email);
        Assert.Equal("Pro", result.Value.Plan);
    }

    [Fact]
    public void CreateUser_WithInvalidData_AccumulatesValidationErrors()
    {
        // Act (short name, invalid email, invalid plan)
        var result = _sut.CreateUser("Al", "invalid-email", "InvalidPlan");

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(3, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.Code == UserErrors.NameTooShort.Code);
        Assert.Contains(result.Errors, e => e.Code == UserErrors.InvalidEmail.Code);
        Assert.Contains(result.Errors, e => e.Code == UserErrors.InvalidPlan.Code);
    }

    [Fact]
    public void CreateUser_WithDuplicateEmail_ReturnsConflictError()
    {
        // Arrange
        _sut.CreateUser("Tony Stark", "duplicate@stark.com", "Free");

        // Act
        var result = _sut.CreateUser("Another Tony", "DUPLICATE@stark.com", "Pro");

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ErrorType.Conflict, result.FirstError.Type);
        Assert.Equal(UserErrors.DuplicateEmail.Code, result.FirstError.Code);
    }

    [Fact]
    public void GetUser_WhenNotFound_ReturnsNotFoundError()
    {
        // Act
        var result = _sut.GetUser(Guid.NewGuid());

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ErrorType.NotFound, result.FirstError.Type);
        Assert.Equal(UserErrors.NotFound.Code, result.FirstError.Code);
    }

    [Fact]
    public void UpgradePlan_UsingThenChaining_ReturnsUpdatedUser()
    {
        // Arrange
        var created = _sut.CreateUser("Bruce Wayne", "bruce@waynecorp.com", "Free").Value;

        // Act
        var result = _sut.UpgradePlan(created.Id, "Enterprise");

        // Assert
        Assert.False(result.IsError);
        Assert.Equal("Enterprise", result.Value.Plan);
    }
}
