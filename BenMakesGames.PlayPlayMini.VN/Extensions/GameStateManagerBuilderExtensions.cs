using Autofac;
using BenMakesGames.PlayPlayMini.VN.GameStates;

namespace BenMakesGames.PlayPlayMini.VN.Extensions;

public static class GameStateManagerBuilderExtensions
{
    public static GameStateManagerBuilder AddVN(this GameStateManagerBuilder builder)
    {
        builder.AddServices((services, config, serviceWatcher) =>
        {
            services.RegisterType<PlayScene>()
                .AsSelf().InstancePerDependency()
                .OnActivating(s => serviceWatcher.RegisterService(s.Instance))
                .OnRelease(s => serviceWatcher.UnregisterService(s));
        });

        return builder;
    }
}
