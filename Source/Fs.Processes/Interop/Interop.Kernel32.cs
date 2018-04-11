using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

internal partial class Interop
{
    [System.Security.SuppressUnmanagedCodeSecurity]
    internal static partial class Kernel32
    {
        internal partial class HandleOptions
        {
            internal const int DUPLICATE_SAME_ACCESS = 2;
            internal const int STILL_ACTIVE = 0x00000103;
            internal const int TOKEN_ADJUST_PRIVILEGES = 0x20;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, BestFitMapping = false, EntryPoint = "CreateProcessW")]
        internal static extern bool CreateProcess (
            string lpApplicationName,
            StringBuilder lpCommandLine,
            ref SECURITY_ATTRIBUTES procSecAttrs,
            ref SECURITY_ATTRIBUTES threadSecAttrs,
            bool bInheritHandles,
            int dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            ref PROCESS_INFORMATION lpProcessInformation
        );

        [StructLayout(LayoutKind.Sequential)]
        internal struct PROCESS_INFORMATION
        {
            internal static readonly int SizeOf = Marshal.SizeOf<PROCESS_INFORMATION>();

            internal IntPtr hProcess;
            internal IntPtr hThread;
            internal int dwProcessId;
            internal int dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct STARTUPINFO
        {
            internal static readonly int SizeOf = Marshal.SizeOf<STARTUPINFO>();

            internal int cb;
            internal IntPtr lpReserved;
            internal string lpDesktop;
            internal string lpTitle;
            internal int dwX;
            internal int dwY;
            internal int dwXSize;
            internal int dwYSize;
            internal int dwXCountChars;
            internal int dwYCountChars;
            internal int dwFillAttribute;
            internal int dwFlags;
            internal short wShowWindow;
            internal short cbReserved2;
            internal IntPtr lpReserved2;
            internal IntPtr hStdInput;
            internal IntPtr hStdOutput;
            internal IntPtr hStdError;
        }

        internal static class CREATEF
        {
            internal const int BreakAwayFromJob = 0x01000000;
            internal const int DefaultErrorMode = 0x04000000;
            internal const int NewConsole = 0x00000010;
            internal const int NewProcessGroup = 0x00000200;
            internal const int NoWindow = 0x08000000;
            internal const int ProtectedProcess = 0x00040000;
            internal const int PreserveCodeAuthzLevel = 0x02000000;
            internal const int SecureProcess = 0x00400000;
            internal const int Suspended = 0x00000004;
            internal const int UnicodeEnvironment = 0x00000400;
            internal const int DebugOnlyThisProcess = 0x00000002;
            internal const int DebugProcess = 0x00000001;
            internal const int Detached = 0x00000008;
            internal const int ExtendedStartupInfo = 0x00080000;
            internal const int InheritParentAffinity = 0x00010000;
        }

        internal static class STARTF
        {
            internal const int FORCEONFEEDBACK = 0x00000040;
            internal const int FORCEOFFFEEDBACK = 0x00000080;
            internal const int PREVENTPINNING = 0x00002000;
            internal const int RUNFULLSCREEN = 0x00000020;
            internal const int TITLEISAPPID = 0x00001000;
            internal const int TITLEISLINKNAME = 0x00000800;
            internal const int UNTRUSTEDSOURCE = 0x00008000;
            internal const int USECOUNTCHARS = 0x00000008;
            internal const int USEFILLATTRIBUTE = 0x00000010;
            internal const int USEHOTKEY = 0x00000200;
            internal const int USEPOSITION = 0x00000004;
            internal const int USESHOWWINDOW = 0x00000001;
            internal const int USESIZE = 0x00000002;
            internal const int USESTDHANDLES = 0x00000100;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool CreatePipe ( out SafeFileHandle hReadPipe, out SafeFileHandle hWritePipe, ref SECURITY_ATTRIBUTES lpPipeAttributes, int nSize );

        [DllImport("kernel32.dll", SetLastError = true, BestFitMapping = false)]
        internal static extern bool DuplicateHandle (
            SafeProcessHandle hSourceProcessHandle,
            SafeHandle hSourceHandle,
            SafeProcessHandle hTargetProcess,
            out SafeFileHandle targetHandle,
            int dwDesiredAccess,
            bool bInheritHandle,
            int dwOptions
        );

        [DllImport("kernel32.dll", SetLastError = true, BestFitMapping = false)]
        internal static extern bool DuplicateHandle (
            SafeProcessHandle hSourceProcessHandle,
            SafeHandle hSourceHandle,
            SafeProcessHandle hTargetProcess,
            out SafeWaitHandle targetHandle,
            int dwDesiredAccess,
            bool bInheritHandle,
            int dwOptions
        );

        [DllImport("kernel32.dll", SetLastError = true, BestFitMapping = false)]
        internal static extern bool DuplicateHandle (
            IntPtr hSourceProcessHandle,
            SafeJobObjectHandle hSourceHandle,
            IntPtr hTargetProcess,
            out SafeJobObjectHandle targetHandle,
            int dwDesiredAccess,
            bool bInheritHandle,
            int dwOptions
        );

        [DllImport("kernel32.dll", SetLastError = true, BestFitMapping = false)]
        internal static extern bool DuplicateHandle (
            IntPtr hSourceProcessHandle,
            SafeProcessHandle hSourceHandle,
            IntPtr hTargetProcess,
            out SafeProcessHandle targetHandle,
            int dwDesiredAccess,
            bool bInheritHandle,
            int dwOptions
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern SafeProcessHandle GetCurrentProcess ();

        [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "GetCurrentProcess")]
        internal static extern IntPtr GetCurrentProcessIntPtr ();

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool GetExitCodeProcess ( SafeProcessHandle processHandle, out int exitCode );

        internal static partial class HandleTypes
        {
            internal const int STD_INPUT_HANDLE = -10;
            internal const int STD_OUTPUT_HANDLE = -11;
            internal const int STD_ERROR_HANDLE = -12;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr GetStdHandle ( int nStdHandle );

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseHandle ( IntPtr handle );

        [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "CreateIoCompletionPort", ExactSpelling = true)]
        internal static extern SafeIoCompletionPortHandle CreateIoCompletionPort (
            IntPtr FileHandle,
            IntPtr ExistingCompletionPort,
            IntPtr CompletionKey,
            int NumberOfConcurrentThreads );

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, BestFitMapping = false, EntryPoint = "CreateJobObjectW")]
        internal static extern SafeJobObjectHandle CreateJobObject ( IntPtr lpJobAttributes, string lpName );

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, BestFitMapping = false, EntryPoint = "CreateJobObjectW")]
        internal static extern SafeJobObjectHandle CreateJobObject ( ref SECURITY_ATTRIBUTES lpJobAttributes, string lpName );

        [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "PostQueuedCompletionStatus")]
        internal static extern bool PostQueuedCompletionStatus (
            SafeIoCompletionPortHandle CompletionPort,
            uint dwNumberOfBytesTransferred,
            IntPtr dwCompletionKey,
            IntPtr lpOverlapped );

        [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "GetQueuedCompletionStatus")]
        internal static extern bool GetQueuedCompletionStatus (
            SafeIoCompletionPortHandle CompletionPort,
            out uint lpNumberOfBytes,
            out IntPtr lpCompletionKey,
            out IntPtr lpOverlapped,
            int dwMilliseconds );

        [StructLayout(LayoutKind.Sequential)]
        internal struct SECURITY_ATTRIBUTES
        {
            internal uint nLength;
            internal IntPtr lpSecurityDescriptor;
            internal BOOL bInheritHandle;
        }

        [DllImport("kernel32.dll", EntryPoint = "ResumeThread")]
        internal static extern uint ResumeThread ( SafeThreadHandle hThread );

        internal enum JobObjectInformationClass
        {
            BasicAccountingInformation = 1,
            BasicLimitInformation,
            BasicProcessIdList,
            BasicUIRestrictions,
            SecurityLimitInformation, // deprecated
            EndOfJobTimeInformation,
            AssociateCompletionPortInformation,
            BasicAndIoAccountingInformation,
            ExtendedLimitInformation,
            JobSetInformation,
            GroupInformation,
            NotificationLimitInformation,
            LimitViolationInformation,
            GroupInformationEx,
            CpuRateControlInformation,
            CompletionFilter,
            CompletionCounter,
            NetRateControlInformation = 32,
            NotificationLimitInformation2,
            LimitViolationInformation2,
            CreateSilo,
            SiloBasicInformation
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct JOBOBJECT_ASSOCIATE_COMPLETION_PORT
        {
            internal static readonly int SizeOf = Marshal.SizeOf<JOBOBJECT_ASSOCIATE_COMPLETION_PORT>();

            internal IntPtr CompletionKey;
            internal IntPtr CompletionPort;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct LARGE_INTEGER
        {
            [FieldOffset(0)]
            internal long QuadPart;
            [FieldOffset(0)]
            internal uint LowPart;
            [FieldOffset(4)]
            internal int HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct JOBOBJECT_BASIC_LIMIT_INFORMATION
        {
            internal static readonly int SizeOf = Marshal.SizeOf<JOBOBJECT_BASIC_LIMIT_INFORMATION>();

            internal LARGE_INTEGER PerProcessUserTimeLimit;
            internal LARGE_INTEGER PerJobUserTimeLimit;
            internal uint LimitFlags;
            internal UIntPtr MinimumWorkingSetSize;
            internal UIntPtr MaximumWorkingSetSize;
            internal uint ActiveProcessLimit;
            internal UIntPtr Affinity;
            internal uint PriorityClass;
            internal uint SchedulingClass;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct IO_COUNTERS
        {
            internal ulong ReadOperationCount;
            internal ulong WriteOperationCount;
            internal ulong OtherOperationCount;
            internal ulong ReadTransferCount;
            internal ulong WriteTransferCount;
            internal ulong OtherTransferCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
        {
            internal static readonly int SizeOf = Marshal.SizeOf<JOBOBJECT_EXTENDED_LIMIT_INFORMATION>();

            internal JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
            internal IO_COUNTERS IoInfo;
            internal UIntPtr ProcessMemoryLimit;
            internal UIntPtr JobMemoryLimit;
            internal UIntPtr PeakProcessMemoryUsed;
            internal UIntPtr PeakJobMemoryUsed;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct JOBOBJECT_NOTIFICATION_LIMIT_INFORMATION
        {
            internal static readonly int SizeOf = Marshal.SizeOf<JOBOBJECT_NOTIFICATION_LIMIT_INFORMATION>();

            internal ulong IoReadBytesLimit;
            internal ulong IoWriteBytesLimit;
            internal LARGE_INTEGER PerJobUserTimeLimit;
            internal ulong JobMemoryLimit;
            internal int RateControlTolerance;
            internal int RateControlToleranceInterval;
            internal uint LimitFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct JOBOBJECT_END_OF_JOB_TIME_INFORMATION
        {
            internal static readonly int SizeOf = Marshal.SizeOf<JOBOBJECT_END_OF_JOB_TIME_INFORMATION>();

            internal uint EndOfJobTimeAction;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct JOBOBJECT_BASIC_UI_RESTRICTIONS
        {
            internal static readonly int SizeOf = Marshal.SizeOf<JOBOBJECT_BASIC_UI_RESTRICTIONS>();

            internal uint UIRestrictionsClass;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct JOBOBJECT_CPU_RATE_CONTROL_INFORMATION
        {
            internal static readonly int SizeOf = Marshal.SizeOf<JOBOBJECT_CPU_RATE_CONTROL_INFORMATION>();

            [FieldOffset(0)]
            internal uint ControlFlags;
            [FieldOffset(4)]
            internal uint CpuRate;
            [FieldOffset(4)]
            internal uint Weight;
            [FieldOffset(4)]
            internal ushort MinRate;
            [FieldOffset(6)]
            internal ushort MaxRate;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct JOBOBJECT_BASIC_ACCOUNTING_INFORMATION
        {
            internal static readonly int SizeOf = Marshal.SizeOf<JOBOBJECT_BASIC_ACCOUNTING_INFORMATION>();

            internal LARGE_INTEGER TotalUserTime;
            internal LARGE_INTEGER TotalKernelTime;
            internal LARGE_INTEGER ThisPeriodTotalUserTime;
            internal LARGE_INTEGER ThisPeriodTotalKernelTime;
            internal uint TotalPageFaultCount;
            internal uint TotalProcesses;
            internal uint ActiveProcesses;
            internal uint TotalTerminatedProcesses;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct JOBOBJECT_LIMIT_VIOLATION_INFORMATION
        {
            internal static readonly int SizeOf = Marshal.SizeOf<JOBOBJECT_LIMIT_VIOLATION_INFORMATION>();

            internal uint LimitFlags;
            internal uint ViolationLimitFlags;
            internal ulong IoReadBytes;
            internal ulong IoReadBytesLimit;
            internal ulong IoWriteBytes;
            internal ulong IoWriteBytesLimit;
            internal LARGE_INTEGER PerJobUserTime;
            internal LARGE_INTEGER PerJobUserTimeLimit;
            internal ulong JobMemory;
            internal ulong JobMemoryLimit;
            internal int RateControlTolerance;
            internal int RateControlToleranceLimit;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct JOBOBJECT_BASIC_AND_IO_ACCOUNTING_INFORMATION
        {
            internal static readonly int SizeOf = Marshal.SizeOf<JOBOBJECT_BASIC_AND_IO_ACCOUNTING_INFORMATION>();

            internal JOBOBJECT_BASIC_ACCOUNTING_INFORMATION BasicInformation;
            internal IO_COUNTERS IoInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct JOBOBJECT_BASIC_PROCESS_ID_LIST
        {
            internal static readonly int SizeOf = Marshal.SizeOf<JOBOBJECT_BASIC_PROCESS_ID_LIST>();
            internal static readonly IntPtr ProcessIdsOffset = Marshal.OffsetOf<Interop.Kernel32.JOBOBJECT_BASIC_PROCESS_ID_LIST>("ProcessIdList");

            internal uint NumberOfAssignedProcesses;
            internal uint NumberOfProcessIdsInList;
            internal UIntPtr ProcessIdList;
        }

        [DllImport("kernel32.dll", SetLastError = true, BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "SetInformationJobObject")]
        internal static extern bool SetInformationJobObject ( SafeJobObjectHandle hJob, JobObjectInformationClass JobObjectInfoClass, [In] ref JOBOBJECT_ASSOCIATE_COMPLETION_PORT lpJobObjectInfo, int cbJobObjectInfoLength );

        [DllImport("kernel32.dll", SetLastError = true, BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "SetInformationJobObject")]
        internal static extern bool SetInformationJobObject ( SafeJobObjectHandle hJob, JobObjectInformationClass JobObjectInfoClass, [In] ref JOBOBJECT_BASIC_LIMIT_INFORMATION lpJobObjectInfo, int cbJobObjectInfoLength );

        [DllImport("kernel32.dll", SetLastError = true, BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "SetInformationJobObject")]
        internal static extern bool SetInformationJobObject ( SafeJobObjectHandle hJob, JobObjectInformationClass JobObjectInfoClass, [In] ref JOBOBJECT_EXTENDED_LIMIT_INFORMATION lpJobObjectInfo, int cbJobObjectInfoLength );

        [DllImport("kernel32.dll", SetLastError = true, BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "SetInformationJobObject")]
        internal static extern bool SetInformationJobObject ( SafeJobObjectHandle hJob, JobObjectInformationClass JobObjectInfoClass, [In] ref JOBOBJECT_END_OF_JOB_TIME_INFORMATION lpJobObjectInfo, int cbJobObjectInfoLength );

        [DllImport("kernel32.dll", SetLastError = true, BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "SetInformationJobObject")]
        internal static extern bool SetInformationJobObject ( SafeJobObjectHandle hJob, JobObjectInformationClass JobObjectInfoClass, [In] ref JOBOBJECT_BASIC_UI_RESTRICTIONS lpJobObjectInfo, int cbJobObjectInfoLength );

        [DllImport("kernel32.dll", SetLastError = true, BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "SetInformationJobObject")]
        internal static extern bool SetInformationJobObject ( SafeJobObjectHandle hJob, JobObjectInformationClass JobObjectInfoClass, [In] ref JOBOBJECT_CPU_RATE_CONTROL_INFORMATION lpJobObjectInfo, int cbJobObjectInfoLength );

        [DllImport("kernel32.dll", SetLastError = true, BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "SetInformationJobObject")]
        internal static extern bool SetInformationJobObject ( SafeJobObjectHandle hJob, JobObjectInformationClass JobObjectInfoClass, [In] ref JOBOBJECT_NOTIFICATION_LIMIT_INFORMATION lpJobObjectInfo, int cbJobObjectInfoLength );

        [DllImport("kernel32.dll", SetLastError = true, BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "QueryInformationJobObject")]
        internal static extern bool QueryInformationJobObject ( SafeJobObjectHandle hJob, JobObjectInformationClass JobObjectInfoClass, [Out] out JOBOBJECT_BASIC_ACCOUNTING_INFORMATION lpJobObjectInfo, int cbJobObjectInfoLength, IntPtr lpReturnLength );

        [DllImport("kernel32.dll", SetLastError = true, BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "QueryInformationJobObject")]
        internal static extern bool QueryInformationJobObject ( SafeJobObjectHandle hJob, JobObjectInformationClass JobObjectInfoClass, [Out] out JOBOBJECT_BASIC_AND_IO_ACCOUNTING_INFORMATION lpJobObjectInfo, int cbJobObjectInfoLength, IntPtr lpReturnLength );

        [DllImport("kernel32.dll", SetLastError = true, BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "QueryInformationJobObject")]
        internal static extern bool QueryInformationJobObject ( SafeJobObjectHandle hJob, JobObjectInformationClass JobObjectInfoClass, [Out] out JOBOBJECT_BASIC_LIMIT_INFORMATION lpJobObjectInfo, int cbJobObjectInfoLength, IntPtr lpReturnLength );

        [DllImport("kernel32.dll", SetLastError = true, BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "QueryInformationJobObject")]
        internal static extern bool QueryInformationJobObject ( SafeJobObjectHandle hJob, JobObjectInformationClass JobObjectInfoClass, [Out] out JOBOBJECT_EXTENDED_LIMIT_INFORMATION lpJobObjectInfo, int cbJobObjectInfoLength, IntPtr lpReturnLength );

        [DllImport("kernel32.dll", SetLastError = true, BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "QueryInformationJobObject")]
        internal static extern bool QueryInformationJobObject ( SafeJobObjectHandle hJob, JobObjectInformationClass JobObjectInfoClass, [Out] out JOBOBJECT_END_OF_JOB_TIME_INFORMATION lpJobObjectInfo, int cbJobObjectInfoLength, IntPtr lpReturnLength );

        [DllImport("kernel32.dll", SetLastError = true, BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "QueryInformationJobObject")]
        internal static extern bool QueryInformationJobObject ( SafeJobObjectHandle hJob, JobObjectInformationClass JobObjectInfoClass, [Out] out JOBOBJECT_BASIC_UI_RESTRICTIONS lpJobObjectInfo, int cbJobObjectInfoLength, IntPtr lpReturnLength );

        [DllImport("kernel32.dll", SetLastError = true, BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "QueryInformationJobObject")]
        internal static extern bool QueryInformationJobObject ( SafeJobObjectHandle hJob, JobObjectInformationClass JobObjectInfoClass, [Out] out JOBOBJECT_CPU_RATE_CONTROL_INFORMATION lpJobObjectInfo, int cbJobObjectInfoLength, IntPtr lpReturnLength );

        [DllImport("kernel32.dll", SetLastError = true, BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "QueryInformationJobObject")]
        internal static extern bool QueryInformationJobObject ( SafeJobObjectHandle hJob, JobObjectInformationClass JobObjectInfoClass, [Out] out JOBOBJECT_NOTIFICATION_LIMIT_INFORMATION lpJobObjectInfo, int cbJobObjectInfoLength, IntPtr lpReturnLength );

        [DllImport("kernel32.dll", SetLastError = true, BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "QueryInformationJobObject")]
        internal static extern bool QueryInformationJobObject ( SafeJobObjectHandle hJob, JobObjectInformationClass JobObjectInfoClass, [Out] out JOBOBJECT_LIMIT_VIOLATION_INFORMATION lpJobObjectInfo, int cbJobObjectInfoLength, IntPtr lpReturnLength );

        [DllImport("kernel32.dll", SetLastError = true, BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "QueryInformationJobObject")]
        internal static extern bool QueryInformationJobObject ( SafeJobObjectHandle hJob, JobObjectInformationClass JobObjectInfoClass, IntPtr lpJobObjectInfo, int cbJobObjectInfoLength, IntPtr lpReturnLength );

        [DllImport("kernel32.dll", SetLastError = true, BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "QueryInformationJobObject")]
        internal static extern bool QueryInformationJobObject ( SafeJobObjectHandle hJob, JobObjectInformationClass JobObjectInfoClass, IntPtr lpJobObjectInfo, int cbJobObjectInfoLength, out int lpReturnLength );

        internal static class JobObjectBasicLimits
        {
            internal const int WorkingSet = 0x00001;
            internal const int ProcessTime = 0x00002;
            internal const int JobTime = 0x00004;
            internal const int ActiveProcess = 0x00008;
            internal const int Affinity = 0x00010;
            internal const int PriorityClass = 0x00020;
            internal const int PreserveJobTime = 0x00040;
            internal const int SchedulingClass = 0x00080;
        }

        internal static class JobObjectExtendedLimits
        {
            internal const int ProcessMemory = 0x0100;
            internal const int JobMemory = 0x0200;
            internal const int JobMemoryHigh = JobMemory;
            internal const int DieOnUnhandledException = 0x0400;
            internal const int BreakawayOK = 0x0800;
            internal const int SilentBreakawayOK = 0x1000;
            internal const int KillOnJobClose = 0x2000;
            internal const int SubsetAffinity = 0x4000;
            internal const int JobMemoryLow = 0x8000;
        }

        internal static class JobObjectNotificationLimits
        {
            internal const int JobReadBytes = 0x010000;
            internal const int JobWriteBytes = 0x020000;
            internal const int RateControl = 0x040000;
            internal const int CpuRateControl = RateControl;
            internal const int IoRateControl = 0x080000;
            internal const int NetRateControl = 0x100000;
        }

        internal static class JobObjectUiRestrictions
        {
            internal const uint None = 0;
            internal const uint Handles = 0x0001;
            internal const uint ReadClipboard = 0x0002;
            internal const uint WriteClipboard = 0x0004;
            internal const uint SystemParameters = 0x0008;
            internal const uint DisplaySettings = 0x0010;
            internal const uint GlobalAtoms = 0x0020;
            internal const uint Desktop = 0x0040;
            internal const uint ExitWindows = 0x0080;
        }

        internal static class JobObjectCpuControl
        {
            internal const uint None = 0;
            internal const uint Enable = 0x0001;
            internal const uint WeightBased = 0x0002;
            internal const uint HardCap = 0x0004;
            internal const uint Notify = 0x0008;
            internal const uint MinMaxRate = 0x0010;
        }

        internal enum JobObjectMessage : uint
        {
            EndOfJobTime = 1,
            EndOfProcessTime = 2,
            ActiveProcessLimit = 3,
            ActiveProcessZero = 4,
            NewProcess = 6,
            ExitProcess = 7,
            AbnormalExitProcess = 8,
            ProcessMemoryLimit = 9,
            JobMemoryLimit = 10,
            NotificationLimit = 11,
            JobCycleTimeLimit = 12,
            SiloTerminated = 13
        }

        [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "IsProcessInJob")]
        internal static extern bool IsProcessInJob ( IntPtr ProcessHandle, IntPtr JobHandle, out BOOL Result );

        [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "AssignProcessToJobObject")]
        internal static extern bool AssignProcessToJobObject ( SafeJobObjectHandle hJob, SafeProcessHandle hProcess );

        [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "TerminateProcess")]
        internal static extern bool TerminateProcess ( SafeProcessHandle hProcess, int exitCode );

        [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "TerminateJobObject")]
        internal static extern bool TerminateJobObject ( SafeJobObjectHandle hJob, int uExitCode );
    }
}
