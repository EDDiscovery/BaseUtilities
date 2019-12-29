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
using OpenTKUtils.GL4.Controls;

// Demonstrate the volumetric calculations needed to compute a plane facing the user inside a bounding box done inside a geo shader
// this one add on tex coord calculation and using a single tight quad shows its working

namespace TestOpenTk
{
    public partial class ShaderControls : Form
    {
        private Controller3D gl3dcontroller = new Controller3D();

        private Timer systemtimer = new Timer();

        public ShaderControls()
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

        /// ////////////////////////////////////////////////////////////////////////////////////////////////////


        private void ShaderTest_Closed(object sender, EventArgs e)
        {
            items.Dispose();
        }

        private void ControllerDraw(MatrixCalc mc, long time)
        {
            ((GLMatrixCalcUniformBlock)items.UB("MCUB")).Set(gl3dcontroller.MatrixCalc);        // set the matrix unform block to the controller 3d matrix calc.

            rObjects.Render(gl3dcontroller.MatrixCalc);

            this.Text = "Looking at " + gl3dcontroller.MatrixCalc.TargetPosition + " eye@ " + gl3dcontroller.MatrixCalc.EyePosition + " dir " + gl3dcontroller.Camera.Current + " Dist " + gl3dcontroller.MatrixCalc.EyeDistance + " Zoom " + gl3dcontroller.Zoom.Current;
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

            float h = 0;
            if ( h != -1)
            {
                items.Add("COS-1L", new GLColourShaderWithWorldCoord((a) => { GLStatics.LineWidth(1); }));

                int dist = 1000;
                Color cr = Color.FromArgb(20, Color.White);
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

            GLForm form = new GLForm(gl3dcontroller.glControl);
            form.Name = "form";

            GLPanel ptop = new GLPanel();
            ptop.Position = new Rectangle(100, 100, 500, 500);
            ptop.BackColor = Color.Red;
            ptop.Name = "paneltop";
            form.Add(ptop);

            GLPanel p2 = new GLPanel();
            p2.Dock = GLBaseControl.DockingType.Left;
            p2.Name = "P2";
            ptop.Add(p2);

            GLPanel p3 = new GLPanel();
            p3.Dock = GLBaseControl.DockingType.Right;
            p3.Name = "P2";
            ptop.Add(p3);

            GLPanel p4 = new GLPanel();
            p4.Dock = GLBaseControl.DockingType.Top;
            p4.Name = "P4";
            p3.Add(p4);

            GLPanel p5 = new GLPanel();
            p5.Dock = GLBaseControl.DockingType.Bottom;
            p5.Name = "P4";
            p3.Add(p5);

            //GLImage i1 = new GLImage(Properties.Resources.dotted);
            //i1.Dock = GLBaseControl.DockingType.Left;
            //i1.Name = "Image1";

            //GLImage i2 = new GLImage(Properties.Resources.dotted);
            //i2.Dock = GLBaseControl.DockingType.Right;
            //i2.Name = "Image2";

            //p1.Add(i1);
            //p1.Add(i2);


            form.PerformLayout();

            form.Render();
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


