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
    public partial class ShaderTestPointSprites : Form
    {
        private OpenTKUtils.WinForm.GLWinFormControl glwfc;
        private Controller3D gl3dcontroller;

        private Timer systemtimer = new Timer();

        public ShaderTestPointSprites()
        {
            InitializeComponent();

            glwfc = new OpenTKUtils.WinForm.GLWinFormControl(glControlContainer);

            systemtimer.Interval = 25;
            systemtimer.Tick += new EventHandler(SystemTick);
            systemtimer.Start();
        }

        GLRenderProgramSortedList rObjects = new GLRenderProgramSortedList();
        GLItemsList items = new GLItemsList();

        public class GLPointSprite : GLShaderStandard
        {
            string vert =
@"
#version 450 core

#include OpenTKUtils.GL4.UniformStorageBlocks.matrixcalc.glsl

layout (location = 0) in vec4 position;     // has w=1
layout (location = 1) in vec4 color;       
out vec4 vs_color;
out float calc_size;

void main(void)
{
    vec4 pn = vec4(position.x,position.y,position.z,0);
    float d = distance(mc.EyePosition,pn);
    float sf = 120-d;

    calc_size = gl_PointSize = clamp(sf,1.0,120.0);
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
            public GLPointSprite() : base()
            {
                CompileLink(vert,frag:frag);
            }

           
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Closed += ShaderTest_Closed;

            gl3dcontroller = new Controller3D();
            gl3dcontroller.PaintObjects = ControllerDraw;
            gl3dcontroller.MatrixCalc.PerspectiveNearZDistance = 0.1f;
            gl3dcontroller.ZoomDistance = 20F;
            gl3dcontroller.Start(glwfc,new Vector3(0, 0, 0), new Vector3(110f, 0, 0f), 1F);

            gl3dcontroller.KeyboardTravelSpeed = (ms,eyedist) =>
            {
                return (float)ms / 50.0f;
            };

            //items.Add("lensflarewhite", new GLTexture2D(Properties.Resources.lensflare_white64));
            items.Add(new GLTexture2D(Properties.Resources.star_grey64), "lensflare");

            items.Add(new GLColourShaderWithWorldCoord(), "COS");

            #region coloured lines

            {
                GLRenderControl rl = GLRenderControl.Lines(1);
                rObjects.Add(items.Shader("COS"),    // horizontal
                             GLRenderableItem.CreateVector4Color4(items, rl,
                                                        GLShapeObjectFactory.CreateLines(new Vector3(-100, 0, -100), new Vector3(-100, 0, 100), new Vector3(10, 0, 0), 21),
                                                        new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green })
                                   );


                rObjects.Add(items.Shader("COS"),    // vertical
                             GLRenderableItem.CreateVector4Color4(items, rl,
                                   GLShapeObjectFactory.CreateLines(new Vector3(-100, 0, -100), new Vector3(100, 0, -100), new Vector3(0, 0, 10), 21),
                                                             new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green })
                                   );
            }

            var p = GLPointsFactory.RandomStars4(100, -100, 100, 100, -100, 100, -100);
            //p = new Vector4[10];
            //for( int i = 0; i < 10; i++)
            //{
            //    p[i] = new Vector4(i, 6.8f, 0,1);
            //}

            items.Add(new GLPointSprite(), "PS1");

            GLRenderControl rp = GLRenderControl.PointSprites();     // by program

            GLRenderDataTexture rt = new GLRenderDataTexture(items.Tex("lensflare"),4);

            rObjects.Add(items.Shader("PS1"),
                         GLRenderableItem.CreateVector4Color4(items, rp,
                               p, new Color4[] { Color.Red, Color.Yellow, Color.Green },rt));

            #endregion

            items.Add( new GLMatrixCalcUniformBlock(), "MCUB");     // create a matrix uniform block 

        }

        private void ShaderTest_Closed(object sender, EventArgs e)
        {
            items.Dispose();
        }

        private void ControllerDraw(GLMatrixCalc mc, long time)
        {
            ((GLMatrixCalcUniformBlock)items.UB("MCUB")).Set(gl3dcontroller.MatrixCalc);        // set the matrix unform block to the controller 3d matrix calc.

            rObjects.Render(glwfc.RenderState,gl3dcontroller.MatrixCalc);

            this.Text = "Looking at " + gl3dcontroller.MatrixCalc.TargetPosition + " dir " + gl3dcontroller.Pos.CameraDirection + " eye@ " + gl3dcontroller.MatrixCalc.EyePosition + " Dist " + gl3dcontroller.MatrixCalc.EyeDistance;
        }

        private void SystemTick(object sender, EventArgs e )
        {
            gl3dcontroller.HandleKeyboardSlewsInvalidate(true, OtherKeys);
            gl3dcontroller.Redraw();
        }

        private void OtherKeys( OpenTKUtils.Common.KeyboardMonitor kb )
        {
            if (kb.HasBeenPressed(Keys.F1, OpenTKUtils.Common.KeyboardMonitor.ShiftState.None))
            {
                gl3dcontroller.CameraLookAt(new Vector3(0, 0, 0), 1, 2);
            }

            if (kb.HasBeenPressed(Keys.F2, OpenTKUtils.Common.KeyboardMonitor.ShiftState.None))
            {
                gl3dcontroller.CameraLookAt(new Vector3(4, 0, 0), 1, 2);
            }

            if (kb.HasBeenPressed(Keys.F3, OpenTKUtils.Common.KeyboardMonitor.ShiftState.None))
            {
                gl3dcontroller.CameraLookAt(new Vector3(10, 0, -10), 1, 2);
            }

            if (kb.HasBeenPressed(Keys.F4, OpenTKUtils.Common.KeyboardMonitor.ShiftState.None))
            {
                gl3dcontroller.CameraLookAt(new Vector3(50, 0, 50), 1, 2);
            }

        }

    }



}


