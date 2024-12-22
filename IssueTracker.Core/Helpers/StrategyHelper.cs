using IssueTracker.Core.Commands.CloseIssuesStrategies;
using IssueTracker.Core.Commands.PostIssuesStrategies;
using IssueTracker.Core.Commands.UpdateIssuesStrategies;
using IssueTracker.Core.Domain;

namespace IssueTracker.Core.Helpers;

public static class StrategyHelper
{
    public static T? GetProviderStrategy<T>(string provider, IEnumerable<T> issueStrategies) where T : class
    {
        if (Enum.TryParse<Providers>(provider, out var parsedProvider))
        {
            return parsedProvider switch
            {
                Providers.GitHub => issueStrategies.FirstOrDefault(s=> s is AddGitHubIssue or UpdateGitHubIssue or CloseGitHubIssue),
                Providers.GitLab => issueStrategies.FirstOrDefault(s => s is AddGitLabIssue or UpdateGitLabIssue or CloseGitLabIssue),
                _ => null
            };
        }
        return null;
    }
}