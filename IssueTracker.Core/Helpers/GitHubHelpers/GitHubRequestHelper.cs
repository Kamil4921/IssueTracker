using System.Text.Json;
using IssueTracker.Core.Domain;
using Microsoft.Extensions.Configuration;

namespace IssueTracker.Core.Helpers.GitHubHelpers;

public static class GitHubRequestHelper
{
    public static HttpRequestMessage CreateAddRequest(IssueDto issueDto, HeaderDto headers, IConfiguration configuration)
    {
        var request = new HttpRequestMessage(HttpMethod.Post,
            $"{configuration["GitHub:BaseUrl"]}{headers.GitHubUserName}/{headers.GitHubRepository}/issues");
        AddCommonHeaders(request, headers, configuration);
        AddIssueContent(request, issueDto, configuration);
        return request;
    }

    public static HttpRequestMessage CreateUpdateRequest(IssueDto issueDto, int issueNumber, HeaderDto headers, IConfiguration configuration)
    {
        var request = new HttpRequestMessage(HttpMethod.Patch,
            $"{configuration["GitHub:BaseUrl"]}{headers.GitHubUserName}/{headers.GitHubRepository}/issues/{issueNumber}");
        AddCommonHeaders(request, headers, configuration);
        AddIssueContent(request, issueDto, configuration);
        return request;
    }

    public static HttpRequestMessage CreateCloseRequest(int issueNumber, HeaderDto headers, IConfiguration configuration)
    {
        var request = new HttpRequestMessage(HttpMethod.Patch,
            $"{configuration["GitHub:BaseUrl"]}{headers.GitHubUserName}/{headers.GitHubRepository}/issues/{issueNumber}");
        AddCommonHeaders(request, headers, configuration);
        AddCloseIssueContent(request, configuration);
        return request;
    }

    private static void AddCommonHeaders(HttpRequestMessage request, HeaderDto headers, IConfiguration configuration)
    {
        request.Headers.Add("Authorization", $"Bearer {headers.AccessToken}");
        request.Headers.Add("X-GitHub-Api-Version", $"{configuration["GitHub:Version"]}");
        request.Headers.Add("User-Agent", $"{configuration["Metadata:User-Agent"]}");
    }

    private static void AddIssueContent(HttpRequestMessage request, IssueDto issueDto, IConfiguration configuration)
    {
        var issue = new Dictionary<string, object>();
        if (!string.IsNullOrWhiteSpace(issueDto.Title))
        {
            issue["title"] = issueDto.Title;
        }

        if (!string.IsNullOrWhiteSpace(issueDto.Description))
        {
            issue["body"] = issueDto.Description;
        }

        var json = JsonSerializer.Serialize(issue);
        var content = new StringContent(json, null, configuration["Metadata:MediaType"]!);
        request.Content = content;
    }

    private static void AddCloseIssueContent(HttpRequestMessage request, IConfiguration configuration)
    {
        var issue = new
        {
            state = "closed"
        };

        var json = JsonSerializer.Serialize(issue);
        var content = new StringContent(json, null, configuration["Metadata:MediaType"]!);
        request.Content = content;
    }
}
