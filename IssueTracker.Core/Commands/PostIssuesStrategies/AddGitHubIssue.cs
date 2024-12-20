using System.Net;
using IssueTracker.Core.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IssueTracker.Core.Commands.PostIssuesStrategies;

public class AddGitHubIssue(IConfiguration configuration, ILogger<AddGitHubIssue> logger) : IAddIssue
{
    public async Task<HttpResponseMessage> AddIssueAsync(IssueDto dto, HttpClient client, HeaderDto headers)
    {
        if (string.IsNullOrWhiteSpace(headers.GitHubUserName) || string.IsNullOrWhiteSpace(headers.GitHubRepository))
        {
            logger.LogError($"Github UserName or repositoryName are null or empty. GitHubUserName: {headers.GitHubUserName}, GitHubRepository: {headers.GitHubRepository}");
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Empty GitHubUserName or GitHubRepository")
            };
        }
        
        var request = new HttpRequestMessage(HttpMethod.Post, $"{configuration["GitHub:BaseUrl"]}{headers.GitHubUserName}/{headers.GitHubRepository}/issues");
        
        request.Headers.Add("Authorization", $"Bearer {headers.AccessToken}");
        request.Headers.Add("X-GitHub-Api-Version", $"{configuration["GitHub:Version"]}");
        request.Headers.Add("User-Agent", $"{configuration["Metadata:User-Agent"]}");

        var issue = new
        {
            title = dto.Title,
            body = dto.Description
        };
        
        var json = System.Text.Json.JsonSerializer.Serialize(issue);
        var content = new StringContent(json, null, configuration["Metadata:MediaType"]!);
        
        request.Content = content;

        var response = await client.SendAsync(request);

        if (response.IsSuccessStatusCode) return response;
        
        var error = await response.Content.ReadAsStringAsync();
        logger.LogError(error);

        return response;
    }
}