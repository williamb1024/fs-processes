using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Security;
using System.Text;

namespace Fs.Processes
{
    /// <summary>
    /// Specifies a set of values used when creating a new process.
    /// </summary>
    public sealed class CreateProcessInfo
    {
        private string _fileName;
        private string _userName;
        private string _domain;
        private string _arguments;
        private string _workingDirectory;
        private List<string> _argumentsList;
        private Dictionary<string, string> _environment;
        private CreateProcessAttributeList _attributeList;

        private static Dictionary<string, string> GetCurrentEnvironment ()
        {
            var currentEnvironment = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            IDictionaryEnumerator environmentEnumerator = System.Environment.GetEnvironmentVariables().GetEnumerator();
            while (environmentEnumerator.MoveNext())
            {
                DictionaryEntry entry = environmentEnumerator.Entry;
                currentEnvironment.Add((string)entry.Key, (string)entry.Value);
            }

            return currentEnvironment;
        }

        internal bool HasStandardRedirect { get { return ((RedirectStandardInput) || (RedirectStandardOutput) || (RedirectStandardError)); } }
        internal bool HasArgumentsList { get { return (_argumentsList != null) && (_argumentsList.Count > 0); } }
        internal bool HasEnvironment { get { return _environment != null; } }
        internal bool HasAttributes { get { return (_attributeList != null) && (_attributeList.Count != 0); } }

        /// <summary>
        /// Gets or sets the executable name for the process.
        /// </summary>
        public string FileName
        {
            get => _fileName ?? String.Empty;
            set => _fileName = value;
        }

        /// <summary>
        /// Gets or sets the command-line arguments to use when creating the process. The contents of this
        /// property is used as-is when building the new process's command line. This property must be
        /// <see cref="String.Empty"/> if <see cref="ArgumentsList"/> is used.
        /// </summary>
        public string Arguments
        {
            get => _arguments ?? String.Empty;
            set => _arguments = value;
        }

        /// <summary>
        /// Gets or sets the working directory to use when creating the process.
        /// </summary>
        public string WorkingDirectory
        {
            get => _workingDirectory ?? String.Empty;
            set => _workingDirectory = value;
        }

        /// <summary>
        /// Gets or sets the value that identifies the domain to use when creating the process. If this value is <c>null</c>,
        /// the <see cref="UserName"/> property must be specified in UPN format.
        /// </summary>
        public string Domain
        {
            get => _domain ?? String.Empty;
            set => _domain = value;
        }

        /// <summary>
        /// Gets or sets the user name to use when creating the process. If you use the UPN format, 
        /// <code>user@DNS_domain_name</code>, the <see cref="Domain"/> property must be <c>null</c>.
        /// </summary>
        public string UserName
        {
            get => _userName ?? String.Empty;
            set => _userName = value;
        }

        /// <summary>
        /// Gets or sets the user password in clear text to use when creating the process.
        /// </summary>
        public string PasswordInClearText { get; set; }

        /// <summary>
        /// Gets or sets a secure string that contains the user password to use when
        /// creating the process.
        /// </summary>
        public SecureString Password { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whehter the Windows user profile is to be
        /// loaded from the registry.
        /// </summary>
        public bool LoadUserProfile { get; set; }

        /// <summary>
        /// Gets or sets the name of the desktop, or the name of both the desktop and window station 
        /// for this process. A backslash in the string indicates that the string includes both the desktop and 
        /// window station names.
        /// </summary>
        public string Desktop { get; set; }

        /// <summary>
        /// Gets or sets the value used as the title for the new process. For console processes, this is the title displayed in the title bar if a 
        /// new console window is created. If <c>null</c>, the name of the executable file is used as the window title instead. This 
        /// parameter must be <c>null</c> for GUI or console processes that do not create a new console window.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets a value that determines how the <see cref="Title"/> value is used.
        /// </summary>
        public TitleIs TitleIs { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Point"/> used as the default window position for GUI processes.
        /// </summary>
        public Point? WindowPosition { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Size"/> used as the default width and height for GUI processes.
        /// </summary>
        public Size? WindowSize { get; set; }

        /// <summary>
        /// Gets or sets the window state to use when the process is created.
        /// </summary>
        public WindowShow? WindowShow { get; set; }

        /// <summary>
        /// Gets or sets whether the cursor is in feedback mode when the process is created.
        /// </summary>
        public bool? ForceFeedback { get; set; }

        /// <summary>
        /// Gets or sets whether any windows created by the process cannot be pinned on the
        /// taskbar. <see cref="TitleIs"/> must be <see cref="TitleIs.AppID"/> if this property
        /// is <c>true</c>.
        /// </summary>
        public bool PreventPinning { get; set; }

        /// <summary>
        /// Gets or sets the default console buffer width and height when a new console window is created
        /// for a console process.
        /// </summary>
        public Size? ConsoleBufferSize { get; set; }

        /// <summary>
        /// Gets or sets the default console colors when a new console window is created for a console
        /// process.
        /// </summary>
        public ConsoleColors? ConsoleFill { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the command line came from an untrusted source.
        /// </summary>
        public bool UntrustedSource { get; set; }

        /// <summary>
        /// Gets or sets the hotkey assigned to the first eligible top-level window created by the new 
        /// process. This property cannot be used with <see cref="RedirectStandardOutput"/>, <see cref="RedirectStandardInput"/> or
        /// <see cref="RedirectStandardError"/>.
        /// </summary>
        public int HotKey { get; set; }

        /// <summary>
        /// Gets the environment variables that apply to the new process and its child processes.
        /// </summary>
        public IDictionary<string, string> Environment { get { return (_environment ?? (_environment = GetCurrentEnvironment())); } }

        /// <summary>
        /// Gets the list of arguments to pass to the new process. Each argument is escaped, if needed, when building the
        /// new process's command line. This property cannot be used with <see cref="Arguments"/>.
        /// </summary>
        public IList<string> ArgumentsList { get { return (_argumentsList ?? (_argumentsList = new List<string>())); } }

        /// <summary>
        /// Gets the list of attributes that contains attributes applied to the new process.
        /// </summary>
        public CreateProcessAttributeList Attributes { get { return (_attributeList ?? (_attributeList = new CreateProcessAttributeList())); } }

        /// <summary>
        /// Gets or sets a value indicating whether the input for the new process is read from the
        /// <see cref="Process.StandardInput"/> stream.
        /// </summary>
        public bool RedirectStandardInput { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whehter the error output of the process is written to 
        /// the <see cref="Process.StandardOutput"/> stream.
        /// </summary>
        public bool RedirectStandardOutput { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whehter the error output of the process is written to 
        /// the <see cref="Process.StandardError"/> stream.
        /// </summary>
        public bool RedirectStandardError { get; set; }

        /// <summary>
        /// Gets or sets the encoding to be used when writing to <see cref="Process.StandardInput"/>. When <c>null</c>, the
        /// console's current encoding is used.
        /// </summary>
        public Encoding StandardInputEncoding { get; set; }

        /// <summary>
        /// Gets or sets the encoding to be used when reading <see cref="Process.StandardOutput"/>. When <c>null</c>, the
        /// console's current encoding is used.
        /// </summary>
        public Encoding StandardOutputEncoding { get; set; }

        /// <summary>
        /// Gets or sets the encoding to be used when reading <see cref="Process.StandardError"/>. When <c>null</c>, the
        /// console's current encoding is used.
        /// </summary>
        public Encoding StandardErrorEncoding { get; set; }
    }
}
