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

namespace TestOpenTk
{
    // demonstrates packed data

    public partial class ShaderTestStarPoints : Form
    {
        private Controller3D gl3dcontroller = new Controller3D();

        private Timer systemtimer = new Timer();

        public ShaderTestStarPoints()
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
                Program = GLProgram.CompileLink(OpenTK.Graphics.OpenGL4.ShaderType.VertexShader, Code(), GetType().Name);
            }

            public override void Start() // seperable do not use a program - that is for the pipeline to hook up
            {
                base.Start();
                GL.Enable(EnableCap.ProgramPointSize);
            }

            public override void Finish() // seperable do not use a program - that is for the pipeline to hook up
            {
                base.Finish();
                GL.Disable(EnableCap.ProgramPointSize);
            }

        }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            gl3dcontroller.ZoomDistance = 100F;
            gl3dcontroller.MatrixCalc.PerspectiveNearZDistance = 0.1f;
            gl3dcontroller.Start(new Vector3(0, 0, 0), new Vector3(110f, 0, 0f), 1F);

            gl3dcontroller.KeyboardTravelSpeed = (ms) =>
            {
                return (float)ms / 20.0f;
            };

            items.Add("STARS", new GLShaderPipeline(new GLShaderStars(), new GLPLFragmentShaderColour()));

            items.Add("COS", new GLColourShaderWithWorldCoord());
            items.Add("COST", new GLColourShaderWithObjectTranslation());
            items.Add("TEX", new GLTexturedShaderWithObjectTranslation());

            rObjects.Add(items.Shader("COS"), GLRenderableItem.CreateVector4Color4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Lines,
                        GLShapeObjectFactory.CreateBox(400, 200, 40, new Vector3(0, 0, 0), new Vector3(0, 0, 0)),
                        new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green }));

            Vector3[] stars = GLPointsFactory.RandomStars(10000, -200, 200, -100, 100, 20, -20);

            rObjects.Add(items.Shader("STARS"), "Stars", GLRenderableItem.CreateVector3Packed2(items,OpenTK.Graphics.OpenGL4.PrimitiveType.Points, 
                                            stars, new Vector3(50000, 50000, 50000), 16));

            using (var bmp = BaseUtils.BitMapHelpers.DrawTextIntoAutoSizedBitmap("200,100", new Size(200, 100), new Font("Arial", 10.0f), Color.Yellow, Color.Blue))
            {
                items.Add("200,100", new GLTexture2D(bmp));
            }

            using (var bmp = BaseUtils.BitMapHelpers.DrawTextIntoAutoSizedBitmap("-200,-100", new Size(200, 100), new Font("Arial", 10.0f), Color.Yellow, Color.Blue))
            {
                items.Add("-200,-100", new GLTexture2D(bmp));
            }

            rObjects.Add(items.Shader("TEX"), GLRenderableItem.CreateVector4Vector2(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Quads,
                        GLShapeObjectFactory.CreateQuad(20.0f, 20.0f, new Vector3(-90, 0, 0)), GLShapeObjectFactory.TexQuad,
                        new GLObjectDataTranslationRotationTexture(items.Tex("200,100"), new Vector3(200, 0, 100))));

            rObjects.Add(items.Shader("TEX"), GLRenderableItem.CreateVector4Vector2(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Quads,
                        GLShapeObjectFactory.CreateQuad(20.0f, 20.0f, new Vector3(-90, 0, 0)), GLShapeObjectFactory.TexQuad,
                        new GLObjectDataTranslationRotationTexture(items.Tex("-200,-100"), new Vector3(-200, 0, -100))));

            items.Add("MCUB", new GLMatrixCalcUniformBlock());     // def binding of 0

            Closed += ShaderTest_Closed;
        }

        private void ShaderTest_Closed(object sender, EventArgs e)
        {
            items.Dispose();
        }

        private void ControllerDraw(MatrixCalc mc, long time)
        {
            float degrees = ((float)time / 5000.0f * 360.0f) % 360f;
            float degreesd2 = ((float)time / 10000.0f * 360.0f) % 360f;

            GLMatrixCalcUniformBlock mcub = (GLMatrixCalcUniformBlock)items.UB("MCUB");
            mcub.Set(gl3dcontroller.MatrixCalc);

            System.Diagnostics.Debug.WriteLine("Draw eye " + gl3dcontroller.MatrixCalc.EyePosition + " to " + gl3dcontroller.Pos.Lookat);
            rObjects.Render(gl3dcontroller.MatrixCalc);
        }

        private void SystemTick(object sender, EventArgs e )
        {
            gl3dcontroller.HandleKeyboard(true, OtherKeys);
            gl3dcontroller.Redraw();
        }

        private void OtherKeys( BaseUtils.KeyboardState kb )
        {
        }

    }
}


