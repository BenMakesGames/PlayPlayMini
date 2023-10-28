using System;
using System.Runtime.CompilerServices;
using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.GraphicsExtensions;

public static class TriangleExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsCounterclockwise(float x1, float y1, float x2, float y2, float x3, float y3)
        => (x2 - x1) * (y3 - y1) - (y2 - y1) * (x3 - x1) < 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DrawFilledTriangle(this GraphicsManager graphics, Vector2 v1, Vector2 v2, Vector2 v3, Color fillColor)
        => graphics.DrawFilledTriangle(v1.X, v1.Y, v2.X, v2.Y, v3.X, v3.Y, fillColor);

    // adapted from https://web.archive.org/web/20050408192410/http://sw-shader.sourceforge.net/rasterizer.html
    // note: I already tried using hardware intrinsics to speed up some of the math in here, but it was actually slower, so... 🤷
    public static void DrawFilledTriangle(this GraphicsManager graphics, float x1, float y1, float x2, float y2, float x3, float y3, Color color)
    {
        if(!IsCounterclockwise(x1, y1, x2, y2, x3, y3))
            (x1, y1, x2, y2) = (x2, y2, x1, y1);

        // 28.4 fixed-point coordinates
        var fixedY1 = (int)Math.Round(16 * y1);
        var fixedY2 = (int)Math.Round(16 * y2);
        var fixedY3 = (int)Math.Round(16 * y3);

        var fixedX1 = (int)Math.Round(16 * x1);
        var fixedX2 = (int)Math.Round(16 * x2);
        var fixedX3 = (int)Math.Round(16 * x3);

        var deltaX12 = fixedX1 - fixedX2;
        var deltaX23 = fixedX2 - fixedX3;
        var deltaX31 = fixedX3 - fixedX1;

        var deltaY12 = fixedY1 - fixedY2;
        var deltaY23 = fixedY2 - fixedY3;
        var deltaY31 = fixedY3 - fixedY1;

        // Fixed-point deltas
        var FDX12 = deltaX12 << 4;
        var FDX23 = deltaX23 << 4;
        var FDX31 = deltaX31 << 4;

        var FDY12 = deltaY12 << 4;
        var FDY23 = deltaY23 << 4;
        var FDY31 = deltaY31 << 4;

        // Bounding rectangle
        var minX = (Math.Min(fixedX1, Math.Min(fixedX2, fixedX3)) + 0xF) >> 4;
        var maxX = (Math.Max(fixedX1, Math.Max(fixedX2, fixedX3)) + 0xF) >> 4;
        var minY = (Math.Min(fixedY1, Math.Min(fixedY2, fixedY3)) + 0xF) >> 4;
        var maxY = (Math.Max(fixedY1, Math.Max(fixedY2, fixedY3)) + 0xF) >> 4;

        // Block size, standard 8x8 (must be power of two)
        const int q = 8;

        // Start in corner of 8x8 block
        minX &= ~(q - 1);
        minY &= ~(q - 1);

        // Half-edge constants
        var c1 = deltaY12 * fixedX1 - deltaX12 * fixedY1;
        var c2 = deltaY23 * fixedX2 - deltaX23 * fixedY2;
        var c3 = deltaY31 * fixedX3 - deltaX31 * fixedY3;

        // Correct for fill convention
        if(deltaY12 < 0 || (deltaY12 == 0 && deltaX12 > 0)) c1++;
        if(deltaY23 < 0 || (deltaY23 == 0 && deltaX23 > 0)) c2++;
        if(deltaY31 < 0 || (deltaY31 == 0 && deltaX31 > 0)) c3++;

        // Loop through blocks
        for(var y = minY; y < maxY; y += q)
        {
            for(var x = minX; x < maxX; x += q)
            {
                // Corners of block
                var blockX1 = x << 4;
                var blockX2 = (x + q - 1) << 4;
                var blockY1 = y << 4;
                var blockY2 = (y + q - 1) << 4;

                // Evaluate half-space functions
                var a00 = c1 + deltaX12 * blockY1 - deltaY12 * blockX1 > 0;
                var a10 = c1 + deltaX12 * blockY1 - deltaY12 * blockX2 > 0;
                var a01 = c1 + deltaX12 * blockY2 - deltaY12 * blockX1 > 0;
                var a11 = c1 + deltaX12 * blockY2 - deltaY12 * blockX2 > 0;
                var a = (a00 ? 1 : 0) | (a10 ? 2 : 0) | (a01 ? 4 : 0) | (a11 ? 8 : 0);

                var b00 = c2 + deltaX23 * blockY1 - deltaY23 * blockX1 > 0;
                var b10 = c2 + deltaX23 * blockY1 - deltaY23 * blockX2 > 0;
                var b01 = c2 + deltaX23 * blockY2 - deltaY23 * blockX1 > 0;
                var b11 = c2 + deltaX23 * blockY2 - deltaY23 * blockX2 > 0;
                var b = (b00 ? 1 : 0) | (b10 ? 2 : 0) | (b01 ? 4 : 0) | (b11 ? 8 : 0);

                var c00 = c3 + deltaX31 * blockY1 - deltaY31 * blockX1 > 0;
                var c10 = c3 + deltaX31 * blockY1 - deltaY31 * blockX2 > 0;
                var c01 = c3 + deltaX31 * blockY2 - deltaY31 * blockX1 > 0;
                var c11 = c3 + deltaX31 * blockY2 - deltaY31 * blockX2 > 0;
                var c = (c00 ? 1: 0) | (c10 ? 2: 0) | (c01 ? 4 : 0) | (c11 ? 8 : 0);

                // Skip block when outside an edge
                if(a == 0x0 || b == 0x0 || c == 0x0) continue;

                // Accept whole block when totally covered
                if(a == 0xF && b == 0xF && c == 0xF)
                    graphics.DrawFilledRectangle(x, y, q, q, color);
                else   // Partially covered block
                {
                    var CY1 = c1 + deltaX12 * blockY1 - deltaY12 * blockX1;
                    var CY2 = c2 + deltaX23 * blockY1 - deltaY23 * blockX1;
                    var CY3 = c3 + deltaX31 * blockY1 - deltaY31 * blockX1;

                    for(var iy = y; iy < y + q; iy++)
                    {
                        var CX1 = CY1;
                        var CX2 = CY2;
                        var CX3 = CY3;

                        var startX = -1;
                        var width = 0;

                        for(var ix = x; ix < x + q; ix++)
                        {
                            if(CX1 > 0 && CX2 > 0 && CX3 > 0)
                            {
                                if(startX == -1) startX = ix;
                                width++;
                            }
                            else if(startX != -1)
                                break;

                            CX1 -= FDY12;
                            CX2 -= FDY23;
                            CX3 -= FDY31;
                        }

                        graphics.DrawRow(startX, startX + width - 1, iy, color);

                        CY1 += FDX12;
                        CY2 += FDX23;
                        CY3 += FDX31;
                    }
                }
            }
        }
    }
}
