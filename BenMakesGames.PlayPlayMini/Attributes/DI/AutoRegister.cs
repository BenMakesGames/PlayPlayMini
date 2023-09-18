using System;

namespace BenMakesGames.PlayPlayMini.Attributes.DI;

[AttributeUsage(AttributeTargets.Class)]
// ReSharper disable once ClassCanBeSealed.Global - performance gain is negligible, and this is a public API
public class AutoRegister: Attribute
{
    public Lifetime Lifetime { get; }
    public Type? InstanceOf { get; set; }

    public AutoRegister(Lifetime lifetime = Lifetime.Singleton)
    {
        Lifetime = lifetime;
    }
}

public enum Lifetime
{
    Singleton,
    PerDependency,
}
