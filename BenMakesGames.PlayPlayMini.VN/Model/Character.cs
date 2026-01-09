using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.VN.Model;

public class Character
{
    /// <summary>
    /// The id of the character - a unique identifier that's used by PlayPlayMini.VN to track the character.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// The name of the character. This name will be shown in dialog boxes when this character speaks.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The PlayPlayMini-loaded spritesheet to use for this character. Use <see cref="Expressions" /> to set the character's sprite.
    /// </summary>
    public required string SpriteSheet { get; set; }

    /// <summary>
    /// The color used for detailing the dialog box when this character is speaking.
    /// </summary>
    public required Color SpeakingColor { get; set; }

    /// <summary>
    /// A dictionary of expressions for this character. The key is the expression name, and the value is the sprite index to use.
    /// </summary>
    /// <remarks>
    /// It's recommended to use constants for expression names, to help prevent typos.
    /// </remarks>
    public required Dictionary<string, int> Expressions { get; set; } = new(StringComparer.CurrentCultureIgnoreCase);
}
