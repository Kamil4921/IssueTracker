using System.Net;
using IssueTracker.Core.Domain;
using IssueTracker.Core.Helpers.GitLabHelpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IssueTracker.Core.Commands.CloseIssuesStrategies;

public class CloseGitLabIssue(IConfiguration configuration, ILogger<CloseGitLabIssue> logger) : ICloseIssue
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
        
        var request = GitLabRequestHelper.CreateCloseRequest(issueNumber, headers, configuration);

        var response = await client.SendAsync(request);

        if (response.IsSuccessStatusCode) return response;

        var error = await response.Content.ReadAsStringAsync();
        logger.LogError(error);

        return response;
    }
}