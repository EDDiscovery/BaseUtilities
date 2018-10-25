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
using System.Drawing;
using System.Linq;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKUtils.GL4
{
    // List of textures

    public class GLTextureList : IDisposable
    {
        private Dictionary<string, IGLTexture> textures;
        private int unnamed = 0;

        public GLTextureList()
        {
            textures = new Dictionary<string, IGLTexture>();
        }

        public IGLTexture Add(string name, IGLTexture r)
        {
            textures.Add(name, r);
            return r;
        }

        public IGLTexture Add(IGLTexture r)
        {
            textures.Add("Unnamed_" + (unnamed++), r);
            return r;
        }

        public IGLTexture this[string key] { get { return textures[key]; } }
        public bool Contains(string key) { return textures.ContainsKey(key); }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (IGLTexture r in textures.Values)
                    r.Dispose();

                textures.Clear();
            }
        }
    }
}
