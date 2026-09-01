using Microsoft.Xna.Framework.Graphics;
using System;

namespace BenMakesGames.PlayPlayMini.Model;

/// <summary>
/// One entry in <c>GraphicsManager.PostProcessChain</c> or
/// <c>GraphicsManager.BackbufferPostProcessChain</c>: the name of a pixel shader (looked up in
/// <c>GraphicsManager.PixelShaders</c>), plus an optional delegate that configures the shader's
/// parameters just before it runs.
/// </summary>
/// <remarks>
/// Chain entries apply in list order (index 0 first, the last entry last), and
/// <paramref name="Configure"/> should be a cached delegate field rather than a lambda created
/// each frame, to keep the draw loop allocation-free. See <c>GraphicsManager.PostProcessChain</c>
/// for the longer explanation, and <c>GraphicsManager.BackbufferPostProcessChain</c> for the
/// physical-pixel-resolution variant (scanlines, phosphor grid, CRT curvature).
/// </remarks>
/// <param name="ShaderName">Name of the shader to apply; must be a key in <c>GraphicsManager.PixelShaders</c>.</param>
/// <param name="Configure">Optional configuration delegate, invoked once per frame, immediately before this entry's shader runs.</param>
public readonly record struct PostProcessEntry(string ShaderName, Action<Effect>? Configure = null);
