using System.Net;
using System.Text.Json;
using IssueTracker.Core.Commands.PostIssuesStrategies;
using IssueTracker.Core.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IssueTracker.Core.Commands.CloseIssuesStrategies;

public class CloseGitHubIssue(IConfiguration configuration, ILogger<AddGitHubIssue> logger) : ICloseIssue
{
    public async Task<HttpResponseMessage> UpdateIssueStateAsync(HttpClient client, int issueNumber, HeaderDto headers)
    {
        if (string.IsNullOrWhiteSpace(headers.GitHubUserName) || string.IsNullOrWhiteSpace(headers.GitHubRepository))
        {
            logger.LogError($"Github UserName or repositoryName are null or empty. GitHubUserName: {headers.GitHubUserName}, GitHubRepository: {headers.GitHubRepository}");
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Wrong GitHubUserName or GitHubRepository")
            };
        }
        
        var request = new HttpRequestMessage(HttpMethod.Patch,
            $"{configuration["GitHub:BaseUrl"]}{headers.GitHubUserName}/{headers.GitHubRepository}/issues/{issueNumber}");

        request.Headers.Add("Authorization", $"Bearer {headers.AccessToken}");
        request.Headers.Add("X-GitHub-Api-Version", $"{configuration["GitHub:Version"]}");
        request.Headers.Add("User-Agent", $"{configuration["Metadata:User-Agent"]}");

        var issue = new
        {
            state = "closed"
        };

        var json = JsonSerializer.Serialize(issue);
        var content = new StringContent(json, null, configuration["Metadata:MediaType"]!);

        request.Content = content;

        var response = await client.SendAsync(request);

        if (response.IsSuccessStatusCode) return response;

        var error = await response.Content.ReadAsStringAsync();
        logger.LogError(error);

        return response;
    }
}