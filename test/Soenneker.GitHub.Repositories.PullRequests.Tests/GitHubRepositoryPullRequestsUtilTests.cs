using Soenneker.GitHub.Repositories.PullRequests.Abstract;
using Soenneker.Tests.FixturedUnit;
using Xunit;
using Xunit.Abstractions;

namespace Soenneker.GitHub.Repositories.PullRequests.Tests;

[Collection("Collection")]
public class GitHubRepositoryPullRequestsUtilTests : FixturedUnitTest
{
    private readonly IGitHubRepositoryPullRequestsUtil _util;

    public GitHubRepositoryPullRequestsUtilTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _util = Resolve<IGitHubRepositoryPullRequestsUtil>(true);
    }
}
