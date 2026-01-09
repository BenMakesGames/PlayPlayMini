using System;
using System.Collections.Generic;

namespace BenMakesGames.PlayPlayMini.UI.UIElements;

public abstract class UIContainer
{
    public IReadOnlyList<IUIElement> Children => RealChildren.AsReadOnly();

    private List<IUIElement> RealChildren = new List<IUIElement>();

    public void RemoveUIElements(params IUIElement[] elements)
    {
        foreach (var e in elements)
            RealChildren.Remove(e);
    }

    public void RemoveUIElements(IEnumerable<IUIElement> elements)
    {
        foreach (IUIElement e in elements)
            RealChildren.Remove(e);
    }

    public void AddUIElements(params IUIElement[] uiElements)
    {
        RealChildren.AddRange(uiElements);
    }

    public void AddUIElements(IEnumerable<IUIElement> elements)
    {
        RealChildren.AddRange(elements);
    }

    public void ClearUIElements()
    {
        RealChildren.Clear();
    }
}