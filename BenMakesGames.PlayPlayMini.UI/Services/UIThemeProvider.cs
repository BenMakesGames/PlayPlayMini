using BenMakesGames.PlayPlayMini.UI.Model;

namespace BenMakesGames.PlayPlayMini.UI.Services;

public abstract class UIThemeProvider
{
    protected abstract Theme CurrentTheme { get; set; }

    public Theme GetTheme() => CurrentTheme;
    public void SetTheme(Theme theme) => CurrentTheme = theme;
}