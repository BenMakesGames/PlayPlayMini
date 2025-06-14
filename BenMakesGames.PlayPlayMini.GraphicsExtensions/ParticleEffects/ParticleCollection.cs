using System.Collections.Generic;
using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.GraphicsExtensions.ParticleEffects;

public interface IParticleSpawner
{
    /// <summary>
    /// Queues particles to be added in the next Update cycle.
    /// </summary>
    /// <param name="particle"></param>
    void AddParticles(params IEnumerable<IParticle> particle);
}

/// <summary>
/// For updating and drawing a collection of particles.
/// </summary>
/// <remarks>
/// If you want different layers of particles (e.g. background, foreground), create multiple instances of this class,
/// update them all, and draw them in the desired order.
/// </remarks>
public sealed class ParticleCollection: IParticleSpawner
{
    private const int InitialParticleCapacity = 256;
    private const int InitialSpawnQueueCapacity = 128;

    private List<IParticle> Particles { get; } = new(InitialParticleCapacity);
    private List<IParticle> SpawnQueue { get; } = new(InitialSpawnQueueCapacity);

    /// <inheritdoc />
    public void AddParticles(params IEnumerable<IParticle> particle)
    {
        SpawnQueue.AddRange(particle);
    }

    /// <summary>
    /// Updates all particles in the collection.
    /// </summary>
    /// <remarks>
    /// For better performance, call this in your GameState's FixedUpdate method rather than its Update method.
    /// </remarks>
    /// <param name="gameTime"></param>
    public void Update(GameTime gameTime)
    {
        Particles.AddRange(SpawnQueue);

        SpawnQueue.Clear();

        for(int i = Particles.Count - 1; i >= 0; i--)
        {
            var particle = Particles[i];

            if (particle.IsAlive)
                particle.Update(this, gameTime);
            else
                Particles.RemoveAt(i);
        }
    }

    /// <summary>
    /// Draws all particles in the collection.
    /// </summary>
    /// <remarks>
    /// Call in your GameState's Draw method.
    /// </remarks>
    /// <param name="graphics"></param>
    public void Draw(GraphicsManager graphics)
    {
        foreach (var particle in Particles)
            particle.Draw(graphics);
    }
}
