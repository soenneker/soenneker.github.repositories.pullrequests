using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.GitHub.Repositories.PullRequests.Abstract;
using Soenneker.GitHub.Repositories.Registrars;

namespace Soenneker.GitHub.Repositories.PullRequests.Registrars;

/// <summary>
/// A utility library for GitHub repository pull request related operations
/// </summary>
public static class GitHubRepositoriesPullRequestsUtilRegistrar
{
    /// <summary>
    /// Adds <see cref="IGitHubRepositoriesPullRequestsUtil"/> as a singleton service. <para/>
    /// </summary>
    public static IServiceCollection AddGitHubRepositoriesPullRequestsUtilAsSingleton(this IServiceCollection services)
    {
        services.AddGitHubRepositoriesUtilAsSingleton();
        services.TryAddSingleton<IGitHubRepositoriesPullRequestsUtil, GitHubRepositoriesPullRequestsUtil>();

        return services;
    }

    /// <summary>
    /// Adds <see cref="IGitHubRepositoriesPullRequestsUtil"/> as a scoped service. <para/>
    /// </summary>
    public static IServiceCollection AddGitHubRepositoriesPullRequestsUtilAsScoped(this IServiceCollection services)
    {
        services.AddGitHubRepositoriesUtilAsScoped();
        services.TryAddScoped<IGitHubRepositoriesPullRequestsUtil, GitHubRepositoriesPullRequestsUtil>();

        return services;
    }
}