using System.Net;
using IssueTracker.Core.Commands.PostIssuesStrategies;
using IssueTracker.Core.Domain;
using IssueTracker.Tests.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace IssueTracker.Tests.GitLabTests;

public class AddGitLabIssueTests
{
    private IConfiguration _configuration;
    private ILogger<AddGitLabIssue> _logger;
    private HttpClient _httpClient;
    private MockHttpMessageHandler _mockHandler;
    private AddGitLabIssue _addGitLabIssue;
    private IssueDto _testIssue;

    [SetUp]
    public void SetUp()
    {
        _configuration = Substitute.For<IConfiguration>();
        _logger = Substitute.For<ILogger<AddGitLabIssue>>();
        _mockHandler = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_mockHandler);
        _addGitLabIssue = new AddGitLabIssue(_configuration, _logger);

        _configuration["GitLab:BaseUrl"].Returns("https://gitlab.com/api/v4/projects/");
        _configuration["Metadata:User-Agent"].Returns("TestUserAgent");
        _configuration["Metadata:MediaType"].Returns("application/json");

        _testIssue = new IssueDto("Test Issue", "This is a test issue.");
    }

    [Test]
    public async Task AddIssueAsync_ShouldReturnBadRequest_WhenGitLabProjectIdIsNull()
    {
        // Arrange
        var headers = new HeaderDto("test-token", null, null, null);

        // Act
        var response = await _addGitLabIssue.AddIssueAsync(_testIssue, _httpClient, headers);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        _logger.Received().LogError("GitLab ProjectId is null");
    }

    [Test]
    public async Task AddIssueAsync_ShouldReturnSuccess_WhenRequestIsValid()
    {
        // Arrange
        var headers = new HeaderDto("test-token", null, null, 1);

        _mockHandler.SendAsyncFunc = (request, cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.Created));

        // Act
        var response = await _addGitLabIssue.AddIssueAsync(_testIssue, _httpClient, headers);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        _logger.DidNotReceiveWithAnyArgs().LogError(default);
    }

    [Test]
    public async Task AddIssueAsync_ShouldLogError_WhenRequestFails()
    {
        // Arrange
        var headers = new HeaderDto("test-token", null, null, 1);

        _mockHandler.SendAsyncFunc = (request, cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                { Content = new StringContent("Internal Server Error") });

        // Act
        var response = await _addGitLabIssue.AddIssueAsync(_testIssue, _httpClient, headers);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        _logger.Received().LogError("Internal Server Error");
    }
}