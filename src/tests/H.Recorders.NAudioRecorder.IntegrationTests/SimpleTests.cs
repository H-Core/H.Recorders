using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using H.Core;
using H.Recorders.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NAudio.Wave;

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
        public async Task Mp3Test()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var cancellationToken = cancellationTokenSource.Token;
            
            CheckDevices();
            
            using var recorder = new NAudioRecorder();
            using var recording = await recorder.StartAsync(RecordingFormat.Mp3, cancellationToken);

            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            await recording.StopAsync(cancellationToken);
            
            File.WriteAllBytes("D:/test.mp3", recording.Data);
        }
    }
}
