using System.Net;
using IssueTracker.Core.Commands.CloseIssuesStrategies;
using IssueTracker.Core.Domain;
using IssueTracker.Tests.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace IssueTracker.Tests.GitHubTests;

public class CloseGitHubIssueTests
{
    private IConfiguration _configuration;
    private ILogger<CloseGitHubIssue> _logger;
    private HttpClient _httpClient;
    private MockHttpMessageHandler _mockHandler;
    private CloseGitHubIssue _closeGitHubIssue;

    [SetUp]
    public void Setup()
    {
        _configuration = Substitute.For<IConfiguration>();
        _logger = Substitute.For<ILogger<CloseGitHubIssue>>();
        _mockHandler = new MockHttpMessageHandler();
        _httpClient = Substitute.For<HttpClient>(_mockHandler);
        _closeGitHubIssue = new CloseGitHubIssue(_configuration, _logger);
        
        _configuration["GitHub:BaseUrl"].Returns("https://api.github.com/repos/");
        _configuration["GitHub:Version"].Returns("v3");
        _configuration["Metadata:User-Agent"].Returns("TestUserAgent");
        _configuration["Metadata:MediaType"].Returns("application/json");
    }

    [Test]
    public async Task CloseIssueAsync_ShouldReturnBadRequest_WhenGitHubUserNameOrRepositoryIsNull()
    {
        // Arrange
        var headers = new HeaderDto("test-token", null, null, null);

        // Act
        var response = await _closeGitHubIssue.UpdateIssueStateAsync(_httpClient, 1, headers);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.AreEqual("Empty GitHubUserName or GitHubRepository", content);
        _logger.Received().LogError("Github UserName or repositoryName are null or empty. GitHubUserName: , GitHubRepository: ");
    }

    [Test]
    public async Task CloseIssueAsync_ShouldReturnSuccess_WhenRequestIsValid()
    {
        // Arrange
        var headers = new HeaderDto("test-token", "test-user", "test-repo", null);
        
        _mockHandler.SendAsyncFunc = (request, cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        // Act
        var response = await _closeGitHubIssue.UpdateIssueStateAsync(_httpClient, 1, headers);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        _logger.DidNotReceiveWithAnyArgs().LogError(default);
    }

    [Test]
    public async Task CloseIssueAsync_ShouldLogError_WhenResponseIsNotSuccessful()
    {
        // Arrange
        var headers = new HeaderDto("test-token", "test-user", "test-repo", null);

        _mockHandler.SendAsyncFunc = (request, cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
                { Content = new StringContent("Internal Server Error") });

        // Act
        var response = await _closeGitHubIssue.UpdateIssueStateAsync(_httpClient, 1, headers);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        _logger.Received().LogError("Internal Server Error");
    }
}