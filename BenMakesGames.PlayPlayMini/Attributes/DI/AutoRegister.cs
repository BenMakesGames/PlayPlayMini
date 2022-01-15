using System;

namespace BenMakesGames.PlayPlayMini.Attributes.DI
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AutoRegister: Attribute
    {
        public Lifetime Lifetime { get; }
        public Type? InstanceOf { get; set; }

        public AutoRegister(Lifetime lifetime)
        {
            Lifetime = lifetime;
        }
    }

    public enum Lifetime
    {
        Singleton,
        PerDependency,
        //PerScope
    }
}
