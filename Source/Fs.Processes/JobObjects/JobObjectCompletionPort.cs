using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace Fs.Processes.JobObjects
{
    internal class JobObjectCompletionPort : IDisposable
    {
        private static readonly object _completionPortLock = new object();
        private static IoCompletionPort _completionPort;
        private static int _completionPortCount;

        private IoCompletionPort _ioCompletionPort;

        private JobObjectCompletionPort ()
        {
        }

        protected virtual void Dispose ( bool disposing )
        {
            IoCompletionPort completionPort = Interlocked.Exchange(ref _ioCompletionPort, null);
            if (completionPort != null)
            {
                lock (_completionPortLock)
                    ReleaseIoCompletionPort();
            }
        }

        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void SetCompletionAction ( IntPtr completionKey, Action<Interop.Kernel32.JobObjectMessage, IntPtr> completionAction )
        {
            var completionPort = _ioCompletionPort;
            if (completionPort == null)
                throw new ObjectDisposedException(GetType().Name);

            if (completionAction == null)
                completionPort.FreeCompletionKey(completionKey);
            else
                completionPort.AllocateCompletionKey(completionKey, completionAction);
        }

        public static JobObjectCompletionPort GetCompletionPort ()
        {
            lock (_completionPortLock)
                return new JobObjectCompletionPort()
                {
                    _ioCompletionPort = AcquireIoCompletionPort()
                };
        }

        private static IoCompletionPort AcquireIoCompletionPort ()
        {
            System.Diagnostics.Debug.Assert(Monitor.IsEntered(_completionPortLock));

            if (_completionPortCount == 0)
                _completionPort = new IoCompletionPort();

            _completionPortCount++;
            return _completionPort;
        }

        private static void ReleaseIoCompletionPort ()
        {
            System.Diagnostics.Debug.Assert(Monitor.IsEntered(_completionPortLock));
            System.Diagnostics.Debug.Assert(_completionPortCount > 0);

            if (_completionPortCount == 1)
            {
                _completionPort.Dispose();
                _completionPort = null;
            }

            _completionPortCount--;
        }

        public SafeIoCompletionPortHandle Handle
        {
            get
            {
                var completionPort = _ioCompletionPort;
                if (completionPort == null)
                    throw new ObjectDisposedException(GetType().Name);

                return completionPort.Handle;
            }
        }

        private class IoCompletionPort
        {
            private readonly Thread _workerThread;
            private readonly Dictionary<IntPtr, Action<Interop.Kernel32.JobObjectMessage, IntPtr>> _completionKeys;

            public IoCompletionPort ()
            {
                _completionKeys = new Dictionary<IntPtr, Action<Interop.Kernel32.JobObjectMessage, IntPtr>>();

                Handle = Interop.Kernel32.CreateIoCompletionPort((IntPtr)(-1), IntPtr.Zero, IntPtr.Zero, 0);
                if ((Handle == null) || (Handle.IsInvalid))
                    throw Errors.Win32Error();

                try
                {
                    // create and start the worker thread..
                    _workerThread = new Thread(IoCompletionPortReader);
                    _workerThread.IsBackground = true;
                    _workerThread.Start();
                }
                catch
                {
                    Handle.Dispose();
                    throw;
                }
            }

            public void Dispose ()
            {
                try
                {
                    if ((!Handle.IsClosed) && (!Handle.IsInvalid))
                    {
                        // post a message to the IOCP to signal the reader should terminate, we only start one
                        // workers, so we only need to post one message. If PostQueuedCompletionStatus fails, we just
                        // move on, because we're not allowed to throw exceptions out of Dispose..

                        if (Interop.Kernel32.PostQueuedCompletionStatus(Handle, 0, IntPtr.Zero, IntPtr.Zero))
                            _workerThread.Join();
                    }
                }
                finally
                {
                    Handle?.Dispose();
                }
            }

            public void AllocateCompletionKey ( IntPtr completionKey, Action<Interop.Kernel32.JobObjectMessage, IntPtr> completionAction )
            {
                if ((completionKey == IntPtr.Zero) || (completionKey == (IntPtr)(-1)))
                    throw new ArgumentOutOfRangeException(nameof(completionKey));

                if (completionAction == null)
                    throw new ArgumentNullException(nameof(completionAction));

                lock (_completionKeys)
                {
                    if (_completionKeys.TryGetValue(completionKey, out var existingAction))
                        throw new InvalidOperationException(Resources.DuplicateCompletionKey);

                    _completionKeys[completionKey] = completionAction;
                }

            }

            public void FreeCompletionKey ( IntPtr completionKey )
            {
                lock (_completionKeys)
                    _completionKeys.Remove(completionKey);
            }

            private void IoCompletionPortReader ()
            {
                while (true)
                {
                    if (Interop.Kernel32.GetQueuedCompletionStatus(Handle, out var numberOfBytes, out var completionKey, out var overlapped, Timeout.Infinite))
                    {
                        // dequeued a successful operation, check for posted quit message before trying to
                        // dispatch the completion..

                        if ((completionKey == IntPtr.Zero) && (overlapped == IntPtr.Zero) && (numberOfBytes == 0))
                            break;

                        Action<Interop.Kernel32.JobObjectMessage, IntPtr> completionAction = null;
                        lock (_completionKeys)
                            if ((!_completionKeys.TryGetValue(completionKey, out completionAction)) ||
                                (completionAction == null))
                                continue;
                        try
                        {
                            completionAction((Interop.Kernel32.JobObjectMessage)numberOfBytes, overlapped);
                        }
                        catch (Exception ex)
                        {
                            // the callback threw and exception, but we don't want to propagate it on this thread,
                            // so pass it off to a thread pool thread and let it fly..

                            ThreadPool.UnsafeQueueUserWorkItem(edi => ((ExceptionDispatchInfo)edi).Throw(), ExceptionDispatchInfo.Capture(ex));
                        }
                    }
                    else if (overlapped == IntPtr.Zero)
                    {
                        // overlapped will be non-zero if a failed operation is dequeued, so this indicates
                        // GetQueuedCompletionStatus actually failed .. which we need to deal with.

                        int completionError = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
                        if (completionError == Interop.Errors.ERROR_ABANDONED_WAIT_0)
                            break; // IoCompletionPort handle has been closed

                        // throw the error (most likely killing the application)
                        throw Errors.Win32Error(completionError);
                    }
                }
            }

            public SafeIoCompletionPortHandle Handle { get; }
        }
    }
}
