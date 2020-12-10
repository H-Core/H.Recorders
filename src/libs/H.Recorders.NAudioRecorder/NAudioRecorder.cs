using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using H.Core.Recorders;
using NAudio.Wave;

namespace H.Recorders
{
    /// <summary>
    /// 
    /// </summary>
    public class NAudioRecorder : Recorder
    {
        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public int Rate { get; set; } = 8000;
        
        /// <summary>
        /// 
        /// </summary>
        public int Bits { get; set; } = 16;
        
        /// <summary>
        /// 
        /// </summary>
        public int Channels { get; set; } = 1;

        private IWaveIn? WaveIn { get; set; }
        private MemoryStream? Stream { get; set; }
        private WaveFileWriter? WaveFileWriter { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public NAudioRecorder()
        {
            AddSetting(nameof(Rate), o => Rate = o, NotNegative, 8000);
            AddSetting(nameof(Bits), o => Bits = o, NotNegative, 16);
            AddSetting(nameof(Channels), o => Channels = o, NotNegative, 1);
        }

        #endregion

        #region Public methods
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            if (IsInitialized)
            {
                return Task.CompletedTask;
            }
            
            IsInitialized = true;

            WaveIn ??= new WaveInEvent
            {
                WaveFormat = new WaveFormat(Rate, Bits, Channels)
            };

            WaveIn.DataAvailable += (_, args) =>
            {
                if (WaveFileWriter != null)
                {
                    WaveFileWriter.Write(args.Buffer, 0, args.BytesRecorded);
                    WaveFileWriter.Flush();
                }

                RawData = RawData.Concat(args.Buffer).ToArray();

                OnRawDataReceived(args.Buffer);
            };

            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream, Encoding.UTF8);

            // Fake Wav header of current format
            writer.Write(Encoding.UTF8.GetBytes("RIFF"));
            writer.Write(int.MaxValue);
            writer.Write(Encoding.UTF8.GetBytes("WAVE"));

            writer.Write(Encoding.UTF8.GetBytes("fmt "));
            WaveIn.WaveFormat.Serialize(writer);

            writer.Write(Encoding.UTF8.GetBytes("data"));
            writer.Write(int.MaxValue);

            stream.Position = 0;
            WavHeader = stream.ToArray();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Calls InitializeAsync if recorder is not initialized.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (IsStarted)
            {
                return;
            }
            if (!IsInitialized)
            {
                await InitializeAsync(cancellationToken).ConfigureAwait(false);
            }

            WaveIn = WaveIn ?? throw new InvalidOperationException("WaveIn is null");

            WaveFileWriter?.Dispose();
            Stream?.Dispose();

            Stream = new MemoryStream();
            WaveFileWriter = new WaveFileWriter(Stream, WaveIn.WaveFormat);

            WaveIn.StartRecording();

            await base.StartAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task StopAsync(CancellationToken cancellationToken = default)
        {
            WaveIn?.StopRecording();

            if (Stream != null)
            {
                Stream.Position = 0;

                WavData = Stream.ToArray();
            }

            await base.StopAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<WaveInCapabilities> GetAvailableDevices()
        {
            return Enumerable
                .Range(0, WaveInEvent.DeviceCount)
                .Select(WaveInEvent.GetCapabilities);
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// 
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();

            WaveFileWriter?.Dispose();
            WaveFileWriter = null;

            Stream?.Dispose();
            Stream = null;

            WaveIn?.Dispose();
            WaveIn = null;

            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
