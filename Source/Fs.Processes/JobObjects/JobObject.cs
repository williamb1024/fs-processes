using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace Fs.Processes.JobObjects
{
    /// <summary>
    /// Represents a Windows JobObject.
    /// </summary>
    public sealed class JobObject : JobObjectInfo, IDisposable
    {
        private static readonly JobObjectInfo _currentJob = new CurrentJobObject();

        private readonly SafeJobObjectHandle _handle;
        private readonly JobObjectCompletionPort _completionPort;

        /// <summary>
        /// Creates a new <see cref="JobObject"/> instance.
        /// </summary>
        /// <param name="Limits">The <see cref="JobLimits"/> to apply to the new <see cref="JobObject"/>.</param>
        /// <param name="Notifications">The <see cref="JobNotifications"/> to apply to the new <see cref="JobObject"/>.</param>
        public JobObject ( JobLimits? Limits = null, JobNotifications? Notifications = null )
        {
            try
            {
                // get the completion port reference..
                _completionPort = JobObjectCompletionPort.GetCompletionPort();

                // create the handle..
                _handle = Interop.Kernel32.CreateJobObject(IntPtr.Zero, null);
                if ((_handle == null) || (_handle.IsInvalid))
                    throw Errors.Win32Error();

                // join the handle to the completion port..
                AssociateWithPort();

                // configure limits..
                if (Limits.HasValue) SetLimits(Limits.Value);
                if ((Notifications.HasValue) && (_jobsLimitViolationSupported)) SetNotifications(Notifications.Value);
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        private void Dispose ( bool disposing )
        {
            // remove completion handler before disposing handle, this should prevent a possible race with
            // another job object recyclying the handle value..

            if ((_handle != null) && (!_handle.IsClosed) && (!_handle.IsInvalid) && (_completionPort != null))
                _completionPort.SetCompletionAction(_handle.DangerousGetHandle(), null);

            _handle?.Dispose();
            _completionPort?.Dispose();
        }

        /// <summary>
        /// Releases all resources associated with the instance.
        /// </summary>
        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Creates a new process and assigns the process to the job object.
        /// </summary>
        /// <param name="createProcessInfo">The <see cref="CreateProcessInfo"/> that contains information this is
        /// used to start the process, including the file name and any command-line arguments.</param>
        /// <returns>A <see cref="Process"/> instance that represents the newly created process.</returns>
        public Process CreateProcess ( CreateProcessInfo createProcessInfo )
        {
            if (createProcessInfo == null)
                throw new ArgumentNullException(nameof(createProcessInfo));

            return CreateProcess(createProcessInfo, ProcessOptions.None);
        }

        /// <summary>
        /// Creates a new process and assigns the process to the job object.
        /// </summary>
        /// <param name="createProcessInfo">The <see cref="CreateProcessInfo"/> that contains information this is
        /// used to start the process, including the file name and any command-line arguments.</param>
        /// <param name="processOptions">A set of <see cref="ProcessOptions"/> that controls how the new process
        /// is created.</param>
        /// <returns>A <see cref="Process"/> instance that represents the newly created process.</returns>
        public Process CreateProcess ( CreateProcessInfo createProcessInfo, ProcessOptions processOptions )
        {
            if (createProcessInfo == null)
                throw new ArgumentNullException(nameof(createProcessInfo));

            CheckDisposed();

            Process newProcess = null;
            try
            {
                // create the process, forcing it to be suspened..
                newProcess = new Process(createProcessInfo, processOptions | ProcessOptions.Suspended);

                // assign the process to this job object..
                if (!Interop.Kernel32.AssignProcessToJobObject(Handle, newProcess.Handle))
                    throw Errors.Win32Error();

                // if the process wasn't requested to be started in a suspended state then
                // resume the process now..

                if ((processOptions & ProcessOptions.Suspended) == 0)
                    newProcess.Resume();
            }
            catch
            {
                // if assignment fails, kill the process and dispose of it..
                newProcess?.Kill();
                newProcess?.Dispose();
                throw;
            }

            return newProcess;
        }

        /// <summary>
        /// Assigns a process to an existing job object.
        /// </summary>
        /// <param name="hProcess">A handle to the process to associate with the job object. The handle
        /// must have PROCESS_SET_QUOTA and PROCESS_TERMINATE access rights.</param>
        public void AssignProcess ( IntPtr hProcess )
        {
            if ((hProcess == IntPtr.Zero) || (hProcess == (IntPtr)(-1)))
                throw new ArgumentException(Resources.ProcessHandleClosedOrInvalid);

            CheckDisposed();
            using (var processHandle = new SafeProcessHandle(hProcess, false))
                AssignProcess(processHandle);
        }

        /// <summary>
        /// Assigns a process to an existing job object.
        /// </summary>
        /// <param name="processHandle">A handle to the process to associate with the job object. The handle
        /// must have PROCESS_SET_QUOTA and PROCESS_TERMINATE access rights.</param>
        public void AssignProcess ( SafeProcessHandle processHandle )
        {
            if (processHandle == null)
                throw new ArgumentNullException(nameof(processHandle));

            if ((processHandle.IsClosed) || (processHandle.IsInvalid))
                throw new ArgumentException(Resources.ProcessHandleClosedOrInvalid);

            CheckDisposed();

            SafeProcessHandle newProcessHandle = null;
            try
            {
                if (!Interop.Kernel32.DuplicateHandle(Interop.Kernel32.GetCurrentProcessIntPtr(),
                                                      processHandle,
                                                      Interop.Kernel32.GetCurrentProcessIntPtr(),
                                                      out newProcessHandle,
                                                      Interop.ProcessAccess.SetQuota | Interop.ProcessAccess.Terminate,
                                                      false,
                                                      0))
                    throw Errors.Win32Error();

                if (!Interop.Kernel32.AssignProcessToJobObject(Handle, newProcessHandle))
                    throw Errors.Win32Error();
            }
            finally
            {
                newProcessHandle?.Dispose();
            }
        }

        /// <summary>
        /// Grants or denies access to a handle to a User object to a job that has a user-interface restrictions. When access is 
        /// granted, all processes associated with the job can subsequently recognize and use the handle. When access is denied, the
        /// processes can no longer use the handle.
        /// </summary>
        /// <param name="objectHandle">A handle to the User object.</param>
        /// <param name="allowAccess"><c>true</c> to grant access to the handle, <c>false</c> to deny access.</param>
        public void GrantUserHandleAccess ( SafeHandle objectHandle, bool allowAccess = true )
        {
            if (objectHandle == null)
                throw new ArgumentNullException(nameof(objectHandle));

            CheckDisposed();
            if (!Interop.User32.UserHandleGrantAccess(objectHandle, Handle, (allowAccess) ? Interop.BOOL.TRUE : Interop.BOOL.FALSE))
                throw Errors.Win32Error();
        }

        /// <summary>
        /// Grants or denies access to a handle to a User object to a job that has a user-interface restrictions. When access is 
        /// granted, all processes associated with the job can subsequently recognize and use the handle. When access is denied, the
        /// processes can no longer use the handle.
        /// </summary>
        /// <param name="objectHandle">A handle to the User object.</param>
        /// <param name="allowAccess"><c>true</c> to grant access to the handle, <c>false</c> to deny access.</param>
        public void GrantUserHandleAccess ( IntPtr objectHandle, bool allowAccess = true )
        {
            CheckDisposed();
            if (!Interop.User32.UserHandleGrantAccess(objectHandle, Handle, (allowAccess) ? Interop.BOOL.TRUE : Interop.BOOL.FALSE))
                throw Errors.Win32Error();
        }

        /// <summary>
        /// Terminate all processes currently associated with the job. If the job is nested, terminate all processes currently
        /// associated with the job and all of its child jobs in the hierarchy.
        /// </summary>
        /// <param name="exitCode">The exit code to be used by all processes and threads in the job object.</param>
        public void Kill ( int exitCode = -1 )
        {
            CheckDisposed();
            if (!Interop.Kernel32.TerminateJobObject(Handle, exitCode))
                throw Errors.Win32Error();
        }

        /// <summary>
        /// Sets the <see cref="JobObject"/>'s limits.
        /// </summary>
        /// <param name="newLimits">The new limits to apply.</param>
        public void SetLimits ( in JobLimits newLimits )
        {
            CheckDisposed();

            newLimits.GetLimits(out var extendedLimits,
                                out var endInfo,
                                out var uiLimits,
                                out var cpuLimits);

            bool handleLocked = false;
            try
            {
                _handle.DangerousAddRef(ref handleLocked);
                if (!handleLocked)
                    throw new ObjectDisposedException(GetType().Name);

                if (!Interop.Kernel32.SetInformationJobObject(_handle,
                                                              Interop.Kernel32.JobObjectInformationClass.ExtendedLimitInformation,
                                                              ref extendedLimits,
                                                              Interop.Kernel32.JOBOBJECT_EXTENDED_LIMIT_INFORMATION.SizeOf))
                    throw Errors.Win32Error();

                if (!Interop.Kernel32.SetInformationJobObject(_handle,
                                                              Interop.Kernel32.JobObjectInformationClass.EndOfJobTimeInformation,
                                                              ref endInfo,
                                                              Interop.Kernel32.JOBOBJECT_END_OF_JOB_TIME_INFORMATION.SizeOf))
                    throw Errors.Win32Error();

                if (!Interop.Kernel32.SetInformationJobObject(_handle,
                                                              Interop.Kernel32.JobObjectInformationClass.BasicUIRestrictions,
                                                              ref uiLimits,
                                                              Interop.Kernel32.JOBOBJECT_BASIC_UI_RESTRICTIONS.SizeOf))
                    throw Errors.Win32Error();

                // CPU limits are supported if limit violations are supported...
                if ((_jobsLimitViolationSupported) &&
                    (!Interop.Kernel32.SetInformationJobObject(_handle,
                                                               Interop.Kernel32.JobObjectInformationClass.CpuRateControlInformation,
                                                               ref cpuLimits,
                                                               Interop.Kernel32.JOBOBJECT_CPU_RATE_CONTROL_INFORMATION.SizeOf)))
                    throw Errors.Win32Error();
            }
            finally
            {
                if (handleLocked) _handle.DangerousRelease();
            }
        }

        /// <summary>
        /// Sets the notifications for the <see cref="JobObject"/>.
        /// </summary>
        /// <param name="newNotifications">The new notifications to apply.</param>
        public void SetNotifications ( in JobNotifications newNotifications )
        {
            CheckDisposed();

            // notifications are supported if limit violations are supported..
            if (_jobsLimitViolationSupported)
            {
                newNotifications.GetLimits(out var notifyLimits);
                if (!Interop.Kernel32.SetInformationJobObject(Handle,
                                                              Interop.Kernel32.JobObjectInformationClass.NotificationLimitInformation,
                                                              ref notifyLimits,
                                                              Interop.Kernel32.JOBOBJECT_NOTIFICATION_LIMIT_INFORMATION.SizeOf))
                    throw Errors.Win32Error();
            }
        }

        private void AssociateWithPort ()
        {
            var portHandle = _completionPort.Handle;
            var jobInfo = new Interop.Kernel32.JOBOBJECT_ASSOCIATE_COMPLETION_PORT
            {
                CompletionKey = Handle.DangerousGetHandle(),
                CompletionPort = portHandle.DangerousGetHandle()
            };

            if (!Interop.Kernel32.SetInformationJobObject(Handle,
                                                          Interop.Kernel32.JobObjectInformationClass.AssociateCompletionPortInformation,
                                                          ref jobInfo,
                                                          Interop.Kernel32.JOBOBJECT_ASSOCIATE_COMPLETION_PORT.SizeOf))
                throw Errors.Win32Error();

            _completionPort.SetCompletionAction(Handle.DangerousGetHandle(), Notification);
        }

        private void Notification ( Interop.Kernel32.JobObjectMessage notifyMessage, IntPtr notifyData )
        {
            // notifications can happen before, during and after Dispose .. we addref the handle to
            // ensure that it isn't closed while we're running..

            bool handleLocked = false;
            try
            {
                // try to lock the handle, if this fails, the handle has been closed..
                _handle.DangerousAddRef(ref handleLocked);
                if (!handleLocked)
                    return;

                // this callback occurs on the thread used to read all activity from the IoCompletionPort,
                // so we don't want to waste time here for anything more than we have to .. figure out what
                // the notification is, request any information we need from the JobObject and then post
                // the notification to the thread pool for delivery..

                if (notifyMessage == Interop.Kernel32.JobObjectMessage.NotificationLimit)
                {
                    if (!Interop.Kernel32.QueryInformationJobObject(_handle,
                                                                    Interop.Kernel32.JobObjectInformationClass.LimitViolationInformation,
                                                                    out Interop.Kernel32.JOBOBJECT_LIMIT_VIOLATION_INFORMATION violationInfo,
                                                                    Interop.Kernel32.JOBOBJECT_LIMIT_VIOLATION_INFORMATION.SizeOf,
                                                                    IntPtr.Zero))
                        throw Errors.Win32Error();

                    ThreadPool.UnsafeQueueUserWorkItem(( state ) => OnLimitViolationNotification(violationInfo), null);
                }
                else
                    ThreadPool.UnsafeQueueUserWorkItem(( state ) => OnGeneralNotification(notifyMessage, notifyData), null);
            }
            finally
            {
                if (handleLocked) _handle.DangerousRelease();
            }
        }

        private void OnGeneralNotification ( Interop.Kernel32.JobObjectMessage notifyMessage, IntPtr notifyData )
        {
            switch (notifyMessage)
            {
                case Interop.Kernel32.JobObjectMessage.NewProcess:
                    ProcessAdded?.Invoke(this, new ProcessIdEventArgs((int)notifyData));
                    break;

                case Interop.Kernel32.JobObjectMessage.AbnormalExitProcess:
                case Interop.Kernel32.JobObjectMessage.ExitProcess:
                    ProcessExited?.Invoke(this, new ProcessExitedEventArgs((int)notifyData, notifyMessage == Interop.Kernel32.JobObjectMessage.AbnormalExitProcess));
                    break;

                case Interop.Kernel32.JobObjectMessage.ActiveProcessLimit:
                    ProcessLimitExceeded?.Invoke(this, EventArgs.Empty);
                    break;

                case Interop.Kernel32.JobObjectMessage.ActiveProcessZero:
                    Idle?.Invoke(this, EventArgs.Empty);
                    break;

                case Interop.Kernel32.JobObjectMessage.EndOfProcessTime:
                    TimeLimitExceeded?.Invoke(this, new TimeLimitEventArgs((int)notifyData));
                    break;

                case Interop.Kernel32.JobObjectMessage.EndOfJobTime:
                    TimeLimitExceeded?.Invoke(this, new TimeLimitEventArgs(null));
                    break;

                case Interop.Kernel32.JobObjectMessage.ProcessMemoryLimit:
                    MemoryLimitExceeded?.Invoke(this, new MemoryLimitEventArgs((int)notifyData, null));
                    break;

                case Interop.Kernel32.JobObjectMessage.JobMemoryLimit:
                    MemoryLimitExceeded?.Invoke(this, new MemoryLimitEventArgs(null, null));
                    break;
            }
        }

        private void OnLimitViolationNotification ( in Interop.Kernel32.JOBOBJECT_LIMIT_VIOLATION_INFORMATION violationInfo )
        {
            // NOTE: the violationInfo contains the violated limits along with the limits that were active
            //       at the time the information was requested (which may differ from when the violation occurred). 

            if ((violationInfo.ViolationLimitFlags & Interop.Kernel32.JobObjectNotificationLimits.JobReadBytes) != 0)
                IoLimitExceeded?.Invoke(this, new IoLimitEventArgs(IoLimitType.Read, violationInfo.IoReadBytes));

            if ((violationInfo.ViolationLimitFlags & Interop.Kernel32.JobObjectNotificationLimits.JobWriteBytes) != 0)
                IoLimitExceeded?.Invoke(this, new IoLimitEventArgs(IoLimitType.Write, violationInfo.IoWriteBytes));

            if ((violationInfo.ViolationLimitFlags & Interop.Kernel32.JobObjectBasicLimits.JobTime) != 0)
                TimeLimitExceeded?.Invoke(this, new TimeLimitEventArgs(TimeSpan.FromTicks(violationInfo.PerJobUserTime.QuadPart)));

            if ((violationInfo.ViolationLimitFlags & Interop.Kernel32.JobObjectExtendedLimits.JobMemory) != 0)
                MemoryLimitExceeded?.Invoke(this, new MemoryLimitEventArgs(violationInfo.JobMemory));

            if ((violationInfo.ViolationLimitFlags & Interop.Kernel32.JobObjectNotificationLimits.CpuRateControl) != 0)
                CpuRateLimitExceeded?.Invoke(this, new RateLimitEventArgs((RateControlTolerance)violationInfo.RateControlTolerance));
        }

        private protected override SafeJobObjectHandle GetHandle ()
        {
            CheckDisposed();
            return _handle;
        }

        /// <summary>
        /// Gets the <see cref="SafeJobObjectHandle"/> associated with this instance.
        /// </summary>
        public SafeJobObjectHandle Handle { get { return GetHandle(); } }
        private protected override bool IsDisposed { get { return ((_handle == null) || (_handle.IsClosed) || (_handle.IsInvalid)); } }

        /// <summary>
        /// Returns a <see cref="JobObjectInfo"/> instance for the current process's JobObject, or <c>null</c> if the current
        /// process is not associated with a JobObject.
        /// </summary>
        public static JobObjectInfo Current
        {
            get
            {
                if (!Interop.Kernel32.IsProcessInJob(Interop.Kernel32.GetCurrentProcessIntPtr(), IntPtr.Zero, out var result))
                    throw Errors.Win32Error();

                return (result != Interop.BOOL.FALSE) ? _currentJob : null;
            }
        }

        /// <summary>
        /// Occurs when a process is assigned to the job.
        /// </summary>
        public event EventHandler<ProcessIdEventArgs> ProcessAdded;

        /// <summary>
        /// Occurs when a process assigned to the job terminates.
        /// </summary>
        public event EventHandler<ProcessExitedEventArgs> ProcessExited;

        /// <summary>
        /// Occurs when a process assigned to the job exceeds the per-process time limit, or when
        /// the job's time limit is exceeded.
        /// </summary>
        public event EventHandler<TimeLimitEventArgs> TimeLimitExceeded;

        /// <summary>
        /// Occurs when a process assigned to the job exceeds the per-process memory limit, or when the
        /// the job's memory limit is exceeded.
        /// </summary>
        public event EventHandler<MemoryLimitEventArgs> MemoryLimitExceeded;

        /// <summary>
        /// Occurs when an I/O limit is exceeded.
        /// </summary>
        public event EventHandler<IoLimitEventArgs> IoLimitExceeded;

        /// <summary>
        /// Occurs when the active processes limit is exceeded.
        /// </summary>
        public event EventHandler<EventArgs> ProcessLimitExceeded;

        /// <summary>
        /// Occurs when the CPU rate limit is exceeded.
        /// </summary>
        public event EventHandler<RateLimitEventArgs> CpuRateLimitExceeded;

        /// <summary>
        /// Occurs when all processes assigned to the job have exited.
        /// </summary>
        public event EventHandler<EventArgs> Idle;

        /// <summary>
        /// Gets a value indicating whether the OS supports CPU rate limits.
        /// </summary>
        public static bool SupportsCpuRates { get => _jobsLimitViolationSupported; }

        /// <summary>
        /// Gets a value indicating whether the OS supports <see cref="JobNotifications"/>.
        /// </summary>
        public static bool SupportsNotifications { get => _jobsLimitViolationSupported; }

        private sealed class CurrentJobObject : JobObjectInfo
        {
            private readonly SafeJobObjectHandle _handle = new SafeJobObjectHandle(IntPtr.Zero, false);

            public CurrentJobObject ()
            {
            }

            private protected override SafeJobObjectHandle GetHandle ()
            {
                return _handle;
            }

            private protected override bool IsDisposed => false;
        }
    }
}
