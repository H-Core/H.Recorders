using System;
using System.Threading;
using System.Threading.Tasks;
using H.Core;
using H.Core.Players;
using H.Core.Runners;
using H.Recorders.Extensions;
using NAudio.Wave;

namespace H.Recorders
{
    /// <summary>
    /// 
    /// </summary>
    public class NAudioPlayer : Runner, IPlayer
    {
        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public NAudioPlayer()
        {
            Add(new AsyncAction("play", async (command, cancellationToken) =>
            {
                var format = Enum.TryParse<AudioFormat>(
                    command.Input.Argument, true, out var result) 
                    ? result
                    : AudioFormat.Raw;
                var bytes = command.Input.Data;

                await PlayAsync(bytes, format, cancellationToken);

                return Value.Empty;
            }, "Data: bytes, Arguments: audioFormat"));
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="format"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task PlayAsync(
            byte[] bytes,
            AudioFormat format = AudioFormat.Raw, 
            CancellationToken cancellationToken = default)
        {
            await bytes.PlayAsync(new WaveFormat(48000, 16, 1), cancellationToken).ConfigureAwait(false);
        }

        #endregion
    }
}
