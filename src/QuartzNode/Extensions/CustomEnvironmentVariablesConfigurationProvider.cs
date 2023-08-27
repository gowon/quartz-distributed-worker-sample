namespace QuartzNode.Extensions;

using Microsoft.Extensions.Configuration.EnvironmentVariables;

// ref: https://github.com/dotnet/runtime/issues/87130#issuecomment-1583859511
// ref: https://github.com/dotnet/runtime/issues/35989
public class CustomEnvironmentVariablesConfigurationProvider : EnvironmentVariablesConfigurationProvider
{
    internal const string DefaultDotReplacement = ":_";
    private readonly string _dotReplacement;

    public CustomEnvironmentVariablesConfigurationProvider(string? dotReplacement = DefaultDotReplacement)
    {
        _dotReplacement = dotReplacement ?? DefaultDotReplacement;
    }

    public CustomEnvironmentVariablesConfigurationProvider(string? prefix,
        string? dotReplacment = DefaultDotReplacement) : base(prefix)
    {
        _dotReplacement = dotReplacment ?? DefaultDotReplacement;
    }

    public override void Load()
    {
        base.Load();

        var data = new Dictionary<string, string?>();

        foreach (var kvp in Data)
        {
            if (kvp.Key.Contains(_dotReplacement))
            {
                data.Add(kvp.Key.Replace(_dotReplacement, ".", StringComparison.OrdinalIgnoreCase), kvp.Value);
            }
            else
            {
                data.Add(kvp.Key, kvp.Value);
            }
        }

        Data = data;
    }
}

public class CustomEnvironmentVariablesConfigurationSource : IConfigurationSource
{
    public string? Prefix { get; set; }
    public string? DotReplacement { get; set; }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new CustomEnvironmentVariablesConfigurationProvider(Prefix, DotReplacement);
    }
}

public static class CustomEnvironmentVariablesExtensions
{
    public static IConfigurationBuilder AddCustomEnvironmentVariables(this IConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Add(new CustomEnvironmentVariablesConfigurationSource());
        return configurationBuilder;
    }

    public static IConfigurationBuilder AddCustomEnvironmentVariables(this IConfigurationBuilder configurationBuilder,
        string? prefix, string? dotReplacement = CustomEnvironmentVariablesConfigurationProvider.DefaultDotReplacement)
    {
        configurationBuilder.Add(new CustomEnvironmentVariablesConfigurationSource
            { Prefix = prefix, DotReplacement = dotReplacement });
        return configurationBuilder;
    }

    public static IConfigurationBuilder AddCustomEnvironmentVariables(this IConfigurationBuilder builder,
        Action<CustomEnvironmentVariablesConfigurationSource>? configureSource)
    {
        return builder.Add(configureSource);
    }
}