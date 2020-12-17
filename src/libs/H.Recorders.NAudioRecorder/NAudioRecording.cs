using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using H.Core.Recorders;
using H.Recorders.Extensions;
using NAudio.Wave;

namespace H.Recorders
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class NAudioRecording : Recording
    {
        #region Properties

        private IWaveIn WaveIn { get; }
        private MemoryStream Stream { get; }
        private WaveFileWriter WaveFileWriter { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="delay"></param>
        /// <param name="deviceNumber"></param>
        /// <param name="numberOfBuffers"></param>
        public NAudioRecording(WaveFormat format, TimeSpan delay, int deviceNumber, int numberOfBuffers)
        {
            WaveIn = new WaveInEvent
            {
                WaveFormat = format,
                BufferMilliseconds = (int)delay.TotalMilliseconds,
                DeviceNumber = deviceNumber,
                NumberOfBuffers = numberOfBuffers,
            };
            WaveIn.DataAvailable += (_, args) =>
            {
                if (WaveFileWriter != null)
                {
                    WaveFileWriter.Write(args.Buffer, 0, args.BytesRecorded);
                    WaveFileWriter.Flush();
                }

                Data = Data.Concat(args.Buffer).ToArray();

                OnDataReceived(args.Buffer);
            };
            
            WavHeader = WaveIn.WaveFormat.ToWavHeader();
            
            Stream = new MemoryStream();
            WaveFileWriter = new WaveFileWriter(Stream, WaveIn.WaveFormat);
            
            WaveIn.StartRecording();
        }

        #endregion

        #region Public methods
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task StopAsync(CancellationToken cancellationToken = default)
        {
            WaveIn.StopRecording();
            Stream.Position = 0;
            WavData = Stream.ToArray();

            return base.StopAsync(cancellationToken);
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// 
        /// </summary>
        public override void Dispose()
        {
            WaveIn.StopRecording();
            Stream.Position = 0;
            WavData = Stream.ToArray();

            WaveFileWriter.Dispose();
            Stream.Dispose();
            WaveIn.Dispose();
            
            base.Dispose();
        }

        #endregion
    }
}
