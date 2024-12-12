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

/// <inheritdoc cref="IGitHubRepositoryPullRequestsUtil"/>
public class GitHubRepositoryPullRequestsUtil : IGitHubRepositoryPullRequestsUtil
{
    private readonly ILogger<GitHubRepositoryPullRequestsUtil> _logger;
    private readonly IGitHubClientUtil _gitHubClientUtil;

    public GitHubRepositoryPullRequestsUtil(ILogger<GitHubRepositoryPullRequestsUtil> logger, IGitHubClientUtil gitHubClientUtil)
    {
        _logger = logger;
        _gitHubClientUtil = gitHubClientUtil;
    }

    public async ValueTask<IReadOnlyList<PullRequest>> GetPullRequests(Repository repository, string? username = null, CancellationToken cancellationToken = default)
    {
        return await GetPullRequests(repository.Owner.Login, repository.Name, username, cancellationToken).NoSync();
    }

    public async ValueTask<IReadOnlyList<PullRequest>> GetPullRequests(string owner, string name, string? username = null, CancellationToken cancellationToken = default)
    {
        GitHubClient client = await _gitHubClientUtil.Get(cancellationToken).NoSync();

        IReadOnlyList<PullRequest>? pullRequests = await client.PullRequest.GetAllForRepository(owner, name, new PullRequestRequest {State = ItemStateFilter.Open}).NoSync();
        return username == null ? pullRequests : pullRequests.Where(pr => pr.User.Login == username).ToList();
    }

    public ValueTask ApproveAllPullRequests(Repository repository, string message, string? username = null, CancellationToken cancellationToken = default)
    {
        return ApproveAllPullRequests(repository.Owner.Login, repository.Name, message, username, cancellationToken);
    }

    public async ValueTask ApproveAllPullRequests(string owner, string name, string message, string? username = null, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<PullRequest> pullRequests = await GetPullRequests(owner, name, username, cancellationToken).NoSync();

        if (!pullRequests.Any())
            return;

        _logger.LogInformation("-- {repo} has {count} PRs open --", name, pullRequests.Count);

        for (var i = 0; i < pullRequests.Count; i++)
        {
            PullRequest pr = pullRequests[i];
            await ApprovePullRequest(owner, name, pr, message, cancellationToken).NoSync();
        }
    }

    public ValueTask ApprovePullRequest(Repository repository, PullRequest pullRequest, string message, CancellationToken cancellationToken = default)
    {
        return ApprovePullRequest(repository.Owner.Login, repository.Name, pullRequest, message, cancellationToken);
    }

    public async ValueTask ApprovePullRequest(string owner, string name, PullRequest pullRequest, string message, CancellationToken cancellationToken = default)
    {
        var review = new PullRequestReviewCreate
        {
            Event = PullRequestReviewEvent.Approve,
            Body = message
        };

        GitHubClient client = await _gitHubClientUtil.Get(cancellationToken).NoSync();

        await client.PullRequest.Review.Create(owner, name, pullRequest.Number, review).NoSync();

        _logger.LogInformation($"Approved PR #{pullRequest.Number}");

        await Task.Delay(1000, cancellationToken);
    }
}