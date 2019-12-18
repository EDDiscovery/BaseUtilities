using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTKUtils;
using OpenTKUtils.Common;
using OpenTKUtils.GL4;
using System;
using System.Drawing;
using System.Windows.Forms;
using BaseUtils;
using System.Collections.Generic;

// Demonstrate the volumetric calculations needed to compute a plane facing the user inside a bounding box done inside a geo shader
// this one add on tex coord calculation and using a single tight quad shows its working

namespace TestOpenTk
{
    public partial class ShaderGalaxy : Form
    {
        private Controller3D gl3dcontroller = new Controller3D();

        private Timer systemtimer = new Timer();

        public ShaderGalaxy()
        {
            InitializeComponent();

            this.glControlContainer.SuspendLayout();
            gl3dcontroller.CreateGLControl();
            this.glControlContainer.Controls.Add(gl3dcontroller.glControl);
            gl3dcontroller.PaintObjects = ControllerDraw;
            this.glControlContainer.ResumeLayout();

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

        private void ControllerDraw(MatrixCalc mc, long time)
        {
            ((GLMatrixCalcUniformBlock)items.UB("MCUB")).Set(gl3dcontroller.MatrixCalc);        // set the matrix unform block to the controller 3d matrix calc.

            if ( galaxy != null )
                galaxy.InstanceCount = volumetricblock.Set(gl3dcontroller.MatrixCalc, boundingbox, 50.0f);        // set up the volumentric uniform

            rObjects.Render(gl3dcontroller.MatrixCalc);


            this.Text = "Looking at " + gl3dcontroller.MatrixCalc.TargetPosition + " eye@ " + gl3dcontroller.MatrixCalc.EyePosition + " dir " + gl3dcontroller.Camera.Current + " Dist " + gl3dcontroller.MatrixCalc.EyeDistance + " Zoom " + gl3dcontroller.Zoom.Current;
        }

        public class GLFixedShader : GLShaderPipeline
        {
            public GLFixedShader(Color c, Action<IGLProgramShader> action = null) : base(action)
            {
                AddVertexFragment(new GLVertexShaderNoTranslation(), new GLFragmentShaderFixedColour(c));
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Closed += ShaderTest_Closed;

            gl3dcontroller.MatrixCalc.PerspectiveNearZDistance = 1f;
            gl3dcontroller.MatrixCalc.PerspectiveFarZDistance = 500000f;
            gl3dcontroller.ZoomDistance = 5000F;
            gl3dcontroller.EliteMovement = true;

            gl3dcontroller.KeyboardTravelSpeed = (ms) =>
            {
                return (float)ms * 10.0f;
            };

            gl3dcontroller.MatrixCalc.InPerspectiveMode = true;
            gl3dcontroller.Start(new Vector3(0, 0,10000), new Vector3(140.75f, 0, 0), 0.5F);

            items.Add("MCUB", new GLMatrixCalcUniformBlock());     // create a matrix uniform block 

            int front = -20000, back = front + 90000, left = -45000, right = left + 90000, vsize = 2000;
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

            {
                items.Add("LINEYELLOW", new GLFixedShader(System.Drawing.Color.Yellow, (a) => { GLStatics.LineWidth(1); }));
                rObjects.Add(items.Shader("LINEYELLOW"),
                GLRenderableItem.CreateVector4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Lines, displaylines));
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

                GLShaderPipeline numshaderx = new GLShaderPipeline(new GLVertexShaderTextureMatrixTranslation(), new GLFragmentShaderTexture2DIndexed(0));
                items.Add("IC-X", numshaderx);
                numshaderx.StartAction += (s) => { items.Tex("Nums").Bind(1); GL.Disable(EnableCap.CullFace); };
                numshaderx.FinishAction += (s) => { GL.Enable(EnableCap.CullFace); };

                rObjects.Add(numshaderx, "xnum",
                                        GLRenderableItem.CreateVector4Vector2Matrix4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Quads,
                                                GLShapeObjectFactory.CreateQuad(500.0f), GLShapeObjectFactory.TexQuad, numberposx,
                                                null, numberposx.Length));

                Matrix4[] numberposz = new Matrix4[(back - front) / 1000 + 1];
                for (int i = 0; i < numberposz.Length; i++)
                {
                    numberposz[i] = Matrix4.CreateScale(1);
                    numberposz[i] *= Matrix4.CreateRotationX(-25f.Radians());
                    numberposz[i] *= Matrix4.CreateTranslation(new Vector3(right + 1000, 0, front + 1000 * i));
                }

                GLShaderPipeline numshaderz = new GLShaderPipeline(new GLVertexShaderTextureMatrixTranslation(), new GLFragmentShaderTexture2DIndexed(25));
                items.Add("IC-Z", numshaderz);
                numshaderz.StartAction += (s) => { items.Tex("Nums").Bind(1); GL.Disable(EnableCap.CullFace); };
                numshaderz.FinishAction += (s) => { GL.Enable(EnableCap.CullFace); };

                rObjects.Add(numshaderz, "ynum",
                                        GLRenderableItem.CreateVector4Vector2Matrix4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Quads,
                                                GLShapeObjectFactory.CreateQuad(500.0f), GLShapeObjectFactory.TexQuad, numberposz,
                                                null, numberposz.Length));
            }

            {
                items.Add("solmarker", new GLTexture2D(numbitmaps[45]));
                items.Add("TEX", new GLTexturedObjectShaderSimple((a) => { GLStatics.CullFace(false); }, (b)=> { GLStatics.DefaultCullFace(); }));
                rObjects.Add(items.Shader("TEX"),
                             GLRenderableItem.CreateVector4Vector2(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Quads,
                             GLShapeObjectFactory.CreateQuad(1000.0f, 1000.0f, new Vector3(0, 0, 0)), GLShapeObjectFactory.TexQuad,
                             new GLObjectDataTranslationRotationTexture(items.Tex("solmarker"), new Vector3(0, 1000, 0))
                             ));
                rObjects.Add(items.Shader("TEX"),
                             GLRenderableItem.CreateVector4Vector2(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Quads,
                             GLShapeObjectFactory.CreateQuad(1000.0f, 1000.0f, new Vector3(0, 0, 0)), GLShapeObjectFactory.TexQuad,
                             new GLObjectDataTranslationRotationTexture(items.Tex("solmarker"), new Vector3(0, -1000, 0))
                             ));
                items.Add("sag", new GLTexture2D(Properties.Resources.dotted));
                rObjects.Add(items.Shader("TEX"),
                             GLRenderableItem.CreateVector4Vector2(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Quads,
                             GLShapeObjectFactory.CreateQuad(1000.0f, 1000.0f, new Vector3(0, 0, 0)), GLShapeObjectFactory.TexQuad,
                             new GLObjectDataTranslationRotationTexture(items.Tex("sag"), new Vector3(25.2f, 2000, 25899))
                             ));
                rObjects.Add(items.Shader("TEX"),
                             GLRenderableItem.CreateVector4Vector2(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Quads,
                             GLShapeObjectFactory.CreateQuad(1000.0f, 1000.0f, new Vector3(0, 0, 0)), GLShapeObjectFactory.TexQuad,
                             new GLObjectDataTranslationRotationTexture(items.Tex("sag"), new Vector3(25.2f, -2000, 25899))
                             ));
                items.Add("bp", new GLTexture2D(Properties.Resources.dotted2));
                rObjects.Add(items.Shader("TEX"),
                             GLRenderableItem.CreateVector4Vector2(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Quads,
                             GLShapeObjectFactory.CreateQuad(1000.0f, 1000.0f, new Vector3(0, 0, 0)), GLShapeObjectFactory.TexQuad,
                             new GLObjectDataTranslationRotationTexture(items.Tex("bp"), new Vector3(-1111f, 0, 65269))
                             ));
            }


            items.Add("COS-1L", new GLColourObjectShaderNoTranslation((a) => { GLStatics.LineWidth(1); }));

            float h = -1;
            if ( h != -1)
            {
                int dist = 1000;
                Color cr = Color.FromArgb(20, Color.Red);
                rObjects.Add(items.Shader("COS-1L"),    // horizontal
                             GLRenderableItem.CreateVector4Color4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Lines,
                                                        GLShapeObjectFactory.CreateLines(new Vector3(left, h, front), new Vector3(left, h, back), new Vector3(dist, 0, 0), (back-front)/dist+1),
                                                        new Color4[] { cr })
                                   );

                rObjects.Add(items.Shader("COS-1L"),  
                             GLRenderableItem.CreateVector4Color4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Lines,
                                                        GLShapeObjectFactory.CreateLines(new Vector3(left, h, front), new Vector3(right, h, front), new Vector3(0, 0,dist), (right-left)/dist+1),
                                                        new Color4[] { cr })
                                   );

            }

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

                //float[] gdata = gaussiantex.GetTextureImageAsFloats(OpenTK.Graphics.OpenGL4.PixelFormat.Red); // read back check
                //for( int i = 0; i < gdata.Length; i++  )
                //{
                //    double v = ((float)i / gdata.Length-0.5)*2*2;
                //    double r = ObjectExtensionsNumbersBool.GaussianDist(v, 2, 1.4);
                // //   System.Diagnostics.Debug.WriteLine(i + ":" + gdata[i] + ": " +  r);
                //}

                // load one upside down and horz flipped, because the volumetric co-ords are 0,0,0 bottom left, 1,1,1 top right
                GLTexture2D galtex = new GLTexture2D(Properties.Resources.Galaxy_L180);
                items.Add("gal", galtex);
                GalaxyShader gs = new GalaxyShader();
                items.Add("Galaxy", gs);
                gs.StartAction = (a) => { galtex.Bind(1); noise3d.Bind(3); gaussiantex.Bind(4); };

                galaxy = GLRenderableItem.CreateNullVertex(OpenTK.Graphics.OpenGL4.PrimitiveType.Points);   // no vertexes, all data from bound volumetric uniform, no instances as yet
                rObjects.Add(items.Shader("Galaxy"), galaxy);
            }

            if ( true)
            {
                int gran = 8;
                Bitmap img = Properties.Resources.Galaxy_L180;
                Bitmap heat = img.Function(img.Width / gran, img.Height / gran, mode: BitMapHelpers.BitmapFunction.HeatMap);
                heat.Save(@"c:\code\heatmap.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

                if (false)
                {
                    List<Vector4> points = new List<Vector4>();

                    int xcw = (right - left)/heat.Width;
                    int zch = (back - front)/heat.Height;

                    for ( int x = 0; x < heat.Width; x++ )
                    {
                        for( int z = 0; z < heat.Height; z++ )
                        {
                            int gx = left + x * xcw + xcw/2;
                            int gz = front + z * zch + zch/2;
                            Color px = heat.GetPixel(x, z);
                            //System.Diagnostics.Debug.WriteLine(x + "," + z + " = " + gx + "," + gz + " : " + px.R);
                            points.Add(new Vector4(gx, 2000, gz, px.R));
                        }
                    }

                    items.Add("ShP", new GLHeatMapIntensity());
                    rObjects.Add(items.Shader("ShP"),
                                 GLRenderableItem.CreateVector4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Points, points.ToArray()));
                }

                if (true)
                {
                    List<Vector4> points = new List<Vector4>();
                    Random rnd = new Random(23);

                    int xcw = (right - left) / heat.Width;
                    int zch = (back - front) / heat.Height;

                    for (int x = 0; x < heat.Width; x++)
                    {
                        for (int z = 0; z < heat.Height; z++)
                        {
                            int i = heat.GetPixel(x, z).R;
                            int ii = i * i * i;
                            if ( ii > 32*32*32)
                            {
                                int gx = left + x * xcw;
                                int gz = front + z * zch;

                                float dx = (float)Math.Abs(gx) / 45000;
                                float dz = (float)Math.Abs(25889-gz) / 45000;
                                double d = Math.Sqrt(dx * dx + dz * dz);     // 0 - 0.1412
                                d = 1 - d;  // 1 = centre, 0 = unit circle
                                d = d * 2 - 1;  // -1 to +1
                                double dist = ObjectExtensionsNumbersBool.GaussianDist(d, 1, 1.4);

                                int c = Math.Min(Math.Max(ii / 100000, 0), 20);

                                dist *= 2000;

                                var rs = GLPointsFactory.RandomStars4(c, gx, gx + xcw, gz, gz + zch, (int)dist, (int)-dist, rnd, w:i);

                                if ( z == heat.Height/2 )
                                    System.Diagnostics.Debug.WriteLine(gx + "," + gz + "; " + dx + "," + dz + " = " + i + " " + c + " " + d + "=h " + dist);
                                    //var rs = GLPointsFactory.RandomStars4(500, gx, gx + xcw, gz, gz + zch, (int)dist, (int)-dist, rnd, w:i);
                                points.AddRange(rs);
                            }
                        }
                    }

                    items.Add("SD", new GLStarDots());
                    rObjects.Add(items.Shader("SD"),
                                 GLRenderableItem.CreateVector4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Points, points.ToArray()));
                }

            }

            if ( false )
            {
                items.Add("lensflare", new GLTexture2D(Properties.Resources.star_grey64));
                items.Add("PS1", new GLPointSprite(items.Tex("lensflare")));
                int dist = 10000;
                var p = GLPointsFactory.RandomStars4(100, -dist, dist, 25899-dist, 25899+dist, 2000, -2000);

                rObjects.Add(items.Shader("PS1"),
                             GLRenderableItem.CreateVector4Color4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Points,
                                   p, new Color4[] { Color.White }));

            }
        }


        public class GalaxyFragmentPipeline : GLShaderPipelineShadersBase
        {
            string fcode = @"
#version 450 core
out vec4 color;

in vec3 vs_texcoord;

layout (binding=1) uniform sampler2D tex;
layout (binding=3) uniform sampler3D noise;     
layout (binding=4) uniform sampler1D gaussian;  

void main(void)
{
//color = texture(tex,vec2(vs_texcoord.x,vs_texcoord.z));     
//color = vec4(vs_texcoord.x, vs_texcoord.z,0,0);
//color.w = 0.5;
//color = vec4(vs_texcoord.x,vs_texcoord.y,vs_texcoord.z,1);

    float dx = abs(0.5-vs_texcoord.x);
    float dz = abs(0.5-vs_texcoord.z);
    float d = 0.7073-sqrt(dx*dx+dz*dz);     // 0 - 0.7
    d = d / 0.7073; // 0.707 is the unit circle, 1 is the max at corners

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

            public GalaxyFragmentPipeline()
            {
                Program = GLProgram.CompileLink(OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader, fcode);
            }
        }


        public class GalaxyShader : GLShaderPipeline
        {
            public GalaxyShader()
            {
                Add(new GLVertexShaderVolumetric(), OpenTK.Graphics.OpenGL4.ShaderType.VertexShader);
                Add(new GLGeometricShaderVolumetric(), OpenTK.Graphics.OpenGL4.ShaderType.GeometryShader);
                Add(new GalaxyFragmentPipeline(), OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader);
            }
        }

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
    float sf = 120-d/80;

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
            public GLPointSprite(IGLTexture tex) : base()
            {
                StartAction = (a) =>
                {
                    tex.Bind(4);
                    GLStatics.EnablePointSprite();
                    GLStatics.PointSizeByProgram();
                    GLStatics.DepthTest(false);
                    //GL.Enable(EnableCap.Blend);
                    //GL.BlendFunc(BlendingFactor.One, BlendingFactor.One);
                    //GL.BlendFunc(BlendingFactor.Src1Alpha, BlendingFactor.OneMinusDstAlpha);
                };

                FinishAction = (a) =>
                {
                    GLStatics.DisablePointSprite();
                    GLStatics.DefaultDepthTest();
                    //GL.Disable(EnableCap.Blend);
                };

                CompileLink(vert, frag: frag);
            }


        }


        public class GLHeatMapIntensity: GLShaderStandard
        {
            string vert =
@"
#version 450 core

#include OpenTKUtils.GL4.UniformStorageBlocks.matrixcalc.glsl

layout (location = 0) in vec4 position;     // has w=1
out vec4 vs_color;

void main(void)
{
    vec4 p = position;
    float intensity = position.w;
    p.w = 1;

    gl_Position = mc.ProjectionModelMatrix * p;        // order important
    gl_PointSize = clamp(intensity/18,1,16);
gl_PointSize = 1;

    if ( intensity>128)
        vs_color = vec4(intensity,0,0,1);
    else if ( intensity>64)
        vs_color = vec4(0,0,intensity,1);
    else if ( intensity>32)
        vs_color = vec4(0,intensity,intensity,1);
    else if ( intensity>16)
        vs_color = vec4(intensity,intensity,0,1);
    else if ( intensity>5)
        vs_color = vec4(0,intensity,0,1);
    else
        vs_color = vec4(0,0,0,0);
}
";
            string frag =
@"
#version 450 core

in vec4 vs_color;
out vec4 color;

void main(void)
{
    if ( vs_color.w>0)
        color = vs_color;
    else
        discard;
}
";
            public GLHeatMapIntensity() : base()
            {
                CompileLink(vert, frag: frag);
                StartAction = (a) => { GLStatics.PointSizeByProgram(); GLStatics.DepthTest(false); };
                FinishAction = (a) => { GLStatics.DefaultPointSize(); GLStatics.DefaultDepthTest();   };

            }
        }


        public class GLStarDots: GLShaderStandard
        {
            string vert =
@"
#version 450 core

#include OpenTKUtils.GL4.UniformStorageBlocks.matrixcalc.glsl

layout (location = 0) in vec4 position;     // has w=1
out vec4 vs_color;

void main(void)
{
    vec4 p = position;
    p.w = 1;
    gl_Position = mc.ProjectionModelMatrix * p;        // order important
    vs_color = vec4(position.w,position.w,position.w,0.1);
}
";
            string frag =
@"
#version 450 core

in vec4 vs_color;
out vec4 color;

void main(void)
{
    color = vs_color;
}
";
            public GLStarDots() : base()
            {
                CompileLink(vert, frag: frag);
                StartAction = (a) => { GLStatics.PointSize(1); GLStatics.DepthTest(false); };
                FinishAction = (a) => { GLStatics.DefaultPointSize(); GLStatics.DefaultDepthTest(); };
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


