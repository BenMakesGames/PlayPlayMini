using System.Collections.Generic;
using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.GraphicsExtensions.ParticleEffects;

/// <summary>
/// Extension methods for managing particle effects.
/// </summary>
public static class Particles
{
    public static void Update(this IList<IParticle> particles, GameTime gameTime)
    {
        for (int i = particles.Count - 1; i >= 0; i--)
        {
            particles[i].Update(gameTime);

            if(!particles[i].IsAlive)
                particles.RemoveAt(i);
        }
    }

    public static void DrawParticles(this GraphicsManager graphics, IList<IParticle> particles)
    {
        foreach(var particle in particles)
            particle.Draw(graphics);
    }
}
