using System.Collections.Generic;
using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.GraphicsExtensions.ParticleEffects;

/// <summary>
/// Interface for particle effects.
/// </summary>
/// <remarks>
/// To use:
/// 1. Create a List&lt;IParticle&gt; to hold particles.
/// 2. In your <c>GameState</c>'s <c>Update</c> method, call <c>YourListOfParticles.Update(gameTime);</c> to update the particles. Dead particles will be removed from the list automatically.
/// 3. In your <c>GameState</c>'s <c>Draw</c> method, call <c>YourListOfParticles.Draw(graphics);</c> to draw the particles.
/// 4. Add new particles to the list as needed.
/// </remarks>
public interface IParticle
{
    /// <summary>
    /// Whether or not the particle is still alive.
    /// </summary>
    /// <remarks>
    /// Particles that are no longer alive will be removed from a <c>List&lt;IParticle&gt;</c> when <c>Update</c> is called on that list.
    /// </remarks>
    bool IsAlive { get; }

    /// <summary>
    /// Update the particle's state.
    /// </summary>
    /// <param name="particleSpawner"></param>
    /// <param name="gameTime"></param>
    void Update(IParticleSpawner particleSpawner, GameTime gameTime);

    /// <summary>
    /// Draw the particle.
    /// </summary>
    /// <param name="graphics"></param>
    void Draw(GraphicsManager graphics);
}
