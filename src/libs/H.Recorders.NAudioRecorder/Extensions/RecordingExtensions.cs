using System;
using System.Threading;
using System.Threading.Tasks;
using H.Core;
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
        /// <param name="settings"></param>
        public static IRecording WithPlayback(this IRecording recording, AudioSettings? settings = null)
        {
            recording = recording ?? throw new ArgumentNullException(nameof(recording));
            settings ??= new AudioSettings();

            var provider = new BufferedWaveProvider(new WaveFormat(settings.Rate, settings.Bits, settings.Channels));
            var output = new WaveOutEvent();
            output.Init(provider);
            output.Play();

            recording.DataReceived += (_, bytes) =>
            {
                provider.AddSamples(bytes, 0, bytes.Length);
            };
            recording.Disposed += (_, _) => output.Dispose();

            return recording;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="recorder"></param>
        /// <param name="settings"></param>
        /// <param name="cancellationToken"></param>
        public static async Task<IRecording> StartWithPlaybackAsync(
            this NAudioRecorder recorder,
            AudioSettings? settings = null,
            CancellationToken cancellationToken = default)
        {
            recorder = recorder ?? throw new ArgumentNullException(nameof(recorder));

            var recording = await recorder.StartAsync(settings, cancellationToken);

            return recording.WithPlayback(settings);
        }
    }
}
