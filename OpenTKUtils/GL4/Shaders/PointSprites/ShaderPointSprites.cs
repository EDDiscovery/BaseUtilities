/*
 * Copyright 2019-2020 Robbyxp1 @ github.com
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

using OpenTK.Graphics.OpenGL4;

namespace OpenTKUtils.GL4
{
    // point sprite shader based on eye position vs sprite position.  Needs point sprite on and program point size

    public class GLPointSpriteShader : GLShaderStandard
    {
        string vert =
@"
        #version 450 core
        // maxsize/scale provided programatically

        #include OpenTKUtils.GL4.UniformStorageBlocks.matrixcalc.glsl

        layout (location = 0) in vec4 position;     // has w=1
        layout (location = 1) in vec4 color;
        out vec4 vs_color;
        out float calc_size;

        void main(void)
        {
            vec4 pn = vec4(position.x,position.y,position.z,0);
            float d = distance(mc.EyePosition,pn);
            float sf = maxsize-d/scale;

            calc_size = gl_PointSize = clamp(sf,1.0,maxsize);
            gl_Position = mc.ProjectionModelMatrix * position;        // order important
            vs_color = color;
        }
        ";

        string frag =
@"
        #version 450 core

        in vec4 vs_color;
        layout (binding = 4 ) uniform sampler2D texin;
        out vec4 color;
        in float calc_size;

        void main(void)
        {
            if ( calc_size < 2 )
                color = vs_color * 0.5;
            else
            {
                vec4 texcol =texture(texin, gl_PointCoord);
                float l = texcol.x*texcol.x+texcol.y*texcol.y+texcol.z*texcol.z;

                if ( l< 0.1 )
                    discard;
                else
                    color = texcol * vs_color;
            }
        }
        ";

        public GLPointSpriteShader(IGLTexture tex, float maxsize = 120, float scale = 80) : base()
        {
            StartAction = (a) =>
            {
                tex.Bind(4);
            };

            CompileLink(vert, frag: frag, vertexconstvars:new object[] { "maxsize", maxsize, "scale", scale });
        }
    }
}

