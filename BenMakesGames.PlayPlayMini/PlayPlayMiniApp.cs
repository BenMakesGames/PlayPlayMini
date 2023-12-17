using BenMakesGames.PlayPlayMini.Attributes.DI;
using BenMakesGames.PlayPlayMini.Model;
using Silk.NET.Windowing;

namespace BenMakesGames.PlayPlayMini;

[AutoRegister]
public sealed class PlayPlayMiniApp
{
    public IWindow SilkWindow { get; }
    public GameTime GameTime { get; } = new();
    
    private GameStateManager GSM { get; }
    
    public PlayPlayMiniApp(GameStateManager gsm)
    {
        SilkWindow = Window.Create(WindowOptions.Default);
        GSM = gsm;
    }

    public void Run()
    {
        SilkWindow.Load += DoLoad;
        SilkWindow.Update += DoUpdate;
        SilkWindow.Render += DoRender;
        SilkWindow.Closing += DoStop;

        SilkWindow.Initialize();
    }

    private void DoLoad()
    {
        GSM.Initialize(this);
        GSM.Load();
    }

    private void DoStop()
    {
        GSM.Stop();
    }
    
    private void DoUpdate(double deltaMs)
    {
        GameTime.Add(deltaMs);
        
        GSM.Update(GameTime);
    }
    
    private void DoRender(double deltaMs)
    {
        GameTime.Add(deltaMs);
        
        GSM.Draw(GameTime);
    }
}