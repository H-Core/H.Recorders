using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using H.Core.Recorders;
using NAudio.Wave;

namespace H.Recorders
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class NAudioRecorder : Recorder
    {
        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public int Rate { get; set; } = 8000;
        
        /// <summary>
        /// 
        /// </summary>
        public int Bits { get; set; } = 16;
        
        /// <summary>
        /// 
        /// </summary>
        public int Channels { get; set; } = 1;

        /// <summary>
        /// 
        /// </summary>
        public TimeSpan Delay { get; set; } = TimeSpan.FromMilliseconds(10); // 100 is recommended.

        /// <summary>
        /// 
        /// </summary>
        public int DeviceNumber { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int NumberOfBuffers { get; set; } = 3;

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public NAudioRecorder()
        {
            AddSetting(nameof(Rate), o => Rate = o, NotNegative, 8000);
            AddSetting(nameof(Bits), o => Bits = o, NotNegative, 16);
            AddSetting(nameof(Channels), o => Channels = o, NotNegative, 1);
        }

        #endregion

        #region Public methods
        
        /// <summary>
        /// Calls InitializeAsync if recorder is not initialized.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<IRecording> StartAsync(CancellationToken cancellationToken = default)
        {
            if (!IsInitialized)
            {
                await InitializeAsync(cancellationToken).ConfigureAwait(false);
            }

            return new NAudioRecording(
                new WaveFormat(Rate, Bits, Channels),
                Delay,
                DeviceNumber,
                NumberOfBuffers);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<WaveInCapabilities> GetAvailableDevices()
        {
            return Enumerable
                .Range(0, WaveInEvent.DeviceCount)
                .Select(WaveInEvent.GetCapabilities);
        }

        #endregion
    }
}
