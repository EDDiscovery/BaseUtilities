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
    // Shader, allows blending of multiple images, and selection of image base through matrix/position
    // instanced position.  Allows you to throw multiple blending differing images at random positions and common rotations.

    public class GLStarPoints: GLShaderProgramBase
    {
        public static string StarColours =
@"
    const ivec3 starcolours[] = ivec3[] (
        ivec3(144,166,255),
        ivec3(148,170,255)
);
";
        
        string vertpos =
        @"
#version 450 core
" + GLMatrixCalcUniformBlock.GLSL
  + StarColours + @"
layout (location = 0) in uvec2 positionpacked;

//out vec4 vs_color;
out flat ivec3 i_color;

out gl_PerVertex {
        vec4 gl_Position;
        float gl_PointSize;
        float gl_ClipDistance[];
    };

float rand1(float n)
{
return fract(sin(n) * 43758.5453123);
}

void main(void)
{
    uint xcoord = positionpacked.x & 0x1fffff;
    uint ycoord = positionpacked.y & 0x1fffff;
    float x = float(xcoord)/16.0-50000;
    float y = float(ycoord)/16.0-50000;
    uint zcoord = positionpacked.x >> 21;
    zcoord = zcoord | ( ((positionpacked.y >> 21) & 0x7ff) << 11);
    float z = float(zcoord)/16.0-50000;

    vec4 position = vec4( x, y, z, 1.0f);

	gl_Position = mc.ProjectionModelMatrix * position;        // order important

    float distance = 50-pow(distance(mc.EyePosition,vec4(x,y,z,0)),2)/20;

    gl_PointSize = clamp(distance,1.0,63.0);
    //vs_color = vec4(rand1(gl_VertexID),0.5,0.5,1.0);
    i_color = ivec3(128,0,0);
}
";


        string frag =

@"
#version 450 core

//in vec4 vs_color;
in flat ivec3 i_color;
out vec4 color;

void main(void)
{
	//color = vs_color;
    color = vec3( float(i_color.x)/256.0, float(i_color.y)/256.0, float(i_color.z)/256.0);
}
";

        public GLStarPoints()      // give the number of images to blend over..
        {
            Compile(vertex: vertpos,  frag: frag );
        }

        public override void Start()
        {
            base.Start();
            GLStatics.PointSizeByProgram();
            GLStatics.Check();
        }

        public override void Finish()
        {
            base.Finish();
        }
    }
}
