using Soenneker.GitHub.Repositories.PullRequests.Abstract;
using Soenneker.Tests.FixturedUnit;
using Xunit;
using Xunit.Abstractions;

namespace Soenneker.GitHub.Repositories.PullRequests.Tests;

[Collection("Collection")]
public class GitHubRepositoriesPullRequestsUtilTests : FixturedUnitTest
{
    private readonly IGitHubRepositoriesPullRequestsUtil _util;

    public GitHubRepositoriesPullRequestsUtilTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _util = Resolve<IGitHubRepositoriesPullRequestsUtil>(true);
    }
}