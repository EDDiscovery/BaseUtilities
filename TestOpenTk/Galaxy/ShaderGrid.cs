using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTKUtils;
using OpenTKUtils.GL4;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestOpenTk
{
    public class DynamicGridShader : GLShaderPipelineShadersBase
    {
        public int ComputeGridSize(GLMatrixCalc mc, out int gridwidth)
        {
            int lines = 21;
            gridwidth = 10000;

            if (mc.EyeDistance >= 10000)
            {
            }
            else if (mc.EyeDistance >= 1000)
            {
                lines = 81 * 2;
                gridwidth = 1000;
            }
            else if (mc.EyeDistance >= 100)
            {
                lines = 321 * 2;
                gridwidth = 100;
            }
            else
            {
                lines = 321 * 2;
                gridwidth = 10;
            }

            return lines;
        }

        public void SetUniforms(GLMatrixCalc mc, int gridwidth, int lines)
        {
            Vector3 start;

            float sy = ObjectExtensionsNumbersBool.Clamp(mc.TargetPosition.Y, -2000, 2000); // need it floating to stop integer giggle at high res

            if (gridwidth == 10000)
            {
                start = new Vector3(-50000, sy, -20000);
            }
            else
            {
                int horzlines = lines / 2;

                int gridstart = (horzlines - 1) * gridwidth / 2;
                int width = (horzlines - 1) * gridwidth;

                int sx = (int)(mc.TargetPosition.X) / gridwidth * gridwidth - gridstart;
                if (sx < -50000)
                    sx = -50000;
                else if (sx + width > 50000)
                    sx = 50000 - width;

                int sz = (int)(mc.TargetPosition.Z) / gridwidth * gridwidth - gridstart;
                if (sz < -20000)
                    sz = -20000;
                else if (sz + width > 70000)
                    sz = 70000 - width;
                start = new Vector3(sx, sy, sz);
            }

            GL.ProgramUniform1(this.Id, 10, lines);
            GL.ProgramUniform1(this.Id, 11, gridwidth);
            GL.ProgramUniform3(this.Id, 12, ref start);
            GLStatics.Check();
        }

        string vcode()
        { return @"
#version 450 core

layout (location=10) uniform int lines;
layout (location=11) uniform int gridwidth;
layout (location=12) uniform vec3 start;

#include OpenTKUtils.GL4.UniformStorageBlocks.matrixcalc.glsl

out gl_PerVertex {
        vec4 gl_Position;
        float gl_PointSize;
        float gl_ClipDistance[];
    };

out vec4 vs_color;

void main(void)
{
    int line = gl_InstanceID;
    int linemod = gl_VertexID;
    float dist = mc.EyeDistance;

    int horzlines = lines/2;
    int width = (horzlines-1)*gridwidth;
    if ( gridwidth==10000 && line < horzlines)
        width = 100000;

    int lpos;
    vec4 position;

    if ( line>= horzlines) // vertical
    {
        line -= horzlines;
        lpos = int(start.x) + line * gridwidth;
        position = vec4( lpos , start.y, clamp(start.z + width * linemod,-20000,70000), 1);
    }
    else    
    {
        lpos = int(start.z) + gridwidth * line;
        position = vec4( clamp(start.x + width * linemod,-50000,50000), start.y, lpos , 1);
    }

    gl_Position = mc.ProjectionModelMatrix * position;        // order important

    float a=1;
    float b = 0.7;

    if ( gridwidth != 10000 ) 
    {
        if ( abs(lpos) % (10*gridwidth) != 0 )
        {
            a =  1.0 - clamp((dist - gridwidth)/float(10*gridwidth),0,0.95f);
            b = 0.5;
        }
    }
                
    vs_color = vec4(color.x*b,color.y*b,color.z*b,a);
}
"; }

        public DynamicGridShader(Color c)
        {
            CompileLink(OpenTK.Graphics.OpenGL4.ShaderType.VertexShader, vcode(), new object[] { "color", c }, completeoutfile: @"c:\code\sh.txt");
        }

    }

    public class DynamicGridCoordShader : GLShaderPipelineShadersBase
    {
        string vcode()
        { return @"
#version 450 core

layout (location=11) uniform int majorlines;
layout (location=12) uniform vec3 start;
layout (location=13) uniform bool flip;

#include OpenTKUtils.GL4.UniformStorageBlocks.matrixcalc.glsl

out gl_PerVertex {
        vec4 gl_Position;
        float gl_PointSize;
        float gl_ClipDistance[];
    };

out VS_OUT
{
    flat int vs_instanced;      // not sure why structuring is needed..
} vs;

out vec2 vs_textureCoordinate;

void main(void)
{
    int bitmap = gl_InstanceID;
    vs.vs_instanced = bitmap;
    int vid = gl_VertexID;
    float dist = mc.EyeDistance;
    
    float sx = start.x;
    float sy = start.y;
    float sz = start.z;

    sx += majorlines *(bitmap/3);
    sz += majorlines *(bitmap%3);

    float textwidth = clamp(dist/3,1,2500);
    if ( flip ) 
    {
        sx -=(1-vid/2)*textwidth;
        sz -= (vid%2)*textwidth/6;
        vs_textureCoordinate = vec2(1-vid/2,1-vid%2);
    }
    else
    {
        sx +=(vid/2)*textwidth;
        sz += (1-vid%2)*textwidth/6;
        vs_textureCoordinate = vec2((vid/2),vid%2);
    }

    gl_Position = mc.ProjectionModelMatrix * vec4(sx,sy,sz,1);        // order important
}
"; }

        int lastsx = int.MinValue, lastsz = int.MinValue;
        int lastsy = int.MinValue;

        public void ComputeUniforms(int gridwidth, GLMatrixCalc mc, Vector3 cameradir, Color textcol, Color? backcol = null)
        {
            float sy = mc.TargetPosition.Y.Clamp(-2000, 2000);

            float multgrid = mc.EyeDistance / gridwidth;

            int tw = 10;
            if (multgrid < 2)
                tw = 1;
            else if (multgrid < 5)
                tw = 5;

            //                System.Diagnostics.Debug.WriteLine("Mult " + multgrid + " tw "+ tw);
            int majorlines = (gridwidth * tw).Clamp(0, 10000);

            int sx = (int)((mc.TargetPosition.X - majorlines).Clamp(-50000, 50000 - majorlines * 2)) + 50000;  //move to positive rep so rounding next is always down

            if (sx % majorlines > majorlines / 2)                // if we are over 1/2 way across, move over
                sx += majorlines;

            sx = sx / majorlines * majorlines - 50000;         // round and adjust back

            bool lookbackwards = (cameradir.Y > 90 || cameradir.Y < -90);
            int zoffset = lookbackwards ? majorlines : 0;

            int sz = (int)((mc.TargetPosition.Z - zoffset).Clamp(-20000, 70000 - majorlines * 2)) + 50000;  //move to positive rep so rounding next is always down

            sz = sz / majorlines * majorlines - 50000;         // round and adjust back

            Vector3 start = new Vector3(sx, sy, sz);
            GL.ProgramUniform1(this.Id, 11, majorlines);
            GL.ProgramUniform3(this.Id, 12, ref start);
            GL.ProgramUniform1(this.Id, 13, lookbackwards ? 1 : 0);
            //System.Diagnostics.Debug.WriteLine(majorlines + " " + start + " " + lookbackwards);

            if (lastsx != sx || lastsz != sz || lastsy != (int)sy)
            {
                for (int i = 0; i < texcoords.Depth; i++)
                {
                    int bsx = sx + majorlines * (i / 3);
                    int bsz = sz + majorlines * (i % 3);
                    string label = bsx.ToStringInvariant() + "," + sy.ToStringInvariant("0") + "," + bsz.ToStringInvariant();
                    BitMapHelpers.DrawTextIntoFixedSizeBitmap(ref texcoords.BitMaps[i], label, gridfnt, textcol, backcol);// Color.Transparent);
                    texcoords.LoadBitmap(texcoords.BitMaps[i], i);
                }

                lastsx = sx;
                lastsz = sz;
                lastsy = (int)sy;
            }
        }

        private GLTexture2DArray texcoords;
        private Font gridfnt;

        public DynamicGridCoordShader(Font f = null)
        {
            texcoords = new GLTexture2DArray();
            texcoords.CreateTexture(200, 25, 9);        // size and number
            texcoords.OwnBitmaps = true;                // it will own the bitmaps
            
            gridfnt = f ?? new Font("MS Sans Serif", 16);

            for (int i = 0; i < 9; i++)
            {
                Bitmap bmp = new Bitmap(texcoords.Width, texcoords.Height); // a bitmap for each number
                texcoords.LoadBitmap(bmp, i);
            }

            CompileLink(OpenTK.Graphics.OpenGL4.ShaderType.VertexShader, vcode());
        }

        public override void Start()
        {
            base.Start();
            texcoords.Bind(1);
        }

        public override void Dispose()
        {
            base.Dispose();
            texcoords.Dispose();
        }
    }

}
