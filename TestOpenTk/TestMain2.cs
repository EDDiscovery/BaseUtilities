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

// A simpler main for testing

namespace TestOpenTk
{
    public partial class TestMain2 : Form
    {
        private OpenTKUtils.WinForm.GLWinFormControl glwfc;
        private Controller3D gl3dcontroller;

        private Timer systemtimer = new Timer();

        GLRenderProgramSortedList rObjects = new GLRenderProgramSortedList();
        GLRenderProgramSortedList rObjectscw = new GLRenderProgramSortedList();
        GLItemsList items = new GLItemsList();
        GLStorageBlock dataoutbuffer;

        public TestMain2()
        {
            InitializeComponent();

            glwfc = new OpenTKUtils.WinForm.GLWinFormControl(glControlContainer);

            systemtimer.Interval = 25;
            systemtimer.Tick += new EventHandler(SystemTick);
            systemtimer.Start();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Closed += ShaderTest_Closed;

            gl3dcontroller = new Controller3D();
            gl3dcontroller.PaintObjects = ControllerDraw;
            gl3dcontroller.MatrixCalc.PerspectiveNearZDistance = 1f;
            gl3dcontroller.MatrixCalc.PerspectiveFarZDistance= 1000f;
            gl3dcontroller.ZoomDistance = 20F;
            gl3dcontroller.Start(glwfc, new Vector3(0, 0, 0), new Vector3(110f, 0, 0f), 1F);

            gl3dcontroller.KeyboardTravelSpeed = (ms,eyedist) =>
            {
                return (float)ms / 100.0f;
            };

            items.Add( new GLTexturedShaderWithObjectTranslation(),"TEXOT");
            items.Add(new GLTexturedShaderWithObjectTranslation(), "TEXOTNoRot");
            items.Add(new GLColourShaderWithWorldCoord(), "COSW");
            items.Add(new GLColourShaderWithObjectTranslation(), "COSOT");
            items.Add(new GLFixedColourShaderWithObjectTranslation(Color.Goldenrod), "FCOSOT");
            items.Add(new GLTexturedShaderWithObjectCommonTranslation(), "TEXOCT");

            items.Add( new GLTexture2D(Properties.Resources.dotted)  ,           "dotted"    );
            items.Add(new GLTexture2D(Properties.Resources.Logo8bpp), "logo8bpp");
            items.Add(new GLTexture2D(Properties.Resources.dotted2), "dotted2");
            items.Add(new GLTexture2D(Properties.Resources.wooden), "wooden");
            items.Add(new GLTexture2D(Properties.Resources.shoppinglist), "shoppinglist");
            items.Add(new GLTexture2D(Properties.Resources.golden), "golden");
            items.Add(new GLTexture2D(Properties.Resources.smile5300_256x256x8), "smile");
            items.Add(new GLTexture2D(Properties.Resources.moonmap1k), "moon");    

            #region coloured lines

            if (true)
            {
                GLRenderControl lines = GLRenderControl.Lines(1);

                rObjects.Add(items.Shader("COSW"),
                             GLRenderableItem.CreateVector4Color4(items, lines,
                                                        GLShapeObjectFactory.CreateLines(new Vector3(-100, -10, -100), new Vector3(-100, -10, 100), new Vector3(10, 0, 0), 21),
                                                        new Color4[] { Color.Red, Color.Red, Color.DarkRed, Color.DarkRed })
                                   );


                rObjects.Add(items.Shader("COSW"),
                             GLRenderableItem.CreateVector4Color4(items, lines,
                                   GLShapeObjectFactory.CreateLines(new Vector3(-100, -10, -100), new Vector3(100, -10, -100), new Vector3(0, 0, 10), 21),
                                                        new Color4[] { Color.Red, Color.Red, Color.DarkRed, Color.DarkRed }));
            }
            if (true)
            {
                GLRenderControl lines = GLRenderControl.Lines(1);

                rObjects.Add(items.Shader("COSW"),
                             GLRenderableItem.CreateVector4Color4(items, lines,
                                   GLShapeObjectFactory.CreateLines(new Vector3(-100, 10, -100), new Vector3(-100, 10, 100), new Vector3(10, 0, 0), 21),
                                                             new Color4[] { Color.Yellow, Color.Orange, Color.Yellow, Color.Orange })
                                   );

                rObjects.Add(items.Shader("COSW"),
                             GLRenderableItem.CreateVector4Color4(items, lines,
                                   GLShapeObjectFactory.CreateLines(new Vector3(-100, 10, -100), new Vector3(100, 10, -100), new Vector3(0, 0, 10), 21),
                                                             new Color4[] { Color.Yellow, Color.Orange, Color.Yellow, Color.Orange })
                                   );
            }

            #endregion

            #region Coloured triangles
            if (true)
            {
                GLRenderControl rc = GLRenderControl.Tri();
                rc.CullFace = false;

                rObjects.Add(items.Shader("COSOT"), "scopen",
                            GLRenderableItem.CreateVector4Color4(items, rc,
                                            GLCubeObjectFactory.CreateSolidCubeFromTriangles(5f),
                                            new Color4[] { Color4.Red, Color4.Green, Color4.Blue, Color4.White, Color4.Cyan, Color4.Orange },
                                            new GLRenderDataTranslationRotation(new Vector3(10, 3, 20))
                            ));

                rObjects.Add(items.Shader("COSOT"), "scopen2",
                            GLRenderableItem.CreateVector4Color4(items, rc,
                                            GLCubeObjectFactory.CreateSolidCubeFromTriangles(5f),
                                            new Color4[] { Color4.Red, Color4.Red, Color4.Red, Color4.Red, Color4.Red, Color4.Red },
                                            new GLRenderDataTranslationRotation(new Vector3(-10, -3, -20))
                            ));
            }

            #endregion
            #region textures
            if (true)
            {
                GLRenderControl rq = GLRenderControl.Quads();

                rObjects.Add(items.Shader("TEXOT"),
                            GLRenderableItem.CreateVector4Vector2(items, rq,
                            GLShapeObjectFactory.CreateQuad(1.0f, 1.0f, new Vector3( -90f.Radians(), 0, 0)), GLShapeObjectFactory.TexQuad,
                            new GLRenderDataTranslationRotationTexture(items.Tex("dotted2"), new Vector3(0,0,0))
                            ));

                rObjects.Add(items.Shader("TEXOT"),
                        GLRenderableItem.CreateVector4Vector2(items, rq,
                            GLShapeObjectFactory.CreateQuad(1.0f, 1.0f, new Vector3(0, 0, 0)), GLShapeObjectFactory.TexQuad,
                            new GLRenderDataTranslationRotationTexture(items.Tex("dotted2"), new Vector3(2,0,0))
                            ));

                rObjects.Add(items.Shader("TEXOT"),
                    GLRenderableItem.CreateVector4Vector2(items, rq,
                            GLShapeObjectFactory.CreateQuad(1.0f, 1.0f, new Vector3(0, 0, 0)), GLShapeObjectFactory.TexQuad,
                            new GLRenderDataTranslationRotationTexture(items.Tex("dotted"), new Vector3(4,0,0))
                            ));

                GLRenderControl rqnc = GLRenderControl.Quads(cullface: false);

                rObjects.Add(items.Shader("TEXOT"), "EDDFlat",
                    GLRenderableItem.CreateVector4Vector2(items, rqnc,
                    GLShapeObjectFactory.CreateQuad(2.0f, items.Tex("logo8bpp").Width, items.Tex("logo8bpp").Height, new Vector3(-0, 0, 0)), GLShapeObjectFactory.TexQuad,
                            new GLRenderDataTranslationRotationTexture(items.Tex("logo8bpp"), new Vector3(6, 0, 0))
                            ));

                rObjects.Add(items.Shader("TEXOT"),
                    GLRenderableItem.CreateVector4Vector2(items, rqnc,
                            GLShapeObjectFactory.CreateQuad(1.5f, new Vector3( 90f.Radians(), 0, 0)), GLShapeObjectFactory.TexQuad,
                            new GLRenderDataTranslationRotationTexture(items.Tex("smile"), new Vector3(8, 0, 0))
                           ));

            }

            #endregion


            #region Matrix Calc Uniform

            items.Add(new GLMatrixCalcUniformBlock(),"MCUB");     // def binding of 0

            #endregion

            dataoutbuffer = items.NewStorageBlock(5);
            dataoutbuffer.AllocateBytes(sizeof(float) * 4 * 32, OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicRead);    // 32 vec4 back

        }

        private void ShaderTest_Closed(object sender, EventArgs e)
        {
            items.Dispose();
        }

        private void ControllerDraw(GLMatrixCalc mc, long time)
        {
            //System.Diagnostics.Debug.WriteLine("Draw");

            float degrees = ((float)time / 5000.0f * 360.0f) % 360f;
            float degreesd2 = ((float)time / 10000.0f * 360.0f) % 360f;
            float degreesd4 = ((float)time / 20000.0f * 360.0f) % 360f;
            float zeroone = (degrees >= 180) ? (1.0f - (degrees - 180.0f) / 180.0f) : (degrees / 180f);

            if (rObjects.Contains("woodbox"))
            {
                ((GLRenderDataTranslationRotation)(rObjects["woodbox"].RenderData)).XRotDegrees = degrees;
                ((GLRenderDataTranslationRotation)(rObjects["woodbox"].RenderData)).ZRotDegrees = degrees;

                ((GLRenderDataTranslationRotation)(rObjects["EDDCube"].RenderData)).YRotDegrees = degrees;
                ((GLRenderDataTranslationRotation)(rObjects["EDDCube"].RenderData)).ZRotDegrees = degreesd2;

            }

            if (items.Contains("TEXOCT"))
                ((GLPLVertexShaderTextureModelCoordsWithObjectCommonTranslation)items.Shader("TEXOCT").Get(OpenTK.Graphics.OpenGL4.ShaderType.VertexShader)).Transform.YRotDegrees = degrees;

            if (items.Contains("TEX2DA"))
                ((GLPLFragmentShaderTexture2DBlend)items.Shader("TEX2DA").Get(OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader)).Blend = zeroone;

            if (items.Contains("tapeshader"))
                ((GLTexturedShaderTriangleStripWithWorldCoord)items.Shader("tapeshader")).TexOffset = new Vector2(degrees / 360f, 0.0f);
            if (items.Contains("tapeshader2"))
                ((GLTexturedShaderTriangleStripWithWorldCoord)items.Shader("tapeshader2")).TexOffset = new Vector2(-degrees / 360f, 0.0f);
            if (items.Contains("tapeshader3"))
                ((GLTexturedShaderTriangleStripWithWorldCoord)items.Shader("tapeshader3")).TexOffset = new Vector2(-degrees / 360f, 0.0f);

            if (items.Contains("TESx1"))
                ((GLTesselationShaderSinewave)items.Shader("TESx1")).Phase = degrees / 360.0f;
            if (items.Contains("TESIx1"))
                ((GLTesselationShaderSinewaveInstanced)items.Shader("TESIx1")).Phase = degrees / 360.0f;

            GLStatics.Check();
            GLMatrixCalcUniformBlock mcub = (GLMatrixCalcUniformBlock)items.UB("MCUB");
            mcub.SetFull(gl3dcontroller.MatrixCalc);

            rObjects.Render(glwfc.RenderState, gl3dcontroller.MatrixCalc);

            GL.FrontFace(FrontFaceDirection.Cw);
            rObjectscw.Render(glwfc.RenderState, gl3dcontroller.MatrixCalc);
            GL.FrontFace(FrontFaceDirection.Ccw);

            var azel = gl3dcontroller.PosCamera.EyePosition.AzEl(gl3dcontroller.PosCamera.Lookat, true);

            this.Text = "Looking at " + gl3dcontroller.MatrixCalc.TargetPosition + " from " + gl3dcontroller.MatrixCalc.EyePosition + " cdir " + gl3dcontroller.PosCamera.CameraDirection + " azel " + azel + " zoom " + gl3dcontroller.PosCamera.ZoomFactor + " dist " + gl3dcontroller.MatrixCalc.EyeDistance + " FOV " + gl3dcontroller.MatrixCalc.FovDeg;

            //GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
            //Vector4[] databack = dataoutbuffer.ReadVector4(0, 4);
            //for (int i = 0; i < databack.Length; i += 1)
            //{
            //   // databack[i] = databack[i] / databack[i].W;
            //   // databack[i].X = databack[i].X * gl3dcontroller.glControl.Width / 2 + gl3dcontroller.glControl.Width/2;
            //   // databack[i].Y = gl3dcontroller.glControl.Height - databack[i].Y * gl3dcontroller.glControl.Height;
            //    System.Diagnostics.Debug.WriteLine("{0}={1}", i, databack[i].ToStringVec(true));
            //}
            //GLStatics.Check();

        }

        private void SystemTick(object sender, EventArgs e )
        {
            gl3dcontroller.HandleKeyboardSlewsInvalidate(true, OtherKeys);
            gl3dcontroller.Redraw();
        }

        private void OtherKeys( OpenTKUtils.Common.KeyboardMonitor kb )
        {
            if (kb.HasBeenPressed(Keys.F5, OpenTKUtils.Common.KeyboardMonitor.ShiftState.None))
            {
                gl3dcontroller.CameraLookAt(new Vector3(0, 0, 0), 1, 2);
            }

            if (kb.HasBeenPressed(Keys.F6, OpenTKUtils.Common.KeyboardMonitor.ShiftState.None))
            {
                gl3dcontroller.CameraLookAt(new Vector3(4, 0, 0), 1, 2);
            }

            if (kb.HasBeenPressed(Keys.F7, OpenTKUtils.Common.KeyboardMonitor.ShiftState.None))
            {
                gl3dcontroller.CameraLookAt(new Vector3(10, 0, -10), 1, 2);
            }

            if (kb.HasBeenPressed(Keys.F8, OpenTKUtils.Common.KeyboardMonitor.ShiftState.None))
            {
                gl3dcontroller.CameraLookAt(new Vector3(50, 0, 50), 1, 2);
            }

            if (kb.HasBeenPressed(Keys.F4, OpenTKUtils.Common.KeyboardMonitor.ShiftState.None))
            {
                gl3dcontroller.ChangePerspectiveMode(!gl3dcontroller.MatrixCalc.InPerspectiveMode);
            }


            if (kb.HasBeenPressed(Keys.O, OpenTKUtils.Common.KeyboardMonitor.ShiftState.None))
            {
                System.Diagnostics.Debug.WriteLine("Order to 90");
                gl3dcontroller.CameraPan(new Vector2(90, 0), 3);
            }
            if (kb.HasBeenPressed(Keys.P, OpenTKUtils.Common.KeyboardMonitor.ShiftState.None))
            {
                System.Diagnostics.Debug.WriteLine("Order to -180");
                gl3dcontroller.CameraPan(new Vector2(90, 180), 3);
            }

            //System.Diagnostics.Debug.WriteLine("kb check");

        }

    }

}


