using System.Net;
using IssueTracker.Core.Domain;
using IssueTracker.Core.Helpers.GitHubHelpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IssueTracker.Core.Commands.CloseIssuesStrategies;

public class CloseGitHubIssue(IConfiguration configuration, ILogger<CloseGitHubIssue> logger) : ICloseIssue
{
    public async Task<HttpResponseMessage> UpdateIssueStateAsync(HttpClient client, int issueNumber, HeaderDto headers)
    {
        if (string.IsNullOrWhiteSpace(headers.GitHubUserName) || string.IsNullOrWhiteSpace(headers.GitHubRepository))
        {
            logger.LogError(
                $"Github UserName or repositoryName are null or empty. GitHubUserName: {headers.GitHubUserName}, GitHubRepository: {headers.GitHubRepository}");
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Wrong GitHubUserName or GitHubRepository")
            };
        }

        var request = GitHubRequestHelper.CreateCloseRequest(issueNumber, headers, configuration);
        
        var response = await client.SendAsync(request);

        if (response.IsSuccessStatusCode) return response;

        var error = await response.Content.ReadAsStringAsync();
        logger.LogError(error);

        return response;
    }
}