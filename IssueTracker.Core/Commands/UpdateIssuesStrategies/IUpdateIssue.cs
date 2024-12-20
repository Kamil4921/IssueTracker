using IssueTracker.Core.Domain;

namespace IssueTracker.Core.Commands.UpdateIssuesStrategies;

public interface IUpdateIssue
{
    Task<HttpResponseMessage> UpdateIssueAsync(IssueDto dto, HttpClient client, int issueNumber, HeaderDto headers);
}