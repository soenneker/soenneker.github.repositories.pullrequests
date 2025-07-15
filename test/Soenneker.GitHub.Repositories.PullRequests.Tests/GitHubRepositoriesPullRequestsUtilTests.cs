using Soenneker.Facts.Local;
using Soenneker.GitHub.Repositories.PullRequests.Abstract;
using Soenneker.Tests.FixturedUnit;
using System.Threading.Tasks;
using Xunit;

namespace Soenneker.GitHub.Repositories.PullRequests.Tests;

[Collection("Collection")]
public class GitHubRepositoriesPullRequestsUtilTests : FixturedUnitTest
{
    private readonly IGitHubRepositoriesPullRequestsUtil _util;

    public GitHubRepositoriesPullRequestsUtilTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _util = Resolve<IGitHubRepositoriesPullRequestsUtil>(true);
    }

    [Fact]
    public void Default()
    {

    }

    [LocalFact]
    public async ValueTask HasFailedRunOnOpenPullRequests()
    { 
        bool result = await _util.HasFailedRunOnOpenPullRequests("", "", true, CancellationToken);
    }
}
