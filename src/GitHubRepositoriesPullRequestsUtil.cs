using Octokit;
using Soenneker.GitHub.Repositories.PullRequests.Abstract;
using System.Threading.Tasks;
using System.Threading;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;
using Microsoft.Extensions.Logging;
using Soenneker.GitHub.Client.Abstract;
using System.Collections.Generic;
using System.Linq;
using Soenneker.GitHub.Repositories.Abstract;
using Soenneker.GitHub.Repositories.Runs.Abstract;
using System;
using Soenneker.Extensions.DateTime;
using Soenneker.Extensions.DateTimeOffset;
using System.Xml.Linq;

namespace Soenneker.GitHub.Repositories.PullRequests;

/// <inheritdoc cref="IGitHubRepositoriesPullRequestsUtil"/>
public class GitHubRepositoriesPullRequestsUtil : IGitHubRepositoriesPullRequestsUtil
{
    private readonly ILogger<GitHubRepositoriesPullRequestsUtil> _logger;
    private readonly IGitHubClientUtil _gitHubClientUtil;
    private readonly IGitHubRepositoriesUtil _gitHubRepositoriesUtil;
    private readonly IGitHubRepositoriesRunsUtil _gitHubRepositoriesRunsUtil;

    public GitHubRepositoriesPullRequestsUtil(ILogger<GitHubRepositoriesPullRequestsUtil> logger, IGitHubClientUtil gitHubClientUtil, IGitHubRepositoriesUtil gitHubRepositoriesUtil,
        IGitHubRepositoriesRunsUtil gitHubRepositoriesRunsUtil)
    {
        _logger = logger;
        _gitHubClientUtil = gitHubClientUtil;
        _gitHubRepositoriesUtil = gitHubRepositoriesUtil;
        _gitHubRepositoriesRunsUtil = gitHubRepositoriesRunsUtil;
    }

    public ValueTask<IReadOnlyList<PullRequest>> GetAll(Repository repository, bool log = true, string? username = null, CancellationToken cancellationToken = default)
    {
        return GetAll(repository.Owner.Login, repository.Name, log, username, cancellationToken);
    }

    public async ValueTask<IReadOnlyList<PullRequest>> GetAll(string owner, string name, bool log = true, string? username = null, CancellationToken cancellationToken = default)
    {
        GitHubClient client = await _gitHubClientUtil.Get(cancellationToken).NoSync();

        if (log)
            _logger.LogDebug("Getting all PRs for {owner}/{name}...", owner, name);

        var allPullRequests = new List<PullRequest>();
        var page = 1;
        const int pageSize = 100; // GitHub's maximum per-page limit

        while (true)
        {
            IReadOnlyList<PullRequest> pullRequests = await client.PullRequest.GetAllForRepository(
                owner,
                name,
                new PullRequestRequest { State = ItemStateFilter.Open },
                new ApiOptions
                {
                    PageSize = pageSize,
                    PageCount = 1,
                    StartPage = page
                }
            ).NoSync();

            if (pullRequests.Count == 0)
            {
                break; // Exit the loop if no more results
            }

            allPullRequests.AddRange(pullRequests);

            if (pullRequests.Count < pageSize)
            {
                break; // Exit the loop if this was the last page
            }

            page++;
        }

        return username == null
            ? allPullRequests
            : allPullRequests.Where(pr => pr.User.Login == username).ToList();
    }

    public async ValueTask<List<PullRequest>> GetAllBetween(string owner, string name, DateTime startAt, DateTime endAt, bool log = true, string? username = null, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<PullRequest> pullRequests = await GetAll(owner, name, log, username, cancellationToken).NoSync();

        var result = new List<PullRequest>();

        foreach (PullRequest pullRequest in pullRequests)
        {
            if (pullRequest.CreatedAt.ToUtcDateTime().IsBetween(startAt, endAt))
            {
                result.Add(pullRequest);
            }
        }

        return result;
    }

    public async ValueTask<List<PullRequest>> GetAllForOwner(string owner, string? username = null, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Repository> allRepos = await _gitHubRepositoriesUtil.GetAllForOwner(owner, cancellationToken);

        _logger.LogDebug("Getting all PRs for {owner}...", owner);

        var allPullRequests = new List<PullRequest>();

        foreach (Repository repo in allRepos)
        {
            IReadOnlyList<PullRequest> pullRequests = await GetAll(repo, false, username, cancellationToken);

            allPullRequests.AddRange(pullRequests);
        }

        return allPullRequests;
    }

    public async ValueTask<List<PullRequest>> GetAllForOwnerBetween(string owner, DateTime startAt, DateTime endAt, string? username = null, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Repository> allRepos = await _gitHubRepositoriesUtil.GetAllForOwner(owner, cancellationToken);

        List<Repository> filteredRepos = allRepos.Where(c => c.CreatedAt.ToUtcDateTime().IsBetween(startAt, endAt)).ToList();

        var result = new List<PullRequest>();

        foreach (Repository repo in filteredRepos)
        {
            IReadOnlyList<PullRequest> pullRequests = await GetAllBetween(owner, repo.Name, startAt, endAt, false, username, cancellationToken);

            result.AddRange(pullRequests);
        }

        return result;
    }

    public ValueTask ApproveAll(Repository repository, string message, string? username = null, int delayMs = 0, CancellationToken cancellationToken = default)
    {
        return ApproveAll(repository.Owner.Login, repository.Name, message, username, delayMs, cancellationToken);
    }

    public async ValueTask ApproveAll(string owner, string name, string message, string? username = null, int delayMs = 0, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<PullRequest> pullRequests = await GetAll(owner, name, true, username, cancellationToken).NoSync();

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
        var review = new PullRequestReviewCreate
        {
            Event = PullRequestReviewEvent.Approve,
            Body = message
        };

        GitHubClient client = await _gitHubClientUtil.Get(cancellationToken).NoSync();

        await client.PullRequest.Review.Create(owner, name, pullRequest.Number, review).NoSync();

        _logger.LogInformation("Approved PR #{number} ({message})", pullRequest.Number, message);
    }

    public async ValueTask<IReadOnlyList<Repository>> FilterRepositoriesWithOpenPullRequests(IReadOnlyList<Repository> repositories, bool log = true, CancellationToken cancellationToken = default)
    {
        var result = new List<Repository>();

        foreach (Repository repository in repositories)
        {
            IReadOnlyList<PullRequest> pullRequests = await GetAll(repository, false, cancellationToken: cancellationToken).NoSync();

            if (!pullRequests.Any())
                continue;

            if (log)
                _logger.LogInformation("-- {repo} has {count} PRs open --", repository.Name, pullRequests.Count);

            result.Add(repository);
        }

        return result;
    }

    public async ValueTask<IReadOnlyList<Repository>> FilterRepositoriesWithFailedBuilds(IReadOnlyList<Repository> repositories, bool log = true, CancellationToken cancellationToken = default)
    {
        var result = new List<Repository>();

        foreach (Repository repository in repositories)
        {
            IReadOnlyList<PullRequest> pullRequests = await GetAll(repository, false, cancellationToken: cancellationToken).NoSync();

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

    public async ValueTask<IReadOnlyList<Repository>> GetAllRepositoriesWithFailedBuildsOnOpenPullRequests(string username, bool log = true, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Repository> repositories = await _gitHubRepositoriesUtil.GetAllForOwner(username, cancellationToken).NoSync();
        return await FilterRepositoriesWithFailedBuilds(repositories, log, cancellationToken).NoSync();
    }

    public async ValueTask<IReadOnlyList<Repository>> GetAllRepositoriesWithOpenPullRequests(string owner, bool log = true, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Repository> repositories = await _gitHubRepositoriesUtil.GetAllForOwner(owner, cancellationToken).NoSync();
        return await FilterRepositoriesWithOpenPullRequests(repositories, log, cancellationToken).NoSync();
    }
}