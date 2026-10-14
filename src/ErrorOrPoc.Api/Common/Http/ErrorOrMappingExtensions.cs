using ErrorOr;
using Microsoft.AspNetCore.Http;

namespace ErrorOrPoc.Api.Common.Http;

public static class ErrorOrMappingExtensions
{
    public static IResult ToProblemResult(this List<Error> errors)
    {
        if (errors.Count == 0)
        {
            return Results.Problem();
        }

        if (errors.All(e => e.Type == ErrorType.Validation))
        {
            var validationErrors = errors
                .GroupBy(e => e.Code)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.Description).ToArray());

            return Results.ValidationProblem(validationErrors);
        }

        var firstError = errors[0];
        var statusCode = firstError.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };

        return Results.Problem(
            statusCode: statusCode,
            title: firstError.Code,
            detail: firstError.Description);
    }

    public static IResult MatchResult<TValue>(
        this ErrorOr<TValue> result,
        Func<TValue, IResult> onSuccess)
    {
        return result.Match(
            onSuccess,
            errors => errors.ToProblemResult());
    }
}
