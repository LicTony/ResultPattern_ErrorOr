using ErrorOrPoc.Api.Common.Http;
using ErrorOrPoc.Api.Contracts;
using ErrorOrPoc.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddSingleton<IUserService, UserService>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

var userGroup = app.MapGroup("/api/users");

userGroup.MapPost("", (CreateUserRequest request, IUserService service) =>
{
    var result = service.CreateUser(request.Name, request.Email, request.Plan);
    return result.MatchResult(user => Results.Created($"/api/users/{user.Id}", user));
});

userGroup.MapGet("", (IUserService service) =>
{
    return Results.Ok(service.GetAll());
});

userGroup.MapGet("{id:guid}", (Guid id, IUserService service) =>
{
    var result = service.GetUser(id);
    return result.MatchResult(user => Results.Ok(user));
});

userGroup.MapPut("{id:guid}/plan", (Guid id, UpgradePlanRequest request, IUserService service) =>
{
    var result = service.UpgradePlan(id, request.NewPlan);
    return result.MatchResult(user => Results.Ok(user));
});

app.Run();

// Expose Program class for WebApplicationFactory in integration tests
public partial class Program { }
