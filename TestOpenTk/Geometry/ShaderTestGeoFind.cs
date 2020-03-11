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
    public partial class ShaderTestGeoFind : Form
    {
        private OpenTKUtils.WinForm.GLWinFormControl glwfc;
        private Controller3D gl3dcontroller;

        private Timer systemtimer = new Timer();

        public ShaderTestGeoFind()
        {
            InitializeComponent();
            glwfc = new OpenTKUtils.WinForm.GLWinFormControl(glControlContainer);

            systemtimer.Interval = 25;
            systemtimer.Tick += new EventHandler(SystemTick);
            systemtimer.Start();
        }

        GLRenderProgramSortedList rObjects = new GLRenderProgramSortedList();
        GLItemsList items = new GLItemsList();

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            gl3dcontroller = new Controller3D();
            gl3dcontroller.PaintObjects = ControllerDraw;
            gl3dcontroller.ZoomDistance = 20F;
            gl3dcontroller.MatrixCalc.PerspectiveNearZDistance = 0.1f;
            gl3dcontroller.Start(glwfc, new Vector3(0, 0, 0), new Vector3(180f, 0, 0f), 1F);

            gl3dcontroller.KeyboardTravelSpeed = (ms, eyedist) =>
            {
                return (float)ms / 50.0f;
            };

            glwfc.MouseDown += mousedown;

            items.Add("MCUB", new GLMatrixCalcUniformBlock());     // def binding of 0

            if (true)
            {
                GLRenderControl lines = GLRenderControl.Lines(1);

                items.Add("COSW", new GLColourShaderWithWorldCoord());

                rObjects.Add(items.Shader("COSW"),
                             GLRenderableItem.CreateVector4Color4(items, lines,
                                                        GLShapeObjectFactory.CreateLines(new Vector3(-100, 0, -100), new Vector3(-100, 0, 100), new Vector3(10, 0, 0), 21),
                                                        new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green })
                                   );


                rObjects.Add(items.Shader("COSW"),
                             GLRenderableItem.CreateVector4Color4(items, lines,
                                   GLShapeObjectFactory.CreateLines(new Vector3(-100, 0, -100), new Vector3(100, 0, -100), new Vector3(0, 0, 10), 21),
                                                             new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green }));
            }


            items.Add("COSOT", new GLColourShaderWithObjectTranslation());


            vecp4 = new Vector4[] { new Vector4(0, 0, 0, 1), new Vector4(10, 0, 0, 1), new Vector4(10, 0, 10, 1) ,
                                    new Vector4(-20, 0, 0, 1), new Vector4(-10, 0, 0, 1), new Vector4(-10, 0, 10, 1)
            };

            GLRenderControl rc = GLRenderControl.Tri();
          //  rc.CullFace = false;
            rObjects.Add(items.Shader("COSOT"), "scopen",
                        GLRenderableItem.CreateVector4Color4(items, rc, vecp4, 
                                        new Color4[] { Color4.Red, Color4.Green, Color4.Blue, Color4.White, Color4.Cyan, Color4.Orange },
                                        new GLRenderDataTranslationRotation(new Vector3(0, 0, 0))
                        ));

            pointbuffer = items.LastBuffer();  // starts with arrays of points

            findshader = items.NewShaderPipeline("FS",new GLPLVertexShaderWorldCoord(), null,null, new GLPLGeoShaderFindTriangles(11, 16), null, null, null);
            GLRenderControl rs = GLRenderControl.Tri();
            rs.RasterizerDiscard = true;

            findrender = GLRenderableItem.CreateVector4(items, rs, pointbuffer, vecp4.Length);

            Closed += ShaderTest_Closed;
        }

        void mousedown(Object sender, GLMouseEventArgs e)
        {
            GLMatrixCalcUniformBlock mcub = (GLMatrixCalcUniformBlock)items.UB("MCUB");
            mcub.Set(gl3dcontroller.MatrixCalc);

            var geo = findshader.Get<GLPLGeoShaderFindTriangles>(OpenTK.Graphics.OpenGL4.ShaderType.GeometryShader);

            geo.SetScreenCoords(e.Location, glwfc.Size);

            //rObjectsFind.Render(glwfc.RenderState, null);

            findrender.Execute(findshader, glwfc.RenderState, null);
           
            var res = geo.GetResult();
            if (res != null)
            {
                for (int i = 0; i < res.Length; i++)
                {
                    System.Diagnostics.Debug.WriteLine(i + " = " + res[i]);
                }
            }


        }

        Vector4[] vecp4;
        GLShaderPipeline findshader;
        GLBuffer pointbuffer;
        GLRenderableItem findrender;

        private void ShaderTest_Closed(object sender, EventArgs e)
        {
            items.Dispose();
        }

        private void ControllerDraw(OpenTKUtils.GLMatrixCalc mc, long time)
        {
            // System.Diagnostics.Debug.WriteLine("Draw eye " + gl3dcontroller.MatrixCalc.EyePosition + " to " + gl3dcontroller.Pos.Current);

            GLMatrixCalcUniformBlock mcub = (GLMatrixCalcUniformBlock)items.UB("MCUB");
            mcub.Set(gl3dcontroller.MatrixCalc);

            for (int i = 0; i < vecp4.Length; i++)
            {
                gl3dcontroller.MatrixCalc.WorldToScreen(vecp4[i], i.ToString());
            }


            rObjects.Render(glwfc.RenderState, gl3dcontroller.MatrixCalc);
            GL.MemoryBarrier(MemoryBarrierFlags.VertexAttribArrayBarrierBit);

        }

        private void SystemTick(object sender, EventArgs e )
        {
            gl3dcontroller.HandleKeyboardSlews(true, OtherKeys);
        }

        private void OtherKeys( OpenTKUtils.Common.KeyboardMonitor kb )
        {
        }
    }
}


