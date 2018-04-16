# Processes

#### Start a New Process

Starting a new process consists of constructing and initializing an instance of @Fs.Processes.CreateProcessInfo, followed by passing the newly constructed instance to one of the @Fs.Processes.Process constructors.

At its most basic, creating an new process is as follows. This code creates a new instance of `notepad.exe`.

```` CSharp
using (var process = new Process(new CreateProcessInfo {
    FileName = "notepad.exe"
})) {
    // the new process continues to run even after the process
    // variable is disposed.
}
````

##### Settings for the New Process

Choosing the executable and arguments for a process is done through the @Fs.Processes.CreateProcessInfo class. The properties of the @Fs.Processes.CreateProcessInfo class are nearly identical to [System.Diagnostics.ProcessStartInfo](https://msdn.microsoft.com/en-us/library/system.diagnostics.processstartinfo(v=vs.110).aspx) properties.

The @Fs.Processes.CreateProcessInfo.FileName property indicates the name of executable image. The @Fs.Processes.CreateProcessInfo.Arguments and @Fs.Processes.CreateProcessInfo.ArgumentsList properties are used to provide arguments for the new process.

Only one of @Fs.Processes.CreateProcessInfo.Arguments and @Fs.Processes.CreateProcessInfo.ArgumentsList may be used. The contents of @Fs.Processes.CreateProcessInfo.Arguments is passed as the command line arguments to the new process exactly as-is. The elements of @Fs.Processes.CreateProcessInfo.ArgumentsList are escaped according the requirements of [CommandLineToArgvW](https://msdn.microsoft.com/en-us/library/windows/desktop/bb776391(v=vs.85).aspx) and join by space separators.

You may further control how the new process is created by passing a set of @Fs.Processes.ProcessOptions flags to [Process(CreateProcessInfo, ProcessOptions)](xref:Fs.Processes.Process.%23ctor%28Fs.Processes.CreateProcessInfo,Fs.Processes.ProcessOptions%29). For example, you may use [ProcessOptions.Suspended](xref:Fs.Processes.ProcessOptions.Suspended) to create the new process in a suspended state.

#### Handle Inheritance

Handle inheritance operates different when creating a new process with the @Fs.Processes.Process class. Unlike [System.Diagnositcs.Process](https://msdn.microsoft.com/en-us/library/system.diagnostics.process(v=vs.110).aspx) inheritable handles are not automatically inherited by the child process. Only handles listed in a @Fs.Processes.InheritHandlesAttribute instance added to [CreateProcessInfo.Attributes](xref:Fs.Processes.CreateProcessInfo.Attributes) are inherited by the new process.

```CSharp
var createProcessInfo = new CreateProcessInfo {
    FileName = "process.exe",
    Attributes = {
        new InheritHandleAttribute { 
            HandleOne,
            HandleTwo,
            HandleTree
        }
    }
};
```

The process created with the `createProcessInfo` settings above will inherit handles `HandleOne`, `HandleTwo` and `HandleThree`. The handles must be marked as inheritable.

##### Exception to the Rule

When [CreateProcessInfo.UserName](xref:Fs.Processes.CreateProcessInfo.UserName) handle inheritance reverts to inheriting all inheritable handles from the current process. The Windows [CreateProcessWithLogonW](https://msdn.microsoft.com/en-us/library/windows/desktop/ms682431(v=vs.85).aspx) API does not support [STARTUPINFOEX](https://msdn.microsoft.com/en-us/library/windows/desktop/ms686329(v=vs.85).aspx).

#### Wait for a Process to Exit

Use the [Process.Exited](xref:Fs.Processes.Process.Exited) property to determine when a process has exited. [Process.Exited](xref:Fs.Processes.Process.Exited) property returns `Task<int>` that completes when the process has exited.

If the [Process](xref:Fs.Processes.Process) is disposed before the underlying process exists, the [Process.Exited](xref:Fs.Processes.Process.Exited) task is transitioned to a canceled state.

#### Redirect Standard Output

The @Fs.Processes.CreateProcessInfo class provides three properties to enable I/O redirection: @Fs.Processes.CreateProcessInfo.RedirectStandardInput, @Fs.Processes.CreateProcessInfo.RedirectStandardOutput and @Fs.Processes.CreateProcessInfo.RedirectStandardError. Set one or more of these properties to `true` to enable I/O redirection for the new process.

Once the new process has started, you may use the [Process](xref:Fs.Processes.Process)'s class @Fs.Processes.Process.StandardInput, @Fs.Processes.Process.StandardOutput or @Fs.Processes.Process.StandardError properties to access the redirected streams. Be aware that accessing more than one stream in a synchronous manner may lead to a deadlock.

##### Asynchronous Reading

Rather than using the @Fs.Processes.Process.StandardOutput or @Fs.Processes.Process.StandardError, you may use @Fs.Processes.Process.BeginReadingStandardOutputAsync(System.Boolean) or @Fs.Processes.Process.BeginReadingStandardErrorAsync(System.Boolean).

Using @Fs.Processes.Process.BeginReadingStandardOutputAsync(System.Boolean) or @Fs.Processes.Process.BeginReadingStandardErrorAsync(System.Boolean) along with @Fs.Processes.Process.OutputDataReceived and @Fs.Processes.Process.ErrorDataReceived events enables asynchronous reading of the redirected streams.

Both @Fs.Processes.Process.BeginReadingStandardOutputAsync(System.Boolean) and @Fs.Processes.Process.BeginReadingStandardErrorAsync(System.Boolean) return a `Task` instance that completes when no more data will be read from the underlying stream. If @Fs.Processes.Process.Dispose is called before the streams complete reading, the `Task` will be transitioned to an canceled state.