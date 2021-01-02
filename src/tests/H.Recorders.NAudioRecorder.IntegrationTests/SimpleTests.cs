using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using H.Core;
using H.Core.Recorders;
using H.Core.Utilities;
using H.Recorders.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.Recorders.IntegrationTests
{
    [TestClass]
    public class SimpleTests
    {
        private static void CheckDevices()
        {
            var devices = NAudioRecorder.GetAvailableDevices().ToList();
            if (!devices.Any())
            {
                Assert.Inconclusive("No available devices for NAudioRecorder.");
            }

            Console.WriteLine("Available devices:");
            foreach (var device in devices)
            {
                Console.WriteLine($" - Name: {device.ProductName}, Channels: {device.Channels}");
            }
        }

        [TestMethod]
        public async Task RealTimePlayRecordTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var cancellationToken = cancellationTokenSource.Token;

            CheckDevices();

            using var recorder = new NAudioRecorder();
            using var recording = await recorder.StartWithPlaybackAsync(null, cancellationToken);
            
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }

        [TestMethod]
        public async Task SilenceDetectionTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var cancellationToken = cancellationTokenSource.Token;

            CheckDevices();

            var source = new TaskCompletionSource<bool>();
            using var exceptions = new ExceptionsBag();
            using var registration = cancellationToken.Register(() => source.TrySetCanceled(cancellationToken));

            using var recorder = new NAudioRecorder();
            using var recording = await recorder.StartWithPlaybackAsync(null, cancellationToken);
            recording.Stopped += (_, _) => source.TrySetResult(true);
            recording.StopWhenSilence(exceptions: exceptions);

            await source.Task;
        }

        [TestMethod]
        public async Task Mp3Test()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var cancellationToken = cancellationTokenSource.Token;
            
            CheckDevices();
            
            using var recorder = new NAudioRecorder();
            using var recording = await recorder.StartAsync(new AudioSettings(AudioFormat.Mp3), cancellationToken);

            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            await recording.StopAsync(cancellationToken);
            
            File.WriteAllBytes("D:/test.mp3", recording.Data);
        }

        [TestMethod]
        public async Task PlayTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var cancellationToken = cancellationTokenSource.Token;

            CheckDevices();

            var settings = new AudioSettings();
            using var recorder = new NAudioRecorder();
            using var player = new NAudioPlayer();
            using var recording = await recorder.StartAsync(settings, cancellationToken);
            
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            var bytes = await recording.StopAsync(cancellationToken);

            await player.PlayAsync(bytes, settings, cancellationToken);
        }
    }
}
