using IssueTracker.Core.Domain;

namespace IssueTracker.Core.Commands.UpdateIssuesStrategies;

public interface IUpdateIssue
{
    Task<HttpResponseMessage> UpdateIssueAsync(IssueDto issueDto, HttpClient client, int issueNumber, HeaderDto headers);
}