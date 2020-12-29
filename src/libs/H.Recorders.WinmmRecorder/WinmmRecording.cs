using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using H.Core;
using H.Core.Recorders;

namespace H.Recorders
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class WinmmRecording : Recording
    {
        #region Private methods

        // Winmm.dll is used for recording speech
        [DllImport("winmm.dll", EntryPoint = "mciSendStringA", CharSet = CharSet.Unicode)]
        private static extern int MciSendString(string lpstrCommand, string lpstrReturnString, int uReturnLength, int hwndCallback);

        private static void MciSendString(string command)
        {
            var result = MciSendString(command, "", 0, 0);
            if (result != 0)
            {
                throw new InvalidOperationException($"MciSendString returns bad result: {result}");
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        public WinmmRecording(AudioFormat format) :
            base(format)
        {
            MciSendString("open new Type waveaudio Alias recsound");
            MciSendString("record recsound");
        }

        #endregion

        #region Public methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task<byte[]> StopAsync(CancellationToken cancellationToken = default)
        {
            var path = Path.GetTempFileName();
            MciSendString("save recsound " + path);
            MciSendString("close recsound ");

            if (!File.Exists(path))
            {
                throw new InvalidOperationException("File is not exists.");
            }

            try
            {
                return Task.FromResult(File.ReadAllBytes(path));
            }
            finally
            {
                File.Delete(path);
            }
        }

        #endregion
    }
}
