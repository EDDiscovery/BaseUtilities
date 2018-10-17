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
    public interface IGLProgramShaders : IDisposable        // shaders in the render queue should inherit from this
    {
        int Id { get; }
        void Start(Matrix4 model, Matrix4 proj);            // Renders call this when program has just started
        void Finish();                                      // Renders call this when program has ended
    }

    public interface IGLPipelineShaders : IDisposable       // pipeline shaders
    {
        int Id { get; }
        void Start(Matrix4 model, Matrix4 proj);            // The IGLProgramShader pipeline calls this when started
        void Finish();                                      // and when the pipeline is removed..  
    }

    public interface IGLObjectInstanceData                  // ALL object data should inherit from this
    {
        void Bind();
    }

    public interface IGLTexture : IDisposable
    {
        int Width { get; }
        int Height { get; }
        void Bind();
    }


}