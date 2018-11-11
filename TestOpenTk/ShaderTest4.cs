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
            gl3dcontroller.Start(new Vector3(0, 0, 0), new Vector3(120f, 0, 0f), 1F);

            gl3dcontroller.TravelSpeed = (ms) =>
            {
                return (float)ms / 20.0f;
            };

            items.Add("ShaderPos", new GLMultipleTexturedBlended(false));
            items.Shader("ShaderPos").StartAction += (s) =>
            {
                items.Tex("Hello").Bind(1);
            };

            items.Add("ShaderMat", new GLMultipleTexturedBlended(true));
            items.Shader("ShaderMat").StartAction += (s) =>
            {
                items.Tex("Bye").Bind(1);
            };

            using (var bmp = BaseUtils.BitMapHelpers.DrawTextIntoAutoSizedBitmap("Hello", new Size(200, 100), new Font("Arial", 10.0f), Color.Yellow, Color.Blue))
            {
                items.Add("Hello", new GLTexture2D(bmp));
            }

            using (var bmp = BaseUtils.BitMapHelpers.DrawTextIntoAutoSizedBitmap("Bye", new Size(200, 100), new Font("Arial", 10.0f), Color.Yellow, Color.Green))
            {
                items.Add("Bye", new GLTexture2D(bmp));
            }

            Vector4[] instancepositions = new Vector4[4];
            instancepositions[0] = new Vector4(-25, 0, 25, 0);
            instancepositions[1] = new Vector4(-25, 0, 0, 0);
            instancepositions[2] = new Vector4(25, 0, 25, 0);
            instancepositions[3] = new Vector4(25, 0, 0, 0);

            rObjects.Add(items.Shader("ShaderPos"),
                GLRenderableItem.CreateVector4Vector2Vector4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Quads,
                        GLShapeObjectFactory.CreateQuad(20.0f, 20.0f, new Vector3(-90, 0, 0)), GLShapeObjectFactory.TexQuad,
                        instancepositions, ic: 4, separbuf: true
                        ));

            Matrix4[] pos2 = new Matrix4[3];
            pos2[0] = Matrix4.CreateRotationX(-80f.Radians());
            pos2[0] *= Matrix4.CreateTranslation(new Vector3(0, 50, 0));
            pos2[1] = Matrix4.CreateRotationX(-70f.Radians());
            pos2[1] *= Matrix4.CreateTranslation(new Vector3(0, 100, 0));
            pos2[2] = Matrix4.CreateRotationZ(-60f.Radians());
            pos2[2] *= Matrix4.CreateTranslation(new Vector3(0, 150, 0));


            rObjects.Add(items.Shader("ShaderMat"),
                GLRenderableItem.CreateVector4Vector2Matrix4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Quads,
                        GLShapeObjectFactory.CreateQuad(20.0f, 20.0f, new Vector3(-90, 0, 0)), GLShapeObjectFactory.TexQuad,
                        null, ic: 3, separbuf: true
                        ));

            matrixbuffer = items.LastBuffer(1);        // buffers created in order, so last is the instance buffer
            matrixbuffer.WriteTranslationMatrix(new Vector3(0, 50, 0));
            matrixbuffer.WriteTranslationMatrix(new Vector3(0, 100, 0));
            matrixbuffer.WriteTranslationRotationDegMatrix(new Vector3(0, 150, 0), new Vector3(90f,0,0));
            matrixbuffer.Complete();


            GLBuffer lb2 = items.LastBuffer(2);

        }

        GLBuffer matrixbuffer;

        private void ShaderTest_Closed(object sender, EventArgs e)
        {
            items.Dispose();
        }

        private void ControllerDraw(Matrix4 model, Matrix4 projection, long time)
        {
            // System.Diagnostics.Debug.WriteLine("Draw eye " + gl3dcontroller.MatrixCalc.EyePosition + " to " + gl3dcontroller.Pos.Current);

            float degrees = ((float)time / 5000.0f * 360.0f) % 360f;
            float zeroone = (degrees >= 180) ? (1.0f - (degrees - 180.0f) / 180.0f) : (degrees / 180f);

            matrixbuffer.Write(Matrix4.CreateTranslation(new Vector3(zeroone * 20, 50, 0)),0,true);

            OpenTKUtils.GLStatics.CullFace(false);
            ((GLMultipleTexturedBlended)items.Shader("ShaderPos")).CommonTransform.YRotDegrees = degrees;
            rObjects.Render(gl3dcontroller.MatrixCalc);

            this.Text = "Looking at " + gl3dcontroller.MatrixCalc.TargetPosition + " dir " + gl3dcontroller.Camera.Current + " Dist " + gl3dcontroller.MatrixCalc.EyeDistance;

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


