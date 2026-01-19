using BenMakesGames.PlayPlayMini.VN.Model;
using BenMakesGames.PlayPlayMini.VN.Model.Buttons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

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
	/// Default number of rows of text to give dialog boxes when using <see cref="StoryStep.SetCharacterDialog"/> and similar.
	/// </summary>
	public static int DialogDefaultRows { get; set; } = 2;

    /// <summary>
    /// The speed at which dialog text is displayed.
    /// </summary>
    public static float DialogSpeed { get; set; } = 40;

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

    /// <summary>
    /// Used to navigate choices and other buttons within a <see cref="ButtonCollection"/>.
    /// </summary>
    public static Keys[] UpKeys = [ Keys.W, Keys.Up, Keys.NumPad8 ];

    /// <summary>
    /// Used to navigate choices and other buttons within a <see cref="ButtonCollection"/>.
    /// </summary>
    public static Keys[] DownKeys = [ Keys.S, Keys.Down, Keys.NumPad2 ];

    /// <summary>
    /// Used to navigate choices and other buttons within a <see cref="ButtonCollection"/>.
    /// </summary>
    public static Keys[] LeftKeys = [ Keys.A, Keys.Left, Keys.NumPad4 ];

    /// <summary>
    /// Used to navigate choices and other buttons within a <see cref="ButtonCollection"/>.
    /// </summary>
    public static Keys[] RightKeys = [ Keys.D, Keys.Right, Keys.NumPad6 ];

    /// <summary>
    /// Used to confirm choices, and advance the dialog.
    /// </summary>
    public static Keys[] SelectKeys = [ Keys.Enter, Keys.Space ];
}
