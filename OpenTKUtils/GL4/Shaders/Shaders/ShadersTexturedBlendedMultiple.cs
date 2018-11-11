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
    // Shader, with tesselation, and Y change in amp using sin

        2DArray next and blend..

    public class GLMultipleTexturedBlended : GLShaderProgramBase
    {
        string vertpos =
        @"
#version 450 core
layout (location = 0) in vec4 position;     
layout (location = 1) in vec2 texco;
layout(location = 2) in vec4 instancepos;       

layout (location = 20) uniform  mat4 projectionmodel;
layout (location = 23) uniform  mat4 commontransform;

out vec2 vs_textureCoordinate;

void main(void)
{
    vec4 p = commontransform * position;
    p = p + instancepos;
	gl_Position = projectionmodel  * p;       
    vs_textureCoordinate = texco;
}
";
        string vertmat =
        @"
#version 450 core
layout (location = 0) in vec4 position;     // from buffer1
layout (location = 1) in vec2 texco;
layout(location = 4) in mat4 mat;       // from buffer2

layout (location = 20) uniform  mat4 projectionmodel;
layout (location = 23) uniform  mat4 commontransform;

out vec2 vs_textureCoordinate;

void main(void)
{
    vec4 p = position;
	gl_Position = projectionmodel  * mat * commontransform * p;       
    vs_textureCoordinate = texco;
}
";

        string frag =

@"
#version 450 core

in vec2 vs_textureCoordinate;
out vec4 color;

layout (binding=1) uniform sampler2D textureObject;
//layout (location = 30) uniform  float blend;

void main(void)
{
    color = texture(textureObject, vs_textureCoordinate);       // vs_texture coords normalised 0 to 1.0f
}
";
        public float Blend { get; set; } = 0;                   // set to animate.
        public GLObjectDataTranslationRotation CommonTransform { get; set; }           // only use this for rotation - position set by object data

        public GLMultipleTexturedBlended(bool matrix)
        {
            Compile(vertex: (matrix ? vertmat : vertpos), frag: frag);
            CommonTransform = new GLObjectDataTranslationRotation();
        }

        public override void Start(Common.MatrixCalc c)
        {
            base.Start(c);

            GL.ProgramUniform1(Id, 25, Blend);

            Matrix4 t = CommonTransform.Transform;
            GL.ProgramUniformMatrix4(Id, 23, false, ref t);

            GL4Statics.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
        }

        public override void Finish()
        {
            base.Finish();
        }
    }
}
