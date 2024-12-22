using System.Net;
using IssueTracker.Core.Commands.PostIssuesStrategies;
using IssueTracker.Core.Domain;
using IssueTracker.Tests.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace IssueTracker.Tests.GitHubTests;

public class AddGitHubIssueTests
{
    private IConfiguration _configuration;
    private ILogger<AddGitHubIssue> _logger;
    private HttpClient _httpClient;
    private MockHttpMessageHandler _mockHandler;
    private AddGitHubIssue _addGitHubIssue;
    private IssueDto _testIssue;

    [SetUp]
    public void Setup()
    {
        _configuration = Substitute.For<IConfiguration>();
        _logger = Substitute.For<ILogger<AddGitHubIssue>>();
        _mockHandler = new MockHttpMessageHandler();
        _httpClient = Substitute.For<HttpClient>(_mockHandler);
        _addGitHubIssue = new AddGitHubIssue(_configuration, _logger);
        
        _configuration["GitHub:BaseUrl"].Returns("https://api.github.com/repos/");
        _configuration["GitHub:Version"].Returns("v3");
        _configuration["Metadata:User-Agent"].Returns("TestUserAgent");
        _configuration["Metadata:MediaType"].Returns("application/json");

        _testIssue = new IssueDto("Test Issue", "This is a test issue.");
    }

    [Test]
    public async Task AddIssueAsync_ShouldReturnBadRequest_WhenGitHubUserNameOrRepositoryIsNull()
    {
        // Arrange
        var headers = new HeaderDto("test-token", null, null, null);

        // Act
        var response = await _addGitHubIssue.AddIssueAsync(_testIssue, _httpClient, headers);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Is.EqualTo("Empty GitHubUserName or GitHubRepository"));
        _logger.Received().LogError("Github UserName or repositoryName are null or empty. GitHubUserName: , GitHubRepository: ");
    }

    [Test]
    public async Task AddIssueAsync_ShouldReturnCreated_WhenRequestIsValid()
    {
        // Arrange
        var headers = new HeaderDto("test-token", "test-user", "test-repo", null);
        
        _mockHandler.SendAsyncFunc = (request, cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.Created));

        // Act
        var response = await _addGitHubIssue.AddIssueAsync(_testIssue, _httpClient, headers);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        _logger.DidNotReceiveWithAnyArgs().LogError(default);
    }

    [Test]
    public async Task AddIssueAsync_ShouldLogError_WhenResponseIsNotSuccessful()
    {
        // Arrange
        var headers = new HeaderDto("test-token", "test-user", "test-repo", null);

        _mockHandler.SendAsyncFunc = (request, cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                { Content = new StringContent("Internal Server Error") });

        // Act
        var response = await _addGitHubIssue.AddIssueAsync(_testIssue, _httpClient, headers);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        _logger.Received().LogError("Internal Server Error");
    }
}