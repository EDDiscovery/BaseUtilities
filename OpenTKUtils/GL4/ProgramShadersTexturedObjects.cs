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
    // Simple rendered texture
    // with optional rot/translation per object - needs a GLObjectDataTranslationRotation per object passing transform into 22 

    public class GLVertexTexturedObjectShaderSimple : IGLProgramShaders
    {
        private GLProgram program;
        public int Id { get { return program.Id; } }

        string fragmenttexture =
@"
#version 450 core
in vec2 vs_textureCoordinate;
uniform sampler2D textureObject;
out vec4 color;

void main(void)
{
	color = texelFetch(textureObject, ivec2(vs_textureCoordinate.x, vs_textureCoordinate.y), 0);
}
";

static string vertexnotx =
@"
#version 450 core
layout (location = 0) in vec4 model;
layout (location = 1) in vec2 textureCoordinate;

out vec2 vs_textureCoordinate;

layout(location = 20) uniform  mat4 projection;
layout (location = 21) uniform  mat4 modelView;

void main(void)
{
	vs_textureCoordinate = textureCoordinate;

	gl_Position = projection * modelView * model ;
}
";


static string vertextx =
@"
#version 450 core
layout (location = 0) in vec4 model;
layout (location = 1) in vec2 textureCoordinate;

out vec2 vs_textureCoordinate;

layout(location = 20) uniform  mat4 projection;
layout (location = 21) uniform  mat4 modelView;
layout (location = 22) uniform  mat4 transform;

void main(void)
{
	vs_textureCoordinate = textureCoordinate;

	gl_Position = projection * modelView * transform * model ;
}
";
        public GLVertexTexturedObjectShaderSimple(bool withtransform)           // if with transform, object needs to pass in uniform 22 the transform
        {
            program = new OpenTKUtils.GL4.GLProgram();
            program.Compile(OpenTK.Graphics.OpenGL4.ShaderType.VertexShader, withtransform ? vertextx : vertexnotx);
            program.Compile(OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader, fragmenttexture);
            string ret = program.Link();
            System.Diagnostics.Debug.Assert(ret == null);
        }

        public void Use(Matrix4 model, Matrix4 projection)
        {
            System.Diagnostics.Debug.WriteLine("Program changed " + program.Id + " ShaderTexture");
            program.Use();
            GL.UniformMatrix4(20, false, ref projection);
            GL.UniformMatrix4(21, false, ref model);        // pass in uniform var the model matrix
        }

        public void Dispose()
        {
            program.Dispose();
        }
    }

    // Perfom a common transform on top of the transform per object.
    // needs a GLObjectDataTranslationRotation per object passing transform into 22 
    // the object can be position/rotated, and then a common translate/rotate can be placed over the top by setting Transform

    public class GLVertexTexturedObjectShaderCommonTransform : IGLProgramShaders
    {
        private GLProgram program;
        public int Id { get { return program.Id; } }

        public GLObjectDataTranslationRotation Transform { get; set; }           // only use this for rotation - position set by object data

        string fragmenttexture =
@"
#version 450 core
in vec2 vs_textureCoordinate;
uniform sampler2D textureObject;
out vec4 color;

void main(void)
{
	color = texelFetch(textureObject, ivec2(vs_textureCoordinate.x, vs_textureCoordinate.y), 0);
}
";

        static string vertextx =
        @"
#version 450 core
layout (location = 0) in vec4 model;
layout (location = 1) in vec2 textureCoordinate;

out vec2 vs_textureCoordinate;

layout(location = 20) uniform  mat4 projection;
layout (location = 21) uniform  mat4 modelView;
layout (location = 22) uniform  mat4 objecttransform;
layout (location = 23) uniform  mat4 commontransform;

void main(void)
{
	vs_textureCoordinate = textureCoordinate;

	gl_Position = projection * modelView * objecttransform * commontransform * model ;
}
";
        public GLVertexTexturedObjectShaderCommonTransform()           // if with transform, object needs to pass in uniform 22 the transform
        {
            program = new OpenTKUtils.GL4.GLProgram();
            program.Compile(OpenTK.Graphics.OpenGL4.ShaderType.VertexShader, vertextx );
            program.Compile(OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader, fragmenttexture);
            string ret = program.Link();
            System.Diagnostics.Debug.Assert(ret == null);
            Transform = new GLObjectDataTranslationRotation();
        }

        public void Use(Matrix4 model, Matrix4 projection)
        {
            System.Diagnostics.Debug.WriteLine("Program changed " + program.Id + " ShaderTexture");
            program.Use();
            GL.UniformMatrix4(20, false, ref projection);
            GL.UniformMatrix4(21, false, ref model);        // pass in uniform var the model matrix
            Matrix4 t = Transform.Transform;
            GL.UniformMatrix4(23, false, ref t);        // pass in to program the common transform.  22 comes from the object data
        }

        public void Dispose()
        {
            program.Dispose();
        }
    }

}
