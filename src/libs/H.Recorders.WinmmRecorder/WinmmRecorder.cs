using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using H.Core.Recorders;

namespace H.Recorders
{
    public class WinmmRecorder : Recorder
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

        #region Public methods

        public override async Task StartAsync(CancellationToken cancellationToken = default)
        {
            MciSendString("open new Type waveaudio Alias recsound");
            MciSendString("record recsound");

            await base.StartAsync(cancellationToken).ConfigureAwait(false);
        }

        public override async Task StopAsync(CancellationToken cancellationToken = default)
        {
            var path = Path.GetTempFileName();
            MciSendString("save recsound " + path);
            MciSendString("close recsound ");

            if (File.Exists(path))
            {
                try
                {
                    WavData = File.ReadAllBytes(path);
                }
                finally
                {
                    File.Delete(path);
                }
            }

            await base.StopAsync(cancellationToken).ConfigureAwait(false);
        }

        #endregion
    }
}
