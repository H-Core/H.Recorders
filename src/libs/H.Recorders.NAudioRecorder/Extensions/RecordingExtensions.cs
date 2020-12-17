using System;
using System.Threading;
using System.Threading.Tasks;
using H.Core.Recorders;
using NAudio.Wave;

namespace H.Recorders.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class RecordingExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="recording"></param>
        /// <param name="format"></param>
        public static void WithPlayback(this IRecording recording, WaveFormat format)
        {
            recording = recording ?? throw new ArgumentNullException(nameof(recording));
            
            var provider = new BufferedWaveProvider(format);
            var output = new WaveOutEvent();
            output.Init(provider);
            output.Play();
            
            recording.DataReceived += (_, bytes) =>
            {
                provider.AddSamples(bytes, 0, bytes.Length);
            };
            recording.Stopped += (_, _) => output.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="recorder"></param>
        /// <param name="cancellationToken"></param>
        public static async Task<IRecording> StartWithPlaybackAsync(
            this NAudioRecorder recorder, 
            CancellationToken cancellationToken = default)
        {
            recorder = recorder ?? throw new ArgumentNullException(nameof(recorder));

            var recording = await recorder.StartAsync(cancellationToken);
            recording.WithPlayback(recorder.WaveFormat);

            return recording;
        }
    }
}
