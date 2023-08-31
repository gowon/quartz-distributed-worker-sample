namespace Core.Quartz.Jobs.Extensions;

/// <summary>
///     Provides a static method as a delegate for registering the IJob.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class QuartzJobProviderAttribute : Attribute
{
    public QuartzJobProviderAttribute(string methodName)
    {
        MethodName = !string.IsNullOrWhiteSpace(methodName)
            ? methodName
            : throw new ArgumentNullException(nameof(methodName));
    }

    /// <summary>
    ///     Gets the member name.
    /// </summary>
    public string MethodName { get; }

    /// <summary>
    ///     Gets or sets the type to retrieve the member from. If not set, then the property will be
    ///     retrieved from the unit test class.
    /// </summary>
    public Type? MemberType { get; set; }
}