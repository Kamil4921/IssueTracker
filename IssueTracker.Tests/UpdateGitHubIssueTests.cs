using System.Net;
using IssueTracker.Core.Commands.UpdateIssuesStrategies;
using IssueTracker.Core.Domain;
using IssueTracker.Tests.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace IssueTracker.Tests;

[TestFixture]
public class UpdateGitHubIssueTests
{
    private IConfiguration _configuration;
    private ILogger<UpdateGitHubIssue> _logger;
    private HttpClient _httpClient;
    private MockHttpMessageHandler _mockHandler;
    private UpdateGitHubIssue _updateGitHubIssue;
    private IssueDto _testIssue;

    [SetUp]
    public void SetUp()
    {
        _configuration = Substitute.For<IConfiguration>();
        _logger = Substitute.For<ILogger<UpdateGitHubIssue>>();
        _mockHandler = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_mockHandler);
        _updateGitHubIssue = new UpdateGitHubIssue(_configuration, _logger);

        _configuration["GitHub:BaseUrl"].Returns("https://api.github.com/repos/");
        _configuration["GitHub:Version"].Returns("v3");
        _configuration["Metadata:User-Agent"].Returns("TestUserAgent");
        _configuration["Metadata:MediaType"].Returns("application/json");

        _testIssue = new IssueDto("Test Issue", "This is a test issue.");
    }

    [Test]
    public async Task UpdateIssueAsync_ShouldReturnBadRequest_WhenGitHubUserNameOrRepositoryIsEmpty()
    {
        // Arrange
        var headers = new HeaderDto("test-token", "", "test-repo", null);

        // Act
        var response = await _updateGitHubIssue.UpdateIssueAsync(_testIssue, _httpClient, 1, headers);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.AreEqual("Empty GitHubUserName or GitHubRepository", content);
        _logger.Received()
            .LogError(
                "Github UserName or repositoryName are null or empty. GitHubUserName: , GitHubRepository: test-repo");
    }

    [Test]
    public async Task UpdateIssueAsync_ShouldReturnSuccess_WhenRequestIsValid()
    {
        // Arrange
        var headers = new HeaderDto("test-token", "test-user", "test-repo", null);

        _mockHandler.SendAsyncFunc = (request, cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        // Act
        var response = await _updateGitHubIssue.UpdateIssueAsync(_testIssue, _httpClient, 1, headers);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        _logger.DidNotReceiveWithAnyArgs().LogError(default);
    }

    [Test]
    public async Task UpdateIssueAsync_ShouldLogError_WhenRequestFails()
    {
        // Arrange
        var headers = new HeaderDto("test-token", "test-user", "test-repo", null);

        _mockHandler.SendAsyncFunc = (request, cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                { Content = new StringContent("Internal Server Error") });

        // Act
        var response = await _updateGitHubIssue.UpdateIssueAsync(_testIssue, _httpClient, 1, headers);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        _logger.Received().LogError("Internal Server Error");
    }
}