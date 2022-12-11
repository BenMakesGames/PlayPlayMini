namespace BenMakesGames.PlayPlayMini.BeepBoop;

public readonly ref struct WaveOptions
{
    public int SampleRate { get; init; } = 44100;
    public float Duration { get; init; } = 1;
    public float MaxAmplitude { get; init; } = 0.5f;
    public float AttackDuration { get; init; } = 0.05f;
    public float DecayDuration { get; init; } = 0.05f;

    public WaveOptions()
    {
    }
}