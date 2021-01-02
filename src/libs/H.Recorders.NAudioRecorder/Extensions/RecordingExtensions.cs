using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using H.Core;
using H.Core.Recorders;
using H.Core.Utilities;
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
        public static IRecording WithPlayback(this IRecording recording, WaveFormat format)
        {
            recording = recording ?? throw new ArgumentNullException(nameof(recording));
            format = format ?? throw new ArgumentNullException(nameof(format));

            var provider = new BufferedWaveProvider(format);
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
        /// Stop when <paramref name="requiredCount"></paramref> of data received will be lower than <paramref name="threshold"></paramref>. <br/>
        /// NAudioRecorder produces 100 DataReceived events per seconds. <br/>
        /// It can be set up by NAudioRecorder.Delay property. <br/>
        /// </summary>
        /// <param name="recording"></param>
        /// <param name="threshold"></param>
        /// <param name="exceptions"></param>
        /// <param name="bufferSize"></param>
        /// <param name="requiredCount"></param>
        /// <param name="bits">Bits of the sample in the DataReceived.</param>
        public static IRecording StopWhen(
            this IRecording recording, 
            int bufferSize = 300, 
            int requiredCount = 250,
            double threshold = 0.02,
            int bits = 16,
            ExceptionsBag? exceptions = null)
        {
            recording = recording ?? throw new ArgumentNullException(nameof(recording));

            var last = new ConcurrentQueue<double>();
            recording.DataReceived += async (_, bytes) =>
            {
                try
                {
                    var max = bytes.GetMaxLevel(bits);

                    last.Enqueue(max);
                    if (last.Count <= bufferSize)
                    {
                        return;
                    }

                    last.TryDequeue(out var _);
                    if (last.ToArray().Count(value => value < threshold) >= requiredCount)
                    {
                        await recording.StopAsync().ConfigureAwait(false);
                    }
                }
                catch (Exception exception)
                {
                    exceptions?.OnOccurred(exception);
                }
            };

            return recording;
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

            var recording = await recorder.StartAsync(AudioFormat.Raw, cancellationToken);

            return recording.WithPlayback(recorder.WaveFormat);
        }
    }
}
