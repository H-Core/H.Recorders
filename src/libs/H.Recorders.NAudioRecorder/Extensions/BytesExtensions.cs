using System;
using System.Threading;
using System.Threading.Tasks;
using H.Core;
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
        /// <param name="settings"></param>
        /// <param name="cancellationToken"></param>
        public static async Task PlayAsync(this byte[] bytes, AudioSettings settings, CancellationToken cancellationToken = default)
        {
            bytes = bytes ?? throw new ArgumentNullException(nameof(bytes));
            settings = settings ?? throw new ArgumentNullException(nameof(settings));

            var provider = new BufferedWaveProvider(new WaveFormat(settings.Rate, settings.Bits, settings.Channels));
            using var output = new WaveOutEvent();
            output.Init(provider);
            output.Play();
            
            provider.AddSamples(bytes, 0, bytes.Length);

            var averageBytesPerSecond = settings.Rate * settings.Channels * (settings.Bits / 8);

            await Task.Delay(TimeSpan.FromSeconds((double)bytes.Length / averageBytesPerSecond), cancellationToken)
                .ConfigureAwait(false);
        }
    }
}