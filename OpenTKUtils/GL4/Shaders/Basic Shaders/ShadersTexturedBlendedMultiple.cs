/*
 * Copyright 2019-2020 Robbyxp1 @ github.com
 * 
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
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKUtils.GL4
{
    // Shader, allows blending of multiple images, and selection of image base through matrix/position
    // instanced position.  Allows you to throw multiple blending differing images at random positions and common rotations.

    public class GLMultipleTexturedBlended : GLShaderStandard
    {
        // 0 : vec4 of model positions (w not read)
        // 1 : vec2 of texture positions
        // 2 : vec4 of instance positions w = image to display

        string vertpos =
        @"
#version 450 core
#include UniformStorageBlocks.matrixcalc.glsl
layout (location = 0) in vec4 position;     
layout (location = 1) in vec2 texco;
layout(location = 2) in vec4 instancepos;       

layout (location = 22) uniform  mat4 commontransform;

out vec2 tc;
out int imagebase;

void main(void)
{
    vec4 p = commontransform * vec4(position.xyz,1);
    p = p + vec4(instancepos.x,instancepos.y,instancepos.z,0);
    imagebase = int(instancepos.w);
	gl_Position = mc.ProjectionModelMatrix  * p;       
    tc = texco;
}
";

        // 0 : vec4 of model positions (w not read)
        // 1 : vec2 of texture positions
        // 4-7 : mat4 of instance transforms, [3][3] = image to display

        string vertmat =
@"
#version 450 core
#include UniformStorageBlocks.matrixcalc.glsl
layout (location = 0) in vec4 position;     // from buffer1
layout (location = 1) in vec2 texco;
layout(location = 4) in mat4 mat;       // from buffer2

layout (location = 23) uniform  mat4 commontransform;

out vec2 tc;
out int imagebase;

void main(void)
{
    mat4 transform = mat;
    imagebase = int(mat[3][3]);     // mat[3][3] which is w in effect holds image number
    transform[3][3] = 1;
	gl_Position = mc.ProjectionModelMatrix * transform * commontransform * position;       
    tc = texco;
}
";
        // geo is used to discard vertexes if imagebase<0

        string geo =
@"        
#version 450 core
layout (triangles) in;               // triangles come in
layout (triangle_strip) out;        // norm op is not to sent them on
layout (max_vertices=3) out;	    // 1 triangle max

in gl_PerVertex
{
    vec4 gl_Position;
    float gl_PointSize;
    float gl_ClipDistance[];
   
} gl_in[];

in vec2 tc[];
in int imagebase[];

out gl_PerVertex 
{
    vec4 gl_Position;
    float gl_PointSize;
    float gl_ClipDistance[];
};

layout (location = 0) out vec2 tcg;
layout (location = 1) out int imagebaseg;

void main(void)
{
    if ( imagebase[0] >= 0 )
    {
        for( int i = 0 ; i < 3; i++ )
        {
            gl_Position = gl_in[i].gl_Position;
            tcg = tc[i];
            imagebaseg = imagebase[i];
            EmitVertex();
        }
    }
}
";


        string frag =
@"
#version 450 core

out vec4 color;
layout (location = 0 ) in vec2 tcg;
layout (location = 1 ) flat in int imagebaseg;      //default is last vertex provides this, but as position is per instance, its just the instance value

layout (binding=1) uniform sampler2DArray textureObject2D;
layout (location = 25) uniform  float mixamount;    // between lo and hi image, 0-1
layout (location = 26) uniform  int loimage;        // index of low image
layout (location = 27) uniform  int hiimage;        // inded of high image


void main(void)
{
    if ( mixamount == 0 )
    {
        color = texture(textureObject2D, vec3(tcg,imagebaseg));
    }
    else    
    {
        vec4 col1 = texture(textureObject2D, vec3(tcg,imagebaseg+loimage));
        vec4 col2 = texture(textureObject2D, vec3(tcg,imagebaseg+hiimage));
        color = mix(col1,col2,mixamount);
    }
}
";
        public float Blend { get; set; } = 0;                   // from 0 to BlendImages-0.0001
        public int BlendImages { get; set; } = 0;

        public GLRenderDataTranslationRotation CommonTransform { get; set; }           // only use this for rotation - position set by object data

        public GLMultipleTexturedBlended(bool matrix, int blendimages)      // give the number of images to blend over, or 0 for not
        {
            CompileLink(vertex: (matrix ? vertmat : vertpos), frag: frag, geo:geo );
            BlendImages = blendimages;
            CommonTransform = new GLRenderDataTranslationRotation();
        }

        public override void Start()
        {
            base.Start();

            Matrix4 t = CommonTransform.Transform;
            GL.ProgramUniformMatrix4(Id, 22, false, ref t);

            int image1 = (int)Math.Floor(Blend);            // compute first and next image indexes
            int image2 = (BlendImages>0) ? ((image1 + 1) % BlendImages) : 0;
            float mix = Blend - image1;                     // and the mix between them

            GL.ProgramUniform1(Id, 25, mix);
            GL.ProgramUniform1(Id, 26, image1);
            GL.ProgramUniform1(Id, 27, image2);

            // System.Diagnostics.Debug.WriteLine("Blend " + image1 + " to " + image2 + " Mix of" + mix);

            OpenTKUtils.GLStatics.Check();
        }

        public override void Finish()
        {
            base.Finish();
        }
    }
}
