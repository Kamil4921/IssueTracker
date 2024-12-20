using IssueTracker.Core.Domain;

namespace IssueTracker.Core.Commands.CloseIssuesStrategies;

public interface ICloseIssue
{
    Task<HttpResponseMessage> UpdateIssueStateAsync(HttpClient client, int issueNumber, HeaderDto headers);
}