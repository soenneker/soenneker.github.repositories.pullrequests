using Octokit;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics.Contracts;

namespace Soenneker.GitHub.Repositories.PullRequests.Abstract;

/// <summary>
/// A utility library for GitHub repository pull request related operations
/// </summary>
public interface IGitHubRepositoriesPullRequestsUtil
{
    /// <summary>
    /// Retrieves all pull requests for a specific repository.
    /// </summary>
    /// <param name="repository">The repository from which to retrieve pull requests.</param>
    /// <param name="username">Optional username to filter the pull requests by author.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> containing a read-only list of pull requests for the repository.
    /// </returns>
    [Pure]
    ValueTask<IReadOnlyList<PullRequest>> GetAll(Repository repository, string? username = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all pull requests for a repository identified by its owner and name.
    /// </summary>
    /// <param name="owner">The username or organization name of the repository owner.</param>
    /// <param name="name">The name of the repository.</param>
    /// <param name="username">Optional username to filter the pull requests by author.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> containing a read-only list of pull requests for the specified repository.
    /// </returns>
    [Pure]
    ValueTask<IReadOnlyList<PullRequest>> GetAll(string owner, string name, string? username = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Approves all pull requests for a specific repository.
    /// </summary>
    /// <param name="repository">The repository containing the pull requests to approve.</param>
    /// <param name="message">The approval message to include with each pull request.</param>
    /// <param name="username">Optional username to filter the pull requests by author.</param>
    /// <param name="delayMs">Optional delay in milliseconds between approving each pull request to avoid rate limits.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    ValueTask ApproveAll(Repository repository, string message, string? username = null, int delayMs = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// Approves all pull requests for a repository identified by its owner and name.
    /// </summary>
    /// <param name="owner">The username or organization name of the repository owner.</param>
    /// <param name="name">The name of the repository containing the pull requests to approve.</param>
    /// <param name="message">The approval message to include with each pull request.</param>
    /// <param name="username">Optional username to filter the pull requests by author.</param>
    /// <param name="delayMs">Optional delay in milliseconds between approving each pull request to avoid rate limits.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    ValueTask ApproveAll(string owner, string name, string message, string? username = null, int delayMs = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// Approves a specific pull request in a repository.
    /// </summary>
    /// <param name="repository">The repository containing the pull request to approve.</param>
    /// <param name="pullRequest">The pull request to approve.</param>
    /// <param name="message">The approval message to include with the pull request.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    ValueTask Approve(Repository repository, PullRequest pullRequest, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Approves a specific pull request in a repository identified by its owner and name.
    /// </summary>
    /// <param name="owner">The username or organization name of the repository owner.</param>
    /// <param name="name">The name of the repository containing the pull request to approve.</param>
    /// <param name="pullRequest">The pull request to approve.</param>
    /// <param name="message">The approval message to include with the pull request.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    ValueTask Approve(string owner, string name, PullRequest pullRequest, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Filters repositories that have pull requests with failed builds.
    /// </summary>
    /// <param name="repositories">The list of repositories to filter.</param>
    /// <param name="log"></param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> containing a read-only list of repositories with failed pull request builds.
    /// </returns>
    [Pure]
    ValueTask<IReadOnlyList<Repository>> FilterRepositoriesWithFailedBuilds(IReadOnlyList<Repository> repositories, bool log = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all repositories that have open pull requests with failed builds for a specific user.
    /// </summary>
    /// <param name="username">The username to filter the repositories by.</param>
    /// <param name="log"></param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> containing a read-only list of repositories with failed builds on open pull requests.
    /// </returns>
    [Pure]
    ValueTask<IReadOnlyList<Repository>> GetAllRepositoriesWithFailedBuildsOnOpenPullRequests(string username, bool log = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all repositories with open pull requests for a specific owner.
    /// </summary>
    /// <param name="owner">The username or organization name of the repository owner.</param>
    /// <param name="log"></param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> containing a read-only list of repositories with open pull requests.
    /// </returns>
    [Pure]
    ValueTask<IReadOnlyList<Repository>> GetAllRepositoriesWithOpenPullRequests(string owner, bool log = true, CancellationToken cancellationToken = default);

}
