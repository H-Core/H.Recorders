using System;
using System.IO;
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
        /// 
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
    }
}
