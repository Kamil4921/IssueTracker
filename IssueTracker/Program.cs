using IssueTracker.Core.Commands.CloseIssuesStrategies;
using IssueTracker.Core.Commands.PostIssuesStrategies;
using IssueTracker.Core.Commands.UpdateIssuesStrategies;
using IssueTracker.Core.Domain;
using Microsoft.AspNetCore.Mvc;
using Polly;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient("GitClient").AddTransientHttpErrorPolicy(policy =>
    policy.WaitAndRetryAsync(3, retry => TimeSpan.FromSeconds(Math.Pow(2, retry))));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<IAddIssue, AddGitHubIssue>();
builder.Services.AddTransient<IAddIssue, AddGitLabIssue>();
builder.Services.AddTransient<IUpdateIssue, UpdateGitHubIssue>();
builder.Services.AddTransient<IUpdateIssue, UpdateGitLabIssue>();
builder.Services.AddTransient<ICloseIssue, CloseGitHubIssue>();
builder.Services.AddTransient<ICloseIssue, CloseGitLabIssue>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.MapPost("/issue/create", async (string provider, [FromHeader] string accessToken,
    [FromHeader] string? gitHubUserName,
    [FromHeader] string? gitHubRepository, [FromHeader] int? gitLabProjectId, [FromBody] IssueDto dto,
    IHttpClientFactory httpClientFactory,
    [FromServices] IEnumerable<IAddIssue> issueStrategies) =>
{
    var headers = new HeaderDto(accessToken, gitHubUserName, gitHubRepository, gitLabProjectId);
    var client = httpClientFactory.CreateClient("GitClient");
    if (!Enum.TryParse<Providers>(provider, out var parsedProvider))
    {
        return Results.BadRequest(new { message = "Invalid provider name" });
    }

    IAddIssue? strategy = parsedProvider switch
    {
        Providers.GitHub => issueStrategies.OfType<AddGitHubIssue>().FirstOrDefault(),
        Providers.GitLab => issueStrategies.OfType<AddGitLabIssue>().FirstOrDefault(),
        _ => null
    };

    if (strategy is null)
    {
        return Results.BadRequest("Invalid provider name");
    }

    var result = await strategy.AddIssueAsync(dto, client, headers);
    
    return result.IsSuccessStatusCode
        ? Results.Created(result.Headers.Location, await result.Content.ReadAsStringAsync())
        : Results.Problem(await result.Content.ReadAsStringAsync(), statusCode: (int)result.StatusCode);
}).WithOpenApi();

app.MapPatch("/issue/update", async (string provider, int issueNumber, [FromHeader] string accessToken,
    [FromHeader] string? gitHubUserName,
    [FromHeader] string? gitHubRepository, [FromHeader] int? gitLabProjectId, [FromBody] IssueDto dto,
    IHttpClientFactory httpClientFactory,
    [FromServices] IEnumerable<IUpdateIssue> issueStrategies) =>
{
    var headers = new HeaderDto(accessToken, gitHubUserName, gitHubRepository, gitLabProjectId);
    var client = httpClientFactory.CreateClient("GitClient");
    if (!Enum.TryParse<Providers>(provider, out var parsedProvider))
    {
        return Results.BadRequest(new { message = "Invalid provider name" });
    }

    IUpdateIssue? strategy = parsedProvider switch
    {
        Providers.GitHub => issueStrategies.OfType<UpdateGitHubIssue>().FirstOrDefault(),
        Providers.GitLab => issueStrategies.OfType<UpdateGitLabIssue>().FirstOrDefault(),
        _ => null
    };

    if (strategy is null)
    {
        return Results.BadRequest("Invalid provider name");
    }

    var result = await strategy.UpdateIssueAsync(dto, client, issueNumber, headers);

    return result.IsSuccessStatusCode
        ? Results.Ok("Issue Updated")
        : Results.Problem(await result.Content.ReadAsStringAsync(), statusCode: (int)result.StatusCode);
}).WithOpenApi();

app.MapPatch("/closeIssue", async (string provider, int issueNumber, [FromHeader] string accessToken,
    [FromHeader] string? gitHubUserName,
    [FromHeader] string? gitHubRepository, [FromHeader] int? gitLabProjectId,
    IHttpClientFactory httpClientFactory,
    [FromServices] IEnumerable<ICloseIssue> issueStrategies) =>
{
    var headers = new HeaderDto(accessToken, gitHubUserName, gitHubRepository, gitLabProjectId);
    var client = httpClientFactory.CreateClient("GitClient");
    if (!Enum.TryParse<Providers>(provider, out var parsedProvider))
    {
        return Results.BadRequest(new { message = "Invalid provider name" });
    }

    ICloseIssue? strategy = parsedProvider switch
    {
        Providers.GitHub => issueStrategies.OfType<CloseGitHubIssue>().FirstOrDefault(),
        Providers.GitLab => issueStrategies.OfType<CloseGitLabIssue>().FirstOrDefault(),
        _ => null
    };

    if (strategy is null)
    {
        return Results.BadRequest("Invalid provider name");
    }

    var result = await strategy.UpdateIssueStateAsync(client, issueNumber, headers);

    return result.IsSuccessStatusCode
        ? Results.Ok("Issue Closed")
        : Results.Problem(await result.Content.ReadAsStringAsync(), statusCode: (int)result.StatusCode);
});
app.Run();