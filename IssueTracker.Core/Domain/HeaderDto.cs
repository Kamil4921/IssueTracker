namespace IssueTracker.Core.Domain;

public record HeaderDto(string AccessToken, string? GitHubUserName, string? GitHubRepository, int? GitLabProjectId);