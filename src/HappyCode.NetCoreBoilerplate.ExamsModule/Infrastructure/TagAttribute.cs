namespace HappyCode.NetCoreBoilerplate.ExamsModule;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class TagAttribute : Attribute
{
    public string Name { get; }

    public TagAttribute(string name)
    {
        Name = name;
    }
} 