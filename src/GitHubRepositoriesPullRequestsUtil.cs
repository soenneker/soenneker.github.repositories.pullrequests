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

namespace Soenneker.GitHub.Repositories.PullRequests;

/// <inheritdoc cref="IGitHubRepositoriesPullRequestsUtil"/>
public class GitHubRepositoriesPullRequestsUtil : IGitHubRepositoriesPullRequestsUtil
{
    private readonly ILogger<GitHubRepositoriesPullRequestsUtil> _logger;
    private readonly IGitHubClientUtil _gitHubClientUtil;

    public GitHubRepositoriesPullRequestsUtil(ILogger<GitHubRepositoriesPullRequestsUtil> logger, IGitHubClientUtil gitHubClientUtil)
    {
        _logger = logger;
        _gitHubClientUtil = gitHubClientUtil;
    }

    public ValueTask<IReadOnlyList<PullRequest>> GetAll(Repository repository, string? username = null, CancellationToken cancellationToken = default)
    {
        return GetAll(repository.Owner.Login, repository.Name, username, cancellationToken);
    }

    public async ValueTask<IReadOnlyList<PullRequest>> GetAll(string owner, string name, string? username = null, CancellationToken cancellationToken = default)
    {
        GitHubClient client = await _gitHubClientUtil.Get(cancellationToken).NoSync();

        IReadOnlyList<PullRequest>? pullRequests = await client.PullRequest.GetAllForRepository(owner, name, new PullRequestRequest {State = ItemStateFilter.Open}).NoSync();
        return username == null ? pullRequests : pullRequests.Where(pr => pr.User.Login == username).ToList();
    }

    public ValueTask ApproveAll(Repository repository, string message, string? username = null, int delayMs = 0, CancellationToken cancellationToken = default)
    {
        return ApproveAll(repository.Owner.Login, repository.Name, message, username, delayMs, cancellationToken);
    }

    public async ValueTask ApproveAll(string owner, string name, string message, string? username = null, int delayMs = 0, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<PullRequest> pullRequests = await GetAll(owner, name, username, cancellationToken).NoSync();

        if (!pullRequests.Any())
            return;

        _logger.LogInformation("-- {repo} has {count} PRs open --", name, pullRequests.Count);

        for (var i = 0; i < pullRequests.Count; i++)
        {
            PullRequest pr = pullRequests[i];
            await Approve(owner, name, pr, message, cancellationToken).NoSync();

            if (delayMs > 0)
                await Task.Delay(delayMs, cancellationToken);
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

    public async ValueTask<IReadOnlyList<Repository>> FilterRepositoriesWithOpenPullRequests(IReadOnlyList<Repository> repositories, CancellationToken cancellationToken)
    {
        var result = new List<Repository>();

        foreach (Repository repository in repositories)
        {
            IReadOnlyList<PullRequest> pullRequests = await GetAll(repository, cancellationToken: cancellationToken).NoSync();

            if (!pullRequests.Any())
                continue;

            _logger.LogInformation("-- {repo} has {count} PRs open --", repository.Name, pullRequests.Count);
            result.Add(repository);
        }

        return result;
    }
}