using System;
using System.Linq;
using System.Threading.Tasks;
using H.Core.Managers;
using H.Core.Recorders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NAudio.Wave;

namespace H.Recorders.IntegrationTests
{
    [TestClass]
    public class NAudioRecorderTests
    {
        private static bool CheckDevices()
        {
            var devices = NAudioRecorder.GetAvailableDevices();
            if (!devices.Any())
            {
                return false;
            }

            Console.WriteLine("Available devices:");
            foreach (var device in devices)
            {
                Console.WriteLine($" - Name: {device.Name}, Channels: {device.Channels}");
            }

            return true;
        }

        [TestMethod]
        public async Task RealTimePlayRecordTest()
        {
            if (!CheckDevices())
            {
                return;
            }

            using var recorder = new NAudioRecorder();
            await recorder.InitializeAsync();

            var provider = new BufferedWaveProvider(new WaveFormat(recorder.Rate, recorder.Bits, recorder.Channels));
            using var output = new WaveOutEvent();
            output.Init(provider);
            output.Play();

            recorder.RawDataReceived += (_, args) =>
            {
                if (args.RawData == null)
                {
                    return;
                }

                provider.AddSamples(args.RawData.ToArray(), 0, args.RawData.Count);
            };
            await recorder.StartAsync();
            
            await Task.Delay(TimeSpan.FromMilliseconds(5000));

            await recorder.StopAsync();
        }

        [TestMethod]
        public async Task ManagerTest()
        {
            if (!CheckDevices())
            {
                return;
            }

            using var recorder = new NAudioRecorder();
            using var manager = new BaseManager
            {
                Recorder = recorder
            };

            await manager.ChangeWithTimeoutAsync(TimeSpan.FromSeconds(5));
        }
    }
}
