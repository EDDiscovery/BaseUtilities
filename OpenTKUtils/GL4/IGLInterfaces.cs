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

namespace OpenTKUtils.GL4
{
    public interface IGLVertexArray : IDisposable       
    {
        int Count { get; }
        void Bind(IGLProgramShader shader);
    }

    public interface IGLInstanceData : IDisposable   
    {
        void Bind(IGLProgramShader shader);          
    }

    public interface IGLShader : IDisposable                // All shaders inherit from this
    {
        int Id { get; }
        void Start(Common.MatrixCalc c);                    // Renders call this when program has just started
        void Finish();                                      // Renders call this when program has ended
    }

    public interface IGLProgramShader : IGLShader           // Shaders suitable for the rendering queue inherit from this
    {
        IGLShader Get(OpenTK.Graphics.OpenGL4.ShaderType t);    // get a subcomponent.  if the shader does not have subcomponents, its should return itself.
        Action<IGLProgramShader> StartAction { get; set; }       
        Action<IGLProgramShader> FinishAction { get; set; }      
    }

    public interface IGLTexture : IDisposable
    {
        int Id { get; }
        int Width { get; }                                  // primary width of mipmap level 0 bitmap on first array entry
        int Height { get; }
        void Bind(int bindingpoint);
    }


}