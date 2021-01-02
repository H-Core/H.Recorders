using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using H.Core;
using H.Core.Recorders;
using H.Recorders.Extensions;
using NAudio.MediaFoundation;
using NAudio.Wave;

namespace H.Recorders
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class NAudioRecording : Recording
    {
        #region Properties

        private bool IsStopped { get; set; }
        private IWaveIn? WaveIn { get; set; }
        private MemoryStream? Stream { get; set; }
        private WaveFileWriter? WaveFileWriter { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="deviceNumber"></param>
        /// <param name="numberOfBuffers"></param>
        public NAudioRecording(AudioSettings settings, int deviceNumber, int numberOfBuffers) :
            base(settings)
        {
            WaveIn = new WaveInEvent
            {
                WaveFormat = new WaveFormat(settings.Rate, settings.Bits, settings.Channels),
                BufferMilliseconds = (int)settings.Delay.TotalMilliseconds,
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

            if (Settings.Format is AudioFormat.Wav or AudioFormat.Mp3)
            {
                Header = WaveIn.WaveFormat.ToWavHeader();
                Stream = new MemoryStream();
                WaveFileWriter = new WaveFileWriter(Stream, WaveIn.WaveFormat);
            }
            
            WaveIn.StartRecording();
        }

        #endregion

        #region Private methods

        private void Stop()
        {
            if (IsStopped)
            {
                return;
            }
            IsStopped = true;

            WaveIn?.Dispose();
            WaveIn = null;

            if (Settings.Format is not (AudioFormat.Wav or AudioFormat.Mp3) ||
                Stream is null)
            {
                return;
            }
            
            Stream.Position = 0;
            Data = Stream.ToArray();

            if (Settings.Format is not AudioFormat.Mp3)
            {
                return;
            }
                
            var path1 = Path.GetTempFileName();
            var path2 = $"{path1}.mp3";
            try
            {
                File.WriteAllBytes(path1, Data);

                var mediaTypes = MediaFoundationEncoder
                    .GetOutputMediaTypes(AudioSubtypes.MFAudioFormat_MP3);
                var mediaType = mediaTypes.First();
                
                using var reader = new MediaFoundationReader(path1);
                using var encoder = new MediaFoundationEncoder(mediaType);
                encoder.Encode(path2, reader);

                Data = File.ReadAllBytes(path2);
            }
            finally
            {
                foreach (var path in new[] { path1, path2 })
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task<byte[]> StopAsync(CancellationToken cancellationToken = default)
        {
            Dispose();
            
            OnStopped(Data);

            return Task.FromResult(Data);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Dispose()
        {
            Stop();

            WaveFileWriter?.Dispose();
            WaveFileWriter = null;
            Stream?.Dispose();
            Stream = null;
            WaveIn?.Dispose();
            WaveIn = null;

            base.Dispose();
        }

        #endregion
    }
}
