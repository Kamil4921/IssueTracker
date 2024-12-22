using IssueTracker.Core.Domain;

namespace IssueTracker.Core.Commands.PostIssuesStrategies;

public interface IAddIssue
{
    Task<HttpResponseMessage> AddIssueAsync(IssueDto issueDto, HttpClient client, HeaderDto headers);
}