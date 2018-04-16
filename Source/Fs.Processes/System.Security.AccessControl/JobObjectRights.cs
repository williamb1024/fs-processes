#if NET46 || NET47
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Security.AccessControl
{
    /// <summary>
    /// Access rights for Job Objects
    /// </summary>
    public enum JobObjectRights
    {
        /// <summary>
        /// Required to call the AssignProcessToJobObject function to assign processes to the job object.
        /// </summary>
        AssignProcess = 0x0001,

        /// <summary>
        /// Required to retrieve certain information abou the job object.
        /// </summary>
        Query = 0x0004,

        /// <summary>
        /// Required to call SetInformationJobObject
        /// </summary>
        SetAttributes = 0x0002,

        /// <summary>
        /// The flag is not supported. You must set security limitations individually for each process.
        /// </summary>
        SetSecurityAttributes = 0x0010,

        /// <summary>
        /// Required to call the TerminateJobObject function.
        /// </summary>
        Terminate = 0x0008,

        /// <summary>
        /// Required to delete the object
        /// </summary>
        Delete               = 0x010000,

        /// <summary>
        /// Required to read information in the security descriptor for the object, not including the information in the SACL.
        /// </summary>
        ReadPermissions      = 0x020000,

        /// <summary>
        /// Required the modify the DACL in the security descriptor of the object.
        /// </summary>
        ChangePermissions    = 0x040000,

        /// <summary>
        /// Required to change the owner in the security descriptor for the object.
        /// </summary>
        TakeOwnership        = 0x080000,

        /// <summary>
        /// The right to use the object for synchronization.
        /// </summary>
        Synchronize          = 0x100000,  // SYNCHRONIZE

        /// <summary>
        /// All access.
        /// </summary>
        FullControl          = 0x1F001F
    }
}
#endif