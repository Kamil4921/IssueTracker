using System.Net;
using System.Text.Json;
using IssueTracker.Core.Commands.PostIssuesStrategies;
using IssueTracker.Core.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IssueTracker.Core.Commands.CloseIssuesStrategies;

public class CloseGitLabIssue(IConfiguration configuration, ILogger<AddGitHubIssue> logger) : ICloseIssue
{
    public async Task<HttpResponseMessage> UpdateIssueStateAsync(HttpClient client, int issueNumber,
        HeaderDto headers)
    {
        if (headers.GitLabProjectId is null)
        {
            logger.LogError("GitLab ProjectId is null");
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("GitLabProjectId is null")
            };
        }

        var issueUrl = $"{configuration["GitLab:BaseUrl"]}/{headers.GitLabProjectId}/issues/{issueNumber}";
        var issue = new Dictionary<string, object>();
        var request = new HttpRequestMessage(HttpMethod.Put, issueUrl);

        request.Headers.Add("PRIVATE-TOKEN", headers.AccessToken);
        request.Headers.Add("User-Agent", $"{configuration["Metadata:User-Agent"]}");

        issue["state_event"] = "close";

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