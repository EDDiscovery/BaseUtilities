/*
 * Copyright © 2022-2022 EDDiscovery development team
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software distributed under
 * the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF
 * ANY KIND, either express or implied. See the License for the specific language
 * governing permissions and limitations under the License.
 */

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

namespace BaseUtils
{
    /** This is a helper class to wrap an app-unique per-user mutex. It can be used to ensure that
     * only a single instance of a piece of code runs in a user session.  If this is used to wrap main()
     * it ensures that only a single instance of the entire application can run per-user.
     * Code copied from http://stackoverflow.com/questions/229565/what-is-a-good-pattern-for-using-a-global-mutex-in-c/229567
     */
    public class SingleUserInstance : IDisposable
    {
        public bool hasHandle = false;
        Mutex mutex;

        private void InitMutex()
        {
            string appGuid = ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value.ToString();
            string usernm = System.Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(Environment.UserDomainName ?? "none" + "-" + Environment.UserName ?? "none"));
            string mutexId = $"Global\\{usernm}-{{{appGuid}}}";
            mutex = new Mutex(false, mutexId);

            try
            {
                var allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
                var securitySettings = new MutexSecurity();
                securitySettings.AddAccessRule(allowEveryoneRule);
                mutex.SetAccessControl(securitySettings);
            }
            catch (PlatformNotSupportedException)
            {
                System.Diagnostics.Trace.WriteLine("Unable to set mutex security");
            }
        }

        public SingleUserInstance(int timeOut)
        {
            InitMutex();
            try
            {
                if (timeOut < 0)
                    hasHandle = mutex.WaitOne(Timeout.Infinite, false);
                else
                    hasHandle = mutex.WaitOne(timeOut, false);

                if (hasHandle == false)
                    throw new TimeoutException("Timeout waiting for exclusive access on SingleInstance");
            }
            catch (AbandonedMutexException)
            {
                hasHandle = true;
            }
        }
        public void Dispose()
        {
            if (mutex != null)
            {
                if (hasHandle)
                    mutex.ReleaseMutex();
                mutex.Dispose();
            }
        }
    }

}
