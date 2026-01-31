namespace BenMakesGames.PlayPlayMini.Model;

public interface IAsset
{
    /// <summary>
    /// An identifier for this asset. Must be unique within its type. (For example, a Picture and a
    /// SpriteSheet may have the same Key, but two Pictures may not have the same Key.)
    /// </summary>
    string Key { get; }
}
