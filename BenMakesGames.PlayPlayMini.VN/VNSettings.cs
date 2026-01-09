using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.VN;

/// <summary>
/// Global settings for the VN system.
/// </summary>
/// <remarks>
/// You CAN change these mid-scene, but changing some of them may have unintended effects. Proceed with caution.
/// This situation may be improved in future releases.
/// </remarks>
public static class VNSettings
{
    /// <summary>
    /// The default background color for scenes.
    /// </summary>
    public static Color SceneDefaultBackgroundColor { get; set; } = Color.Black;

    /// <summary>
    /// The name of the PlayPlayMini font to use for writing dialog text.
    /// </summary>
    public static string DialogFont { get; set; } = "Font";

    /// <summary>
    /// If this is set, characters will be drawn with a 1px outline in this color.
    /// </summary>
    /// <remarks>
    /// The prerelease version has a very poor implementation of this feature. It really
    /// only works for black outlines.
    /// </remarks>
    public static Color? CharacterOutlineColor { get; set; } = Color.Black;

    public static Color DialogSpeakingBackgroundColor { get; set; } = Color.White;
    public static Color DialogSpeakingTextColor { get; set; } = Color.Black;

    public static Color DialogThinkingBackgroundColor { get; set; } = Color.Black;
    public static Color DialogThinkingTextColor { get; set; } = Color.White;

    public static Color DialogTransparentTextColor { get; set; } = Color.White;
    public static Color DialogTransparentOutlineColor { get; set; } = Color.Black;

    public static Color ChoiceBackgroundColor { get; set; } = Color.White;
    public static Color ChoiceTextColor { get; set; } = Color.Black;
    public static Color ChoiceHoveredBackgroundColor { get; set; } = Color.Navy;
    public static Color ChoiceHoveredTextColor { get; set; } = Color.White;
}
