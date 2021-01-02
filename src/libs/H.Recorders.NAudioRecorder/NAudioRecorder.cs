using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using H.Core;
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
            SupportedSettings.Add(new AudioSettings());
            SupportedSettings.Add(new AudioSettings(AudioFormat.Wav));
            SupportedSettings.Add(new AudioSettings(AudioFormat.Mp3));
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Calls InitializeAsync if recorder is not initialized.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task<IRecording> StartAsync(AudioSettings? settings = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IRecording>(new NAudioRecording(
                settings ?? new AudioSettings(),
                DeviceNumber,
                NumberOfBuffers));
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
