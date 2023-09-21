namespace QuartzNode.Extensions;

using Microsoft.FeatureManagement;

public static class FeatureFlagEndpointFilterFactory
{
    public static TBuilder AddEndpointFilterForFeature<TBuilder>(this TBuilder builder, string featureFlag)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.AddEndpointFilter(Create(featureFlag));
        return builder;
    }

    // ref: https://timdeschryver.dev/blog/implementing-a-feature-flag-based-endpoint-filter#feature-flag-implementation-as-an-endpointfilter
    public static Func<EndpointFilterInvocationContext, EndpointFilterDelegate, ValueTask<object?>> Create(
        string featureFlag)
    {
        return async (context, next) =>
        {
            var featureManager = context.HttpContext.RequestServices.GetRequiredService<IFeatureManager>();
            var isEnabled = await featureManager.IsEnabledAsync(featureFlag);
            if (!isEnabled)
            {
                return TypedResults.NotFound();
            }

            return await next(context);
        };
    }
}