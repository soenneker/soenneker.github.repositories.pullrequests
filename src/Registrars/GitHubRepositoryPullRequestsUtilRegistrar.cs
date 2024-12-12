using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.GitHub.Client.Registrars;
using Soenneker.GitHub.Repositories.PullRequests.Abstract;

namespace Soenneker.GitHub.Repositories.PullRequests.Registrars;

/// <summary>
/// A utility library for GitHub repository pull request related operations
/// </summary>
public static class GitHubRepositoriesPullRequestsUtilRegistrar
{
    /// <summary>
    /// Adds <see cref="IGitHubRepositoriesPullRequestsUtil"/> as a singleton service. <para/>
    /// </summary>
    public static void AddGitHubRepositoriesPullRequestsUtilAsSingleton(this IServiceCollection services)
    {
        services.AddGitHubClientUtilAsSingleton();
        services.TryAddSingleton<IGitHubRepositoriesPullRequestsUtil, GitHubRepositoriesPullRequestsUtil>();
    }

    /// <summary>
    /// Adds <see cref="IGitHubRepositoriesPullRequestsUtil"/> as a scoped service. <para/>
    /// </summary>
    public static void AddGitHubRepositoriesPullRequestsUtilAsScoped(this IServiceCollection services)
    {
        services.AddGitHubClientUtilAsSingleton();
        services.TryAddScoped<IGitHubRepositoriesPullRequestsUtil, GitHubRepositoriesPullRequestsUtil>();
    }
}