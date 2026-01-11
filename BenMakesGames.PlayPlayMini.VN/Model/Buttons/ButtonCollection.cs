using System.Collections;
using BenMakesGames.PlayPlayMini.GraphicsExtensions;
using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.VN.Model.Buttons;

public sealed class ButtonCollection: IReadOnlyList<IButton>
{
    public MouseManager? Mouse { get; set; }
    public KeyboardManager? Keyboard { get; set; }

    private List<IButton> Buttons { get; set; }

    public IButton? SelectedButton { get; set; }

    public ButtonCollection(IEnumerable<IButton> buttons)
    {
        Buttons = buttons.ToList();
        DoMouseInput();
    }

    public void SetButtons(IEnumerable<IButton> buttons)
    {
        Buttons = buttons.ToList();
        DoMouseInput();
    }

    public void Draw(GraphicsManager graphics)
    {
        foreach (var button in this)
            button.Draw(graphics, button == SelectedButton);
    }

    /// <summary>
    /// Returns true if a button was activated.
    /// </summary>
    /// <param name="gsm"></param>
    /// <param name="owningGameState"></param>
    /// <returns></returns>
    public bool Input(GameStateManager gsm, AbstractGameState owningGameState)
    {
        if (gsm.CurrentState != owningGameState || !gsm.IsActive || Buttons.Count == 0)
            return false;

        DoMouseInput();
        DoKeyboardInput();

        if (ActivatedButton() is { Click: not null } button)
        {
            button.Click();
            return true;
        }

        return false;
    }

    private void DoMouseInput()
    {
        if (Mouse is { Moved: true } && Mouse.IsInWindow())
        {
            SelectedButton = Buttons.FirstOrDefault(
                b => b.IsEnabled && b.Contains(Mouse)
            );
        }
    }

    private void DoKeyboardInput()
    {
        if (Keyboard is null) return;

        if(Keyboard.PressedAnyKey(VNSettings.DownKeys))
        {
            if(SelectedButton is null)
                SelectedButton = Buttons.FirstOrDefault(b => b.IsEnabled);
            else
            {
                SelectedButton = Buttons
                    .Where(b => b.IsEnabled && b.GetCenter().Y > SelectedButton.GetCenter().Y)
                    .OrderBy(b => Vector2.Distance(b.GetCenter(), SelectedButton.GetCenter()))
                    .FirstOrDefault()
                    ?? SelectedButton;
            }
        }

        if(Keyboard.PressedAnyKey(VNSettings.UpKeys))
        {
            if(SelectedButton is null)
                SelectedButton = Buttons.LastOrDefault(b => b.IsEnabled);
            else
            {
                SelectedButton = Buttons
                    .Where(b => b.IsEnabled && b.GetCenter().Y < SelectedButton.GetCenter().Y)
                    .OrderBy(b => Vector2.Distance(b.GetCenter(), SelectedButton.GetCenter()))
                    .FirstOrDefault()
                    ?? SelectedButton;
            }
        }

        if (Keyboard.PressedAnyKey(VNSettings.LeftKeys))
        {
            if (SelectedButton is null)
                SelectedButton = Buttons.FirstOrDefault(b => b.IsEnabled);
            else
            {
                SelectedButton = Buttons
                    .Where(b => b.IsEnabled && b.GetCenter().X < SelectedButton.GetCenter().X)
                    .OrderBy(b => Vector2.Distance(b.GetCenter(), SelectedButton.GetCenter()))
                    .FirstOrDefault()
                    ?? SelectedButton;
            }
        }

        if (Keyboard.PressedAnyKey(VNSettings.RightKeys))
        {
            if (SelectedButton is null)
                SelectedButton = Buttons.FirstOrDefault(b => b.IsEnabled);
            else
            {
                SelectedButton = Buttons
                    .Where(b => b.IsEnabled && b.GetCenter().X > SelectedButton.GetCenter().X)
                    .OrderBy(b => Vector2.Distance(b.GetCenter(), SelectedButton.GetCenter()))
                    .FirstOrDefault()
                    ?? SelectedButton;
            }
        }
    }

    private IButton? ActivatedButton() =>
        (Mouse is { LeftClicked: true } || (Keyboard is not null && Keyboard.PressedAnyKey(VNSettings.SelectKeys)))
        && SelectedButton is { IsEnabled: true } button
            ? button
            : null;

    /// <inheritdoc/>
    public IEnumerator<IButton> GetEnumerator() => Buttons.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Buttons.GetEnumerator();

    /// <inheritdoc/>
    public int Count => Buttons.Count;

    /// <inheritdoc/>
    public IButton this[int index] => Buttons[index];
}
