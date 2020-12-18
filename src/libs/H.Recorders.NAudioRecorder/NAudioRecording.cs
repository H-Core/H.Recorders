using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using H.Core;
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
        private MemoryStream? Stream { get; }
        private WaveFileWriter? WaveFileWriter { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="waveFormat"></param>
        /// <param name="delay"></param>
        /// <param name="deviceNumber"></param>
        /// <param name="numberOfBuffers"></param>
        public NAudioRecording(RecordingFormat format, WaveFormat waveFormat, TimeSpan delay, int deviceNumber, int numberOfBuffers) :
            base(format)
        {
            WaveIn = new WaveInEvent
            {
                WaveFormat = waveFormat,
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

            if (Format == RecordingFormat.Wav)
            {
                Header = WaveIn.WaveFormat.ToWavHeader();
                Stream = new MemoryStream();
                WaveFileWriter = new WaveFileWriter(Stream, WaveIn.WaveFormat);
            }
            
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
            
            if (Format is RecordingFormat.Wav &&
                Stream is not null)
            {
                Stream.Position = 0;
                Data = Stream.ToArray();
            }

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

            if (Format is RecordingFormat.Wav &&
                Stream is not null)
            {
                Stream.Position = 0;
                Data = Stream.ToArray();
            }

            WaveFileWriter?.Dispose();
            Stream?.Dispose();
            WaveIn.Dispose();
            
            base.Dispose();
        }

        #endregion
    }
}
