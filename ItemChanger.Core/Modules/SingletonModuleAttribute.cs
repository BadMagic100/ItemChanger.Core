namespace ItemChanger.Modules;

/// <summary>
/// Marker interface defining a module type as a singleton. Attempting to add a second instance of
/// a singleton module will result in an error
/// </summary>
[System.AttributeUsage(System.AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class SingletonModuleAttribute : System.Attribute { }
