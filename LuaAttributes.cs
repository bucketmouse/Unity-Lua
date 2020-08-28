
using System;

/// <summary>
/// Used to document lua functions
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class LuaApiFunction : Attribute
{
    public string name = string.Empty;
    public string description = string.Empty;
    public string returns = string.Empty;
    public string notes = string.Empty;
    public string warning = string.Empty;
    public string success = string.Empty;
    public string codeExample = string.Empty;
}

/// <summary>
/// Used to document lua variables
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class LuaApiVariable : Attribute
{
    public string name = string.Empty;
    public string description = string.Empty;
}

/// <summary>
/// Used for generating lua counterparts to C# enums as well as document them
/// </summary>
[AttributeUsage(AttributeTargets.Enum, AllowMultiple = false)]
public class LuaApiEnum : Attribute
{
    public LuaApiEnum(string name, string description="No description provided.") { this.name = name; this.description = description; }
    public string name = string.Empty;
    public string description = string.Empty;
}

/// <summary>
/// Used to document enum values
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple=false)]
public class LuaApiEnumValue : Attribute
{
    public LuaApiEnumValue(string description = "No description provided.", bool hidden=false) { this.description = description; this.hidden = hidden; }
    public string description = string.Empty;
    public bool hidden = false;
}
