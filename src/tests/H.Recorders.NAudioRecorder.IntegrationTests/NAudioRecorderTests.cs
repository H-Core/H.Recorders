using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NAudio.Wave;

namespace H.Recorders.IntegrationTests
{
    [TestClass]
    public class NAudioRecorderTests
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
            CheckDevices();

            using var recorder = new NAudioRecorder();

            var provider = new BufferedWaveProvider(new WaveFormat(recorder.Rate, recorder.Bits, recorder.Channels));
            using var output = new WaveOutEvent();
            output.Init(provider);
            output.Play();

            using var recording = await recorder.StartAsync();
            recording.DataReceived += (_, bytes) =>
            {
                provider.AddSamples(bytes, 0, bytes.Length);
            };

            await Task.Delay(TimeSpan.FromSeconds(5));

            await recording.StopAsync();
        }
    }
}
