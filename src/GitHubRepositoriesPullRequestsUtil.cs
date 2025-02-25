using Microsoft.Extensions.Logging;
using Octokit;
using Soenneker.Extensions.DateTime;
using Soenneker.Extensions.DateTimeOffset;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;
using Soenneker.GitHub.Client.Abstract;
using Soenneker.GitHub.Repositories.Abstract;
using Soenneker.GitHub.Repositories.PullRequests.Abstract;
using Soenneker.GitHub.Repositories.Runs.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Soenneker.GitHub.Repositories.PullRequests;

/// <inheritdoc cref="IGitHubRepositoriesPullRequestsUtil"/>
public class GitHubRepositoriesPullRequestsUtil : IGitHubRepositoriesPullRequestsUtil
{
    private readonly ILogger<GitHubRepositoriesPullRequestsUtil> _logger;
    private readonly IGitHubClientUtil _gitHubClientUtil;
    private readonly IGitHubRepositoriesUtil _gitHubRepositoriesUtil;
    private readonly IGitHubRepositoriesRunsUtil _gitHubRepositoriesRunsUtil;

    public GitHubRepositoriesPullRequestsUtil(ILogger<GitHubRepositoriesPullRequestsUtil> logger, IGitHubClientUtil gitHubClientUtil,
        IGitHubRepositoriesUtil gitHubRepositoriesUtil, IGitHubRepositoriesRunsUtil gitHubRepositoriesRunsUtil)
    {
        _logger = logger;
        _gitHubClientUtil = gitHubClientUtil;
        _gitHubRepositoriesUtil = gitHubRepositoriesUtil;
        _gitHubRepositoriesRunsUtil = gitHubRepositoriesRunsUtil;
    }

    public ValueTask<IReadOnlyList<PullRequest>> GetAll(Repository repository, string? username = null, DateTime? startAt = null, DateTime? endAt = null,
        bool log = true, CancellationToken cancellationToken = default)
    {
        return GetAll(repository.Owner.Login, repository.Name, username, startAt, endAt, log, cancellationToken);
    }

    public async ValueTask<IReadOnlyList<PullRequest>> GetAll(string owner, string name, string? username = null, DateTime? startAt = null,
        DateTime? endAt = null, bool log = true, CancellationToken cancellationToken = default)
    {
        if (log)
            _logger.LogDebug("Getting all GitHub PRs for {owner}/{name}...", owner, name);

        GitHubClient client = await _gitHubClientUtil.Get(cancellationToken).NoSync();

        var result = new List<PullRequest>();
        var page = 1;
        const int pageSize = 100; // GitHub's maximum per-page limit

        while (true)
        {
            IReadOnlyList<PullRequest> pullRequests = await client.PullRequest.GetAllForRepository(owner, name,
                                                                      new PullRequestRequest {State = ItemStateFilter.Open}, new ApiOptions
                                                                      {
                                                                          PageSize = pageSize,
                                                                          PageCount = 1,
                                                                          StartPage = page
                                                                      })
                                                                  .NoSync();

            if (pullRequests.Count == 0)
            {
                break; // Exit the loop if no more results
            }

            if (startAt == null && endAt == null)
            {
                result.AddRange(pullRequests);
            }
            else if (startAt != null && endAt == null)
            {
                foreach (PullRequest pullRequest in pullRequests)
                {
                    if (pullRequest.CreatedAt.ToUtcDateTime() >= startAt.Value)
                    {
                        if (log)
                            _logger.LogInformation("PR #{number} created at {createdAt}", pullRequest.Number, pullRequest.CreatedAt);

                        result.Add(pullRequest);
                    }
                }
            }
            else if (startAt == null && endAt != null)
            {
                foreach (PullRequest pullRequest in pullRequests)
                {
                    if (pullRequest.CreatedAt.ToUtcDateTime() <= endAt.Value)
                    {
                        if (log)
                            _logger.LogInformation("PR #{number} created at {createdAt}", pullRequest.Number, pullRequest.CreatedAt);

                        result.Add(pullRequest);
                    }
                }
            }
            else if (startAt != null && endAt != null)
            {
                foreach (PullRequest pullRequest in pullRequests)
                {
                    if (pullRequest.CreatedAt.ToUtcDateTime().IsBetween(startAt.Value, endAt.Value))
                    {
                        if (log)
                            _logger.LogInformation("PR #{number} created at {createdAt}", pullRequest.Number, pullRequest.CreatedAt);

                        result.Add(pullRequest);
                    }
                }
            }

            if (pullRequests.Count < pageSize)
            {
                break; // Exit the loop if this was the last page
            }

            page++;
        }

        return username == null ? result : result.Where(pr => pr.User.Login == username).ToList();
    }

    public async ValueTask<List<PullRequest>> GetAllForOwner(string owner, string? username = null, DateTime? startAt = null, DateTime? endAt = null,
        bool log = false, CancellationToken cancellationToken = default)
    {
        if (log)
            _logger.LogInformation("Getting all GitHub PRs for for owner ({owner})...", owner);

        IReadOnlyList<Repository> repos = await _gitHubRepositoriesUtil.GetAllForOwner(owner, null, endAt, cancellationToken);

        var allPullRequests = new List<PullRequest>();

        foreach (Repository repo in repos)
        {
            IReadOnlyList<PullRequest> pullRequests = await GetAll(repo, username, startAt, endAt, log, cancellationToken);

            allPullRequests.AddRange(pullRequests);
        }

        return allPullRequests;
    }

    public async ValueTask<List<PullRequest>> GetAllNonApproved(string owner, string name, string? username = null, DateTime? startAt = null,
        DateTime? endAt = null, bool log = true, CancellationToken cancellationToken = default)
    {
        if (log)
            _logger.LogInformation("Getting all non-approved GitHub PRs for repo ({owner}/{name})...", owner, name);

        IReadOnlyList<PullRequest> pullRequests = await GetAll(owner, name, username, startAt, endAt, log, cancellationToken).NoSync();

        var result = new List<PullRequest>();

        foreach (PullRequest pullRequest in pullRequests)
        {
            bool approved = await IsApproved(owner, name, pullRequest.Number, cancellationToken).NoSync();

            if (!approved)
                result.Add(pullRequest);
        }

        return result;
    }

    public async ValueTask<List<PullRequest>> GetAllNonApprovedForOwner(string owner, string? username = null, DateTime? startAt = null, DateTime? endAt = null,
        bool log = true, CancellationToken cancellationToken = default)
    {
        if (log)
            _logger.LogInformation("Getting all non-approved GitHub PRs for owner ({owner})...", owner);

        IReadOnlyList<Repository> repos = await _gitHubRepositoriesUtil.GetAllForOwner(owner, null, endAt, cancellationToken).NoSync();

        var result = new List<PullRequest>();

        foreach (Repository repo in repos)
        {
            IReadOnlyList<PullRequest> pullRequests = await GetAllNonApproved(owner, repo.Name, username, startAt, endAt, log, cancellationToken);

            result.AddRange(pullRequests);
        }

        return result;
    }

    public async ValueTask<bool> IsApproved(string owner, string repo, int pullRequestNumber, CancellationToken cancellationToken = default)
    {
        GitHubClient client = await _gitHubClientUtil.Get(cancellationToken).NoSync();

        IReadOnlyList<PullRequestReview> reviews = await client.PullRequest.Review.GetAll(owner, repo, pullRequestNumber).NoSync();

        return reviews.Any(review => review.State == PullRequestReviewState.Approved);
    }

    public ValueTask ApproveAll(Repository repository, string message, string? username = null, DateTime? startAt = null, DateTime? endAt = null,
        int delayMs = 0, CancellationToken cancellationToken = default)
    {
        return ApproveAll(repository.Owner.Login, repository.Name, message, startAt, endAt, username, delayMs, cancellationToken);
    }

    public async ValueTask ApproveAll(string owner, string name, string message, DateTime? startAt = null, DateTime? endAt = null, string? username = null,
        int delayMs = 0, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Approving all PRs for {owner}/{name}...", owner, name);

        IReadOnlyList<PullRequest> pullRequests = await GetAll(owner, name, username, startAt, endAt, true, cancellationToken).NoSync();

        if (!pullRequests.Any())
            return;

        _logger.LogInformation("-- {repo} has {count} PRs open --", name, pullRequests.Count);

        for (var i = 0; i < pullRequests.Count; i++)
        {
            PullRequest pr = pullRequests[i];
            await Approve(owner, name, pr, message, cancellationToken).NoSync();

            if (delayMs > 0)
                await Task.Delay(delayMs, cancellationToken).NoSync();
        }
    }

    public ValueTask Approve(Repository repository, PullRequest pullRequest, string message, CancellationToken cancellationToken = default)
    {
        return Approve(repository.Owner.Login, repository.Name, pullRequest, message, cancellationToken);
    }

    public async ValueTask Approve(string owner, string name, PullRequest pullRequest, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Approving PR #{number} ({message})...", pullRequest.Number, message);

        var review = new PullRequestReviewCreate
        {
            Event = PullRequestReviewEvent.Approve,
            Body = message
        };

        GitHubClient client = await _gitHubClientUtil.Get(cancellationToken).NoSync();

        await client.PullRequest.Review.Create(owner, name, pullRequest.Number, review).NoSync();

        _logger.LogInformation("Approved PR #{number} ({message})", pullRequest.Number, message);
    }

    public async ValueTask<IReadOnlyList<Repository>> FilterRepositoriesWithOpenPullRequests(IReadOnlyList<Repository> repositories, DateTime? startAt = null,
        DateTime? endAt = null, bool log = true, CancellationToken cancellationToken = default)
    {
        var result = new List<Repository>();

        foreach (Repository repository in repositories)
        {
            IReadOnlyList<PullRequest> pullRequests = await GetAll(repository, null, startAt, endAt, false, cancellationToken: cancellationToken).NoSync();

            if (!pullRequests.Any())
                continue;

            if (log)
                _logger.LogInformation("-- {repo} has {count} PRs open --", repository.Name, pullRequests.Count);

            result.Add(repository);
        }

        return result;
    }

    public async ValueTask<IReadOnlyList<Repository>> FilterRepositoriesWithFailedBuilds(IReadOnlyList<Repository> repositories, DateTime? startAt = null,
        DateTime? endAt = null, bool log = true, CancellationToken cancellationToken = default)
    {
        var result = new List<Repository>();

        foreach (Repository repository in repositories)
        {
            IReadOnlyList<PullRequest> pullRequests = await GetAll(repository, null, startAt, endAt, false, cancellationToken: cancellationToken).NoSync();

            foreach (PullRequest pr in pullRequests)
            {
                bool hasFailedBuild = await _gitHubRepositoriesRunsUtil.HasFailedRun(repository, pr, cancellationToken).NoSync();

                if (!hasFailedBuild)
                    continue;

                if (log)
                    _logger.LogInformation("Repository ({repo}) has a PR ({title}) with a failed build", repository.FullName, pr.Title);

                result.Add(repository);
                break;
            }
        }

        return result;
    }

    public async ValueTask<IReadOnlyList<Repository>> GetAllRepositoriesWithFailedBuildsOnOpenPullRequests(string owner, DateTime? startAt = null,
        DateTime? endAt = null, bool log = true, CancellationToken cancellationToken = default)
    {
        if (log)
            _logger.LogInformation("Getting all GitHub repos with failed builders for owner ({owner})...", owner);

        IReadOnlyList<Repository> repositories = await _gitHubRepositoriesUtil.GetAllForOwner(owner, null, endAt, cancellationToken).NoSync();
        return await FilterRepositoriesWithFailedBuilds(repositories, startAt, endAt, log, cancellationToken).NoSync();
    }

    public async ValueTask<IReadOnlyList<Repository>> GetAllRepositoriesWithOpenPullRequests(string owner, DateTime? startAt = null, DateTime? endAt = null,
        bool log = true, CancellationToken cancellationToken = default)
    {
        if (log)
            _logger.LogInformation("Getting all GitHub repos with open pull requests for owner ({owner})...", owner);

        IReadOnlyList<Repository> repositories = await _gitHubRepositoriesUtil.GetAllForOwner(owner, null, endAt, cancellationToken).NoSync();
        return await FilterRepositoriesWithOpenPullRequests(repositories, startAt, endAt, log, cancellationToken).NoSync();
    }
}