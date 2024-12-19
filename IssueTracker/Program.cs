using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

var tokenGh = configuration["GitHub:AccessToken"];
var tokenGl = configuration["GitLab:AccessToken"];


app.MapPost("/issues/create", async (string owner, string repo, [FromBody]IssueDto dto, string? token, IHttpClientFactory httpClientFactory) =>
{
    var client = httpClientFactory.CreateClient();
   
    var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.github.com/repos/{owner}/{repo}/issues");
    
    request.Headers.Add("Authorization", $"Bearer {tokenGh}");
    request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
    request.Headers.Add("User-Agent", "IssueTracker");
    var issue = new
    {
        title = dto.Title,
        body = dto.Body
    };
    var json = System.Text.Json.JsonSerializer.Serialize(issue);
    var content = new StringContent(json, null, "application/json");
    request.Content = content;
    

    var response = await client.SendAsync(request);

    if (!response.IsSuccessStatusCode)
    {
        var errorContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Error: {response.StatusCode}, {errorContent}");
        return Results.StatusCode((int)response.StatusCode);
    }
    
    response.EnsureSuccessStatusCode();

    return Results.Ok(new { message = "Issue Created" });
}).WithOpenApi();

app.MapPost("/issuesGitLab/create", async (int project, [FromBody]IssueDto dto, string? token, IHttpClientFactory httpClientFactory) =>
{
    var client = httpClientFactory.CreateClient();
   
    var request = new HttpRequestMessage(HttpMethod.Post, $"https://gitlab.com/api/v4/projects/{project}/issues");
    
    request.Headers.Add("PRIVATE-TOKEN", tokenGl);
    request.Headers.Add("User-Agent", "IssueTracker");
    var issue = new
    {
        title = dto.Title
    };
    var json = System.Text.Json.JsonSerializer.Serialize(issue);
    var content = new StringContent(json, null, "application/json");
    request.Content = content;
    
    var response = await client.SendAsync(request);

    if (!response.IsSuccessStatusCode)
    {
        var errorContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Error: {response.StatusCode}, {errorContent}");
        return Results.StatusCode((int)response.StatusCode);
    }
    
    response.EnsureSuccessStatusCode();

    return Results.Ok(new { message = "Issue Created" });
}).WithOpenApi();

app.Run();

public class IssueDto { public string Title { get; set; } public string Body { get; set; } }