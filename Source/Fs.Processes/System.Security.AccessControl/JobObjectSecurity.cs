#if NET46 || NET47
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace System.Security.AccessControl
{
    /// <summary>
    /// Represents the Windows Access Control for a named <see cref="Fs.Processes.JobObjects.JobObject"/>. This class
    /// is not available in netstandard 2.0.
    /// </summary>
    public sealed class JobObjectSecurity: NativeObjectSecurity
    {
        /// <summary>
        /// Initializes a new instance of <see cref="JobObjectSecurity"/> with default values.
        /// </summary>
        public JobObjectSecurity ()
            : base(false, ResourceType.KernelObject)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="JobObjectSecurity"/> with the specified sections of the access
        /// control security rules from the JobObject with the specified name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="includeSections"></param>
        public JobObjectSecurity ( string name, AccessControlSections includeSections )
            : base(false, ResourceType.KernelObject, name, includeSections, _HandleErrorCode, null)
        {
        }

        private static Exception _HandleErrorCode ( int errorCode, string name, SafeHandle handle, object context )
        {
            return null;
        }

        /// <summary>
        /// Creates a new access control rule for the specified user, with the specified access rights, access control, and flags. 
        /// </summary>
        /// <param name="identityReference">An IdentityReference that identifies the user or group the rule applies to.</param>
        /// <param name="accessMask">A bitwise combination of <see cref="JobObjectRights"/> values specifying the access rights to audit, cast to an integer.</param>
        /// <param name="isInherited">Meaningless for named JobObject handles, because they have no hierarchy.</param>
        /// <param name="inheritanceFlags">Meaningless for named JobObject handles, because they have no hierarchy.</param>
        /// <param name="propagationFlags">Meaningless for named JobObject handles, because they have no hierarchy.</param>
        /// <param name="type">One of the AccessControlType values specifying whether the rights are allowed or denied.</param>
        /// <returns>A <see cref="JobObjectAccessRule"/> object representing the specified access rule for the specified user. The return type of the method is the base
        /// class, AccessRule, but the value can be cast safely to the derived class.</returns>
        public override AccessRule AccessRuleFactory ( IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type )
        {
            return new JobObjectAccessRule(identityReference, accessMask, isInherited, inheritanceFlags, propagationFlags, type);
        }

        /// <summary>
        /// Creates a new audit rule, specifying the user the rule applies to, the access rights to audit, and the outcome that triggers the audit rule.
        /// </summary>
        /// <param name="identityReference">An IdentityReference that identifies the user or group the rule applies to.</param>
        /// <param name="accessMask">A bitwise combination of <see cref="JobObjectRights"/> values specifying the access rights to audit, cast to an integer.</param>
        /// <param name="isInherited">Meaningless for named JobObject handles, because they have no hierarchy.</param>
        /// <param name="inheritanceFlags">Meaningless for named JobObject handles, because they have no hierarchy.</param>
        /// <param name="propagationFlags">Meaningless for named JobObject handles, because they have no hierarchy.</param>
        /// <param name="flags">A bitwise combination of AuditFlags values that specify whether to audit successful access, failed access, or both.</param>
        /// <returns>A <see cref="JobObjectAuditRule"/> object representing the specified audit rule for the specified user. The return type of the method is the base
        /// class, AuditRule, but the value can be cast safely to the derived class.</returns>
        public override AuditRule AuditRuleFactory ( IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags )
        {
            return new JobObjectAuditRule(identityReference, accessMask, isInherited, inheritanceFlags, propagationFlags, flags);
        }

        /// <summary>
        /// Searches for a matching access control rule with which the new rule can be merged. If none are found, adds the new rule.
        /// </summary>
        /// <param name="rule">The access control rule to add.</param>
        public void AddAccessRule ( JobObjectAccessRule rule ) => base.AddAccessRule(rule);

        /// <summary>
        /// Removes all access control rules with the same user and AccessControlType (allow or deny) as the specified rule, and then adds the specified rule.
        /// </summary>
        /// <param name="rule">The access control rule to add.</param>
        public void SetAccessRule ( JobObjectAccessRule rule ) => base.SetAccessRule(rule);

        /// <summary>
        /// Removes all access control rules with the same user as the specified rule, regardless of AccessControlType, and then adds the specified rule.
        /// </summary>
        /// <param name="rule">The access control rule to add.</param>
        public void ResetAccessRule ( JobObjectAccessRule rule ) => base.ResetAccessRule(rule);

        /// <summary>
        /// Searches for an access control rule with the same user and AccessControlType (allow or deny) as the specified rule, 
        /// and with compatible inheritance and propagation flags; if such a rule is found, the rights contained in the specified access rule are removed from it.
        /// </summary>
        /// <param name="rule">The access control rule to match.</param>
        /// <returns><c>true</c> if a compatible rule is found; otherwise, <c>false</c>.</returns>
        public bool RemoveAccessRule ( JobObjectAccessRule rule ) => base.RemoveAccessRule(rule);

        /// <summary>
        /// Searches for all access control rules with the same user and AccessControlType (allow or deny) as the specified rule and, if found, removes them.
        /// </summary>
        /// <param name="rule">The access control rule to match.</param>
        public void RemoveAccessRuleAll ( JobObjectAccessRule rule ) => base.RemoveAccessRuleAll(rule);

        /// <summary>
        /// Searches for an access control rule that exactly matches the specified rule and, if found, removes it.
        /// </summary>
        /// <param name="rule">The access control rule to remove.</param>
        public void RemoveAccessRuleSpecific ( JobObjectAccessRule rule ) => base.RemoveAccessRuleSpecific(rule);

        /// <summary>
        /// Searches for a matching audit control rule with which the new rule can be merged. If none are found, adds the new rule.
        /// </summary>
        /// <param name="rule">The Audit control rule to add.</param>
        public void AddAuditRule ( JobObjectAuditRule rule ) => base.AddAuditRule(rule);

        /// <summary>
        /// Removes all audit control rules with the same user and AuditControlType (allow or deny) as the specified rule, and then adds the specified rule.
        /// </summary>
        /// <param name="rule">The Audit control rule to add.</param>
        public void SetAuditRule ( JobObjectAuditRule rule ) => base.SetAuditRule(rule);

        /// <summary>
        /// Searches for an audit control rule with the same user and AuditControlType (allow or deny) as the specified rule, 
        /// and with compatible inheritance and propagation flags; if such a rule is found, the rights contained in the specified Audit rule are removed from it.
        /// </summary>
        /// <param name="rule">The Audit control rule to match.</param>
        /// <returns><c>true</c> if a compatible rule is found; otherwise, <c>false</c>.</returns>
        public bool RemoveAuditRule ( JobObjectAuditRule rule ) => base.RemoveAuditRule(rule);

        /// <summary>
        /// Searches for all audit control rules with the same user and AuditControlType (allow or deny) as the specified rule and, if found, removes them.
        /// </summary>
        /// <param name="rule">The Audit control rule to match.</param>
        public void RemoveAuditRuleAll ( JobObjectAuditRule rule ) => base.RemoveAuditRuleAll(rule);

        /// <summary>
        /// Searches for an audit control rule that exactly matches the specified rule and, if found, removes it.
        /// </summary>
        /// <param name="rule">The Audit control rule to remove.</param>
        public void RemoveAuditRuleSpecific ( JobObjectAuditRule rule ) => base.RemoveAuditRuleSpecific(rule);

        /// <summary>
        /// Gets the enumeration that the <see cref="JobObjectSecurity"/> class uses to represent access rights.
        /// </summary>
        public override Type AccessRightType => typeof(JobObjectRights);

        /// <summary>
        /// Gets the type that the <see cref="JobObjectSecurity"/> class uses to represent access rules.
        /// </summary>
        public override Type AccessRuleType => typeof(JobObjectAccessRule);

        /// <summary>
        /// Gets the type that the <see cref="JobObjectSecurity"/> class uses to represent audit rules.
        /// </summary>
        public override Type AuditRuleType => typeof(JobObjectAuditRule);
    }
}
#endif