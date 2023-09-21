namespace QuartzNode.Extensions;

using Microsoft.FeatureManagement;

public static class FeatureManagementExtensions
{
    /// <summary>
    ///     Generates a feature manager.
    /// </summary>
    /// <param name="configuration">
    ///     A specific <see cref="T:Microsoft.Extensions.Configuration.IConfiguration" /> instance that
    ///     will be used to obtain feature settings.
    /// </param>
    /// <param name="configure">The action used to configure the feature settings.</param>
    /// <returns>
    ///     A <see cref="T:Microsoft.FeatureManagement.IFeatureManager" /> that can be used to check which features are
    ///     enabled.
    /// </returns>
    public static IFeatureManager GenerateFeatureManager(this IConfiguration configuration,
        Action<IFeatureManagementBuilder> configure = null)
    {
        var builder = new ServiceCollection().AddFeatureManagement(configuration);
        configure?.Invoke(builder);

        return builder.Services.BuildServiceProvider()
            .GetRequiredService<IFeatureManager>();
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