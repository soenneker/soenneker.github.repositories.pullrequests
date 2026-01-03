using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.GitHub.OpenApiClient.Models;

namespace Soenneker.GitHub.Repositories.PullRequests.Abstract;

/// <summary>
/// Utility for interacting with GitHub pull requests in repositories
/// </summary>
public interface IGitHubRepositoriesPullRequestsUtil
{
    /// <summary>
    /// Gets all pull requests for a specific repository.
    /// </summary>
    ValueTask<List<PullRequest>> GetAll(Repository repository, string? username = null, DateTimeOffset? startAt = null, DateTimeOffset? endAt = null, bool log = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all pull requests for a specific repository by owner and name.
    /// </summary>
    ValueTask<List<PullRequest>> GetAll(string owner, string name, string? username = null, DateTimeOffset? startAt = null, DateTimeOffset? endAt = null, bool log = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all pull requests for a specific owner across all their repositories.
    /// </summary>
    ValueTask<List<PullRequest>> GetAllForOwner(string owner, string? username = null, DateTimeOffset? startAt = null, DateTimeOffset? endAt = null, bool log = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all non-approved pull requests for a specific repository.
    /// </summary>
    ValueTask<List<PullRequest>> GetAllNonApproved(string owner, string name, string? username = null, DateTimeOffset? startAt = null, DateTimeOffset? endAt = null, bool log = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all non-approved pull requests across all repositories for a specific owner.
    /// </summary>
    ValueTask<List<PullRequest>> GetAllNonApprovedForOwner(string owner, string? username = null, DateTimeOffset? startAt = null, DateTimeOffset? endAt = null, bool log = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether a pull request is approved.
    /// </summary>
    ValueTask<bool> IsApproved(string owner, string repo, int pullRequestNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Approves all non-approved pull requests for a specific repository.
    /// </summary>
    ValueTask ApproveAll(Repository repository, string message, string? username = null, DateTimeOffset? startAt = null, DateTimeOffset? endAt = null, int delayMs = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// Approves all non-approved pull requests by owner and repo name.
    /// </summary>
    ValueTask ApproveAll(string owner, string name, string message, DateTimeOffset? startAt = null, DateTimeOffset? endAt = null, string? username = null, int delayMs = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// Approves a specific pull request.
    /// </summary>
    ValueTask Approve(Repository repository, PullRequest pullRequest, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Approves a specific pull request by owner and repo name.
    /// </summary>
    ValueTask Approve(string owner, string name, PullRequest pullRequest, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Filters a list of repositories to only those with open pull requests.
    /// </summary>
    ValueTask<List<Repository>> FilterRepositoriesWithOpenPullRequests(List<Repository> repositories, DateTimeOffset? startAt = null, DateTimeOffset? endAt = null, bool log = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Filters a list of repositories to only those with failed CI builds on any pull request.
    /// </summary>
    ValueTask<List<Repository>> FilterRepositoriesWithFailedBuilds(List<Repository> repositories, DateTimeOffset? startAt = null, DateTimeOffset? endAt = null, bool log = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all repositories for a given owner with at least one open pull request that has a failed build.
    /// </summary>
    ValueTask<List<Repository>> GetAllRepositoriesWithFailedBuildsOnOpenPullRequests(string owner, DateTimeOffset? startAt = null, DateTimeOffset? endAt = null, bool log = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all repositories for a given owner with at least one open pull request.
    /// </summary>
    ValueTask<List<Repository>> GetAllRepositoriesWithOpenPullRequests(string owner, DateTimeOffset? startAt = null, DateTimeOffset? endAt = null, bool log = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Merges a specific pull request.
    /// </summary>
    ValueTask Merge(string owner, string name, PullRequest pullRequest, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Merges all pull requests for a specific repository.
    /// </summary>
    ValueTask MergeAll(string owner, string name, string message, DateTimeOffset? startAt = null, DateTimeOffset? endAt = null, string? username = null, int delayMs = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// Merges all pull requests with passing checks for a specific repository.
    /// </summary>
    ValueTask MergeAllWithPassingChecks(string owner, string name, string message, DateTimeOffset? startAt = null, DateTimeOffset? endAt = null, string? username = null, int delayMs = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// Incrementally merges all pull requests for an owner across all their repositories.
    /// Gets all repositories, shuffles them, and for each repository merges all open PRs that can be merged.
    /// This approach helps avoid rate limiting by merging incrementally rather than batching all PRs first.
    /// </summary>
    ValueTask MergeForOwnerIncrementally(string owner, string message, string? username = null, DateTimeOffset? startAt = null, DateTimeOffset? endAt = null, bool checkForPassingChecks = true, int delayMs = 0, bool log = true, CancellationToken cancellationToken = default);

    ValueTask<bool> HasFailedRunOnOpenPullRequests(string owner, string name, bool log, CancellationToken cancellationToken);
}