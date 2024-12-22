using System.Net;
using IssueTracker.Core.Domain;
using IssueTracker.Core.Helpers.GitLabHelpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IssueTracker.Core.Commands.PostIssuesStrategies;

public class AddGitLabIssue(IConfiguration configuration, ILogger<AddGitLabIssue> logger) : IAddIssue
{
    public async Task<HttpResponseMessage> AddIssueAsync(IssueDto issueDto, HttpClient client, HeaderDto headers)
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
        
        var request = GitLabRequestHelper.CreateAddRequest(issueDto, headers, configuration);
        
        var response = await client.SendAsync(request);

        if (response.IsSuccessStatusCode) return response;

        var error = await response.Content.ReadAsStringAsync();
        logger.LogError(error);

        return response;
    }
}