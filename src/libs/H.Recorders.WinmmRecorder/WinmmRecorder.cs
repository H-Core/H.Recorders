using System.Threading;
using System.Threading.Tasks;
using H.Core;
using H.Core.Recorders;

namespace H.Recorders
{
    /// <summary>
    /// 
    /// </summary>
    public class WinmmRecorder : Recorder
    {
        #region Public methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task<IRecording> StartAsync(AudioFormat format, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IRecording>(new WinmmRecording(format));
        }

        #endregion
    }
}
