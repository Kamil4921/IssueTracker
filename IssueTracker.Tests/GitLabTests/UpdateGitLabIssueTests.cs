using System.Net;
using IssueTracker.Core.Commands.UpdateIssuesStrategies;
using IssueTracker.Core.Domain;
using IssueTracker.Tests.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace IssueTracker.Tests.GitLabTests;

public class UpdateGitLabIssueTests
{
    private IConfiguration _configuration;
    private ILogger<UpdateGitLabIssue> _logger;
    private HttpClient _httpClient;
    private MockHttpMessageHandler _mockHandler;
    private UpdateGitLabIssue _updateGitLabIssue;  
    private IssueDto _testIssue;

    [SetUp]
    public void Setup()
    {
        _configuration = Substitute.For<IConfiguration>();
        _logger = Substitute.For<ILogger<UpdateGitLabIssue>>();
        _mockHandler = new MockHttpMessageHandler();
        _httpClient = Substitute.For<HttpClient>(_mockHandler);
        _updateGitLabIssue = new UpdateGitLabIssue(_configuration, _logger);
        
        _configuration["GitLab:BaseUrl"].Returns("https://gitlab.com/api/v4/projects/");
        _configuration["Metadata:User-Agent"].Returns("TestUserAgent");
        _configuration["Metadata:MediaType"].Returns("application/json");
        
        _testIssue = new IssueDto("Test Issue", "This is a test issue.");
    }

    [Test]
    public async Task UpdateIssueAsync_ShouldReturnBadRequest_WhenGitLabProjectIdIsNull()
    {
        // Arrange
        var headers = new HeaderDto("test-token", null, null, null);

        // Act
        var response = await _updateGitLabIssue.UpdateIssueAsync(_testIssue, _httpClient, 1, headers);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        _logger.Received().LogError("GitLab ProjectId is null");
    }

    [Test]
    public async Task UpdateIssueAsync_ShouldReturnSuccess_WhenRequestIsValid()
    {
        // Arrange
        var headers = new HeaderDto("test-token", null, null, 1);
        
        _mockHandler.SendAsyncFunc = (request, cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        // Act
        var response = await _updateGitLabIssue.UpdateIssueAsync(_testIssue, _httpClient, 1, headers);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}