[![](https://img.shields.io/nuget/v/soenneker.github.repositories.pullrequests.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.github.repositories.pullrequests/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.github.repositories.pullrequests/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.github.repositories.pullrequests/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.github.repositories.pullrequests.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.github.repositories.pullrequests/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.github.repositories.pullrequests/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.github.repositories.pullrequests/actions/workflows/codeql.yml)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.GitHub.Repositories.PullRequests

Retrieve, review, rebase, and merge GitHub pull requests across individual repositories or an entire owner.

## Installation

```bash
dotnet add package Soenneker.GitHub.Repositories.PullRequests
```

## Configuration

Add a GitHub token to your configuration:

```json
{
  "GH": {
    "Token": "github-token"
  }
}
```

The token needs access to every repository being queried. Approval, rebase, and merge operations also require the corresponding pull-request permissions.

## Registration

```csharp
services.AddGitHubRepositoriesPullRequestsUtilAsSingleton();
```

Use `AddGitHubRepositoriesPullRequestsUtilAsScoped()` when the consumer should be scoped.

## Usage

```csharp
public sealed class PullRequestService
{
    private readonly IGitHubRepositoriesPullRequestsUtil _pullRequests;

    public PullRequestService(IGitHubRepositoriesPullRequestsUtil pullRequests)
    {
        _pullRequests = pullRequests;
    }

    public ValueTask<List<PullRequest>> GetOpenPullRequests(
        CancellationToken cancellationToken = default)
    {
        return _pullRequests.GetAll(
            "soenneker",
            "soenneker.github.repositories.pullrequests",
            cancellationToken: cancellationToken);
    }
}
```

`GetAll` returns open pull requests. The optional `username`, `startAt`, and `endAt` arguments filter by author and creation time.

The library also supports:

- finding repositories with open pull requests or failed pull-request runs
- checking approval status and approving one or more pull requests
- rebasing open pull requests whose branches are behind their base branches
- squash-merging pull requests, including variants that require passing checks

Approval, rebase, and merge methods change repository state. Date and author filters should be used when a bulk operation is intended for only part of a repository's open pull requests.
