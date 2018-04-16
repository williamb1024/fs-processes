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
    /// Represents a set of access rights to be audited for a user or group.
    /// </summary>
    public sealed class JobObjectAuditRule : AuditRule
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="JobObjectAuditRule"/> class, specifying the user or group 
        /// to audit, the rights to audit, and whether to audit success, failure, or both.
        /// </summary>
        /// <param name="identity">The user or group the rule applies to.</param>
        /// <param name="jobRights">A bitwise combination of <see cref="JobObjectRights"/> values specifygin the kinds of access to audit.</param>
        /// <param name="flags">A bitwise combination of AuditFlags values specifying whether to audit success, failure, or both.</param>
        public JobObjectAuditRule ( IdentityReference identity, JobObjectRights jobRights, AuditFlags flags )
            : this(identity, (int)jobRights, false, InheritanceFlags.None, PropagationFlags.None, flags)
        {
        }

        internal JobObjectAuditRule ( IdentityReference identity, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags )
            : base(identity, accessMask, isInherited, inheritanceFlags, propagationFlags, flags)
        {
        }

        /// <summary>
        /// Gets the access rights audited by the audit rule.
        /// </summary>
        public JobObjectRights JobObjectRights
        {
            get { return (JobObjectRights)base.AccessMask; }
        }
    }
}
#endif