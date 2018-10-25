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

using OpenTK;
using System;

namespace OpenTKUtils.GL4
{
    public interface IGLRenderable : IDisposable            // all renderables inherit from this.
    {
        void Bind(IGLProgramShaders shader);
        void Render();
        IGLObjectInstanceData InstanceData { get; }
    }

    public interface IGLProgramShaders : IDisposable        // All shaders inherit from this
    {
        int Id { get; }
        void Start(Common.MatrixCalc c);                    // Renders call this when program has just started
        void Finish();                                      // Renders call this when program has ended
        IGLProgramShaders GetVertex();
        IGLProgramShaders GetFragment();
    }

    public interface IGLSharedProgramShaders : IGLProgramShaders    // Just to mark them as shared - no extra functionality. Just to allow you to see its a shared prog
    {
    }

    public interface IGLObjectInstanceData                  // ALL object data should inherit from this
    {
        void Bind(IGLProgramShaders shader);                // callled before object is drawn
    }

    public interface IGLTexture : IDisposable
    {
        int Width { get; }                                  // primary width of mipmap level 0 bitmap on first array entry
        int Height { get; }
        void Bind();
    }


}