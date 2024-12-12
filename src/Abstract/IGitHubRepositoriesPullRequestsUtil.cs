using Octokit;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Soenneker.GitHub.Repositories.PullRequests.Abstract;

/// <summary>
/// A utility library for GitHub repository pull request related operations
/// </summary>
public interface IGitHubRepositoriesPullRequestsUtil
{
    ValueTask<IReadOnlyList<PullRequest>> GetPullRequests(Repository repository, string? username = null, CancellationToken cancellationToken = default);

    ValueTask<IReadOnlyList<PullRequest>> GetPullRequests(string owner, string name, string? username = null, CancellationToken cancellationToken = default);

    ValueTask ApproveAllPullRequests(Repository repository, string message, string? username = null, CancellationToken cancellationToken = default);

    ValueTask ApproveAllPullRequests(string owner, string name, string message, string? username = null, CancellationToken cancellationToken = default);

    ValueTask ApprovePullRequest(Repository repository, PullRequest pullRequest, string message, CancellationToken cancellationToken = default);

    ValueTask ApprovePullRequest(string owner, string name, PullRequest pullRequest, string message, CancellationToken cancellationToken = default);
}
