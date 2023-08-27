namespace Core.Quartz.Jobs.Extensions;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class QuartzJobDataAttribute : Attribute
{
    public QuartzJobDataAttribute(string name, object defaultValue)
    {
        Name = name;
        DefaultValue = defaultValue;
    }

    public string Name { get; }
    public object DefaultValue { get; }
}