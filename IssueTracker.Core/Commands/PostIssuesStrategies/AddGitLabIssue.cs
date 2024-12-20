using System.Net;
using IssueTracker.Core.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IssueTracker.Core.Commands.PostIssuesStrategies;

public class AddGitLabIssue(IConfiguration configuration, ILogger<AddGitHubIssue> logger) : IAddIssue
{
    public async Task<HttpResponseMessage> AddIssueAsync(IssueDto dto, HttpClient client, HeaderDto headers)
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
        
        var request = new HttpRequestMessage(HttpMethod.Post, $"{configuration["GitLab:BaseUrl"]}{headers.GitLabProjectId}/issues");
        
        request.Headers.Add("PRIVATE-TOKEN", $"{headers.AccessToken}");
        request.Headers.Add("User-Agent", $"{configuration["Metadata:User-Agent"]}");

        var issue = new
        {
            title = dto.Title,
            description = dto.Description
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