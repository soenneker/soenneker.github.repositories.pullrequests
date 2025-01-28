using Octokit;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Soenneker.GitHub.Repositories.PullRequests.Abstract;

/// <summary>
/// A utility library for GitHub repository pull request related operations
/// </summary>
public interface IGitHubRepositoriesPullRequestsUtil
{
    /// <summary>
    /// Retrieves all pull requests for the specified repository.
    /// </summary>
    /// <param name="repository">The repository to retrieve pull requests for.</param>
    /// <param name="username">Optional: The username of the pull request author.</param>
    /// <param name="startAt">Optional: The start date for filtering pull requests.</param>
    /// <param name="endAt">Optional: The end date for filtering pull requests.</param>
    /// <param name="log">Optional: Whether to log the operation.</param>
    /// <param name="cancellationToken">Optional: Cancellation token.</param>
    /// <returns>A list of matching pull requests.</returns>
    ValueTask<IReadOnlyList<PullRequest>> GetAll(Repository repository, string? username = null, DateTime? startAt = null, DateTime? endAt = null, bool log = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all pull requests for a specific repository by owner and name.
    /// </summary>
    /// <param name="owner">The owner of the repository.</param>
    /// <param name="name">The name of the repository.</param>
    /// <param name="username">Optional: The username of the pull request author.</param>
    /// <param name="startAt">Optional: The start date for filtering pull requests.</param>
    /// <param name="endAt">Optional: The end date for filtering pull requests.</param>
    /// <param name="log">Optional: Whether to log the operation.</param>
    /// <param name="cancellationToken">Optional: Cancellation token.</param>
    /// <returns>A list of matching pull requests.</returns>
    ValueTask<IReadOnlyList<PullRequest>> GetAll(string owner, string name, string? username = null, DateTime? startAt = null, DateTime? endAt = null, bool log = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all pull requests for all repositories owned by the specified owner.
    /// </summary>
    /// <param name="owner">The owner of the repositories.</param>
    /// <param name="username">Optional: The username of the pull request author.</param>
    /// <param name="startAt">Optional: The start date for filtering pull requests.</param>
    /// <param name="endAt">Optional: The end date for filtering pull requests.</param>
    /// <param name="cancellationToken">Optional: Cancellation token.</param>
    /// <returns>A list of pull requests across all repositories.</returns>
    ValueTask<List<PullRequest>> GetAllForOwner(string owner, string? username = null, DateTime? startAt = null, DateTime? endAt = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all non-approved pull requests for a specific repository.
    /// </summary>
    /// <param name="owner">The owner of the repository.</param>
    /// <param name="name">The name of the repository.</param>
    /// <param name="username">Optional: The username of the pull request author.</param>
    /// <param name="startAt">Optional: The start date for filtering pull requests.</param>
    /// <param name="endAt">Optional: The end date for filtering pull requests.</param>
    /// <param name="log">Optional: Whether to log the operation.</param>
    /// <param name="cancellationToken">Optional: Cancellation token.</param>
    /// <returns>A list of non-approved pull requests.</returns>
    ValueTask<List<PullRequest>> GetAllNonApproved(string owner, string name, string? username = null, DateTime? startAt = null, DateTime? endAt = null, bool log = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all non-approved pull requests across all repositories owned by the specified owner.
    /// </summary>
    /// <param name="owner">The owner of the repositories.</param>
    /// <param name="username">Optional: The username of the pull request author.</param>
    /// <param name="startAt">Optional: The start date for filtering pull requests.</param>
    /// <param name="endAt">Optional: The end date for filtering pull requests.</param>
    /// <param name="cancellationToken">Optional: Cancellation token.</param>
    /// <returns>A list of non-approved pull requests across all repositories.</returns>
    ValueTask<List<PullRequest>> GetAllNonApprovedForOwner(string owner, string? username = null, DateTime? startAt = null, DateTime? endAt = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether a pull request has been approved.
    /// </summary>
    /// <param name="owner">The owner of the repository.</param>
    /// <param name="repo">The name of the repository.</param>
    /// <param name="pullRequestNumber">The number of the pull request.</param>
    /// <param name="cancellationToken">Optional: Cancellation token.</param>
    /// <returns>True if the pull request is approved; otherwise, false.</returns>
    ValueTask<bool> IsApproved(string owner, string repo, int pullRequestNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Approves all pull requests for a specific repository.
    /// </summary>
    /// <param name="repository">The repository containing the pull requests.</param>
    /// <param name="message">The approval message.</param>
    /// <param name="username">Optional: The username of the pull request author.</param>
    /// <param name="startAt">Optional: The start date for filtering pull requests.</param>
    /// <param name="endAt">Optional: The end date for filtering pull requests.</param>
    /// <param name="delayMs">Optional: Delay in milliseconds between approvals.</param>
    /// <param name="cancellationToken">Optional: Cancellation token.</param>
    ValueTask ApproveAll(Repository repository, string message, string? username = null, DateTime? startAt = null, DateTime? endAt = null, int delayMs = 0,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Approves a specific pull request.
    /// </summary>
    /// <param name="repository">The repository containing the pull request.</param>
    /// <param name="pullRequest">The pull request to approve.</param>
    /// <param name="message">The approval message.</param>
    /// <param name="cancellationToken">Optional: Cancellation token.</param>
    ValueTask Approve(Repository repository, PullRequest pullRequest, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Filters repositories to include only those with open pull requests.
    /// </summary>
    /// <param name="repositories">The list of repositories to filter.</param>
    /// <param name="startAt">Optional: The start date for filtering pull requests.</param>
    /// <param name="endAt">Optional: The end date for filtering pull requests.</param>
    /// <param name="log">Optional: Whether to log the operation.</param>
    /// <param name="cancellationToken">Optional: Cancellation token.</param>
    /// <returns>A list of repositories with open pull requests.</returns>
    ValueTask<IReadOnlyList<Repository>> FilterRepositoriesWithOpenPullRequests(IReadOnlyList<Repository> repositories, DateTime? startAt = null, DateTime? endAt = null, bool log = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Filters repositories to include only those with failed builds on open pull requests.
    /// </summary>
    /// <param name="repositories">The list of repositories to filter.</param>
    /// <param name="startAt">Optional: The start date for filtering pull requests.</param>
    /// <param name="endAt">Optional: The end date for filtering pull requests.</param>
    /// <param name="log">Optional: Whether to log the operation.</param>
    /// <param name="cancellationToken">Optional: Cancellation token.</param>
    /// <returns>A list of repositories with failed builds on open pull requests.</returns>
    ValueTask<IReadOnlyList<Repository>> FilterRepositoriesWithFailedBuilds(IReadOnlyList<Repository> repositories, DateTime? startAt = null, DateTime? endAt = null, bool log = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all repositories with failed builds on open pull requests for a specified owner.
    /// </summary>
    /// <param name="username">The username of the repository owner.</param>
    /// <param name="startAt">Optional: The start date for filtering pull requests.</param>
    /// <param name="endAt">Optional: The end date for filtering pull requests.</param>
    /// <param name="log">Optional: Whether to log the operation.</param>
    /// <param name="cancellationToken">Optional: Cancellation token.</param>
    /// <returns>A list of repositories with failed builds on open pull requests.</returns>
    ValueTask<IReadOnlyList<Repository>> GetAllRepositoriesWithFailedBuildsOnOpenPullRequests(string username, DateTime? startAt = null, DateTime? endAt = null, bool log = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all repositories with open pull requests for a specified owner.
    /// </summary>
    /// <param name="owner">The username of the repository owner.</param>
    /// <param name="startAt">Optional: The start date for filtering pull requests.</param>
    /// <param name="endAt">Optional: The end date for filtering pull requests.</param>
    /// <param name="log">Optional: Whether to log the operation.</param>
    /// <param name="cancellationToken">Optional: Cancellation token.</param>
    /// <returns>A list of repositories with open pull requests.</returns>
    ValueTask<IReadOnlyList<Repository>> GetAllRepositoriesWithOpenPullRequests(string owner, DateTime? startAt = null, DateTime? endAt = null, bool log = true,
        CancellationToken cancellationToken = default);
}