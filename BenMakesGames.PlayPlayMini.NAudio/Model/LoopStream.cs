using NAudio.Wave;

namespace BenMakesGames.PlayPlayMini.NAudio.Model;

/// <summary>
/// Wraps a <see cref="WaveStream" /> to provide endless looping.
/// </summary>
public sealed class LoopStream : WaveStream
{
    private WaveStream SourceStream { get; }

    /// <summary>
    /// Creates a new Loop stream
    /// </summary>
    /// <param name="sourceStream">The stream to read from. Note: the Read method of this stream should return 0 when it reaches the end
    /// or else we will not loop to the start again.</param>
    public LoopStream(WaveStream sourceStream)
    {
        SourceStream = sourceStream;
    }

    /// <summary>
    /// Use this to turn looping on or off
    /// </summary>
    public bool EnableLooping { get; set; } = true;

    /// <summary>
    /// Return source stream's wave format
    /// </summary>
    public override WaveFormat WaveFormat => SourceStream.WaveFormat;

    /// <summary>
    /// LoopStream simply returns
    /// </summary>
    public override long Length => SourceStream.Length;

    /// <summary>
    /// LoopStream simply passes on positioning to source stream
    /// </summary>
    public override long Position
    {
        get => SourceStream.Position;
        set => SourceStream.Position = value;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int totalBytesRead = 0;

        while (totalBytesRead < count)
        {
            int bytesRead = SourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);

            if (bytesRead == 0)
            {
                if (SourceStream.Position == 0 || !EnableLooping)
                {
                    // something wrong with the source stream
                    break;
                }

                // loop
                SourceStream.Position = 0;
            }

            totalBytesRead += bytesRead;
        }

        return totalBytesRead;
    }
}
