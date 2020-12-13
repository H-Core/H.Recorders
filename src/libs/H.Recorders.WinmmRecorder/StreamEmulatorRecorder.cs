using System;
using System.Threading;
using H.Core.Attributes;
using H.Core.Recorders;

namespace H.Recorders
{
    /// <summary>
    /// 
    /// </summary>
    [AllowMultipleInstance(false)]
    public class StreamEmulatorRecorder : ParentRecorder
    {
        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public int Interval { get; }
        
        /// <summary>
        /// 
        /// </summary>
        public Timer Timer { get; }

        #endregion

        #region Events

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<byte[]>? NewPartialData;
        private void OnNewPartialData() => NewPartialData?.Invoke(this, RawData);

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="recorder"></param>
        /// <param name="interval"></param>
        public StreamEmulatorRecorder(IRecorder recorder, int interval)
        {
            Recorder = recorder;
            Interval = interval;
            
            Timer = new Timer(OnTimer, null, 0, Interval);
        }

        #endregion

        #region Event handlers

        private async void OnTimer(object sender)
        {
            if (!IsStarted)
            {
                return;
            } 

            await StopAsync().ConfigureAwait(false);
            OnNewPartialData();
            //File.WriteAllBytes($"D:/voice_{new Random().Next()}.wav", Data);
            await StartAsync().ConfigureAwait(false);
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// 
        /// </summary>
        public override void Dispose()
        {
            Timer.Dispose();

            base.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
