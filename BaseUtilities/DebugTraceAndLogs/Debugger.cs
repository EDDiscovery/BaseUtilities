/*
 * Copyright © 2024 - 2024 EDDiscovery development team
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

namespace BaseUtils
{
    // use instead of ASSERT and it breaks on the caller not the callee..

    public static class DebuggerHelpers
    {
        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerHidden]
        public static void BreakAssert(bool value, string report)
        {
            if (!value)
            {
                System.Diagnostics.Debug.WriteLine("!!! Break Assert: " + report);
                System.Diagnostics.Debugger.Break();
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerHidden]
        public static void BreakAssert(bool value, Func<string> report)
        {
            if (!value)
            {
                string reportstring = report();
                System.Diagnostics.Debug.WriteLine("!!! Break Assert: " + reportstring);
                System.Diagnostics.Debugger.Break();
            }
        }
    }

}

