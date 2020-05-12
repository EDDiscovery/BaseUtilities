/*
 * Copyright 2019 Robbyxp1 @ github.com
 * Part of the EDDiscovery Project
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

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTKUtils.GL4;
using OpenTKUtils.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTKUtils;

namespace TestOpenTk
{
    // demonstrates packed data

    public partial class ShaderTestStarPoints : Form
    {
        private OpenTKUtils.WinForm.GLWinFormControl glwfc;
        private Controller3D gl3dcontroller;

        private Timer systemtimer = new Timer();

        public ShaderTestStarPoints()
        {
            InitializeComponent();

            glwfc = new OpenTKUtils.WinForm.GLWinFormControl(glControlContainer);

            systemtimer.Interval = 25;
            systemtimer.Tick += new EventHandler(SystemTick);
            systemtimer.Start();
        }

        GLRenderProgramSortedList rObjects = new GLRenderProgramSortedList();
        GLItemsList items = new GLItemsList();

        public class GLShaderStars : GLShaderPipelineShadersBase
        {
            public string Code()
            {
                return
    @"
#version 450 core
#include OpenTKUtils.GL4.UniformStorageBlocks.matrixcalc.glsl
layout (location = 0) in uvec2 positionpacked;

out vec4 vs_color;

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
    vs_color = vec4(rand1(gl_VertexID),0.5,0.5,1.0);
}
";
            }

            public GLShaderStars()
            {
                CompileLink(OpenTK.Graphics.OpenGL4.ShaderType.VertexShader, Code(), auxname:GetType().Name);
            }
        }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            gl3dcontroller = new Controller3D();
            gl3dcontroller.PaintObjects = ControllerDraw;
            gl3dcontroller.ZoomDistance = 100F;
            gl3dcontroller.MatrixCalc.PerspectiveNearZDistance = 0.1f;
            gl3dcontroller.Start(glwfc,new Vector3(0, 0, 0), new Vector3(110f, 0, 0f), 1F);

            gl3dcontroller.KeyboardTravelSpeed = (ms,eyedist) =>
            {
                return (float)ms / 20.0f;
            };

            {
                items.Add(new GLColourShaderWithWorldCoord(), "COS");
                GLRenderControl rl = GLRenderControl.Lines(1);

                rObjects.Add(items.Shader("COS"), GLRenderableItem.CreateVector4Color4(items, rl,
                            GLShapeObjectFactory.CreateBox(400, 200, 40, new Vector3(0, 0, 0), new Vector3(0, 0, 0)),
                            new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green }));
            }

            {
                items.Add(new GLTexturedShaderWithObjectTranslation(), "TEX");

                using (var bmp = BitMapHelpers.DrawTextIntoAutoSizedBitmap("200,100", new Size(200, 100), new Font("Arial", 10.0f), Color.Yellow, Color.Blue))
                {
                    items.Add(new GLTexture2D(bmp), "200,100");
                }

                using (var bmp = BitMapHelpers.DrawTextIntoAutoSizedBitmap("-200,-100", new Size(200, 100), new Font("Arial", 10.0f), Color.Yellow, Color.Blue))
                {
                    items.Add(new GLTexture2D(bmp), "-200,-100");
                }

                GLRenderControl rq = GLRenderControl.Quads();

                rObjects.Add(items.Shader("TEX"), GLRenderableItem.CreateVector4Vector2(items, rq,
                            GLShapeObjectFactory.CreateQuad(20.0f, 20.0f, new Vector3(-90, 0, 0)), GLShapeObjectFactory.TexQuad,
                            new GLRenderDataTranslationRotationTexture(items.Tex("200,100"), new Vector3(200, 0, 100))));

                rObjects.Add(items.Shader("TEX"), GLRenderableItem.CreateVector4Vector2(items, rq,
                            GLShapeObjectFactory.CreateQuad(20.0f, 20.0f, new Vector3(-90, 0, 0)), GLShapeObjectFactory.TexQuad,
                            new GLRenderDataTranslationRotationTexture(items.Tex("-200,-100"), new Vector3(-200, 0, -100))));
            }


            if (true)
            {
                Vector3[] stars = GLPointsFactory.RandomStars(10000, -200, 200, -100, 100, 20, -20);

                items.Add(new GLShaderPipeline(new GLShaderStars(), new GLPLFragmentShaderColour()), "STARS");

                GLRenderControl rp = GLRenderControl.PointsByProgram();

                rObjects.Add(items.Shader("STARS"), "Stars", GLRenderableItem.CreateVector3Packed2(items, rp,
                                                stars, new Vector3(50000, 50000, 50000), 16));

            }

            GL.Enable(EnableCap.PointSmooth);

            items.Add( new GLMatrixCalcUniformBlock(), "MCUB");     // def binding of 0

            Closed += ShaderTest_Closed;
        }

        private void ShaderTest_Closed(object sender, EventArgs e)
        {
            items.Dispose();
        }

        private void ControllerDraw(OpenTKUtils.GLMatrixCalc mc, long time)
        {
            float degrees = ((float)time / 5000.0f * 360.0f) % 360f;
            float degreesd2 = ((float)time / 10000.0f * 360.0f) % 360f;

            GLMatrixCalcUniformBlock mcub = (GLMatrixCalcUniformBlock)items.UB("MCUB");
            mcub.Set(gl3dcontroller.MatrixCalc);

            System.Diagnostics.Debug.WriteLine("Draw eye " + gl3dcontroller.MatrixCalc.EyePosition + " to " + gl3dcontroller.PosCamera.Lookat);
            rObjects.Render(glwfc.RenderState,gl3dcontroller.MatrixCalc);
        }

        private void SystemTick(object sender, EventArgs e )
        {
            gl3dcontroller.HandleKeyboardSlewsInvalidate(true, OtherKeys);
            gl3dcontroller.Redraw();
        }

        private void OtherKeys( OpenTKUtils.Common.KeyboardMonitor kb )
        {
        }

    }
}


