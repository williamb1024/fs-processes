using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.Win32.SafeHandles;

internal partial class Interop
{
    internal class ProcessWaitHandle: WaitHandle
    {
        public ProcessWaitHandle ( SafeProcessHandle processHandle )
        {
            SafeWaitHandle waitHandle = null;
            SafeProcessHandle currentProcess = Kernel32.GetCurrentProcess();

            if (!Kernel32.DuplicateHandle(currentProcess,
                                          processHandle,
                                          currentProcess,
                                          out waitHandle,
                                          0,
                                          false,
                                          Kernel32.HandleOptions.DUPLICATE_SAME_ACCESS))
                throw Fs.Processes.Errors.Win32Error();

            this.SetSafeWaitHandle(waitHandle);
        }
    }
}
