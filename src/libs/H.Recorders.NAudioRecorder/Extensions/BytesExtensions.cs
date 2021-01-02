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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static double GetMaxLevel(this byte[] bytes)
        {
            bytes = bytes ?? throw new ArgumentNullException(nameof(bytes));

            var max = 0.0;
            // interpret as 16 bit audio
            for (var index = 0; index < bytes.Length; index += 2)
            {
                var sample = (short)((bytes[index + 1] << 8) |
                                      bytes[index + 0]);
                var level = Math.Abs(sample / 32768.0);
                if (level > max)
                {
                    max = level;
                }
            }

            return max;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="bits">Usually 16 or 32</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public static double GetMaxLevel(this byte[] bytes, int bits)
        {
            bytes = bytes ?? throw new ArgumentNullException(nameof(bytes));

            var max = 0.0;
            var buffer = new WaveBuffer(bytes);
            for (var index = 0; index < bytes.Length / (bits / 8); index++)
            {
                var sample = bits switch
                {
                    8 => 1.0 * buffer.ByteBuffer[index] / byte.MaxValue,
                    16 => 1.0 * buffer.ShortBuffer[index] / short.MaxValue,
                    32 => 1.0 * buffer.FloatBuffer[index],
                    _ => throw new NotImplementedException($"Bits: {bits} are unsupported.")
                };
                var level = Math.Abs(sample);
                if (level > max)
                {
                    max = level;
                }
            }

            return max;
        }
    }
}