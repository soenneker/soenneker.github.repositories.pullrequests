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
    ValueTask<IReadOnlyList<PullRequest>> GetAll(Repository repository, string? username = null, CancellationToken cancellationToken = default);

    ValueTask<IReadOnlyList<PullRequest>> GetAll(string owner, string name, string? username = null, CancellationToken cancellationToken = default);

    ValueTask ApproveAll(Repository repository, string message, string? username = null, int delayMs = 0, CancellationToken cancellationToken = default);

    ValueTask ApproveAll(string owner, string name, string message, string? username = null, int delayMs = 0, CancellationToken cancellationToken = default);

    ValueTask Approve(Repository repository, PullRequest pullRequest, string message, CancellationToken cancellationToken = default);

    ValueTask Approve(string owner, string name, PullRequest pullRequest, string message, CancellationToken cancellationToken = default);

    ValueTask<IReadOnlyList<Repository>> GetAllRepositoriesWithOpenPullRequests(string owner, CancellationToken cancellationToken = default);
}
