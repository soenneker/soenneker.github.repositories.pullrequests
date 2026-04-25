using System.Threading;
using Soenneker.Tests.Attributes.Local;
using Soenneker.GitHub.Repositories.PullRequests.Abstract;
using Soenneker.Tests.HostedUnit;
using System.Threading.Tasks;

namespace Soenneker.GitHub.Repositories.PullRequests.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public class GitHubRepositoriesPullRequestsUtilTests : HostedUnitTest
{
    private readonly IGitHubRepositoriesPullRequestsUtil _util;

    public GitHubRepositoriesPullRequestsUtilTests(Host host) : base(host)
    {
        _util = Resolve<IGitHubRepositoriesPullRequestsUtil>(true);
    }

    [Test]
    public void Default()
    {

    }

    [LocalOnly]
    public async ValueTask HasFailedRunOnOpenPullRequests(CancellationToken cancellationToken)
    { 
        bool result = await _util.HasFailedRunOnOpenPullRequests("", "", true, cancellationToken);
    }
}
