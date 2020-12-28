using System;
using System.IO;
using System.Linq;
using System.Text;
using NAudio.Wave;

namespace H.Recorders.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class WaveFormatExtensions
    {
        /// <summary>
        /// Wav header length.
        /// </summary>
        public const int WavHeaderLength = 44;

        /// <summary>
        /// Creates WAV 44 header bytes.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="fileSize"></param>
        /// <param name="dataSize"></param>
        /// <returns></returns>
        public static byte[] ToWavHeader(this WaveFormat format, int fileSize = int.MaxValue, int dataSize = int.MaxValue)
        {
            format = format ?? throw new ArgumentNullException(nameof(format));

            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream, Encoding.UTF8);

            writer.Write(Encoding.UTF8.GetBytes("RIFF"));
            writer.Write(fileSize);
            writer.Write(Encoding.UTF8.GetBytes("WAVE"));

            writer.Write(Encoding.UTF8.GetBytes("fmt "));
            format.Serialize(writer);

            writer.Write(Encoding.UTF8.GetBytes("data"));
            writer.Write(dataSize);

            stream.Position = 0;

            return stream.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static byte[] ToWavBytes(this WaveFormat format, byte[] bytes)
        {
            format = format ?? throw new ArgumentNullException(nameof(format));
            bytes = bytes ?? throw new ArgumentNullException(nameof(bytes));

            return Combine(
                format.ToWavHeader(bytes.Length + WavHeaderLength, bytes.Length),
                bytes);
        }

        /// <summary>
        /// Combines arrays.
        /// </summary>
        /// <param name="arrays"></param>
        /// <returns></returns>
        public static byte[] Combine(params byte[][] arrays)
        {
            arrays = arrays ?? throw new ArgumentNullException(nameof(arrays));

            var bytes = new byte[arrays.Sum(x => x.Length)];
            var offset = 0;

            foreach (var data in arrays)
            {
                Buffer.BlockCopy(data, 0, bytes, offset, data.Length);
                offset += data.Length;
            }

            return bytes;
        }
    }
}
