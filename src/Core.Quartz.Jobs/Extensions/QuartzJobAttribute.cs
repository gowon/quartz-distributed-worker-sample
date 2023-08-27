namespace Core.Quartz.Jobs.Extensions;

[AttributeUsage(AttributeTargets.Class)]
public class QuartzJobAttribute : Attribute
{
    public QuartzJobAttribute(string name, string groupName, string? description = null)
    {
        Name = name;
        GroupName = groupName;
        Description = description;
    }

    public string Name { get; }
    public string GroupName { get; }
    public string? Description { get; }
}