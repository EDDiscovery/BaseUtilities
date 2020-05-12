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
using OpenTKUtils;
using OpenTKUtils.Common;
using OpenTKUtils.GL4;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

// Demonstrate the volumetric calculations needed to compute a plane facing the user inside a bounding box.

namespace TestOpenTk
{
    public partial class ShaderTestVolumetric : Form
    {
        private OpenTKUtils.WinForm.GLWinFormControl glwfc;
        private Controller3D gl3dcontroller;

        private Timer systemtimer = new Timer();

        public ShaderTestVolumetric()
        {
            InitializeComponent();

            glwfc = new OpenTKUtils.WinForm.GLWinFormControl(glControlContainer);

            systemtimer.Interval = 25;
            systemtimer.Tick += new EventHandler(SystemTick);
            systemtimer.Start();
        }

        GLRenderProgramSortedList rObjects = new GLRenderProgramSortedList();
        GLItemsList items = new GLItemsList();

        public class GLFixedShader : GLShaderPipeline
        {
            public GLFixedShader(Color c, Action<IGLProgramShader> action = null) : base(action)
            {
                AddVertexFragment(new GLPLVertexShaderWorldCoord(), new GLPLFragmentShaderFixedColour(c));
            }
        }

        public class GLFixedProjectionShader : GLShaderPipeline
        {
            public GLFixedProjectionShader(Color c, Action<IGLProgramShader> action = null) : base(action)
            {
                AddVertexFragment(new GLPLVertexShaderModelViewCoord(), new GLPLFragmentShaderFixedColour(c));
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
            gl3dcontroller.Start(glwfc,new Vector3(0, 0, 0), new Vector3(90,0,0), 1F);

            gl3dcontroller.KeyboardTravelSpeed = (ms,eyedist) =>
            {
                return (float)ms / 100.0f;
            };

            items.Add(new GLColourShaderWithWorldCoord(), "COSW");
            GLRenderControl rl1 = GLRenderControl.Lines(1);

            {

                rObjects.Add(items.Shader("COSW"), "L1",   // horizontal
                             GLRenderableItem.CreateVector4Color4(items, rl1,
                                                        GLShapeObjectFactory.CreateLines(new Vector3(-100, 0, -100), new Vector3(-100, 0, 100), new Vector3(10, 0, 0), 21),
                                                        new Color4[] { Color.Gray })
                                   );


                rObjects.Add(items.Shader("COSW"),    // vertical
                             GLRenderableItem.CreateVector4Color4(items, rl1,
                                   GLShapeObjectFactory.CreateLines(new Vector3(-100, 0, -100), new Vector3(100, 0, -100), new Vector3(0, 0, 10), 21),
                                                             new Color4[] { Color.Gray })
                                   );

            }

            int hsize = 10, vsize = 5, zsize = 10;
            boundingbox = new Vector4[]
            {
                    new Vector4(-hsize,-vsize,zsize,1),
                    new Vector4(-hsize,vsize,zsize,1),
                    new Vector4(hsize,vsize,zsize,1),
                    new Vector4(hsize,-vsize,zsize,1),

                    new Vector4(-hsize,-vsize,-zsize,1),
                    new Vector4(-hsize,vsize,-zsize,1),
                    new Vector4(hsize,vsize,-zsize,1),
                    new Vector4(hsize,-vsize,-zsize,1),
            };


            {
                GLRenderControl rll = GLRenderControl.LineLoop(4);

                rObjects.Add(items.Shader("COSW"),
                            GLRenderableItem.CreateVector4(items, rll, boundingbox));

                Vector4[] extralines = new Vector4[]
                {
                    new Vector4(-hsize,-vsize,zsize,1),
                    new Vector4(-hsize,-vsize,-zsize,1),

                    new Vector4(-hsize,vsize,zsize,1),
                    new Vector4(-hsize,vsize,-zsize,1),

                    new Vector4(hsize,vsize,zsize,1),
                    new Vector4(hsize,vsize,-zsize,1),

                    new Vector4(hsize,-vsize,zsize,1),
                    new Vector4(hsize,-vsize,-zsize,1),

                    new Vector4(-hsize,-vsize,zsize,1),
                    new Vector4(hsize,-vsize,zsize,1),

                    new Vector4(-hsize,-vsize,-zsize,1),
                    new Vector4(hsize,-vsize,-zsize,1),
                };

                GLRenderControl rl = GLRenderControl.Lines(4);
                rObjects.Add(items.Shader("COSW"),
                            GLRenderableItem.CreateVector4(items, rl, extralines));
            }

            items.Add(new GLFixedShader(System.Drawing.Color.Purple), "LINEPURPLE");

            indicatorlinebuffer = new GLBuffer();           // new buffer
            indicatorlinebuffer.AllocateBytes(sizeof(float) * 4 * 2, OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicCopy);       // set size of vec buffer
            rObjects.Add(items.Shader("LINEPURPLE"), GLRenderableItem.CreateVector4(items, rl1, indicatorlinebuffer, 2));

            items.Add(new GLFixedProjectionShader(System.Drawing.Color.Yellow), "DOTYELLOW");
            interceptpointbuffer = new GLBuffer();           // new buffer
            interceptpointbuffer.AllocateBytes(sizeof(float) * 4 * 12, OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicCopy);       // set size of vec buffer
            GLRenderControl rp1 = GLRenderControl.Points(10);
            interceptri = GLRenderableItem.CreateVector4(items, rp1, interceptpointbuffer, 0);
            rObjects.Add(items.Shader("DOTYELLOW"), interceptri);

            items.Add(new GLFixedProjectionShader(System.Drawing.Color.FromArgb(60,Color.Blue)), "SURFACEBLUE");
            surfacebuffer = new GLBuffer();           // new buffer
            surfacebuffer.AllocateBytes(sizeof(float) * 4 * (6+2), OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicCopy);       // set size of vec buffer
            GLRenderControl rtf = GLRenderControl.TriFan();
            surfaceri = GLRenderableItem.CreateVector4(items, rtf, surfacebuffer, 0);
            rObjects.Add(items.Shader("SURFACEBLUE"), surfaceri);

            items.Add( new GLMatrixCalcUniformBlock(), "MCUB");     // create a matrix uniform block 

        }

        Vector4[] boundingbox;
        GLBuffer indicatorlinebuffer;
        GLBuffer interceptpointbuffer;
        GLRenderableItem interceptri;
        GLBuffer surfacebuffer;
        GLRenderableItem surfaceri;

        private void ShaderTest_Closed(object sender, EventArgs e)
        {
            items.Dispose();
        }

        private void ControllerDraw(GLMatrixCalc mc, long time)
        {
            ((GLMatrixCalcUniformBlock)items.UB("MCUB")).Set(gl3dcontroller.MatrixCalc);        // set the matrix unform block to the controller 3d matrix calc.


            Vector4[] modelboundingbox = boundingbox.Transform(gl3dcontroller.MatrixCalc.ModelMatrix);

            for (int i = 0; i < boundingbox.Length; i++)
            {
                System.Diagnostics.Debug.WriteLine(i + " = " + modelboundingbox[i].ToStringVec());
            }

            modelboundingbox.MinMaxZ(out int minz, out int maxz);

            System.Diagnostics.Debug.WriteLine("min " + minz + " max " + maxz);
            indicatorlinebuffer.StartWrite(0, sizeof(float) * 4 * 2);
            indicatorlinebuffer.Write(boundingbox[minz]);
            indicatorlinebuffer.Write(boundingbox[maxz]);
            indicatorlinebuffer.StopReadWrite();

            float percent = 0.2f;
            float zdist = modelboundingbox[maxz].Z - modelboundingbox[minz].Z;
            {
                float zpoint = modelboundingbox[maxz].Z - zdist * percent;
                //System.Diagnostics.Debug.WriteLine("Zpoint is" + zpoint);

                Vector4[] intercepts = new Vector4[6];
                int count = 0;
                modelboundingbox[0].FindVectorFromZ(modelboundingbox[1], ref intercepts, ref count, zpoint);
                modelboundingbox[1].FindVectorFromZ(modelboundingbox[2], ref intercepts, ref count, zpoint);
                modelboundingbox[2].FindVectorFromZ(modelboundingbox[3], ref intercepts, ref count, zpoint);
                modelboundingbox[3].FindVectorFromZ(modelboundingbox[0], ref intercepts, ref count, zpoint);

                modelboundingbox[4].FindVectorFromZ(modelboundingbox[5], ref intercepts, ref count, zpoint);
                modelboundingbox[5].FindVectorFromZ(modelboundingbox[6], ref intercepts, ref count, zpoint);
                modelboundingbox[6].FindVectorFromZ(modelboundingbox[7], ref intercepts, ref count, zpoint);
                modelboundingbox[7].FindVectorFromZ(modelboundingbox[4], ref intercepts, ref count, zpoint);

                modelboundingbox[0].FindVectorFromZ(modelboundingbox[4], ref intercepts, ref count, zpoint);
                modelboundingbox[1].FindVectorFromZ(modelboundingbox[5], ref intercepts, ref count, zpoint);
                modelboundingbox[2].FindVectorFromZ(modelboundingbox[6], ref intercepts, ref count, zpoint);
                modelboundingbox[3].FindVectorFromZ(modelboundingbox[7], ref intercepts, ref count, zpoint);

                // texturecoords can be worked out by zpercent and knowing its direction (x,y,z)..

                if (count >= 3)
                {
                    Vector4 avg = intercepts.Average();
                    float[] angles = new float[6];
                    for (int i = 0; i < count; i++)
                    {
                        angles[i] = -(float)Math.Atan2(intercepts[i].Y - avg.Y, intercepts[i].X - avg.X);        // all on the same z plane, so x/y only need be considered
                        System.Diagnostics.Debug.WriteLine("C" + intercepts[i].ToStringVec() + " " + angles[i].Degrees());
                    }

                    Array.Sort(angles, intercepts, 0, count);       // sort by angles, sorting intercepts, from 0 to count

                    for (int i = 0; i < count; i++)
                    {
                        //    System.Diagnostics.Debug.WriteLine(intercepts[i].ToStringVec() + " " + angles[i].Degrees());
                    }

                    interceptpointbuffer.StartWrite(0, sizeof(float) * 4 * count);
                    int ji = 0;
                    for (; ji < count; ji++)
                        interceptpointbuffer.Write(intercepts[ji]);
                    interceptpointbuffer.StopReadWrite();
                    interceptri.DrawCount = count;

                    surfacebuffer.StartWrite(0, sizeof(float) * 4 * (2 + count));
                    surfacebuffer.Write(avg);
                    for (ji = 0; ji < count; ji++)
                        surfacebuffer.Write(intercepts[ji]);

                    surfacebuffer.Write(intercepts[0]);
                    surfacebuffer.StopReadWrite();

                    surfaceri.DrawCount = count + 2;


                }
            }

            rObjects.Render(glwfc.RenderState, gl3dcontroller.MatrixCalc);

            this.Text = "Looking at " + gl3dcontroller.MatrixCalc.TargetPosition + " dir " + gl3dcontroller.PosCamera.CameraDirection + " eye@ " + gl3dcontroller.MatrixCalc.EyePosition + " Dist " + gl3dcontroller.MatrixCalc.EyeDistance;
        }

        private void SystemTick(object sender, EventArgs e )
        {
            gl3dcontroller.HandleKeyboardSlewsInvalidate(true);
        }
    }
}


