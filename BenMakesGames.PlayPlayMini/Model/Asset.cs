using System;

namespace BenMakesGames.PlayPlayMini.Model;

public abstract record Asset(Type AssetType);

public abstract record Asset<T>(): Asset(typeof(T));
