using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.GitHub.Client.Registrars;
using Soenneker.GitHub.Repositories.PullRequests.Abstract;

namespace Soenneker.GitHub.Repositories.PullRequests.Registrars;

/// <summary>
/// A utility library for GitHub repository pull request related operations
/// </summary>
public static class GitHubRepositoryPullRequestsUtilRegistrar
{
    /// <summary>
    /// Adds <see cref="IGitHubRepositoryPullRequestsUtil"/> as a singleton service. <para/>
    /// </summary>
    public static void AddGitHubRepositoryPullRequestsUtilAsSingleton(this IServiceCollection services)
    {
        services.AddGitHubClientUtilAsSingleton();
        services.TryAddSingleton<IGitHubRepositoryPullRequestsUtil, GitHubRepositoryPullRequestsUtil>();
    }

    /// <summary>
    /// Adds <see cref="IGitHubRepositoryPullRequestsUtil"/> as a scoped service. <para/>
    /// </summary>
    public static void AddGitHubRepositoryPullRequestsUtilAsScoped(this IServiceCollection services)
    {
        services.AddGitHubClientUtilAsSingleton();
        services.TryAddScoped<IGitHubRepositoryPullRequestsUtil, GitHubRepositoryPullRequestsUtil>();
    }
}