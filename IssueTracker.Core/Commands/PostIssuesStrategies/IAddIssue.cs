using IssueTracker.Core.Domain;

namespace IssueTracker.Core.Commands.PostIssuesStrategies;

public interface IAddIssue
{
    Task<HttpResponseMessage> AddIssueAsync(IssueDto dto, HttpClient client, HeaderDto headers);
}