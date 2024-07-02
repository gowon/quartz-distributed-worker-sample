namespace QuartzNode.Extensions;

using Microsoft.FeatureManagement;

public static class FeatureManagementExtensions
{
    private static Lazy<IFeatureManager> _lazyFeatureManager = new(() => throw new InvalidOperationException(
        $"Please initialize Feature Manager using '{nameof(EnableFeatureManagementDependencyInjection)}()' on the host builder context first before using Feature-based injection."));

    public static void EnableFeatureManagementDependencyInjection(this WebHostBuilderContext context,
        Action<IFeatureManagementBuilder>? configure = null)
    {
        _lazyFeatureManager = new Lazy<IFeatureManager>(() =>
        {
            var builder = new ServiceCollection().AddFeatureManagement(context.Configuration);
            configure?.Invoke(builder);
            return builder.Services.BuildServiceProvider().GetRequiredService<IFeatureManager>();
        });
    }

    public static IFeatureManager GetFeatureManager(this WebHostBuilderContext context)
    {
        context.EnableFeatureManagementDependencyInjection();
        return _lazyFeatureManager.Value;
    }

    public static IServiceCollection AddForFeature(this IServiceCollection services, string feature,
        Action<IServiceCollection>? enabledAction = null, Action<IServiceCollection>? disabledAction = null)
    {
        if (_lazyFeatureManager.Value.IsEnabled(feature))
        {
            enabledAction?.Invoke(services);
        }
        else
        {
            disabledAction?.Invoke(services);
        }

        return services;
    }

    public static IServiceCollection AddForFeature<TContext>(this IServiceCollection services, string feature,
        TContext context, Action<IServiceCollection>? enabledAction = null,
        Action<IServiceCollection>? disabledAction = null)
    {
        if (_lazyFeatureManager.Value.IsEnabled(feature, context))
        {
            enabledAction?.Invoke(services);
        }
        else
        {
            disabledAction?.Invoke(services);
        }

        return services;
    }

    /// <summary>Checks whether a given feature is enabled.</summary>
    /// <param name="featureManager">The feature manager.</param>
    /// <param name="feature">The name of the feature to check.</param>
    /// <returns>True if the feature is enabled, otherwise false.</returns>
    public static bool IsEnabled(this IFeatureManager featureManager, string feature)
    {
        return featureManager.IsEnabledAsync(feature).GetAwaiter().GetResult();
    }

    /// <summary>Checks whether a given feature is enabled.</summary>
    /// <param name="featureManager">The feature manager.</param>
    /// <param name="feature">The name of the feature to check.</param>
    /// <param name="context">
    ///     A context providing information that can be used to evaluate whether a feature should be on or
    ///     off.
    /// </param>
    /// <returns>True if the feature is enabled, otherwise false.</returns>
    public static bool IsEnabled<TContext>(this IFeatureManager featureManager, string feature, TContext context)
    {
        return featureManager.IsEnabledAsync(feature, context).GetAwaiter().GetResult();
    }
}