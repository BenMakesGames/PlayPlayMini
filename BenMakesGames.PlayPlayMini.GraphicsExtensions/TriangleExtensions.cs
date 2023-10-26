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
        var Y1 = (int)Math.Round(16 * y1);
        var Y2 = (int)Math.Round(16 * y2);
        var Y3 = (int)Math.Round(16 * y3);

        var X1 = (int)Math.Round(16 * x1);
        var X2 = (int)Math.Round(16 * x2);
        var X3 = (int)Math.Round(16 * x3);

        // Deltas
        var DX12 = X1 - X2;
        var DX23 = X2 - X3;
        var DX31 = X3 - X1;

        var DY12 = Y1 - Y2;
        var DY23 = Y2 - Y3;
        var DY31 = Y3 - Y1;

        // Fixed-point deltas
        var FDX12 = DX12 << 4;
        var FDX23 = DX23 << 4;
        var FDX31 = DX31 << 4;

        var FDY12 = DY12 << 4;
        var FDY23 = DY23 << 4;
        var FDY31 = DY31 << 4;

        // Bounding rectangle
        var minx = (Math.Min(X1, Math.Min(X2, X3)) + 0xF) >> 4;
        var maxx = (Math.Max(X1, Math.Max(X2, X3)) + 0xF) >> 4;
        var miny = (Math.Min(Y1, Math.Min(Y2, Y3)) + 0xF) >> 4;
        var maxy = (Math.Max(Y1, Math.Max(Y2, Y3)) + 0xF) >> 4;

        // Block size, standard 8x8 (must be power of two)
        const int q = 8;

        // Start in corner of 8x8 block
        minx &= ~(q - 1);
        miny &= ~(q - 1);

        // Half-edge constants
        var C1 = DY12 * X1 - DX12 * Y1;
        var C2 = DY23 * X2 - DX23 * Y2;
        var C3 = DY31 * X3 - DX31 * Y3;

        // Correct for fill convention
        if(DY12 < 0 || (DY12 == 0 && DX12 > 0)) C1++;
        if(DY23 < 0 || (DY23 == 0 && DX23 > 0)) C2++;
        if(DY31 < 0 || (DY31 == 0 && DX31 > 0)) C3++;

        // Loop through blocks
        for(var y = miny; y < maxy; y += q)
        {
            for(var x = minx; x < maxx; x += q)
            {
                // Corners of block
                var blockX1 = x << 4;
                var blockX2 = (x + q - 1) << 4;
                var blockY1 = y << 4;
                var blockY2 = (y + q - 1) << 4;

                // Evaluate half-space functions
                var a00 = C1 + DX12 * blockY1 - DY12 * blockX1 > 0;
                var a10 = C1 + DX12 * blockY1 - DY12 * blockX2 > 0;
                var a01 = C1 + DX12 * blockY2 - DY12 * blockX1 > 0;
                var a11 = C1 + DX12 * blockY2 - DY12 * blockX2 > 0;
                var a = (a00 ? 1 : 0) | (a10 ? 2 : 0) | (a01 ? 4 : 0) | (a11 ? 8 : 0);
        
                var b00 = C2 + DX23 * blockY1 - DY23 * blockX1 > 0;
                var b10 = C2 + DX23 * blockY1 - DY23 * blockX2 > 0;
                var b01 = C2 + DX23 * blockY2 - DY23 * blockX1 > 0;
                var b11 = C2 + DX23 * blockY2 - DY23 * blockX2 > 0;
                var b = (b00 ? 1 : 0) | (b10 ? 2 : 0) | (b01 ? 4 : 0) | (b11 ? 8 : 0);
        
                var c00 = C3 + DX31 * blockY1 - DY31 * blockX1 > 0;
                var c10 = C3 + DX31 * blockY1 - DY31 * blockX2 > 0;
                var c01 = C3 + DX31 * blockY2 - DY31 * blockX1 > 0;
                var c11 = C3 + DX31 * blockY2 - DY31 * blockX2 > 0;
                var c = (c00 ? 1: 0) | (c10 ? 2: 0) | (c01 ? 4 : 0) | (c11 ? 8 : 0);

                // Skip block when outside an edge
                if(a == 0x0 || b == 0x0 || c == 0x0) continue;

                // Accept whole block when totally covered
                if(a == 0xF && b == 0xF && c == 0xF)
                    graphics.DrawFilledRectangle(x, y, q, q, color);
                else   // Partially covered block
                {
                    var CY1 = C1 + DX12 * blockY1 - DY12 * blockX1;
                    var CY2 = C2 + DX23 * blockY1 - DY23 * blockX1;
                    var CY3 = C3 + DX31 * blockY1 - DY31 * blockX1;

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