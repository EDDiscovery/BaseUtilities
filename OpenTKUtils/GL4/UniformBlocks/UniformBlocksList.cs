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

    public class GLUniformBlockList: IDisposable
    {
        private Dictionary<int, GLUniformBlock> blocks;

        public GLUniformBlockList()
        {
            blocks = new Dictionary<int, GLUniformBlock>();
        }

        public GLUniformBlock Add(GLUniformBlock u)
        {
            blocks.Add(u.BindingIndex, u);
            return u;
        }

        public GLUniformBlock this[int key] { get { return blocks[key]; } }
        public bool Contains(int key) { return blocks.ContainsKey(key); }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (IGLTexture r in blocks.Values)
                    r.Dispose();

                blocks.Clear();
            }
        }
    }
}
