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
using OpenTKUtils;
using OpenTKUtils.Common;
using OpenTKUtils.GL4;
using System;
using System.Drawing;
using System.Windows.Forms;

// Demonstrate the volumetric calculations needed to compute a plane facing the user inside a bounding box done inside a geo shader
// this one add on tex coord calculation and using a single tight quad shows its working

namespace TestOpenTk
{
    public partial class ShaderTestVolumetric7 : Form
    {
        private OpenTKUtils.WinForm.GLWinFormControl glwfc;
        private Controller3D gl3dcontroller;

        private Timer systemtimer = new Timer();

        public ShaderTestVolumetric7()
        {
            InitializeComponent();

            glwfc = new OpenTKUtils.WinForm.GLWinFormControl(glControlContainer);

            systemtimer.Interval = 25;
            systemtimer.Tick += new EventHandler(SystemTick);
            systemtimer.Start();
        }

        GLRenderProgramSortedList rObjects = new GLRenderProgramSortedList();
        GLItemsList items = new GLItemsList();
        Vector4[] boundingbox;
        GLVolumetricUniformBlock volumetricblock;
        GLRenderableItem galaxy;

        /// ////////////////////////////////////////////////////////////////////////////////////////////////////


        private void ShaderTest_Closed(object sender, EventArgs e)
        {
            items.Dispose();
        }

        private void ControllerDraw(GLMatrixCalc mc, long time)
        {
            ((GLMatrixCalcUniformBlock)items.UB("MCUB")).Set(gl3dcontroller.MatrixCalc);        // set the matrix unform block to the controller 3d matrix calc.

            if ( galaxy != null )
                galaxy.InstanceCount = volumetricblock.Set(gl3dcontroller.MatrixCalc, boundingbox, 10.0f);        // set up the volumentric uniform

            rObjects.Render(glwfc.RenderState, gl3dcontroller.MatrixCalc);


            this.Text = "Looking at " + gl3dcontroller.MatrixCalc.TargetPosition + " eye@ " + gl3dcontroller.MatrixCalc.EyePosition + " dir " + gl3dcontroller.Camera.Current + " Dist " + gl3dcontroller.MatrixCalc.EyeDistance + " Zoom " + gl3dcontroller.Zoom.Current;
        }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Closed += ShaderTest_Closed;

            gl3dcontroller = new Controller3D();
            gl3dcontroller.PaintObjects = ControllerDraw;
            gl3dcontroller.MatrixCalc.PerspectiveNearZDistance = 1f;
            gl3dcontroller.MatrixCalc.PerspectiveFarZDistance = 500000f;
            gl3dcontroller.ZoomDistance = 5000F;
            gl3dcontroller.EliteMovement = true;

            gl3dcontroller.KeyboardTravelSpeed = (ms) =>
            {
                return (float)ms * 10.0f;
            };

            gl3dcontroller.MatrixCalc.InPerspectiveMode = true;
            gl3dcontroller.Start(glwfc,new Vector3(0, 0, -35000), new Vector3(126.75f, 0, 0), 0.31622F);

            int front = -20000, back = front + 90000, left = -45000, right = left + 90000, vsize = 2000;

            items.Add("COSW", new GLColourShaderWithWorldCoord());
            GLRenderControl rl1 = GLRenderControl.Lines(1);

            float h = -1;
            if (h != -1)
            {
                int dist = 1000;
                Color cr = Color.FromArgb(20, Color.Red);
                rObjects.Add(items.Shader("COS-1L"),    // horizontal
                             GLRenderableItem.CreateVector4Color4(items, rl1,
                                                        GLShapeObjectFactory.CreateLines(new Vector3(left, h, front), new Vector3(left, h, back), new Vector3(dist, 0, 0), (back - front) / dist + 1),
                                                        new Color4[] { cr })
                                   );

                rObjects.Add(items.Shader("COS-1L"),
                             GLRenderableItem.CreateVector4Color4(items, rl1,
                                                        GLShapeObjectFactory.CreateLines(new Vector3(left, h, front), new Vector3(right, h, front), new Vector3(0, 0, dist), (right - left) / dist + 1),
                                                        new Color4[] { cr })
                                   );

            }

            {

                Vector4[] displaylines = new Vector4[]
                {
                new Vector4(left,-vsize,front,1),   new Vector4(left,+vsize,front,1),
                new Vector4(left,+vsize,front,1),      new Vector4(right,+vsize,front,1),
                new Vector4(right,+vsize,front,1),     new Vector4(right,-vsize,front,1),
                new Vector4(right,-vsize,front,1),  new Vector4(left,-vsize,front,1),

                new Vector4(left,-vsize,back,1),    new Vector4(left,+vsize,back,1),
                new Vector4(left,+vsize,back,1),       new Vector4(right,+vsize,back,1),
                new Vector4(right,+vsize,back,1),      new Vector4(right,-vsize,back,1),
                new Vector4(right,-vsize,back,1),   new Vector4(left,-vsize,back,1),

                new Vector4(left,-vsize,front,1),   new Vector4(left,-vsize,back,1),
                new Vector4(left,+vsize,front,1),      new Vector4(left,+vsize,back,1),
                new Vector4(right,-vsize,front,1),  new Vector4(right,-vsize,back,1),
                new Vector4(right,+vsize,front,1),     new Vector4(right,+vsize,back,1),
                };

                items.Add("LINEYELLOW", new GLFixedColourShaderWithWorldCoord(System.Drawing.Color.Yellow));
                rObjects.Add(items.Shader("LINEYELLOW"),
                GLRenderableItem.CreateVector4(items, rl1, displaylines));
            }


            Bitmap[] numbitmaps = new Bitmap[116];

            {
                Font fnt = new Font("Arial", 20);
                for (int i = 0; i < numbitmaps.Length; i++)
                {
                    int v = -45000 + 1000 * i;      // range from -45000 to +70000 
                    numbitmaps[i] = new Bitmap(100, 100);
                    BaseUtils.BitMapHelpers.DrawTextCentreIntoBitmap(ref numbitmaps[i], v.ToString(), fnt, Color.Red, Color.AliceBlue);
                }

                GLTexture2DArray numtextures = new GLTexture2DArray(numbitmaps, ownbitmaps: true);
                items.Add("Nums", numtextures);

                Matrix4[] numberposx = new Matrix4[(right - left) / 1000 + 1];
                for (int i = 0; i < numberposx.Length; i++)
                {
                    numberposx[i] = Matrix4.CreateScale(1);
                    numberposx[i] *= Matrix4.CreateRotationX(-25f.Radians());
                    numberposx[i] *= Matrix4.CreateTranslation(new Vector3(left + 1000 * i, 0, front));
                }

                GLShaderPipeline numshaderx = new GLShaderPipeline(new GLPLVertexShaderTextureModelCoordWithMatrixTranslation(), new GLPLFragmentShaderTexture2DIndexed(0));
                items.Add("IC-X", numshaderx);

                GLRenderControl rq = GLRenderControl.Quads(cullface: false);
                GLRenderDataTexture rt = new GLRenderDataTexture(items.Tex("Nums"));

                rObjects.Add(numshaderx, "xnum",
                                        GLRenderableItem.CreateVector4Vector2Matrix4(items, rq,
                                                GLShapeObjectFactory.CreateQuad(500.0f), GLShapeObjectFactory.TexQuad, numberposx,
                                                rt, numberposx.Length));

                Matrix4[] numberposz = new Matrix4[(back - front) / 1000 + 1];
                for (int i = 0; i < numberposz.Length; i++)
                {
                    numberposz[i] = Matrix4.CreateScale(1);
                    numberposz[i] *= Matrix4.CreateRotationX(-25f.Radians());
                    numberposz[i] *= Matrix4.CreateTranslation(new Vector3(right + 1000, 0, front + 1000 * i));
                }

                GLShaderPipeline numshaderz = new GLShaderPipeline(new GLPLVertexShaderTextureModelCoordWithMatrixTranslation(), new GLPLFragmentShaderTexture2DIndexed(25));
                items.Add("IC-Z", numshaderz);

                rObjects.Add(numshaderz, "ynum",
                                        GLRenderableItem.CreateVector4Vector2Matrix4(items, rq,
                                                GLShapeObjectFactory.CreateQuad(500.0f), GLShapeObjectFactory.TexQuad, numberposz,
                                                rt, numberposz.Length));
            }

            {
                items.Add("solmarker", new GLTexture2D(numbitmaps[45]));
                items.Add("TEX", new GLTexturedShaderWithObjectTranslation());

                GLRenderControl rq = GLRenderControl.Quads(cullface: false);

                rObjects.Add(items.Shader("TEX"),
                             GLRenderableItem.CreateVector4Vector2(items, rq,
                             GLShapeObjectFactory.CreateQuad(1000.0f, 1000.0f, new Vector3(0, 0, 0)), GLShapeObjectFactory.TexQuad,
                             new GLRenderDataTranslationRotationTexture(items.Tex("solmarker"), new Vector3(0, 1000, 0))
                             ));
                rObjects.Add(items.Shader("TEX"),
                             GLRenderableItem.CreateVector4Vector2(items, rq,
                             GLShapeObjectFactory.CreateQuad(1000.0f, 1000.0f, new Vector3(0, 0, 0)), GLShapeObjectFactory.TexQuad,
                             new GLRenderDataTranslationRotationTexture(items.Tex("solmarker"), new Vector3(0, -1000, 0))
                             ));
                items.Add("sag", new GLTexture2D(Properties.Resources.dotted));
                rObjects.Add(items.Shader("TEX"),
                             GLRenderableItem.CreateVector4Vector2(items, rq,
                             GLShapeObjectFactory.CreateQuad(1000.0f, 1000.0f, new Vector3(0, 0, 0)), GLShapeObjectFactory.TexQuad,
                             new GLRenderDataTranslationRotationTexture(items.Tex("sag"), new Vector3(25.2f, 2000, 25899))
                             ));
                rObjects.Add(items.Shader("TEX"),
                             GLRenderableItem.CreateVector4Vector2(items, rq, 
                             GLShapeObjectFactory.CreateQuad(1000.0f, 1000.0f, new Vector3(0, 0, 0)), GLShapeObjectFactory.TexQuad,
                             new GLRenderDataTranslationRotationTexture(items.Tex("sag"), new Vector3(25.2f, -2000, 25899))
                             ));
                items.Add("bp", new GLTexture2D(Properties.Resources.dotted2));
                rObjects.Add(items.Shader("TEX"),
                             GLRenderableItem.CreateVector4Vector2(items, rq, 
                             GLShapeObjectFactory.CreateQuad(1000.0f, 1000.0f, new Vector3(0, 0, 0)), GLShapeObjectFactory.TexQuad,
                             new GLRenderDataTranslationRotationTexture(items.Tex("bp"), new Vector3(-1111f, 0, 65269))
                             ));
            }

            boundingbox = new Vector4[]
            {
                new Vector4(left,-vsize,front,1),
                new Vector4(left,vsize,front,1),
                new Vector4(right,vsize,front,1),
                new Vector4(right,-vsize,front,1),

                new Vector4(left,-vsize,back,1),
                new Vector4(left,vsize,back,1),
                new Vector4(right,vsize,back,1),
                new Vector4(right,-vsize,back,1),
            };

            if (true)
            {
                volumetricblock = new GLVolumetricUniformBlock();
                items.Add("VB", volumetricblock);

                int sc = 1;
                GLTexture3D noise3d = new GLTexture3D(1024 * sc, 64 * sc, 1024 * sc, OpenTK.Graphics.OpenGL4.SizedInternalFormat.R32f); // red channel only
                items.Add("Noise", noise3d);
                ComputeShaderNoise3D csn = new ComputeShaderNoise3D(noise3d.Width, noise3d.Height, noise3d.Depth, 128 * sc, 16 * sc, 128 * sc);       // must be a multiple of localgroupsize in csn
                csn.StartAction += (A) => { noise3d.BindImage(3); };
                csn.Run();      // compute noise

                GLTexture1D gaussiantex = new GLTexture1D(1024, OpenTK.Graphics.OpenGL4.SizedInternalFormat.R32f); // red channel only
                items.Add("Gaussian", gaussiantex);

                // set centre=width, higher widths means more curve, higher std dev compensate

                ComputeShaderGaussian gsn = new ComputeShaderGaussian(gaussiantex.Width, 2.0f, 2.0f, 1.4f, 4);
                gsn.StartAction += (A) => { gaussiantex.BindImage(4); };
                gsn.Run();      // compute noise

                GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);

                float[] gdata = gaussiantex.GetTextureImageAsFloats(OpenTK.Graphics.OpenGL4.PixelFormat.Red); // read back check

                // load one upside down and horz flipped, because the volumetric co-ords are 0,0,0 bottom left, 1,1,1 top right
                GLTexture2D galtex = new GLTexture2D(Properties.Resources.Galaxy_L180);
                items.Add("gal", galtex);
                GalaxyShader gs = new GalaxyShader();
                items.Add("Galaxy", gs);
                gs.StartAction = (a) => { galtex.Bind(1); noise3d.Bind(3); gaussiantex.Bind(4); };

                GLRenderControl rv = GLRenderControl.ToTri(OpenTK.Graphics.OpenGL4.PrimitiveType.Points);
                galaxy = GLRenderableItem.CreateNullVertex(rv);   // no vertexes, all data from bound volumetric uniform, no instances as yet
                rObjects.Add(items.Shader("Galaxy"), galaxy);
            }

            items.Add("MCUB", new GLMatrixCalcUniformBlock());     // create a matrix uniform block 





        }

        public class GalaxyShader : GLShaderStandard
        {
            string vcode =
            @"
            #version 450 core

            out int instance;

            void main(void)
            {
                instance = gl_InstanceID;
            }
            ";

            string fcode = @"
#version 450 core
out vec4 color;

in vec3 vs_texcoord;

layout (binding=1) uniform sampler2D tex;
layout (binding=3) uniform sampler3D noise;     
layout (binding=4) uniform sampler1D gaussian;  

void main(void)
{
//        color = texture(tex,vec2(vs_texcoord.x,vs_texcoord.z));     
//color = vec4(vs_texcoord.x, vs_texcoord.z,0,0);
//color.w = 0.5;


    float dx = abs(0.5-vs_texcoord.x);
    float dz = abs(0.5-vs_texcoord.z);
    float d = 0.7072-sqrt(dx*dx+dz*dz);     // 0 - 0.7
    d = d / 0.7272; // 0.707 is the unit circle, 1 is the max at corners

    if ( d > (1-0.707) )               // limit to circle around centre
    {
        vec4 c = texture(tex,vec2(vs_texcoord.x,vs_texcoord.z)); 
        float brightness = sqrt(c.x*c.x+c.y*c.y+c.z*c.z)/1.733;         // 0-1

        if ( brightness > 0.001 )
        {
            float g = texture(gaussian,d).x;      // look up single sided gaussian function, 0-1
            float h = abs(vs_texcoord.y-0.5)*2;     // 0-1 also

            if ( g > h )
            {
                float nv = texture(noise,vs_texcoord.xyz).x;
                float alpha = min(max(brightness,0.5),(brightness>0.05) ? 0.3 : brightness);    // beware the 8 bit alpha (0.0039 per bit).
                color = vec4(c.xyz*(1.0+nv*0.2),alpha*(1.0+nv*0.1));        // noise adjusting brightness and alpha a little
            }
            else 
                discard;
        }
        else
            discard;
    }
    else
        discard;


}
            ";

            public GalaxyShader()
            {
                CompileLink(vertex: vcode, frag: fcode, geo: "#include TestOpenTk.Volumetrics.volumetricgeo6.glsl");
            }
        }

        
        private void SystemTick(object sender, EventArgs e)
        {
            var cdmt = gl3dcontroller.HandleKeyboard(true, OtherKeys);
            if (cdmt.AnythingChanged)
                gl3dcontroller.Redraw();
        }

        private void OtherKeys(BaseUtils.KeyboardState kb)
        {
            if (kb.IsPressedRemove(Keys.F1, BaseUtils.KeyboardState.ShiftState.None))
            {
                int times = 1000;
                System.Diagnostics.Debug.WriteLine("Start test");
                long tickcount = gl3dcontroller.Redraw(times);
                System.Diagnostics.Debug.WriteLine("Redraw {0} ms per {1}", tickcount, (float)tickcount/(float)times);
            }
        }
    }
}


