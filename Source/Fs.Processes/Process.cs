using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace Fs.Processes
{
    /// <summary>
    /// A class that represents a Windows Process. Similar to <see cref="System.Diagnostics.Process"/>, but not
    /// intended as a complete replacement.
    /// </summary>
    [System.Diagnostics.DebuggerTypeProxy(typeof(Process.DebuggerTypeProxy))]
    public sealed class Process : IDisposable
    {
        private enum StreamReaderMode
        {
            Undefined,
            Synchronous,
            Asynchronous
        }

        private static readonly Dictionary<string, string> _driveLetters;
        private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        private static object _createProcessLock = GetCreateProcessLock();

        private bool _disposed;
        private bool _standardInputAccessed;
        private StreamReader _standardOutput;
        private ProcessStreamReader _standardOutputReader;
        private StreamReaderMode _standardOutputMode;
        private StreamReader _standardError;
        private ProcessStreamReader _standardErrorReader;
        private StreamReaderMode _standardErrorMode;
        private StreamWriter _standardInput;
        private WaitHandle _waitHandle;
        private RegisteredWaitHandle _registeredWaitHandle;
        private TaskCompletionSource<int> _exitedTaskCompletionSource;
        private Task _exitedTask;
        private int? _exitCode;

        private SafeProcessHandle _processHandle;
        private SafeThreadHandle _threadHandle;
        private int _processId;

        static Process ()
        {
            _driveLetters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (char driveLetter = 'A'; driveLetter <= 'Z'; driveLetter++)
                _driveLetters.Add($"={driveLetter}:", $"{driveLetter}:");
        }

        /// <summary>
        /// Creates a new process and assigns the process to the job object.
        /// </summary>
        /// <param name="createProcessInfo">The <see cref="CreateProcessInfo"/> that contains information this is
        /// used to start the process, including the file name and any command-line arguments.</param>
        public Process ( CreateProcessInfo createProcessInfo )
             : this(createProcessInfo, ProcessOptions.None)
        {
        }

        /// <summary>
        /// Creates a new process and assigns the process to the job object.
        /// </summary>
        /// <param name="createProcessInfo">The <see cref="CreateProcessInfo"/> that contains information this is
        /// used to start the process, including the file name and any command-line arguments.</param>
        /// <param name="processOptions">A set of <see cref="ProcessOptions"/> that controls how the new process
        /// is created.</param>
        public Process ( CreateProcessInfo createProcessInfo, ProcessOptions processOptions )
        {
            if (createProcessInfo == null)
                throw new ArgumentNullException(nameof(createProcessInfo));

            if (createProcessInfo.FileName.Length == 0)
                throw new InvalidOperationException(Resources.FileNameMissing);

            if ((createProcessInfo.StandardInputEncoding != null) && (!createProcessInfo.RedirectStandardInput))
                throw new InvalidOperationException(Resources.StandardInputEncodingNotAllowed);

            if ((createProcessInfo.StandardOutputEncoding != null) && (!createProcessInfo.RedirectStandardOutput))
                throw new InvalidOperationException(Resources.StandardOutputEncodingNotAllowed);

            if ((createProcessInfo.StandardErrorEncoding != null) && (!createProcessInfo.RedirectStandardError))
                throw new InvalidOperationException(Resources.StandardErrorEncodingNotAllowed);

            if ((!String.IsNullOrEmpty(createProcessInfo.Arguments)) && (createProcessInfo.HasArgumentsList))
                throw new InvalidOperationException(Resources.ArgumentsAndArgumentsListInitialized);

            ValidateProcessOptions(processOptions);
            CreateProcess(createProcessInfo, processOptions);
        }

        private void Dispose ( bool disposing )
        {
            if (!_disposed)
                try
                {
                    if (disposing)
                    {
                        if ((_processHandle != null) && (!_processHandle.IsClosed) && (!_processHandle.IsInvalid))
                            CompleteProcessExitedTask(true);

                        _processHandle?.Dispose();
                        _threadHandle?.Dispose();

                        try
                        {
                            // only close the redirected streams if we're responsible for them, which means the stream has 
                            // not been accessed for synchronous operations (or even looked at in the case of StdIn)

                            CloseStandardStream(_standardOutput, _standardOutputMode, _standardOutputReader);
                            CloseStandardStream(_standardError, _standardErrorMode, _standardErrorReader);

                            if ((_standardInput != null) && (!_standardInputAccessed))
                                _standardInput.Close();
                        }
                        finally
                        {
                            _standardInput = null;
                            _standardOutput = null;
                            _standardOutputReader = null;
                            _standardError = null;
                            _standardErrorReader = null;
                        }
                    }
                }
                finally
                {
                    _disposed = true;
                }
        }

        /// <summary>
        /// Releases all resources used by the instance.
        /// </summary>
        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Begins asynchronous read operations on the redirected <see cref="StandardOutput"/> stream of the process.
        /// </summary>
        /// <param name="readAsLines">When <c>true</c> data is buffered until a complete line is read, otherwise data is
        /// dispatched as it is received.</param>
        /// <returns>A <see cref="Task"/> that completes when all data has been read.</returns>
        public Task BeginReadingStandardOutputAsync ( bool readAsLines = true )
        {
            CheckDisposed();
            return BeginReadingStandardStreamAsync(ref _standardOutputMode,
                                                   ref _standardOutputReader,
                                                   RedirectedOutputData,
                                                   _standardOutput,
                                                   "StandardOutput",
                                                   readAsLines);
        }

        /// <summary>
        /// Cancels the asynchronous read operation on the redirected <see cref="StandardOutput"/> stream of the process.
        /// </summary>
        public void CancelReadingStandardOutput ()
        {
            CheckDisposed();
            if (_standardOutputReader != null)
                _standardOutputReader.EndReading();
        }

        /// <summary>
        /// Begins asynchronous read operations on the redirected <see cref="StandardError"/> stream of the process.
        /// </summary>
        /// <param name="readAsLines">When <c>true</c> data is buffered until a complete line is read, otherwise data is
        /// dispatched as it is received.</param>
        /// <returns>A <see cref="Task"/> that completes when all data has been read.</returns>
        public Task BeginReadingStandardErrorAsync ( bool readAsLines = true )
        {
            CheckDisposed();
            return BeginReadingStandardStreamAsync(ref _standardErrorMode,
                                                   ref _standardErrorReader,
                                                   RedirectedErrorData,
                                                   _standardError,
                                                   "StandardError",
                                                   readAsLines);
        }

        /// <summary>
        /// Cancels the asynchronous read operation on the redirected <see cref="StandardError"/> stream of the process.
        /// </summary>
        public void CancelReadingStandardError ()
        {
            CheckDisposed();
            if (_standardErrorReader != null)
                _standardErrorReader.EndReading();
        }

        private Task BeginReadingStandardStreamAsync ( ref StreamReaderMode streamReaderMode, ref ProcessStreamReader streamReader, Action<string> callback, StreamReader standardStream, string standardStreamName, bool waitForLines )
        {
            if (streamReaderMode == StreamReaderMode.Undefined)
                streamReaderMode = StreamReaderMode.Asynchronous;
            else if (streamReaderMode == StreamReaderMode.Synchronous)
                throw new InvalidOperationException(Resources.CantMixSyncAsyncOperation);

            if (streamReader == null)
            {
                if (standardStream == null)
                    throw new InvalidOperationException(String.Format(Resources.CantGetStandardStream, standardStreamName));

                streamReader = new ProcessStreamReader(standardStream.BaseStream, callback, standardStream.CurrentEncoding, waitForLines);
            }

            return streamReader.BeginReadingAsync();
        }

        /// <summary>
        /// Decrements the primary thread's suspend count. When the suspend count is decremented to zero, the
        /// execution of the thread is resumed.
        /// </summary>
        /// <returns><c>true</c> if the thread is resumed; otherwise, <c>false</c>.</returns>
        public bool Resume ()
        {
            CheckDisposed();

            uint resumeResult = Interop.Kernel32.ResumeThread(_threadHandle);
            if (resumeResult == uint.MaxValue)
                throw Errors.Win32Error();

            // thread is resumed if result == 0...
            return (resumeResult == 0);
        }

        /// <summary>
        /// Immediately stops the process.
        /// </summary>
        /// <param name="exitCode">The value to be used as the process's exit code.</param>
        public void Kill ( int exitCode = -1 )
        {
            CheckDisposed();

            if (!Interop.Kernel32.TerminateProcess(_processHandle, exitCode))
            {
                int errorCode = Marshal.GetLastWin32Error();
                if (errorCode == Interop.Errors.ERROR_ACCESS_DENIED)
                    // indicates the process is already terminating or exited..
                    return;

                throw Errors.Win32Error(errorCode);
            }
        }

        private void CreateProcess ( CreateProcessInfo startupInfo, ProcessOptions processOptions )
        {
            var processStartupInfo = new Interop.Kernel32.STARTUPINFO();
            var processInfo = new Interop.Kernel32.PROCESS_INFORMATION();
            var securityAttrs = new Interop.Kernel32.SECURITY_ATTRIBUTES();

            // generate the command line we'll pass to CreateProcess..
            StringBuilder commandLine = BuildCommandLine(startupInfo.FileName,
                                                         startupInfo.Arguments,
                                                         startupInfo.HasArgumentsList ? startupInfo.ArgumentsList : null);

            SafeProcessHandle processHandle = null;
            SafeThreadHandle threadHandle = null;

            SafeFileHandle parentInputPipeHandle = null;
            SafeFileHandle childInputPipeHandle = null;
            SafeFileHandle parentOutputPipeHandle = null;
            SafeFileHandle childOutputPipeHandle = null;
            SafeFileHandle parentErrorPipeHandle = null;
            SafeFileHandle childErrorPipeHandle = null;
            GCHandle environmentHandle = default;

            try
            {
                // always create with a unicode environment (according to the MSDN, this flag must be set
                // if the parent process has unicode characters in it's environment, so we just always set it)

                int createFlags = Interop.Kernel32.CREATEF.UnicodeEnvironment | (int)processOptions;
                processStartupInfo.cb = Interop.Kernel32.STARTUPINFO.SizeOf;

                // fill in most of STARTUPINFO (while validating it)
                PrepareStartupInfo(ref processStartupInfo, startupInfo);

                // TODO: consider providing support for STARTUPINFOEX and eliminate the _createProcessLock, since we
                //   can specify specific handles to inherit in the attribute list instead of "everything"... actually,
                //   the lock would have to stay (in case System.Diagnostics.Process is used to start a process)

                lock (_createProcessLock)
                {
                    try
                    {
                        if (startupInfo.HasStandardRedirect)
                        {
                            // NOTE: These handles must be created, duplicated and closed (when inheritable) while holding
                            // the _createProcessLock, otherwise the handles could be inherited by processes we don't mean
                            // for them to be given to, which would prevent the streams from closing at the correct times.

                            CreatePipe(out parentInputPipeHandle, out childInputPipeHandle, Interop.Kernel32.HandleTypes.STD_INPUT_HANDLE, startupInfo.RedirectStandardInput);
                            CreatePipe(out parentOutputPipeHandle, out childOutputPipeHandle, Interop.Kernel32.HandleTypes.STD_OUTPUT_HANDLE, startupInfo.RedirectStandardOutput);
                            CreatePipe(out parentErrorPipeHandle, out childErrorPipeHandle, Interop.Kernel32.HandleTypes.STD_ERROR_HANDLE, startupInfo.RedirectStandardError);

                            processStartupInfo.hStdInput = childInputPipeHandle.DangerousGetHandle();
                            processStartupInfo.hStdOutput = childOutputPipeHandle.DangerousGetHandle();
                            processStartupInfo.hStdError = childErrorPipeHandle.DangerousGetHandle();

                            processStartupInfo.dwFlags |= Interop.Kernel32.STARTF.USESTDHANDLES;
                        }

                        string environmentBlock = null;
                        if (startupInfo.HasEnvironment)
                            environmentBlock = GetEnvironmentBlock(startupInfo.Environment);

                        string workingDirectory = startupInfo.WorkingDirectory;
                        if (String.IsNullOrEmpty(workingDirectory))
                            workingDirectory = Directory.GetCurrentDirectory();

                        try
                        {
                            if (environmentBlock != null)
                                environmentHandle = GCHandle.Alloc(environmentBlock, GCHandleType.Pinned);

                            bool processCreated;
                            int processCreateError = 0;

                            if (startupInfo.UserName.Length != 0)
                            {
                                if ((startupInfo.Password != null) && (startupInfo.PasswordInClearText != null))
                                    throw new ArgumentException(Resources.CantSetDuplicatePassword);

                                Interop.Advapi32.LogonFlags logonFlags = (Interop.Advapi32.LogonFlags)0;
                                IntPtr passwordPtr = IntPtr.Zero;
                                GCHandle passwordHandle = default;

                                try
                                {
                                    if (startupInfo.LoadUserProfile)
                                        logonFlags |= Interop.Advapi32.LogonFlags.LOGON_WITH_PROFILE;

                                    if (startupInfo.Password != null)
                                        passwordPtr = Marshal.SecureStringToGlobalAllocUnicode(startupInfo.Password);

                                    if (startupInfo.Password == null)
                                        passwordHandle = GCHandle.Alloc(startupInfo.PasswordInClearText ?? String.Empty, GCHandleType.Pinned);

                                    if (!(processCreated = Interop.Advapi32.CreateProcessWithLogonW(
                                        startupInfo.UserName,
                                        startupInfo.Domain,
                                        passwordHandle.IsAllocated ? passwordHandle.AddrOfPinnedObject() : passwordPtr,
                                        logonFlags,
                                        null,
                                        commandLine,
                                        createFlags,
                                        (environmentBlock != null) ? environmentHandle.AddrOfPinnedObject() : IntPtr.Zero,
                                        workingDirectory,
                                        ref processStartupInfo,
                                        ref processInfo)))
                                        processCreateError = Marshal.GetLastWin32Error();
                                }
                                finally
                                {
                                    if (passwordPtr != IntPtr.Zero)
                                        Marshal.ZeroFreeGlobalAllocUnicode(passwordPtr);

                                    if (passwordHandle.IsAllocated)
                                        passwordHandle.Free();
                                }
                            }
                            else
                            {
                                if (!(processCreated = Interop.Kernel32.CreateProcess(
                                    null,
                                    commandLine,
                                    ref securityAttrs,
                                    ref securityAttrs,
                                    true,
                                    createFlags,
                                    (environmentBlock != null) ? environmentHandle.AddrOfPinnedObject() : IntPtr.Zero,
                                    workingDirectory,
                                    ref processStartupInfo,
                                    ref processInfo)))
                                    processCreateError = Marshal.GetLastWin32Error();
                            }

                            if ((processInfo.hProcess != IntPtr.Zero) && (processInfo.hProcess != INVALID_HANDLE_VALUE))
                                processHandle = new SafeProcessHandle(processInfo.hProcess, true);

                            if ((processInfo.hThread != IntPtr.Zero) && (processInfo.hThread != INVALID_HANDLE_VALUE))
                                threadHandle = new SafeThreadHandle(processInfo.hThread, true);

                            if (!processCreated)
                            {
                                if ((processCreateError == Interop.Errors.ERROR_BAD_EXE_FORMAT) ||
                                    (processCreateError == Interop.Errors.ERROR_EXE_MACHINE_TYPE_MISMATCH))
                                    throw new System.ComponentModel.Win32Exception(Resources.InvalidApplication);

                                throw Errors.Win32Error(processCreateError);
                            }
                        }
                        finally
                        {
                            if (environmentHandle.IsAllocated)
                                environmentHandle.Free();
                        }
                    }
                    finally
                    {
                        childErrorPipeHandle?.Dispose();
                        childOutputPipeHandle?.Dispose();
                        childInputPipeHandle?.Dispose();
                    }
                }

                if (startupInfo.RedirectStandardInput)
                    _standardInput = new StreamWriter(new FileStream(parentInputPipeHandle, FileAccess.Write, 4096, false),
                                                      startupInfo.StandardInputEncoding ?? Console.InputEncoding,
                                                      4096)
                    {
                        AutoFlush = true
                    };

                if (startupInfo.RedirectStandardOutput)
                    _standardOutput = new StreamReader(new FileStream(parentOutputPipeHandle, FileAccess.Read, 4096, false),
                                                       startupInfo.StandardOutputEncoding ?? Console.OutputEncoding,
                                                       true,
                                                       4096);

                if (startupInfo.RedirectStandardError)
                    _standardError = new StreamReader(new FileStream(parentErrorPipeHandle, FileAccess.Read, 4096, false),
                                                      startupInfo.StandardErrorEncoding ?? Console.OutputEncoding,
                                                      true,
                                                      4096);

                _processHandle = processHandle;
                _threadHandle = threadHandle;
                _processId = (int)processInfo.dwProcessId;
            }
            catch
            {
                Dispose(true);
                parentInputPipeHandle?.Dispose();
                parentOutputPipeHandle?.Dispose();
                parentErrorPipeHandle?.Dispose();
                processHandle?.Dispose();
                threadHandle?.Dispose();
                throw;
            }
        }

        private SafeProcessHandle GetProcessHandle ( int desiredAccess )
        {
            CheckDisposed();
            return new SafeProcessHandle(_processHandle.DangerousGetHandle(), false);
        }

        private void ProcessSignaledCallback ( object waitHandleContext, bool wasSignaled )
        {
            lock (this)
            {
                if (waitHandleContext != _waitHandle)
                    return;

                CompleteProcessExitedTask(false);
            }
        }

        private void CompleteProcessExitedTask ( bool isCancelled )
        {
            if (_exitedTaskCompletionSource != null)
            {
                TaskCompletionSource<int> exitedTaskCompletionSource = null;
                RegisteredWaitHandle registeredWaitHandle = null;
                WaitHandle waitHandle = null;

                lock (this)
                {
                    if (_exitedTaskCompletionSource == null)
                        return;

                    registeredWaitHandle = _registeredWaitHandle;
                    _registeredWaitHandle = null;

                    waitHandle = _waitHandle;
                    _waitHandle = null;

                    exitedTaskCompletionSource = _exitedTaskCompletionSource;
                    _exitedTaskCompletionSource = null;
                }

                registeredWaitHandle?.Unregister(null);
                waitHandle?.Dispose();

                if (isCancelled)
                    exitedTaskCompletionSource?.TrySetCanceled();
                else
                    exitedTaskCompletionSource?.TrySetResult(0);
            }
        }

        private Task GetProcessExitedTask ()
        {
            if (_exitedTask == null)
            {
                lock (this)
                {
                    if (_exitedTask != null)
                        return _exitedTask;

                    using (var processHandle = GetProcessHandle(Interop.ProcessAccess.Synchronize))
                    {
                        var processWaitHandle = new Interop.ProcessWaitHandle(processHandle);
                        if (processWaitHandle.WaitOne(0))
                        {
                            processWaitHandle.Dispose();
                            return _exitedTask = Task.CompletedTask;
                        }

                        try
                        {
                            _exitedTaskCompletionSource = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
                            _waitHandle = processWaitHandle;

                            // register a wait in the threadpool for the handle to become signalled..
                            _registeredWaitHandle = ThreadPool.UnsafeRegisterWaitForSingleObject(
                                processWaitHandle,
                                ProcessSignaledCallback,
                                processWaitHandle,
                                Timeout.Infinite,
                                true);

                            return _exitedTask = _exitedTaskCompletionSource.Task;
                        }
                        catch
                        {
                            // if anything goes wrong, just try to throw it all away...
                            _waitHandle = null;
                            _exitedTaskCompletionSource = null;
                            processWaitHandle.Dispose();
                            throw;
                        }
                    }
                }
            }

            return _exitedTask;
        }

        private void RedirectedOutputData ( string data )
        {
            OutputDataReceived?.Invoke(this, new ProcessDataReceivedEventArgs(data));
        }

        private void RedirectedErrorData ( string data )
        {
            ErrorDataReceived?.Invoke(this, new ProcessDataReceivedEventArgs(data));
        }

        private int? GetExitCode ()
        {
            if (!_exitCode.HasValue)
            {
                using (var processWaitHandle = new Interop.ProcessWaitHandle(_processHandle))
                    if (processWaitHandle.WaitOne(0))
                    {
                        if (!Interop.Kernel32.GetExitCodeProcess(_processHandle, out int exitCode))
                            throw Errors.Win32Error();

                        _exitCode = exitCode;
                        return exitCode;
                    }
            }

            return _exitCode;
        }

        private void CheckDisposed ()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        private static void CreatePipe ( out SafeFileHandle readHandle, out SafeFileHandle writeHandle )
        {
            var securityAttributes = new Interop.Kernel32.SECURITY_ATTRIBUTES { bInheritHandle = Interop.BOOL.TRUE };

            if ((!Interop.Kernel32.CreatePipe(out readHandle, out writeHandle, ref securityAttributes, 0)) ||
                (readHandle.IsInvalid) ||
                (writeHandle.IsInvalid))
                throw Errors.Win32Error();
        }

        private static void CreatePipe ( out SafeFileHandle parentHandle, out SafeFileHandle childHandle, int stdHandle, bool isRedirected )
        {
            if (isRedirected)
            {
                SafeFileHandle inheritableParentHandle = null;
                try
                {
                    if (stdHandle == Interop.Kernel32.HandleTypes.STD_INPUT_HANDLE)
                        CreatePipe(out childHandle, out inheritableParentHandle);
                    else
                        CreatePipe(out inheritableParentHandle, out childHandle);

                    SafeProcessHandle currentProcess = Interop.Kernel32.GetCurrentProcess();
                    if (!Interop.Kernel32.DuplicateHandle(currentProcess,
                                                          inheritableParentHandle,
                                                          currentProcess,
                                                          out parentHandle,
                                                          0,
                                                          false,
                                                          Interop.Kernel32.HandleOptions.DUPLICATE_SAME_ACCESS))
                        throw Errors.Win32Error();
                }
                finally
                {
                    if ((inheritableParentHandle != null) && (!inheritableParentHandle.IsInvalid))
                        inheritableParentHandle.Dispose();
                }
            }
            else
            {
                parentHandle = null;
                childHandle = new SafeFileHandle(Interop.Kernel32.GetStdHandle(stdHandle), false);
            }
        }

        private static void PrepareStartupInfo ( ref Interop.Kernel32.STARTUPINFO startupInfo, CreateProcessInfo createProcessInfo )
        {
            startupInfo.lpDesktop = createProcessInfo.Desktop;
            startupInfo.lpTitle = createProcessInfo.Title;

            switch (createProcessInfo.TitleIs)
            {
                case TitleIs.AppID:
                    if (String.IsNullOrWhiteSpace(createProcessInfo.Title))
                        throw new InvalidOperationException(Resources.InvalidAppIdTitle);

                    startupInfo.dwFlags |= Interop.Kernel32.STARTF.TITLEISAPPID;
                    break;

                case TitleIs.LinkName:
                    if (String.IsNullOrWhiteSpace(createProcessInfo.Title))
                        throw new InvalidOperationException(Resources.InvalidLinkNameTitle);

                    startupInfo.dwFlags |= Interop.Kernel32.STARTF.TITLEISLINKNAME;
                    break;
            }

            if (createProcessInfo.PreventPinning)
            {
                if (createProcessInfo.TitleIs != TitleIs.AppID)
                    throw new InvalidOperationException(Resources.PreventPinningOnlyWithAppIdTitle);

                startupInfo.dwFlags |= Interop.Kernel32.STARTF.PREVENTPINNING;
            }

            if (createProcessInfo.WindowPosition.HasValue)
            {
                startupInfo.dwX = createProcessInfo.WindowPosition.Value.X;
                startupInfo.dwY = createProcessInfo.WindowPosition.Value.Y;
                startupInfo.dwFlags |= Interop.Kernel32.STARTF.USEPOSITION;
            }

            if (createProcessInfo.WindowSize.HasValue)
            {
                startupInfo.dwXSize = createProcessInfo.WindowSize.Value.Width;
                startupInfo.dwYSize = createProcessInfo.WindowSize.Value.Height;
                startupInfo.dwFlags |= Interop.Kernel32.STARTF.USESIZE;
            }

            if (createProcessInfo.ConsoleBufferSize.HasValue)
            {
                startupInfo.dwXCountChars = createProcessInfo.ConsoleBufferSize.Value.Width;
                startupInfo.dwYCountChars = createProcessInfo.ConsoleBufferSize.Value.Height;
                startupInfo.dwFlags |= Interop.Kernel32.STARTF.USECOUNTCHARS;
            }

            if (createProcessInfo.WindowShow.HasValue)
            {
                startupInfo.wShowWindow = (short)createProcessInfo.WindowShow.Value;
                startupInfo.dwFlags |= Interop.Kernel32.STARTF.USESHOWWINDOW;
            }

            if (createProcessInfo.ForceFeedback.HasValue)
                startupInfo.dwFlags |= (createProcessInfo.ForceFeedback.Value) ?
                    Interop.Kernel32.STARTF.FORCEONFEEDBACK :
                    Interop.Kernel32.STARTF.FORCEOFFFEEDBACK;

            if (createProcessInfo.UntrustedSource)
                startupInfo.dwFlags |= Interop.Kernel32.STARTF.UNTRUSTEDSOURCE;

            if (createProcessInfo.HotKey != 0)
            {
                if (createProcessInfo.HasStandardRedirect)
                    throw new InvalidOperationException(Resources.HotKeyWithRedirection);

                startupInfo.hStdInput = (IntPtr)createProcessInfo.HotKey;
                startupInfo.dwFlags |= Interop.Kernel32.STARTF.USEHOTKEY;
            }

            if (createProcessInfo.ConsoleFill.HasValue)
            {
                startupInfo.dwFillAttribute = createProcessInfo.ConsoleFill.Value.GetFillAttribute();
                startupInfo.dwFlags |= Interop.Kernel32.STARTF.USEFILLATTRIBUTE;
            }
        }

        private static StringBuilder BuildCommandLine ( string executableFileName, string arguments, IList<string> argumentsList )
        {
            StringBuilder commandLine = new StringBuilder();

            executableFileName = executableFileName.Trim();
            var isQuoted = (executableFileName.StartsWith("\"", StringComparison.Ordinal) && executableFileName.EndsWith("\"", StringComparison.Ordinal));

            if (isQuoted) commandLine.Append('"');
            commandLine.Append(executableFileName);
            if (isQuoted) commandLine.Append('"');

            if (argumentsList != null)
            {
                // if the ProcessStartupInfo is using the ArgumentsList list, then we will escape
                // each of the arguments as needed.

                if (argumentsList.Count > 0)
                    foreach (var argument in argumentsList)
                        ProcessArgumentEscaper.Escape(commandLine.Append(' '), argument);
            }
            else if (!String.IsNullOrEmpty(arguments))
            {
                commandLine.Append(' ');
                commandLine.Append(arguments);
            }

            return commandLine;
        }

        private static string GetEnvironmentBlock ( IDictionary<string, string> variables )
        {
            if (variables == null)
                return null;

            SortedDictionary<string, string> variablePairs = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var drivePair in _driveLetters) 
                if (!variables.ContainsKey(drivePair.Key))
                {
                    var currentPath = Path.GetFullPath(drivePair.Value);
                    if ((currentPath == null) || (currentPath.Length <= 3))
                        continue;

                    variablePairs.Add(drivePair.Key, currentPath);
                }

            foreach (var variablePair in variables)
                variablePairs[variablePair.Key] = variablePair.Value;

            StringBuilder environmentBuilder = new StringBuilder();
            foreach (var variablePair in variablePairs)
            {
                environmentBuilder.Append(variablePair.Key);
                environmentBuilder.Append('=');
                environmentBuilder.Append(variablePair.Value);
                environmentBuilder.Append('\0');
            }

            return environmentBuilder.ToString();
        }

        private static object GetCreateProcessLock ()
        {
            // this code needs to ensure that Process.Start is synchronized with our own Process.Start, otherwise we
            // could inadvernatly create duplicated handles that would not be closed until a process terminates. We use
            // reflection to access the object used for locking.

            var ProcessType = typeof(System.Diagnostics.Process);

            // try a few variations on the lock name, CoreFX differs from the Desktop framework...
            var processLockField = ProcessType.GetField("s_createProcessLock", BindingFlags.Static | BindingFlags.NonPublic);
            if (processLockField == null)
                processLockField = ProcessType.GetField("s_CreateProcessLock", BindingFlags.Static | BindingFlags.NonPublic);

            if (processLockField == null)
                throw new InvalidOperationException("Cannot locate s_CreateProcessLock field in Process type.");

            return processLockField.GetValue(null);
        }

        private static StreamReader GetStreamReader ( StreamReader streamReader, ref StreamReaderMode streamReaderMode, string streamName )
        {
            if (streamReader == null)
                throw new InvalidOperationException(String.Format(Resources.CantGetStandardStream, streamName));

            if (streamReaderMode == StreamReaderMode.Undefined)
                streamReaderMode = StreamReaderMode.Synchronous;
            else if (streamReaderMode == StreamReaderMode.Asynchronous)
                throw new InvalidOperationException(Resources.CantMixSyncAsyncOperation);

            return streamReader;
        }

        private static void CloseStandardStream ( StreamReader standardStream, StreamReaderMode standardStreamMode, ProcessStreamReader streamReader )
        {
            if ((standardStream != null) && (standardStreamMode != StreamReaderMode.Synchronous))
            {
                if (standardStreamMode == StreamReaderMode.Asynchronous)
                    streamReader.EndReading();

                standardStream.Close();
            }
        }

        private static void ValidateProcessOptions ( ProcessOptions options, string parameterName = "processOptions" )
        {
            if (!options.IsValid())
                throw new ArgumentException(Resources.ProcessOptionsInvalid, parameterName);
        }


        /// <summary>
        /// Occurs each time a process writes data to its redirected <see cref="StandardOutput"/> stream.
        /// </summary>
        public event EventHandler<ProcessDataReceivedEventArgs> OutputDataReceived;

        /// <summary>
        /// Occurs each time a process writes data to its redirected <see cref="StandardError"/> stream.
        /// </summary>
        public event EventHandler<ProcessDataReceivedEventArgs> ErrorDataReceived;

        /// <summary>
        /// Gets the <see cref="SafeProcessHandle"/> associated with this instance.
        /// </summary>
        public SafeProcessHandle Handle { get { CheckDisposed(); return _processHandle; } }

        /// <summary>
        /// Gets the unique identifier of the process.
        /// </summary>
        public int Id { get { CheckDisposed(); return _processId; } }

        /// <summary>
        /// Gets a stream used to write to the redirected input of the proces.
        /// </summary>
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        public StreamWriter StandardInput
        {
            get
            {
                CheckDisposed();
                if (_standardInput == null)
                    throw new InvalidOperationException(String.Format(Resources.CantGetStandardStream, "StandardInput"));

                _standardInputAccessed = true;
                return _standardInput;
            }
        }

        /// <summary>
        /// Gets a stream used to read from the redirected output of the process.
        /// </summary>
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        public StreamReader StandardOutput { get { CheckDisposed(); return GetStreamReader(_standardOutput, ref _standardOutputMode, "StandardOutput"); } }

        /// <summary>
        /// Gets a stream used to read from the redirected error output of the process.
        /// </summary>
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        public StreamReader StandardError { get { CheckDisposed(); return GetStreamReader(_standardError, ref _standardErrorMode, "StandardError"); } }

        /// <summary>
        /// Gets a <see cref="Task"/> that completes when the process exits. The <see cref="Task"/> is cancelled 
        /// if the <see cref="Process"/> is disposed before the process exits.
        /// </summary>
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        public Task Exited
        {
            get
            {
                CheckDisposed();
                return (_exitedTask ?? GetProcessExitedTask());
            }
        }

        /// <summary>
        /// Gets the value associated with the process when it exited, or <c>null</c> if the process has not 
        /// exited.
        /// </summary>
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        public int? ExitCode
        {
            get
            {
                CheckDisposed();
                return GetExitCode();
            }
        }

        private class DebuggerTypeProxy
        {
            private Process _process;

            public DebuggerTypeProxy ( Process process )
            {
                _process = process;
            }

            public SafeProcessHandle Handle { get { return _process._processHandle; } }
            public int Id { get { return _process._processId; } }

            public StreamWriter StandardInput { get { return _process._standardInput; } }
            public StreamReader StandardOutput { get { return _process._standardOutput; } }
            public StreamReaderMode StandardOutputMode { get { return _process._standardOutputMode; } }
            public ProcessStreamReader StandardOutputReader { get { return _process._standardOutputReader; } }
            public StreamReader StandardError { get { return _process._standardError; } }
            public StreamReaderMode StandardErrorMode { get { return _process._standardErrorMode; } }
            public ProcessStreamReader StandardErrorReader { get { return _process._standardErrorReader; } }
            public Task Exited { get { return _process._exitedTask; } }
            public int? ExitCode { get { return _process._exitCode; } }
        }
    }
}
