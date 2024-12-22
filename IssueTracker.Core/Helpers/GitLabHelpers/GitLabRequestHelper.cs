using System.Text.Json;
using IssueTracker.Core.Domain;
using Microsoft.Extensions.Configuration;

namespace IssueTracker.Core.Helpers.GitLabHelpers;

public static class GitLabRequestHelper
{
    public static HttpRequestMessage CreateAddRequest(IssueDto issueDto, HeaderDto headers, IConfiguration configuration)
    {
        var request = new HttpRequestMessage(HttpMethod.Post,
            $"{configuration["GitLab:BaseUrl"]}{headers.GitLabProjectId}/issues");
        AddCommonHeaders(request, headers, configuration);
        AddIssueContent(request, issueDto, configuration);
        return request;
    }
    
    public static HttpRequestMessage CreateUpdateRequest(IssueDto issueDto, int issueNumber, HeaderDto headers, IConfiguration configuration)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, $"{configuration["GitLab:BaseUrl"]}/{headers.GitLabProjectId}/issues/{issueNumber}");
        AddCommonHeaders(request, headers, configuration);
        AddIssueContent(request, issueDto, configuration);
        return request;
    }

    public static HttpRequestMessage CreateCloseRequest(int issueNumber, HeaderDto headers, IConfiguration configuration)
    {
        var request = new HttpRequestMessage(HttpMethod.Put,
            $"{configuration["GitLab:BaseUrl"]}/{headers.GitLabProjectId}/issues/{issueNumber}");
        AddCommonHeaders(request, headers, configuration);
        AddCloseIssueContent(request, configuration);
        return request;
    }

    private static void AddCommonHeaders(HttpRequestMessage request, HeaderDto headers, IConfiguration configuration)
    {
        request.Headers.Add("PRIVATE-TOKEN", $"{headers.AccessToken}");
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
            issue["description"] = issueDto.Description;
        }

        var json = JsonSerializer.Serialize(issue);
        var content = new StringContent(json, null, configuration["Metadata:MediaType"]!);

        request.Content = content;
    }
    
    private static void AddCloseIssueContent(HttpRequestMessage request, IConfiguration configuration)
    {
        var issue = new Dictionary<string, object>();
        issue["state_event"] = "close";

        var json = JsonSerializer.Serialize(issue);
        var content = new StringContent(json, null, configuration["Metadata:MediaType"]!);

        request.Content = content;
    }
}