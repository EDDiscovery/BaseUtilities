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
    public partial class ShaderTestGalaxy : Form
    {
        private Controller3D gl3dcontroller = new Controller3D();

        private Timer systemtimer = new Timer();

        public ShaderTestGalaxy()
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

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Closed += ShaderTest_Closed;

            gl3dcontroller.MatrixCalc.PerspectiveNearZDistance = 0.1f;
            gl3dcontroller.MatrixCalc.ZoomDistance = 20F;
            gl3dcontroller.Start(new Vector3(0, 0, 0), new Vector3(110f, 0, 0f), 1F);

            gl3dcontroller.TravelSpeed = (ms) =>
            {
                return (float)ms / 100.0f;
            };

            items.Add("COS-1L", new GLColourObjectShaderNoTranslation((a) => { GLStatics.LineWidth(1); }));
            items.Add("TEX", new GLTexturedObjectShaderSimple());
            items.Add("COST-FP", new GLColourObjectShaderTranslation((a) => { GLStatics4.PolygonMode(OpenTK.Graphics.OpenGL4.MaterialFace.FrontAndBack, OpenTK.Graphics.OpenGL4.PolygonMode.Fill); }));
            items.Add("COST-LP", new GLColourObjectShaderTranslation((a) => { GLStatics4.PolygonMode(OpenTK.Graphics.OpenGL4.MaterialFace.FrontAndBack, OpenTK.Graphics.OpenGL4.PolygonMode.Line); }));
            items.Add("COST-1P", new GLColourObjectShaderTranslation((a) => { GLStatics.PointSize(1.0F); }));
            items.Add("COST-2P", new GLColourObjectShaderTranslation((a) => { GLStatics.PointSize(2.0F); }));
            items.Add("COST-10P", new GLColourObjectShaderTranslation((a) => { GLStatics.PointSize(10.0F); }));
            items.Add("CROT", new GLTexturedObjectShaderTransformWithCommonTransform());

            items.Add("dotted", new GLTexture2D(Properties.Resources.dotted));
            items.Add("logo8bpp", new GLTexture2D(Properties.Resources.Logo8bpp));
            items.Add("dotted2", new GLTexture2D(Properties.Resources.dotted2));
            items.Add("wooden", new GLTexture2D(Properties.Resources.wooden));
            items.Add("shoppinglist", new GLTexture2D(Properties.Resources.shoppinglist));
            items.Add("golden", new GLTexture2D(Properties.Resources.golden));
            items.Add("smile", new GLTexture2D(Properties.Resources.smile5300_256x256x8));
            items.Add("moon", new GLTexture2D(Properties.Resources.moonmap1k));


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


