using System.Threading;
using System.Threading.Tasks;
using H.Core;
using H.Core.Players;
using H.Recorders.Extensions;

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
        /// <param name="settings"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task PlayAsync(
            byte[] bytes,
            AudioSettings? settings = null, 
            CancellationToken cancellationToken = default)
        {
            settings ??= new AudioSettings();

            await bytes.PlayAsync(settings, cancellationToken).ConfigureAwait(false);
        }

        #endregion
    }
}
