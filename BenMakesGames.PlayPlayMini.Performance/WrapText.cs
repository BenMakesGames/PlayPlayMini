using BenchmarkDotNet.Attributes;
using BenMakesGames.PlayPlayMini.Extensions;
using BenMakesGames.PlayPlayMini.Model;

namespace BenMakesGames.PlayPlayMini.Performance;

[MemoryDiagnoser(false)]
public class WrapText
{
    private static readonly Font Font = new(null!, 6, 8, 0, 0, ' ');
    private static readonly string Text = "Far out in the uncharted backwaters of the unfashionable end of the western spiral arm of the Galaxy lies a small unregarded yellow sun.";

    [Benchmark]
    public string Original() => Text.WrapText(Font, 100);
}
