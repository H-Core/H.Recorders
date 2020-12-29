using System.Threading;
using System.Threading.Tasks;
using H.Core;
using H.Core.Players;
using H.Recorders.Extensions;
using NAudio.Wave;

namespace H.Recorders
{
    /// <summary>
    /// 
    /// </summary>
    public class NAudioPlayer : Player
    {
        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="format"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task PlayAsync(
            byte[] bytes,
            AudioFormat format = AudioFormat.Raw, 
            CancellationToken cancellationToken = default)
        {
            await bytes.PlayAsync(new WaveFormat(48000, 16, 1), cancellationToken).ConfigureAwait(false);
        }

        #endregion
    }
}
