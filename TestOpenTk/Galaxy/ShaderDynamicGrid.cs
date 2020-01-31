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
    public partial class ShaderDynamicGrid : Form
    {
        private OpenTKUtils.WinForm.GLWinFormControl glwfc;
        private Controller3D gl3dcontroller;

        private Timer systemtimer = new Timer();

        public ShaderDynamicGrid()
        {
            InitializeComponent();

            glwfc = new OpenTKUtils.WinForm.GLWinFormControl(glControlContainer);

            systemtimer.Interval = 25;
            systemtimer.Tick += new EventHandler(SystemTick);
            systemtimer.Start();
        }

        GLRenderProgramSortedList rObjects = new GLRenderProgramSortedList();
        GLItemsList items = new GLItemsList();

        /// ////////////////////////////////////////////////////////////////////////////////////////////////////


        private void ShaderTest_Closed(object sender, EventArgs e)
        {
            items.Dispose();
        }

        private void ControllerDraw(GLMatrixCalc mc, long time)
        {
            ((GLMatrixCalcUniformBlock)items.UB("MCUB")).Set(gl3dcontroller.MatrixCalc);        // set the matrix unform block to the controller 3d matrix calc.

            int lines = 82;
            if (gl3dcontroller.MatrixCalc.EyeDistance < 200)
            {
                lines = 164*2;
            }
            else if (gl3dcontroller.MatrixCalc.EyeDistance < 1000)
            {
                lines = 164;
            }

            //IGLShader s = items.Shader("DYNGRIDCourse");
            IGLShader s = items.PLShader("PLGRIDShaderCourse");

            GL.ProgramUniform1(s.Id, 25, lines);
            GLStatics.Check();

            IGLRenderableItem i = rObjects["DYNGRIDRENDER"];
            i.InstanceCount = lines;

            rObjects.Render(glwfc.RenderState, gl3dcontroller.MatrixCalc);

            this.Text = "Looking at " + gl3dcontroller.MatrixCalc.TargetPosition + " eye@ " + gl3dcontroller.MatrixCalc.EyePosition + " dir " + gl3dcontroller.Camera.Current + " Dist " + gl3dcontroller.MatrixCalc.EyeDistance + " Zoom " + gl3dcontroller.Zoom.Current;
        }

        public class DynamicGridShader : GLShaderPipelineShadersBase
        {


            string vcode(Color c)
            { return @"
#version 450 core
" + GLShader.CreateVars(new object[] { "color" , c}) + @"

layout (location = 25) uniform int lines;

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

    int fadeinmajor  = 1000;
    int fadeinminor  = 200;

    int gridwidth = 1000;

    if ( dist < fadeinminor )
    {
        gridwidth = 10;
    }
    else if ( dist < fadeinmajor )
    {
        gridwidth = 100;
    }

    int horzlines = lines/2;
    int gridstart = (horzlines-1)/2*gridwidth;

    vec4 position;
    int sx = int(mc.TargetPosition.x)/gridwidth*gridwidth-gridstart;
    int sz = int(mc.TargetPosition.z)/gridwidth*gridwidth-gridstart;
    int sy = int(mc.TargetPosition.y);
    int width = (horzlines-1)*gridwidth;

    int lpos;

    if ( line>= horzlines) 
    {
        line -= horzlines;
        lpos = sx + line * gridwidth;
        position = vec4( lpos , sy, sz + width * linemod, 1);
    }
    else    
    {
        lpos = sz + gridwidth * line;
        position = vec4( sx + width * linemod, sy, lpos , 1);
    }

    gl_Position = mc.ProjectionModelMatrix * position;        // order important

    float fade=1;

    if ( dist > fadeinmajor )
    {
        fade = 1.0 - clamp((dist -fadeinmajor)/(fadeinmajor*2),0,1);
    }
    else if ( dist > fadeinminor )
    {
        if ( abs(lpos) % 1000 == 0 )
            fade = 1;
        else
            fade =  0.7 - clamp((dist-fadeinminor)/(fadeinmajor-fadeinminor),0,1)*0.7;
    }
    else 
    {
        if ( abs(lpos) % 100 == 0 )
            fade = 1;
        else
            fade = 0.7 - clamp((dist)/(fadeinminor),0,1)*0.7;
    }

    //float cpos = 0.5-abs((float(line)/horzlines)-0.5);

    //float fade = 0.1+cpos*1.7;

    //float c1 = 1.0;

    //if ( mc.EyeDistance>fadein)
    //    c1 = 
    //else if ( mc.EyeDistance < fadeout )
    //    c1 = 1.0 - clamp((fadeout - mc.EyeDistance)/(fadeout),0,1);
    //fade *= c1;
    vs_color = vec4(color.x,color.y,color.z,fade);
}
"; }

            public DynamicGridShader(Color c)
            {
                CompileLink(OpenTK.Graphics.OpenGL4.ShaderType.VertexShader, vcode(c));
            }

            public override void Start()
            {
            }
        }

        public class GLFixedShader : GLShaderPipeline
        {
            public GLFixedShader(Color c, Action<IGLProgramShader> action = null) : base(action)
            {
                AddVertexFragment(new GLPLVertexShaderWorldCoord(), new GLPLFragmentShaderFixedColour(c));
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Closed += ShaderTest_Closed;

            gl3dcontroller = new Controller3D();
            gl3dcontroller.MatrixCalc.PerspectiveNearZDistance = 1f;
            gl3dcontroller.MatrixCalc.PerspectiveFarZDistance = 500000f;
            gl3dcontroller.ZoomDistance = 5000F;
            gl3dcontroller.EliteMovement = true;
            gl3dcontroller.PaintObjects = ControllerDraw;

            gl3dcontroller.KeyboardTravelSpeed = (ms) =>
            {
                return (float)ms * 1.0f;
            };

            gl3dcontroller.Zoom.ZoomFact = 1.1f;

            gl3dcontroller.MatrixCalc.InPerspectiveMode = true;
            gl3dcontroller.Start(glwfc, new Vector3(0, 0, 10000), new Vector3(140.75f, 0, 0), 0.5F);

            items.Add("MCUB", new GLMatrixCalcUniformBlock());     // create a matrix uniform block 

            int front = -20000, back = front + 90000, left = -45000, right = left + 90000, vsize = 2000;
 
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
                items.Add("LINEYELLOW", new GLFixedShader(System.Drawing.Color.Yellow));
                GLRenderControl rl = GLRenderControl.Lines(1);
                rObjects.Add(items.Shader("LINEYELLOW"), GLRenderableItem.CreateVector4(items, rl, displaylines));
            }


            {
                items.Add("PLGRIDShaderCourse", new DynamicGridShader(Color.Yellow));
                items.Add("PLShaderColour", new GLPLFragmentShaderColour());

                GLRenderControl rl = GLRenderControl.Lines(1);
                rl.DepthTest = false;

            //  items.Add("DYNGRIDFine", new GLShaderPipeline(items.PLShader("PLGRIDShaderFine"), items.PLShader("PLShaderColour")));
              //  rObjects.Add(items.Shader("DYNGRIDFine"), GLRenderableItem.CreateNullVertex(rl, dc: 2, ic: 82));

                items.Add("DYNGRIDCourse", new GLShaderPipeline(items.PLShader("PLGRIDShaderCourse"), items.PLShader("PLShaderColour")));
                rObjects.Add(items.Shader("DYNGRIDCourse"), "DYNGRIDRENDER", GLRenderableItem.CreateNullVertex(rl, dc: 2));
                
            }

















            //Bitmap[] numbitmaps = new Bitmap[116];

            //{
            //    Font fnt = new Font("Arial", 20);
            //    for (int i = 0; i < numbitmaps.Length; i++)
            //    {
            //        int v = -45000 + 1000 * i;      // range from -45000 to +70000 
            //        numbitmaps[i] = new Bitmap(100, 100);
            //        BaseUtils.BitMapHelpers.DrawTextCentreIntoBitmap(ref numbitmaps[i], v.ToString(), fnt, Color.Red, Color.AliceBlue);
            //    }

            //    GLTexture2DArray numtextures = new GLTexture2DArray(numbitmaps, ownbitmaps: true);
            //    items.Add("Nums", numtextures);

            //    Matrix4[] numberposx = new Matrix4[(right - left) / 1000 + 1];
            //    for (int i = 0; i < numberposx.Length; i++)
            //    {
            //        numberposx[i] = Matrix4.CreateScale(1);
            //        numberposx[i] *= Matrix4.CreateRotationX(-25f.Radians());
            //        numberposx[i] *= Matrix4.CreateTranslation(new Vector3(left + 1000 * i, 0, front));
            //    }

            //    GLShaderPipeline numshaderx = new GLShaderPipeline(new GLPLVertexShaderTextureModelCoordWithMatrixTranslation(), new GLPLFragmentShaderTexture2DIndexed(0));
            //    items.Add("IC-X", numshaderx);

            //    GLRenderControl rq = GLRenderControl.Quads(cullface: false);
            //    GLRenderDataTexture rt = new GLRenderDataTexture(items.Tex("Nums"));

            //    rObjects.Add(numshaderx, "xnum",
            //                            GLRenderableItem.CreateVector4Vector2Matrix4(items, rq,
            //                                    GLShapeObjectFactory.CreateQuad(500.0f), GLShapeObjectFactory.TexQuad, numberposx,
            //                                    rt, numberposx.Length));

            //    Matrix4[] numberposz = new Matrix4[(back - front) / 1000 + 1];
            //    for (int i = 0; i < numberposz.Length; i++)
            //    {
            //        numberposz[i] = Matrix4.CreateScale(1);
            //        numberposz[i] *= Matrix4.CreateRotationX(-25f.Radians());
            //        numberposz[i] *= Matrix4.CreateTranslation(new Vector3(right + 1000, 0, front + 1000 * i));
            //    }

            //    GLShaderPipeline numshaderz = new GLShaderPipeline(new GLPLVertexShaderTextureModelCoordWithMatrixTranslation(), new GLPLFragmentShaderTexture2DIndexed(25));
            //    items.Add("IC-Z", numshaderz);

            //    rObjects.Add(numshaderz, "ynum",
            //                            GLRenderableItem.CreateVector4Vector2Matrix4(items, rq,
            //                                    GLShapeObjectFactory.CreateQuad(500.0f), GLShapeObjectFactory.TexQuad, numberposz,
            //                                    rt, numberposz.Length));
            //}




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
                System.Diagnostics.Debug.WriteLine("Redraw {0} ms per {1}", tickcount, (float)tickcount / (float)times);
            }
        }


    }
}


