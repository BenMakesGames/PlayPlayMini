namespace BenMakesGames.PlayPlayMini.VN.Model.Dialog;

public interface IDialog
{
    DialogStyle Style { get; }
    string Text { get; }
    Character? Speaker { get; }
    int Lines { get; }
}

public enum DialogStyle
{
    Speaking,
    Thinking,
    None,
    NoneInverted,
}

public sealed record Dialog(DialogStyle Style, string Text, Character? Speaker = null, int Lines = 2) : IDialog
{
}

public sealed record CharacterDialog(string Text, Character Speaker, int Lines = 2) : IDialog
{
    public DialogStyle Style => DialogStyle.Speaking;
}

public sealed record TransparentDialog(string Text, bool InvertedText = false, int Lines = 2) : IDialog
{
    public DialogStyle Style => InvertedText ? DialogStyle.NoneInverted : DialogStyle.None;
    public Character? Speaker => null;
}

public sealed record ThinkingDialog(string Text, int Lines = 2) : IDialog
{
    public DialogStyle Style => DialogStyle.Thinking;
    public Character? Speaker => null;
}
