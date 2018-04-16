#if NET46 || NET47
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace System.Security.AccessControl
{
    /// <summary>
    /// Represents a set of access rights allowed or denied for a user or group.
    /// </summary>
    public sealed class JobObjectAccessRule : AccessRule
    {
        /// <summary>
        /// Initializes a new instance of the JobObjectAccessRule class, specifying the user or group the rule applies to, 
        /// the access rights, and whether the specified access rights are allowed or denied.
        /// </summary>
        /// <param name="identity">The user or group the rule applies to. Must be a type that can be converted to a SecurityIdentifier.</param>
        /// <param name="jobRights">A bitwise combination of <see cref="JobObjectRights"/> values specifying the rights allowed or denied.</param>
        /// <param name="type">One of the AccessControlType values specifying whether the rights are allowed or denied.</param>
        public JobObjectAccessRule ( IdentityReference identity, JobObjectRights jobRights, AccessControlType type )
            : this(identity, (int)jobRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        /// <summary>
        /// Initializes a new instance of the JobObjectAccessRule class, specifying the user or group the rule applies to, 
        /// the access rights, and whether the specified access rights are allowed or denied.
        /// </summary>
        /// <param name="identity">The name of the user or group the rule applies to.</param>
        /// <param name="jobRights">A bitwise combination of <see cref="JobObjectRights"/> values specifying the rights allowed or denied.</param>
        /// <param name="type">One of the AccessControlType values specifying whether the rights are allowed or denied.</param>
        public JobObjectAccessRule ( string identity, JobObjectRights jobRights, AccessControlType type )
            : this(new NTAccount(identity), (int)jobRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        internal JobObjectAccessRule ( IdentityReference identity, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type )
            : base(identity, accessMask, isInherited, inheritanceFlags, propagationFlags, type)
        {
        }

        /// <summary>
        /// Gets the right allowed or denied by the access rule.
        /// </summary>
        public JobObjectRights JobObjectRights
        {
            get { return (JobObjectRights)base.AccessMask; }
        }
    }
}
#endif
