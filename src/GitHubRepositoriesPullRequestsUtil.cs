using Microsoft.Extensions.Logging;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;
using Soenneker.GitHub.ClientUtil.Abstract;
using Soenneker.GitHub.OpenApiClient;
using Soenneker.GitHub.OpenApiClient.Models;
using Soenneker.GitHub.OpenApiClient.Repos.Item.Item.Pulls.Item.Reviews;
using Soenneker.GitHub.Repositories.Abstract;
using Soenneker.GitHub.Repositories.PullRequests.Abstract;
using Soenneker.GitHub.Repositories.Runs.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Utils.Delay;
using Repository = Soenneker.GitHub.OpenApiClient.Models.Repository;

namespace Soenneker.GitHub.Repositories.PullRequests;

///<inheritdoc cref="IGitHubRepositoriesPullRequestsUtil"/>
public sealed class GitHubRepositoriesPullRequestsUtil : IGitHubRepositoriesPullRequestsUtil
{
    private readonly ILogger<GitHubRepositoriesPullRequestsUtil> _logger;
    private readonly IGitHubOpenApiClientUtil _gitHubOpenApiClientUtil;
    private readonly IGitHubRepositoriesUtil _gitHubRepositoriesUtil;
    private readonly IGitHubRepositoriesRunsUtil _gitHubRepositoriesRunsUtil;

    public GitHubRepositoriesPullRequestsUtil(ILogger<GitHubRepositoriesPullRequestsUtil> logger, IGitHubOpenApiClientUtil gitHubOpenApiClientUtil,
        IGitHubRepositoriesUtil gitHubRepositoriesUtil, IGitHubRepositoriesRunsUtil gitHubRepositoriesRunsUtil)
    {
        _logger = logger;
        _gitHubOpenApiClientUtil = gitHubOpenApiClientUtil;
        _gitHubRepositoriesUtil = gitHubRepositoriesUtil;
        _gitHubRepositoriesRunsUtil = gitHubRepositoriesRunsUtil;
    }

    private static List<Repository> ConvertToRepositories(List<MinimalRepository> minimalRepositories)
    {
        return minimalRepositories.Select(r => new Repository
                                  {
                                      Name = r.Name,
                                      FullName = r.FullName,
                                      Owner = r.Owner,
                                      Private = r.Private,
                                      Description = r.Description,
                                      Fork = r.Fork,
                                      CreatedAt = r.CreatedAt,
                                      UpdatedAt = r.UpdatedAt,
                                      PushedAt = r.PushedAt,
                                      DefaultBranch = r.DefaultBranch,
                                      Language = r.Language,
                                      Visibility = r.Visibility,
                                      ForksCount = r.ForksCount,
                                      StargazersCount = r.StargazersCount,
                                      WatchersCount = r.WatchersCount,
                                      OpenIssuesCount = r.OpenIssuesCount,
                                      Topics = r.Topics,
                                      Archived = r.Archived,
                                      Disabled = r.Disabled,
                                      AllowForking = r.AllowForking,
                                      IsTemplate = r.IsTemplate,
                                      WebCommitSignoffRequired = r.WebCommitSignoffRequired
                                  })
                                  .ToList();
    }

    public ValueTask<List<PullRequest>> GetAll(Repository repository, string? username = null, DateTime? startAt = null, DateTime? endAt = null,
        bool log = true, CancellationToken cancellationToken = default)
    {
        return GetAll(repository.Owner.Login, repository.Name, username, startAt, endAt, log, cancellationToken);
    }

    public async ValueTask<List<PullRequest>> GetAllForOwner(string owner, string? username = null, DateTime? startAt = null, DateTime? endAt = null,
        bool log = false, CancellationToken cancellationToken = default)
    {
        if (log)
            _logger.LogInformation("Getting all PRs for owner {owner}...", owner);

        List<MinimalRepository> minimalRepositories = await _gitHubRepositoriesUtil.GetAllForOwner(owner, startAt, endAt, cancellationToken).NoSync();
        List<Repository> repositories = ConvertToRepositories(minimalRepositories);

        var allPullRequests = new List<PullRequest>();
        Dictionary<Repository, List<PullRequest>> pullRequestsByRepo =
            await GetPullRequestsForRepositories(repositories, username, startAt, endAt, cancellationToken).NoSync();

        foreach ((Repository _, List<PullRequest> prs) in pullRequestsByRepo)
        {
            allPullRequests.AddRange(prs);
        }

        if (log)
            _logger.LogInformation("Found {count} PRs for owner {owner}", allPullRequests.Count, owner);

        return allPullRequests;
    }

    public async ValueTask<List<PullRequest>> GetAll(string owner, string name, string? username = null, DateTime? startAt = null, DateTime? endAt = null,
        bool log = true, CancellationToken cancellationToken = default)
    {
        if (log)
            _logger.LogInformation("Getting all PRs for {owner}/{name}...", owner, name);

        GitHubOpenApiClient client = await _gitHubOpenApiClientUtil.Get(cancellationToken).NoSync();

        var allPullRequests = new List<PullRequest>();
        var page = 1;

        while (true)
        {
            List<PullRequestSimple>? pullRequests = await client.Repos[owner][name]
                                                                .Pulls.GetAsync(requestConfiguration =>
                                                                {
                                                                    requestConfiguration.QueryParameters.Page = page;
                                                                    requestConfiguration.QueryParameters.State = "open";
                                                                }, cancellationToken)
                                                                .NoSync();

            if (pullRequests == null || pullRequests.Count == 0)
                break;

            foreach (PullRequestSimple pr in pullRequests)
            {
                if (username != null && pr.User?.Login != username)
                    continue;

                if (startAt != null && pr.CreatedAt < startAt)
                    continue;

                if (endAt != null && pr.CreatedAt > endAt)
                    continue;

                allPullRequests.Add(new PullRequest
                {
                    Number = pr.Number,
                    Title = pr.Title,
                    CreatedAt = pr.CreatedAt,
                    User = pr.User
                });
            }

            page++;
        }

        if (log)
            _logger.LogInformation("Found {count} PRs for {owner}/{name}", allPullRequests.Count, owner, name);

        return allPullRequests;
    }


    private async ValueTask<Dictionary<Repository, List<PullRequest>>> GetPullRequestsForRepositories(IEnumerable<Repository> repositories, string? username,
        DateTime? startAt, DateTime? endAt, CancellationToken cancellationToken)
    {
        var dict = new Dictionary<Repository, List<PullRequest>>();

        foreach (Repository repo in repositories)
        {
            List<PullRequest> prs = await GetAll(repo, username, startAt, endAt, false, cancellationToken).NoSync();
            dict[repo] = prs;
        }

        return dict;
    }

    public async ValueTask<List<Repository>> FilterRepositoriesWithOpenPullRequests(List<Repository> repositories, DateTime? startAt = null,
        DateTime? endAt = null, bool log = true, CancellationToken cancellationToken = default)
    {
        var result = new List<Repository>();
        Dictionary<Repository, List<PullRequest>> pullRequestsByRepo =
            await GetPullRequestsForRepositories(repositories, null, startAt, endAt, cancellationToken).NoSync();

        foreach ((Repository repo, List<PullRequest> prs) in pullRequestsByRepo)
        {
            if (prs.Count > 0)
            {
                if (log)
                    _logger.LogInformation("-- {repo} has {count} PRs open --", repo.Name, prs.Count);

                result.Add(repo);
            }
        }

        return result;
    }

    public async ValueTask<List<Repository>> FilterRepositoriesWithFailedBuilds(List<Repository> repositories, DateTime? startAt = null, DateTime? endAt = null,
        bool log = true, CancellationToken cancellationToken = default)
    {
        var result = new List<Repository>();
        Dictionary<Repository, List<PullRequest>> pullRequestsByRepo =
            await GetPullRequestsForRepositories(repositories, null, startAt, endAt, cancellationToken).NoSync();

        foreach ((Repository repo, List<PullRequest> prs) in pullRequestsByRepo)
        {
            foreach (PullRequest pr in prs)
            {
                if (await _gitHubRepositoriesRunsUtil.HasFailedRun(repo, pr, cancellationToken).NoSync())
                {
                    if (log)
                        _logger.LogInformation("Repository ({repo}) has a PR ({title}) with a failed build", repo.FullName, pr.Title);

                    result.Add(repo);
                    break;
                }
            }
        }

        return result;
    }

    public async ValueTask<List<Repository>> GetAllRepositoriesWithFailedBuildsOnOpenPullRequests(string owner, DateTime? startAt = null,
        DateTime? endAt = null, bool log = true, CancellationToken cancellationToken = default)
    {
        if (log)
            _logger.LogInformation("Getting all repositories with failed builds on open PRs for owner {owner}...", owner);

        List<MinimalRepository> minimalRepositories = await _gitHubRepositoriesUtil.GetAllForOwner(owner, startAt, endAt, cancellationToken).NoSync();
        List<Repository> repositories = ConvertToRepositories(minimalRepositories);

        List<Repository> result = await FilterRepositoriesWithFailedBuilds(repositories, startAt, endAt, log, cancellationToken).NoSync();

        if (log)
            _logger.LogInformation("Found {count} repositories with failed builds on open PRs for owner {owner}", result.Count, owner);

        return result;
    }

    public async ValueTask<List<Repository>> GetAllRepositoriesWithOpenPullRequests(string owner, DateTime? startAt = null, DateTime? endAt = null,
        bool log = true, CancellationToken cancellationToken = default)
    {
        if (log)
            _logger.LogInformation("Getting all repositories with open PRs for owner {owner}...", owner);

        List<MinimalRepository> minimalRepositories = await _gitHubRepositoriesUtil.GetAllForOwner(owner, startAt, endAt, cancellationToken).NoSync();
        List<Repository> repositories = ConvertToRepositories(minimalRepositories);

        List<Repository> result = await FilterRepositoriesWithOpenPullRequests(repositories, startAt, endAt, log, cancellationToken).NoSync();

        if (log)
            _logger.LogInformation("Found {count} repositories with open PRs for owner {owner}", result.Count, owner);

        return result;
    }

    public async ValueTask<bool> IsApproved(string owner, string repo, int pullRequestNumber, CancellationToken cancellationToken = default)
    {
        GitHubOpenApiClient client = await _gitHubOpenApiClientUtil.Get(cancellationToken).NoSync();

        List<PullRequestReview>? reviews =
            await client.Repos[owner][repo].Pulls[pullRequestNumber].Reviews.GetAsync(cancellationToken: cancellationToken).NoSync();

        return reviews?.Any(r => r.State == "APPROVED") == true;
    }

    public async ValueTask<List<PullRequest>> GetAllNonApproved(string owner, string name, string? username = null, DateTime? startAt = null,
        DateTime? endAt = null, bool log = true, CancellationToken cancellationToken = default)
    {
        if (log)
            _logger.LogInformation("Getting all non-approved PRs for {owner}/{name}...", owner, name);

        List<PullRequest> pullRequests = await GetAll(owner, name, username, startAt, endAt, false, cancellationToken).NoSync();

        var result = new List<PullRequest>();

        foreach (PullRequest pr in pullRequests)
        {
            if (!await IsApproved(owner, name, pr.Number ?? 0, cancellationToken).NoSync())
                result.Add(pr);
        }

        if (log)
            _logger.LogInformation("Found {count} non-approved PRs for {owner}/{name}", result.Count, owner, name);

        return result;
    }

    public async ValueTask<List<PullRequest>> GetAllNonApprovedForOwner(string owner, string? username = null, DateTime? startAt = null, DateTime? endAt = null,
        bool log = true, CancellationToken cancellationToken = default)
    {
        if (log)
            _logger.LogInformation("Getting all non-approved PRs for owner {owner}...", owner);

        List<MinimalRepository> repos = await _gitHubRepositoriesUtil.GetAllForOwner(owner, startAt, endAt, cancellationToken).NoSync();
        var result = new List<PullRequest>();

        foreach (MinimalRepository repo in repos)
        {
            List<PullRequest> prs = await GetAllNonApproved(repo.Owner.Login, repo.Name, username, startAt, endAt, false, cancellationToken).NoSync();
            result.AddRange(prs);
        }

        if (log)
            _logger.LogInformation("Found {count} non-approved PRs for owner {owner}", result.Count, owner);

        return result;
    }

    public ValueTask Approve(Repository repository, PullRequest pullRequest, string message, CancellationToken cancellationToken = default)
    {
        return Approve(repository.Owner.Login, repository.Name, pullRequest, message, cancellationToken);
    }

    public async ValueTask Approve(string owner, string name, PullRequest pullRequest, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Approving PR #{number} ({message})...", pullRequest.Number, message);

        GitHubOpenApiClient client = await _gitHubOpenApiClientUtil.Get(cancellationToken).NoSync();

        var review = new ReviewsPostRequestBody
        {
            Body = message,
            Event = ReviewsPostRequestBody_event.APPROVE
        };

        await client.Repos[owner][name].Pulls[pullRequest.Number ?? 0].Reviews.PostAsync(review, cancellationToken: cancellationToken).NoSync();

        _logger.LogInformation("Approved PR #{number} ({message})", pullRequest.Number, message);
    }

    public async ValueTask ApproveAll(string owner, string name, string message, DateTime? startAt = null, DateTime? endAt = null, string? username = null,
        int delayMs = 0, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Approving all PRs for {owner}/{name}...", owner, name);

        List<PullRequest> pullRequests = await GetAllNonApproved(owner, name, username, startAt, endAt, false, cancellationToken).NoSync();

        foreach (PullRequest pr in pullRequests)
        {
            await Approve(owner, name, pr, message, cancellationToken).NoSync();

            if (delayMs > 0)
                await DelayUtil.Delay(delayMs, _logger, cancellationToken).NoSync();
        }

        _logger.LogInformation("Approved all PRs for {owner}/{name}", owner, name);
    }

    public async ValueTask ApproveAll(Repository repository, string message, string? username = null, DateTime? startAt = null, DateTime? endAt = null,
        int delayMs = 0, CancellationToken cancellationToken = default)
    {
        await ApproveAll(repository.Owner.Login, repository.Name, message, startAt, endAt, username, delayMs, cancellationToken).NoSync();
    }
}