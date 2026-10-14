using ErrorOr;
using ErrorOrPoc.Api.Common.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Xunit;

namespace ErrorOrPoc.Tests.Http;

public class ErrorOrMappingExtensionsTests
{
    [Fact]
    public void ToProblemResult_WhenSingleNotFoundError_ReturnsProblemHttpResultWith404()
    {
        // Arrange
        List<Error> errors = [Error.NotFound("Entity.NotFound", "Item not found")];

        // Act
        var result = errors.ToProblemResult();

        // Assert
        var problem = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problem.StatusCode);
        Assert.Equal("Entity.NotFound", problem.ProblemDetails.Title);
        Assert.Equal("Item not found", problem.ProblemDetails.Detail);
    }

    [Fact]
    public void ToProblemResult_WhenSingleConflictError_ReturnsProblemHttpResultWith409()
    {
        // Arrange
        List<Error> errors = [Error.Conflict("User.Conflict", "Email in use")];

        // Act
        var result = errors.ToProblemResult();

        // Assert
        var problem = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status409Conflict, problem.StatusCode);
        Assert.Equal("User.Conflict", problem.ProblemDetails.Title);
    }

    [Fact]
    public void ToProblemResult_WhenValidationErrors_ReturnsValidationProblem()
    {
        // Arrange
        List<Error> errors =
        [
            Error.Validation("User.Name", "Name is too short"),
            Error.Validation("User.Email", "Invalid email format")
        ];

        // Act
        var result = errors.ToProblemResult();

        // Assert
        var problem = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
        var validationDetails = Assert.IsAssignableFrom<HttpValidationProblemDetails>(problem.ProblemDetails);
        Assert.True(validationDetails.Errors.ContainsKey("User.Name"));
        Assert.True(validationDetails.Errors.ContainsKey("User.Email"));
    }
}
