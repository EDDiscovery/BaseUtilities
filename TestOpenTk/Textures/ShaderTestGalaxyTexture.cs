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
    public partial class ShaderTestGalaxyTexture : Form
    {
        private OpenTKUtils.WinForm.GLWinFormControl glwfc;
        private Controller3D gl3dcontroller;

        private Timer systemtimer = new Timer();

        public ShaderTestGalaxyTexture()
        {
            InitializeComponent();

            glwfc = new OpenTKUtils.WinForm.GLWinFormControl(glControlContainer);

            systemtimer.Interval = 25;
            systemtimer.Tick += new EventHandler(SystemTick);
            systemtimer.Start();
        }

        GLRenderProgramSortedList rObjects = new GLRenderProgramSortedList();
        GLItemsList items = new GLItemsList();


        public class GLGalShader : GLShaderStandard
        {
            string vert =
@"
#version 450 core
#include OpenTKUtils.GL4.UniformStorageBlocks.matrixcalc.glsl
layout (location = 0) in vec4 position;
out gl_PerVertex {
        vec4 gl_Position;
        float gl_PointSize;
        float gl_ClipDistance[];
    };

layout(location = 1) in vec2 texco;

layout(location = 0) out vec2 vs_textureCoordinate;

layout (location = 22) uniform  mat4 transform;

void main(void)
{
	gl_Position = mc.ProjectionModelMatrix * transform * position;        // order important
    vs_textureCoordinate = texco;
}
";
            string frag =
@"
#version 450 core
layout (location=0) in vec2 vs_textureCoordinate;
layout (binding=1) uniform sampler2D textureObject;
out vec4 color;

void main(void)
{
    color = texture(textureObject, vs_textureCoordinate) * vec4(1,1,1,0.8);     
}
";
            public GLGalShader() : base()
            {
                CompileLink(vert, frag: frag);
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
                return (float)ms / 100.0f;
            };


            items.Add( new GLColourShaderWithWorldCoord(), "COSW");
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

            items.Add(new GLTexture2D(Properties.Resources.galheightmap7), "gal");

            items.Add(new GLGalShader(), "TEX-NC");

            GLRenderControl rg = GLRenderControl.Quads(cullface: false);

            rObjects.Add(items.Shader("TEX-NC"),
                        GLRenderableItem.CreateVector4Vector2(items, rg,
                        GLShapeObjectFactory.CreateQuad(200.0f, 200.0f, new Vector3(0, 0, 0)), GLShapeObjectFactory.TexQuad,
                        new GLRenderDataTranslationRotationTexture(items.Tex("gal"), new Vector3(0, 0, 0))
                        ));

            items.Add( new GLMatrixCalcUniformBlock(), "MCUB");     // create a matrix uniform block 

        }

        private void ShaderTest_Closed(object sender, EventArgs e)
        {
            items.Dispose();
        }

        private void ControllerDraw(GLMatrixCalc mc, long time)
        {
            ((GLMatrixCalcUniformBlock)items.UB("MCUB")).Set(gl3dcontroller.MatrixCalc);        // set the matrix unform block to the controller 3d matrix calc.

            rObjects.Render(glwfc.RenderState, gl3dcontroller.MatrixCalc);

            this.Text = "Looking at " + gl3dcontroller.MatrixCalc.TargetPosition + " dir " + gl3dcontroller.Camera.Current + " eye@ " + gl3dcontroller.MatrixCalc.EyePosition + " Dist " + gl3dcontroller.MatrixCalc.EyeDistance;
        }

        private void SystemTick(object sender, EventArgs e )
        {
            gl3dcontroller.HandleKeyboardSlews(true, OtherKeys);
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


