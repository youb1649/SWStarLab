using System;
using System.Threading;
using System.Diagnostics;

namespace MNF
{
    public class ThreadAdapter : IDisposable
    {
        Thread messageDispatchThread = null;

        public delegate void ThreadEventHandler(bool isSignal);
        public event ThreadEventHandler ThreadEvent;

        AutoResetEvent messageEvent = null;
        bool isStop = false;
        int waitTime = 100;

        public int WaitTime
        {
            get { return waitTime; }
            set { waitTime = value; messageEvent.Set(); }
        }

        public ThreadAdapter(AutoResetEvent messageEvent)
        {
            this.messageEvent = messageEvent;
        }

        public bool Start()
        {
            LogManager.Instance.Write("ThreadAdapter.Start()");

            Debug.Assert(isStop == false);

            if (messageEvent == null)
                messageEvent = new AutoResetEvent(false);

            if (messageDispatchThread != null)
                return false;

            messageDispatchThread = new Thread(RunThread);
            messageDispatchThread.Start();

            isStop = false;

            return true;
        }

        public void Stop()
        {
            LogManager.Instance.Write("ThreadAdapter.Stop()");

            isStop = true;

            if (messageEvent != null)
                messageEvent.Set();

            messageDispatchThread.Join();
        }

        public bool IsRunning()
        {
            return (isStop == false);
        }

        void RunThread()
        {
            while (isStop == false)
            {
                ThreadEvent(messageEvent.WaitOne(waitTime));
            }
        }

        #region IDisposable Support
        bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            LogManager.Instance.Write("ThreadAdapter.Dispose()");

            if (!disposedValue)
            {
                if (disposing)
                {
                    LogManager.Instance.Write("ThreadAdapter.Dispose(bool disposing) - disposing");
                    // TODO: dispose managed state (managed objects).
                    if (messageEvent != null)
                    {
                        messageEvent.Close();
                        messageEvent = null;
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ThreadAdapter() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            LogManager.Instance.Write("ThreadAdapter.Dispose()");
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
