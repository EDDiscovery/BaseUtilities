/*
 * Copyright © 2015 - 2018 EDDiscovery development team
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
 * 
 * EDDiscovery is not affiliated with Frontier Developments plc.
 */

using System;
using System.Collections.Generic;

namespace OpenTKUtils.GL4
{
    public class GLProgramShaderList : IDisposable
    {
        private Dictionary<string, IGLProgramShaders> programshaders;
        private int unnamed = 0;

        public GLProgramShaderList()
        {
            programshaders = new Dictionary<string, IGLProgramShaders>();
        }

        public void Add(string name, IGLProgramShaders r)
        {
            programshaders.Add(name, r);
        }

        public void Add(IGLProgramShaders r)
        {
            programshaders.Add("Unnamed_" + (unnamed++), r);
        }

        public IGLProgramShaders this[string key] { get { return programshaders[key]; } }
        public bool Contains(string key) { return programshaders.ContainsKey(key); }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (IGLProgramShaders r in programshaders.Values)
                    r.Dispose();

                programshaders.Clear();
            }
        }
    }

}
