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
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKUtils.GL4
{
    // Simple rendered with optional rot/translation

    public class GLColourObjectShaderTranslation : GLProgramShaderPipeline
    {
        public GLColourObjectShaderTranslation() : base()
        {
            AddVertex(new GLVertexShaderTransform());
            AddFragment(new GLFragmentShaderPassThru());
        }
    }

    public class GLColourObjectShaderNoTranslation : GLProgramShaderPipeline
    {
        public GLColourObjectShaderNoTranslation() : base()
        {
            AddVertex(new GLVertexShaderNoTranslation());
            AddFragment(new GLFragmentShaderPassThru());
        }
    }


}
