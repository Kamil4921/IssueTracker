using IssueTracker.Core.Commands.CloseIssuesStrategies;
using IssueTracker.Core.Commands.PostIssuesStrategies;
using IssueTracker.Core.Commands.UpdateIssuesStrategies;
using IssueTracker.Core.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace IssueTracker.Tests;

public class Tests
{
    private List<IAddIssue> _addIssueStrategies;
    private List<IUpdateIssue> _updateIssueStrategies;
    private List<ICloseIssue> _closeIssueStrategies;

    [SetUp]
    public void SetUp()
    {
        var configuration = Substitute.For<IConfiguration>(); 
        
        var gitHubAddLogger = Substitute.For<ILogger<AddGitHubIssue>>();
        var gitLabAddLogger = Substitute.For<ILogger<AddGitLabIssue>>();
        var addGitHubIssue = Substitute.ForPartsOf<AddGitHubIssue>(configuration, gitHubAddLogger);
        var addGitLabIssue = Substitute.ForPartsOf<AddGitLabIssue>(configuration, gitLabAddLogger);
        _addIssueStrategies = new List<IAddIssue> { addGitHubIssue, addGitLabIssue };
        
        var gitHubUpdateLogger = Substitute.For<ILogger<UpdateGitHubIssue>>();
        var gitLabUpdateLogger = Substitute.For<ILogger<UpdateGitLabIssue>>();
        var updateGitHubIssue = Substitute.ForPartsOf<UpdateGitHubIssue>(configuration, gitHubUpdateLogger);
        var updateGitLabIssue = Substitute.ForPartsOf<UpdateGitLabIssue>(configuration, gitLabUpdateLogger);
        _updateIssueStrategies = new List<IUpdateIssue> { updateGitHubIssue, updateGitLabIssue };
        
        var gitHubCloseLogger = Substitute.For<ILogger<CloseGitHubIssue>>();
        var gitLabCloseLogger = Substitute.For<ILogger<CloseGitLabIssue>>();
        var closeGitHubIssue = Substitute.ForPartsOf<CloseGitHubIssue>(configuration, gitHubCloseLogger);
        var closeGitLabIssue = Substitute.ForPartsOf<CloseGitLabIssue>(configuration, gitLabCloseLogger);
        _closeIssueStrategies = new List<ICloseIssue> { closeGitHubIssue, closeGitLabIssue };
    }

    [Test]
    public void GetProviderStrategy_ForAddIssue_ShouldReturnGitHubStrategy()
    {
        // Act
        var result = StrategyHelper.GetProviderStrategy("GitHub", _addIssueStrategies);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<IAddIssue>(result);
    }
    
    [Test]
    public void GetProviderStrategy_ForAddIssue_ShouldReturnGitLabStrategy()
    {
        // Act
        var result = StrategyHelper.GetProviderStrategy("GitLab", _addIssueStrategies);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<IAddIssue>(result);
    }
    
    [Test]
    public void GetProviderStrategy_ForInvalidProvider_InAddIssue_ShouldReturnNull()
    {
        // Act
        var result = StrategyHelper.GetProviderStrategy("InvalidProvider", _addIssueStrategies);

        // Assert
        Assert.IsNull(result);
    }
    
    [Test]
    public void GetProviderStrategy_ForUpdateIssue_ShouldReturnGitHubStrategy()
    {
        // Act
        var result = StrategyHelper.GetProviderStrategy("GitHub", _updateIssueStrategies);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<IUpdateIssue>(result);
    }

    [Test]
    public void GetProviderStrategy_ForUpdateIssue_ShouldReturnGitLabStrategy()
    {
        // Act
        var result = StrategyHelper.GetProviderStrategy("GitLab", _updateIssueStrategies);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<IUpdateIssue>(result);
    }

    [Test]
    public void GetProviderStrategy_ForInvalidProvider_InUpdateIssue_ShouldReturnNull()
    {
        // Act
        var result = StrategyHelper.GetProviderStrategy("InvalidProvider", _updateIssueStrategies);

        // Assert
        Assert.IsNull(result);
    }

    [Test]
    public void GetProviderStrategy_ForCloseIssue_ShouldReturnGitHubStrategy()
    {
        // Act
        var result = StrategyHelper.GetProviderStrategy("GitHub", _closeIssueStrategies);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<ICloseIssue>(result);
    }

    [Test]
    public void GetProviderStrategy_ForCloseIssue_ShouldReturnGitLabStrategy()
    {
        // Act
        var result = StrategyHelper.GetProviderStrategy("GitLab", _closeIssueStrategies);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<ICloseIssue>(result);
    }

    [Test]
    public void GetProviderStrategy_ForInvalidProvider_InCloseIssue_ShouldReturnNull()
    {
        // Act
        var result = StrategyHelper.GetProviderStrategy("InvalidProvider", _closeIssueStrategies);

        // Assert
        Assert.IsNull(result);
    }
}