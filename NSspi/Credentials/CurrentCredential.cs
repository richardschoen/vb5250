﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NSspi.Credentials
{
    /// <summary>
    /// Acquires a handle to the credentials of the user associated with the current process.
    /// </summary>
    public class CurrentCredential : Credential
    {
        /// <summary>
        /// Initializes a new instance of the CurrentCredential class.
        /// </summary>
        /// <param name="securityPackage">The security package to acquire the credential handle
        /// from.</param>
        /// <param name="use">The manner in which the credentials will be used - Inbound typically
        /// represents servers, outbound typically represent clients.</param>
        public CurrentCredential( string securityPackage, CredentialUse use ) :
            base( securityPackage )
        {
            Init( use );
        }

        private void Init( CredentialUse use )
        {
            string packageName;
            TimeStamp rawExpiry = new TimeStamp();
            SecurityStatus status = SecurityStatus.InternalError;

            // -- Package --
            // Copy off for the call, since this.SecurityPackage is a property.
            packageName = this.SecurityPackage;

            this.Handle = new SafeCredentialHandle();

            // The finally clause is the actual constrained region. The VM pre-allocates any stack space,
            // performs any allocations it needs to prepare methods for execution, and postpones any 
            // instances of the 'uncatchable' exceptions (ThreadAbort, StackOverflow, OutOfMemory).
            RuntimeHelpers.PrepareConstrainedRegions();
            try { }
            finally
            {
                status = CredentialNativeMethods.AcquireCredentialsHandle(
                   null,
                   packageName,
                   use,
                   IntPtr.Zero,
                   IntPtr.Zero,
                   IntPtr.Zero,
                   IntPtr.Zero,
                   ref this.Handle.rawHandle,
                   ref rawExpiry
               );
            }

            if ( status != SecurityStatus.OK )
            {
                throw new SSPIException( "Failed to call AcquireCredentialHandle", status );
            }

            this.Expiry = rawExpiry.ToDateTime();
        }

    }
}
