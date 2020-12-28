using System;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;

namespace H.Recorders.Extensions
{
    /// <summary>
    /// byte[] extensions.
    /// </summary>
    public static class BytesExtensions
    {
        /// <summary>
        /// Plays RAW or WAV bytes.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="format"></param>
        /// <param name="cancellationToken"></param>
        public static async Task PlayAsync(this byte[] bytes, WaveFormat format, CancellationToken cancellationToken = default)
        {
            bytes = bytes ?? throw new ArgumentNullException(nameof(bytes));
            format = format ?? throw new ArgumentNullException(nameof(format));

            var provider = new BufferedWaveProvider(format);
            using var output = new WaveOutEvent();
            output.Init(provider);
            output.Play();

            provider.AddSamples(bytes, 0, bytes.Length);

            await Task.Delay(TimeSpan.FromSeconds((double)bytes.Length / format.AverageBytesPerSecond), cancellationToken)
                .ConfigureAwait(false);
        }
    }
}