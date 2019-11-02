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
        private Controller3D gl3dcontroller = new Controller3D();

        private Timer systemtimer = new Timer();

        public ShaderTestPointSprites()
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
            public GLPointSprite(IGLTexture tex) : base()
            {
                StartAction = (a) => 
                {
                    tex.Bind(4);
                    GLStatics.EnablePointSprite();
                    GLStatics.PointSizeByProgram();
                    GL.Enable(EnableCap.Blend);
                    //GL.BlendFunc(BlendingFactor.One, BlendingFactor.One);
                    //GL.BlendFunc(BlendingFactor.Src1Alpha, BlendingFactor.OneMinusDstAlpha);
                };

                FinishAction = (a) => 
                {
                    GLStatics.DisablePointSprite();
                    GL.Disable(EnableCap.Blend);
                };

                CompileLink(vert,frag:frag);
            }

           
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Closed += ShaderTest_Closed;

            gl3dcontroller.MatrixCalc.PerspectiveNearZDistance = 0.1f;
            gl3dcontroller.ZoomDistance = 20F;
            gl3dcontroller.Start(new Vector3(0, 0, 0), new Vector3(110f, 0, 0f), 1F);

            gl3dcontroller.KeyboardTravelSpeed = (ms) =>
            {
                return (float)ms / 50.0f;
            };

            //items.Add("lensflarewhite", new GLTexture2D(Properties.Resources.lensflare_white64));
            items.Add("lensflare", new GLTexture2D(Properties.Resources.star_grey64));

            items.Add("COS-1L", new GLColourObjectShaderNoTranslation((a) => { GLStatics.LineWidth(1); }));

            items.Add("PS1", new GLPointSprite(items.Tex("lensflare")));

            #region coloured lines

            rObjects.Add(items.Shader("COS-1L"),    // horizontal
                         GLRenderableItem.CreateVector4Color4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Lines,
                                                    GLShapeObjectFactory.CreateLines(new Vector3(-100, 0, -100), new Vector3(-100, 0, 100), new Vector3(10, 0, 0), 21),
                                                    new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green })
                               );


            rObjects.Add(items.Shader("COS-1L"),    // vertical
                         GLRenderableItem.CreateVector4Color4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Lines,
                               GLShapeObjectFactory.CreateLines(new Vector3(-100, 0, -100), new Vector3(100, 0, -100), new Vector3(0, 0, 10), 21),
                                                         new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green })
                               );


            var p = GLPointsFactory.RandomStars4(100, 23, -100, 100, 100, -100, 100, -100);
            //p = new Vector4[10];
            //for( int i = 0; i < 10; i++)
            //{
            //    p[i] = new Vector4(i, 6.8f, 0,1);
            //}
            rObjects.Add(items.Shader("PS1"),
                         GLRenderableItem.CreateVector4Color4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Points,
                               p, new Color4[] { Color.Red, Color.Yellow, Color.Green }));

            #endregion

            items.Add("MCUB", new GLMatrixCalcUniformBlock());     // create a matrix uniform block 

        }

        private void ShaderTest_Closed(object sender, EventArgs e)
        {
            items.Dispose();
        }

        private void ControllerDraw(MatrixCalc mc, long time)
        {
            ((GLMatrixCalcUniformBlock)items.UB("MCUB")).Set(gl3dcontroller.MatrixCalc);        // set the matrix unform block to the controller 3d matrix calc.

            rObjects.Render(gl3dcontroller.MatrixCalc);

            this.Text = "Looking at " + gl3dcontroller.MatrixCalc.TargetPosition + " dir " + gl3dcontroller.Camera.Current + " eye@ " + gl3dcontroller.MatrixCalc.EyePosition + " Dist " + gl3dcontroller.MatrixCalc.EyeDistance;
        }

        private void SystemTick(object sender, EventArgs e )
        {
            gl3dcontroller.HandleKeyboard(true, OtherKeys);
            gl3dcontroller.Redraw();
        }

        private void OtherKeys( BaseUtils.KeyboardState kb )
        {
            if (kb.IsPressedRemove(Keys.F1, BaseUtils.KeyboardState.ShiftState.None))
            {
                gl3dcontroller.CameraLookAt(new Vector3(0, 0, 0), 1, 2);
            }

            if (kb.IsPressedRemove(Keys.F2, BaseUtils.KeyboardState.ShiftState.None))
            {
                gl3dcontroller.CameraLookAt(new Vector3(4, 0, 0), 1, 2);
            }

            if (kb.IsPressedRemove(Keys.F3, BaseUtils.KeyboardState.ShiftState.None))
            {
                gl3dcontroller.CameraLookAt(new Vector3(10, 0, -10), 1, 2);
            }

            if (kb.IsPressedRemove(Keys.F4, BaseUtils.KeyboardState.ShiftState.None))
            {
                gl3dcontroller.CameraLookAt(new Vector3(50, 0, 50), 1, 2);
            }

        }

    }



}


