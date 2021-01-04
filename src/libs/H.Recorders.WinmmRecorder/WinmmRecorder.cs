using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using H.Core;
using H.Core.Recorders;

namespace H.Recorders
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class WinmmRecorder : Recorder
    {
        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public WinmmRecorder()
        {
            SupportedSettings.Add(new AudioSettings(AudioFormat.Wav));
        }

        #endregion

        #region Public methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task<IRecording> StartAsync(AudioSettings? settings = null, CancellationToken cancellationToken = default)
        {
            settings ??= SupportedSettings.First();

            return Task.FromResult<IRecording>(new WinmmRecording(settings));
        }

        #endregion
    }
}
