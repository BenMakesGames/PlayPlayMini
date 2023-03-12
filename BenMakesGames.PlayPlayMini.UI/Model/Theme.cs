using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.UI.Model;

public record Theme(
    Color WindowColor,
    string FontName,
    string ButtonSpriteSheetName,
    Color ButtonLabelColor,
    Color ButtonLabelDisabledColor,
    string CheckboxSpriteSheetName,
    string? ButtonHoverSoundName = null,
    string? ButtonClickSoundName = null
);