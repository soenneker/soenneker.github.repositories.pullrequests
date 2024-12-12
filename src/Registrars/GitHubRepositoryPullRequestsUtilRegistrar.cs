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
    public static IServiceCollection AddGitHubRepositoriesPullRequestsUtilAsSingleton(this IServiceCollection services)
    {
        services.AddGitHubClientUtilAsSingleton();
        services.TryAddSingleton<IGitHubRepositoriesPullRequestsUtil, GitHubRepositoriesPullRequestsUtil>();

        return services;
    }

    /// <summary>
    /// Adds <see cref="IGitHubRepositoriesPullRequestsUtil"/> as a scoped service. <para/>
    /// </summary>
    public static IServiceCollection AddGitHubRepositoriesPullRequestsUtilAsScoped(this IServiceCollection services)
    {
        services.AddGitHubClientUtilAsSingleton();
        services.TryAddScoped<IGitHubRepositoriesPullRequestsUtil, GitHubRepositoriesPullRequestsUtil>();

        return services;
    }
}