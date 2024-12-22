using System.Net;
using IssueTracker.Core.Commands.CloseIssuesStrategies;
using IssueTracker.Core.Domain;
using IssueTracker.Tests.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace IssueTracker.Tests.GitLabTests;

public class CloseGitLabIssueTests
{
    private IConfiguration _configuration;
    private ILogger<CloseGitLabIssue> _logger;
    private HttpClient _httpClient;
    private MockHttpMessageHandler _mockHandler;
    private CloseGitLabIssue _closeGitLabIssue;  

    [SetUp]
    public void Setup()
    {
        _configuration = Substitute.For<IConfiguration>();
        _logger = Substitute.For<ILogger<CloseGitLabIssue>>();
        _mockHandler = new MockHttpMessageHandler();
        _httpClient = Substitute.For<HttpClient>(_mockHandler);
        _closeGitLabIssue = new CloseGitLabIssue(_configuration, _logger);
        
        _configuration["GitLab:BaseUrl"].Returns("https://gitlab.com/api/v4/projects/");
        _configuration["Metadata:User-Agent"].Returns("TestUserAgent");
        _configuration["Metadata:MediaType"].Returns("application/json");
    }

    [Test]
    public async Task CloseIssueAsync_ShouldReturnBadRequest_WhenGitLabProjectIdIsNull()
    {
        // Arrange
        var headers = new HeaderDto("test-token", null, null, null);

        // Act
        var response = await _closeGitLabIssue.UpdateIssueStateAsync(_httpClient, 1, headers);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        _logger.Received().LogError("GitLab ProjectId is null");
    }

    [Test]
    public async Task CloseIssueAsync_ShouldReturnSuccess_WhenRequestIsValid()
    {
        // Arrange
        var headers = new HeaderDto("test-token", null, null, 1);

        _mockHandler.SendAsyncFunc = (request, cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.Created));

        // Act
        var response = await _closeGitLabIssue.UpdateIssueStateAsync(_httpClient, 1, headers);

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        _logger.DidNotReceiveWithAnyArgs().LogError(default);
    }

    [Test]
    public async Task CloseIssueAsync_ShouldLogError_WhenRequestFails()
    {
        // Arrange
        var headers = new HeaderDto("test-token", null, null, 1);

        _mockHandler.SendAsyncFunc = (request, cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                { Content = new StringContent("Internal Server Error") });

        // Act
        var response = await _closeGitLabIssue.UpdateIssueStateAsync(_httpClient, 1, headers);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        _logger.Received().LogError("Internal Server Error");
    }
}