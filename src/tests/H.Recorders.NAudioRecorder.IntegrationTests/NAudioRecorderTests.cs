using System;
using System.Linq;
using System.Threading.Tasks;
using H.Recorders.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            using var recording = await recorder.StartWithPlaybackAsync();

            await Task.Delay(TimeSpan.FromSeconds(5));

            await recording.StopAsync();
        }
    }
}
