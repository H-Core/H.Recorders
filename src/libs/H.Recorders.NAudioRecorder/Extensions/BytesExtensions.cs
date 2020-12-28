using System;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;

namespace H.Recorders.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class BytesExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="format"></param>
        /// <param name="cancellationToken"></param>
        public static async Task PlayAsync(this byte[] bytes, WaveFormat format, CancellationToken cancellationToken = default)
        {
            bytes = bytes ?? throw new ArgumentNullException(nameof(bytes));
            format = format ?? throw new ArgumentNullException(nameof(format));

            var source = new TaskCompletionSource<bool>(false);
            using var registration = cancellationToken.Register(() => source.TrySetCanceled());

            var provider = new BufferedWaveProvider(format);
            using var output = new WaveOutEvent();
            output.PlaybackStopped += (_, _) => source.TrySetResult(true);
            output.Init(provider);
            output.Play();

            provider.AddSamples(bytes, 0, bytes.Length);

            await source.Task.ConfigureAwait(false);
        }
    }
}
