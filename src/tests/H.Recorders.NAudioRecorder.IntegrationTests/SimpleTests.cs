using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using H.Core;
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
            using var recording = await recorder.StartWithPlaybackAsync(cancellationToken);
            
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }

        [TestMethod]
        public async Task NoiseDetectionTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var cancellationToken = cancellationTokenSource.Token;

            CheckDevices();

            var source = new TaskCompletionSource<bool>();
            using var exceptions = new ExceptionsBag();
            using var registration = cancellationToken.Register(() => source.TrySetCanceled(cancellationToken));

            using var recorder = new NAudioRecorder();
            using var recording = await recorder.StartWithPlaybackAsync(cancellationToken);
            recording.Stopped += (_, _) => source.TrySetResult(true);
            recording.StopWhen(exceptions: exceptions);

            await source.Task;
        }

        [TestMethod]
        public async Task Mp3Test()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var cancellationToken = cancellationTokenSource.Token;
            
            CheckDevices();
            
            using var recorder = new NAudioRecorder();
            using var recording = await recorder.StartAsync(AudioFormat.Mp3, cancellationToken);

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

            using var recorder = new NAudioRecorder
            {
                Rate = 48000,
                Bits = 16,
                Channels = 1,
            };
            using var player = new NAudioPlayer();
            using var recording = await recorder.StartAsync(AudioFormat.Raw, cancellationToken);
            
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            var bytes = await recording.StopAsync(cancellationToken);

            await player.PlayAsync(bytes, AudioFormat.Raw, cancellationToken);
        }
    }
}
