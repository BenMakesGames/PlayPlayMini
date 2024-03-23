# General Tips

### Embrace Nullable Reference Types

The PlayPlayMini templates use `<Nullable>enable</Nullable>` (MS's current recommendation), AND `<WarningsAsErrors>Nullable</WarningsAsErrors>` (a community recommendation for all new C# projects).

If you're not familiar with nullable reference types, become familiar, and embrace them! Use `?` when you want to allow `null`s, and aim to never type `null!`. (Sometimes you'll have to, but it should be rare!)

The ` required` and `init` keywords will help you accomplish this.

**Why?**

Being strict about where you allow `null`s will save you from many common runtime bugs (null reference exceptions). Newer languages are more strict about `null` usage; for historic reasons, C# made this strictness optional, but Microsoft is pushing it more and more with every release of C# and .NET.

### LINQ _Can_ Be Great at 60FPS!

You may have heard people say, very strongly, to *never* use LINQ in games for performance reasons.

Since .NET 7, this advice is less relevant, and sometimes plain bad: not only has Microsoft ensured that many (but not all!) LINQ methods perform no memory allocations (the source of the performance concerns), but using LINQ can sometimes be _more_ performant than a traditional `for` loop thanks to LINQ's use of "vectorization" (hardware-level parallelization).

* Read more about the .NET 7 improvements here: https://devblogs.microsoft.com/dotnet/performance_improvements_in_net_7/#linq
* MS made further improvements in .NET 8: https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-8/#linq
* And more will be coming in .NET 9!

Language and compiler writers have been working on performance for many many years. Before you make a performance-based decision based on suggestions from internet randos, do some research; if/when you're having performance issues, do your own benchmarking!

* PlayPlayMini comes with a [FrameCounter service](/api/BenMakesGames.PlayPlayMini.Services.FrameCounter.html) you can use to help keep an eye on how your changes affect your game's FPS!
* [BenchmarkDotNet](https://benchmarkdotnet.org/) is a great tool to learn when you've got a free afternoon.

### Prefer Composition over Inheritance

It's tempting to think things like "all my game objects will have positions, so I should make a base class for that, and inherit it", and end up with `public class Ship: Position` and `public class EnemyShip: Ship` and `public class Boss: EnemyShip` and so on. Taking this path typically feels good in the beginning, but leads to a tangled mess of code that's hard to change in the long-run.

Inheritance is powerful, and absolutely has its use-cases, but those use-cases are more niche than people generally think.

To nudge you to do the right thing, use `sealed` on all your `class`es and `record`s by default. (Example: `public sealed class YourClassName`). You can edit the templates your IDE uses for new `class`es, `record`s, etc; edit these to add the `sealed` keyword! You can always remove the keyword for those rare cases where you truly need inheritance.

**Further reading:**

* Internet search: [Composition over inheritance](https://duckduckgo.com/?q=composition+over+inheritance)
* Internet search: [Good Code is Easy to Change](https://duckduckgo.com/?q=good+code+is+easy+to+change) (deep inheritance trees are hard to change)
* Book: "The Art of Readable Code" by Dustin Boswell and Trevor Foucher (touches on this topic, and much more)