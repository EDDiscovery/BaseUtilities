 using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTKUtils;
using OpenTKUtils.Common;
using OpenTKUtils.GL4;
using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

// Demonstrate the volumetric calculations needed to compute a plane facing the user inside a bounding box done inside a geo shader
// this one add on tex coord calculation and using a single tight quad shows its working

namespace TestOpenTk
{
    public partial class ShaderTestVolumetricNoise : Form
    {
        private OpenTKUtils.WinForm.GLWinFormControl glwfc;
        private Controller3D gl3dcontroller;

        private Timer systemtimer = new Timer();

        public ShaderTestVolumetricNoise()
        {
            InitializeComponent();

            glwfc = new OpenTKUtils.WinForm.GLWinFormControl(glControlContainer);

            systemtimer.Interval = 25;
            systemtimer.Tick += new EventHandler(SystemTick);
            systemtimer.Start();
        }


        /// ////////////////////////////////////////////////////////////////////////////////////////////////////

        public class GLFixedShader : GLShaderPipeline
        {
            public GLFixedShader(Color c, Action<IGLProgramShader> action = null) : base(action)
            {
                AddVertexFragment(new GLPLVertexShaderWorldCoord(), new GLPLFragmentShaderFixedColour(c));
            }
        }

        
        void DebugProc(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
        {
            //string s = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(message);
            //System.Diagnostics.Debug.WriteLine("{0} {1} {2} {3} {4} {5}", source, type, id, severity, length, s);
           //  s = null;
        }

        GLRenderProgramSortedList rObjects = new GLRenderProgramSortedList();
        GLItemsList items = new GLItemsList();
        Vector4[] boundingbox;
        GLStorageBlock dataoutbuffer;
        GLVolumetricUniformBlock volumetricblock;
        GLAtomicBlock atomicbuffer;
        GLRenderableItem noisebox;

        private void ShaderTest_Closed(object sender, EventArgs e)
        {
            items.Dispose();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Closed += ShaderTest_Closed;

            //GLStatics.EnableDebug(DebugProc);

            gl3dcontroller = new Controller3D();
            gl3dcontroller.PaintObjects = ControllerDraw;
            gl3dcontroller.MatrixCalc.PerspectiveNearZDistance = 1f;
            gl3dcontroller.MatrixCalc.PerspectiveFarZDistance = 500000f;
            gl3dcontroller.ZoomDistance = 5000F;
            gl3dcontroller.EliteMovement = true;

            gl3dcontroller.KeyboardTravelSpeed = (ms,eyedist) =>
            {
                return (float)ms * 10.0f;
            };

            gl3dcontroller.MatrixCalc.InPerspectiveMode = true;
            gl3dcontroller.Start(glwfc,new Vector3(0, 0, -35000), new Vector3(135f, 0, 0), 0.31622F);


            items.Add("COSW", new GLColourShaderWithWorldCoord());
            GLRenderControl rl1 = GLRenderControl.Lines(1);

            float h = -1;
            if ( h != -1 )
            {
                Color cr = Color.FromArgb(60, Color.Gray);
                rObjects.Add(items.Shader("COS-1L"),    // horizontal
                             GLRenderableItem.CreateVector4Color4(items, rl1,
                                                        GLShapeObjectFactory.CreateLines(new Vector3(-35000, h, -35000), new Vector3(-35000, h, 35000), new Vector3(1000, 0, 0), 70),
                                                        new Color4[] { cr })
                                   );

                rObjects.Add(items.Shader("COS-1L"),  
                             GLRenderableItem.CreateVector4Color4(items, rl1,
                                                        GLShapeObjectFactory.CreateLines(new Vector3(-35000, h, -35000), new Vector3(35000, h, -35000), new Vector3(0, 0,1000), 70),
                                                        new Color4[] { cr })
                                   );
            }

            int hsize = 35000, vsize = 2000, zsize = 35000;

            {
                int left = -hsize, right = hsize, bottom = -vsize, top = +vsize, front = -zsize, back = zsize;
                Vector4[] lines2 = new Vector4[]
                {
                new Vector4(left,bottom,front,1),   new Vector4(left,top,front,1),
                new Vector4(left,top,front,1),      new Vector4(right,top,front,1),
                new Vector4(right,top,front,1),     new Vector4(right,bottom,front,1),
                new Vector4(right,bottom,front,1),  new Vector4(left,bottom,front,1),

                new Vector4(left,bottom,back,1),    new Vector4(left,top,back,1),
                new Vector4(left,top,back,1),       new Vector4(right,top,back,1),
                new Vector4(right,top,back,1),      new Vector4(right,bottom,back,1),
                new Vector4(right,bottom,back,1),   new Vector4(left,bottom,back,1),

                new Vector4(left,bottom,front,1),   new Vector4(left,bottom,back,1),
                new Vector4(left,top,front,1),      new Vector4(left,top,back,1),
                new Vector4(right,bottom,front,1),  new Vector4(right,bottom,back,1),
                new Vector4(right,top,front,1),     new Vector4(right,top,back,1),

                };

                items.Add("LINEYELLOW", new GLFixedShader(System.Drawing.Color.Yellow));
                rObjects.Add(items.Shader("LINEYELLOW"),
                            GLRenderableItem.CreateVector4(items, rl1, lines2));
            }

            // bounding box

            boundingbox = new Vector4[]
            {
                new Vector4(-hsize,-vsize,-zsize,1),
                new Vector4(-hsize,vsize,-zsize,1),
                new Vector4(hsize,vsize,-zsize,1),
                new Vector4(hsize,-vsize,-zsize,1),

                new Vector4(-hsize,-vsize,zsize,1),
                new Vector4(-hsize,vsize,zsize,1),
                new Vector4(hsize,vsize,zsize,1),
                new Vector4(hsize,-vsize,zsize,1),
            };

            items.Add("MCUB", new GLMatrixCalcUniformBlock());     // create a matrix uniform block 

            dataoutbuffer = items.NewStorageBlock(5);
            dataoutbuffer.Allocate(sizeof(float) * 4 * 32, OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicRead);    // 32 vec4 back

            atomicbuffer = items.NewAtomicBlock(6);
            atomicbuffer.Allocate(sizeof(float) * 32, OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicCopy);

            dataoutbuffer = items.NewStorageBlock(5);
            dataoutbuffer.Allocate(sizeof(float) * 4 * 256, OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicRead);    // 32 vec4 back

            volumetricblock = new GLVolumetricUniformBlock();
            items.Add("VB",volumetricblock);

            {
                Bitmap[] numbers = new Bitmap[70];
                Matrix4[] numberpos = new Matrix4[numbers.Length];
                Matrix4[] numberpos2 = new Matrix4[numbers.Length];

                Font fnt = new Font("Arial", 20);

                for (int i = 0; i < numbers.Length; i++)
                {
                    int v = -35000 + i * 1000;
                    numbers[i] = new Bitmap(100, 100);
                    BaseUtils.BitMapHelpers.DrawTextCentreIntoBitmap(ref numbers[i], v.ToString(), fnt, Color.Red, Color.AliceBlue);

                    numberpos[i] = Matrix4.CreateScale(1);
                    numberpos[i] *= Matrix4.CreateRotationX(-25f.Radians());
                    numberpos[i] *= Matrix4.CreateTranslation(new Vector3(35500, 0, v));
                    numberpos2[i] = Matrix4.CreateScale(1);
                    numberpos2[i] *= Matrix4.CreateRotationX(-25f.Radians());
                    numberpos2[i] *= Matrix4.CreateTranslation(new Vector3(v, 0, -35500));
                }

                GLTexture2DArray array = new GLTexture2DArray(numbers, ownbitmaps: true);
                items.Add("Nums", array);
                items.Add("IC-2", new GLShaderPipeline(new GLPLVertexShaderTextureModelCoordWithMatrixTranslation(), new GLPLFragmentShaderTexture2DIndexed(0)));

                GLRenderControl rq = GLRenderControl.Quads(cullface: false);
                GLRenderDataTexture rt = new GLRenderDataTexture(items.Tex("Nums"));

                rObjects.Add(items.Shader("IC-2"), "1-b",
                                        GLRenderableItem.CreateVector4Vector2Matrix4(items, rq,
                                                GLShapeObjectFactory.CreateQuad(500.0f), GLShapeObjectFactory.TexQuad, numberpos,
                                                rt, numberpos.Length));

                rObjects.Add(items.Shader("IC-2"), "1-b2",
                                        GLRenderableItem.CreateVector4Vector2Matrix4(items, rq,
                                                GLShapeObjectFactory.CreateQuad(500.0f), GLShapeObjectFactory.TexQuad, numberpos2,
                                                rt, numberpos.Length));
            }

            GLTexture3D noise3d = new GLTexture3D(1024, 64, 1024, OpenTK.Graphics.OpenGL4.SizedInternalFormat.R32f); // red channel only

            //{     // shows program fill
            //    for (int ly = 0; ly < noise3d.Depth; ly++)
            //    {
            //        float[] fd = new float[noise3d.Width * noise3d.Height];
            //        float[] fdi = new float[noise3d.Width * noise3d.Height];
            //        for (int x = 0; x < noise3d.Width; x++)
            //        {
            //            for (int y = 0; y < noise3d.Height; y++)
            //            {
            //                int p = (y * noise3d.Width + x) * 1;

            //                float xv = (float)x / (float)noise3d.Width;
            //                float yv = (float)y / (float)noise3d.Height;

            //                var c = ((Math.Sin(2 * Math.PI * xv) / 2) + 0.5);
            //                c += ((Math.Cos(2 * Math.PI * yv) / 2) + 0.5);

            //                c /= 2;

            //                fd[p + 0] = (float)(c);
            //                fdi[p + 0] = 1.0f-(float)(c);
            //            }
            //        }

            //        noise3d.StoreZPlane(ly, 0, 0, noise3d.Width, noise3d.Height, fd, OpenTK.Graphics.OpenGL4.PixelFormat.Red);        // only a single float per pixel, stored in RED

            //    }
            //}

            ShaderNoise ns = new ShaderNoise();
            ns.StartAction = (a) => { noise3d.Bind(3); };


            items.Add("NS", ns);
            GLRenderControl rv = GLRenderControl.ToTri(OpenTK.Graphics.OpenGL4.PrimitiveType.Points);
            noisebox = GLRenderableItem.CreateNullVertex(rv);   // no vertexes, all data from bound volumetric uniform, no instances as yet

            rObjects.Add(items.Shader("NS"), noisebox);

            ComputeShaderNoise csn = new ComputeShaderNoise(noise3d.Width, noise3d.Height, noise3d.Depth,32,4,32);       // must be a multiple of localgroupsize in csn
            csn.StartAction += (A) => { noise3d.BindImage(3); };
            items.Add("CE1", csn);

            GLComputeShaderList p = new GLComputeShaderList();      // demonstrate a render list holding a compute shader.
            p.Add(csn);
            p.Run();
        }

        private void ControllerDraw(GLMatrixCalc mc, long time)
        {
            items.Get<GLMatrixCalcUniformBlock>("MCUB").Set(gl3dcontroller.MatrixCalc);        // set the matrix unform block to the controller 3d matrix calc.

            noisebox.InstanceCount = volumetricblock.Set(gl3dcontroller.MatrixCalc, boundingbox, 10.0f);        // set up the volumentric uniform

            dataoutbuffer.ZeroBuffer();
            atomicbuffer.ZeroBuffer();

            rObjects.Render(glwfc.RenderState,gl3dcontroller.MatrixCalc);

            GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);

            Vector4[] databack = dataoutbuffer.ReadVector4(0, 10);

            //System.Diagnostics.Debug.WriteLine("avg {0} txtavg {1}", databack[0].ToStringVec(), databack[1].ToStringVec());

            for (int i = 0; i < databack.Length; i += 1)
            {
         //       System.Diagnostics.Debug.WriteLine("db " + databack[i].ToStringVec());
            }

            this.Text = "Looking at " + gl3dcontroller.MatrixCalc.TargetPosition + " eye@ " + gl3dcontroller.MatrixCalc.EyePosition + " dir " + gl3dcontroller.Camera.Current + " Dist " + gl3dcontroller.MatrixCalc.EyeDistance + " Zoom " + gl3dcontroller.Zoom.Current;
        }

        public class ComputeShaderNoise: GLShaderCompute
        {
            static int Localgroupsize = 8;

            private string gencode(int w, int h, int d , int wb, int hb, int db)
            {
                return
@"
#version 450 core
#include OpenTKUtils.GL4.Shaders.Functions.noise3.glsl
#include OpenTKUtils.GL4.Shaders.Functions.random.glsl

layout (local_size_x = 8, local_size_y = 8, local_size_z = 8) in;

layout (binding=3, r32f ) uniform image3D img;

void main(void)
{
    ivec3 p = ivec3(gl_GlobalInvocationID.xyz);

    float w = " + w.ToStringInvariant() + @";       // grab the constants from caller
    float h = " + h.ToStringInvariant() + @";
    float d = " + d.ToStringInvariant() + @";
    float wb = " + wb.ToStringInvariant() + @";     // these set the granularity of the image..
    float hb = " + hb.ToStringInvariant() + @";
    float db = " + db.ToStringInvariant() + @";

    vec3 np = vec3( float(gl_GlobalInvocationID.x)/w*wb, float(gl_GlobalInvocationID.y)/h*hb,float(gl_GlobalInvocationID.z)/d*db);

    float f = gradientnoiseT1(np);
    vec4 color = vec4(f*0.5+0.5,0,0,1);             // red only

    imageStore( img, p, color);                     // store back the computed noise
}
";
            }

            public ComputeShaderNoise(int width, int height, int depth, int wb, int hb, int db) : base(width/Localgroupsize,height/Localgroupsize,depth/Localgroupsize)
            {
                CompileLink( gencode(width,height,depth,wb,hb,db)) ;
            }

        }


        public class ShaderNoise : GLShaderStandard
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
#include OpenTKUtils.GL4.Shaders.Functions.noise2.glsl
#include OpenTKUtils.GL4.Shaders.Functions.noise3.glsl
#include OpenTKUtils.GL4.Shaders.Functions.random.glsl
#include OpenTKUtils.GL4.Shaders.Functions.colours.glsl
out vec4 color;

in vec3 vs_texcoord;

layout (binding=3) uniform sampler3D tex;

void main(void)
{   
    //ivec3 texpos = ivec3(vs_texcoord.x*128,vs_texcoord.y*16,vs_texcoord.z*128);   
    //int lod=0;
    //vec4 n2 = texelFetch(tex, texpos,lod);  // pixel samples, need pixel position

    vec3 texpos = vec3(vs_texcoord.x,vs_texcoord.y,vs_texcoord.z);
    vec4 n2 = texture(tex, texpos);         // texture needs 0->1 across whole surface

    color = vec4(n2.x,0,0,1);
}
            ";

            public ShaderNoise()
            {
                var an = System.Reflection.Assembly.GetExecutingAssembly().GetName();

                CompileLink(vertex: vcode, frag: fcode, geo: "#include " + an.Name + ".Volumetrics.volumetricgeoNoise.glsl");
            }
        }


        private void SystemTick(object sender, EventArgs e)
        {
            var cdmt = gl3dcontroller.HandleKeyboardSlews(true, OtherKeys);
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

