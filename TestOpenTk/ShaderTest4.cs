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
    public partial class ShaderTest4 : Form
    {
        private Controller3D gl3dcontroller = new Controller3D();

        private Timer systemtimer = new Timer();

        public ShaderTest4()
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

        // Demonstrate buffer feedback AND geo shader add vertex/dump vertex

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Closed += ShaderTest_Closed;

            gl3dcontroller.MatrixCalc.ZoomDistance = 100F;
            gl3dcontroller.MatrixCalc.PerspectiveNearZDistance = 0.1f;
            gl3dcontroller.Start(new Vector3(0, 0, 0), new Vector3(170f, 0, 0f), 1F);

            gl3dcontroller.TravelSpeed = (ms) =>
            {
                return (float)ms / 20.0f;
            };

            items.Add("Shader", new GLMultipleTexturedBlended(false));

            items.Shader("Shader").StartAction += (s) => 
            {
                items.Tex("Hello").Bind(1);
            };

            using (var bmp = BaseUtils.BitMapHelpers.DrawTextIntoAutoSizedBitmap("Hello", new Size(200, 100), new Font("Arial", 10.0f), Color.Yellow, Color.Blue))
            {
                items.Add("Hello", new GLTexture2D(bmp));
            }

            var b1 = items.NewBuffer();
            var b2 = items.NewBuffer();
            var b3 = items.NewBuffer();
            var b4 = items.NewBuffer();

            Vector4[] instancepositions = new Vector4[4];
            //instancepositions[0] = new Vector4(2, 0, 2, 0);
            //instancepositions[1] = new Vector4(-2, 0, 2, 0);
            //instancepositions[2] = new Vector4(-2, 0, -2, 0);
            //instancepositions[3] = new Vector4(2, 0, -2, 0);            
            instancepositions[0] = new Vector4(-25, 0, 25, 0);
            instancepositions[1] = new Vector4(-25, 0, 0, 0);
            instancepositions[2] = new Vector4(25, 0, 25, 0);
            instancepositions[3] = new Vector4(25, 0, 0, 0);

            Vector4[] instancerotations = new Vector4[4];
            instancerotations[0] = new Vector4(1, 0, 2, 0);
            instancerotations[1] = new Vector4(2, 0, 2, 0);
            instancerotations[2] = new Vector4(3, 0, -2, 0);
            instancerotations[3] = new Vector4(4, 0, -2, 0);

            rObjects.Add(items.Shader("Shader"),
                GLRenderableItem.CreateVector4Vector2InstancePosRot(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Quads,
                        GLShapeObjectFactory.CreateQuad(20.0f, 20.0f, new Vector3(-90, 0, 0)), GLShapeObjectFactory.TexQuad,
                        instancepositions, instancerotations, ic:4
                        ));

        }

        private void ShaderTest_Closed(object sender, EventArgs e)
        {
            items.Dispose();
        }

        private void ControllerDraw(Matrix4 model, Matrix4 projection, long time)
        {
           // System.Diagnostics.Debug.WriteLine("Draw eye " + gl3dcontroller.MatrixCalc.EyePosition + " to " + gl3dcontroller.Pos.Current);

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


