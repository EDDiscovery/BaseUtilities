using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTKUtils;
using OpenTKUtils.Common;
using OpenTKUtils.GL4;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using OpenTKUtils.GL4.Controls;

// Demonstrate the volumetric calculations needed to compute a plane facing the user inside a bounding box done inside a geo shader
// this one add on tex coord calculation and using a single tight quad shows its working

namespace TestOpenTk
{
    public partial class ShaderControls : Form
    {
        private OpenTKUtils.WinForm.GLWinFormControl glwfc;
        private Controller3D gl3dcontroller;

        private Timer systemtimer = new Timer();

        public ShaderControls()
        {
            InitializeComponent();

            glwfc = new OpenTKUtils.WinForm.GLWinFormControl(glControlContainer);

            systemtimer.Interval = 25;
            systemtimer.Tick += new EventHandler(SystemTick);
            systemtimer.Start();
        }

        GLRenderProgramSortedList rObjects = new GLRenderProgramSortedList();
        GLItemsList items = new GLItemsList();
        Vector4[] boundingbox;
        GLControlDisplay displaycontrol;

        /// ////////////////////////////////////////////////////////////////////////////////////////////////////


        private void ShaderTest_Closed(object sender, EventArgs e)
        {
            items.Dispose();
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

            GLRenderControl rl = GLRenderControl.Lines(1);

            {
                items.Add("LINEYELLOW", new GLFixedShader(System.Drawing.Color.Yellow));
                rObjects.Add(items.Shader("LINEYELLOW"),
                GLRenderableItem.CreateVector4(items, rl, displaylines));
            }

            float h = 0;
            if ( h != -1)
            {
                items.Add("COS-1L", new GLColourShaderWithWorldCoord());

                int dist = 1000;
                Color cr = Color.FromArgb(100, Color.White);
                rObjects.Add(items.Shader("COS-1L"),    // horizontal
                             GLRenderableItem.CreateVector4Color4(items, rl,
                                                        GLShapeObjectFactory.CreateLines(new Vector3(left, h, front), new Vector3(left, h, back), new Vector3(dist, 0, 0), (back - front) / dist + 1),
                                                        new Color4[] { cr })
                                   );

                rObjects.Add(items.Shader("COS-1L"),
                             GLRenderableItem.CreateVector4Color4(items, rl,
                                                        GLShapeObjectFactory.CreateLines(new Vector3(left, h, front), new Vector3(right, h, front), new Vector3(0, 0, dist), (right - left) / dist + 1),
                                                        new Color4[] { cr })
                                   );

            }


            {
                items.Add("TEX", new GLTexturedShaderWithObjectTranslation());
                items.Add("dotted2", new GLTexture2D(Properties.Resources.dotted2));

                GLRenderControl rt = GLRenderControl.Tri();

                rObjects.Add(items.Shader("TEX"),
                    GLRenderableItem.CreateVector4Vector2(items, rt,
                            GLCubeObjectFactory.CreateSolidCubeFromTriangles(2000f), GLCubeObjectFactory.CreateCubeTexTriangles(),
                            new GLRenderDataTranslationRotationTexture(items.Tex("dotted2"), new Vector3(-2, 0, 0))
                            ));
            }

            {
                items.Add("FCS1", new GLFixedColourShaderWithWorldCoord(Color.FromArgb(150, Color.Green)));
                items.Add("FCS2", new GLFixedColourShaderWithWorldCoord(Color.FromArgb(80, Color.Red)));

                GLRenderControl rq = GLRenderControl.Quads();

                rObjects.Add(items.Shader("FCS1"),
                    GLRenderableItem.CreateVector4(items, rq,
                                                GLShapeObjectFactory.CreateQuad(1000, pos: new Vector3(4000, 500, 0))));
                rObjects.Add(items.Shader("FCS2"),
                    GLRenderableItem.CreateVector4(items, rq,
                                                GLShapeObjectFactory.CreateQuad(1000, pos: new Vector3(4000, 1000, 0))));
            }

            if (true)
            {
                bool testtable = true;
                bool testflow = true;
                bool testtextbox = true;
                bool testcombobox = true;
                bool testscrollbar = true;
                bool testvsp = true;
                bool testlb = true;
                bool testbuttons = true;
                bool testtabcontrol = true;
                bool testdatetime = true;

                //testtable = testflow = testtextbox = testcombobox = testscrollbar = testvsp = testlb = testbuttons = testtabcontrol = testdatetime = false;

                displaycontrol = new GLControlDisplay(glwfc);       // hook form to the window - its the master
                displaycontrol.Focusable = true;          // we want to be able to focus and receive key presses.
                displaycontrol.Name = "displaycontrol";
                displaycontrol.SuspendLayout();

                GLForm pform = new GLForm("form", "GL Control demonstration", new Rectangle(10, 0, 1000, 800), Color.FromArgb(200, Color.Red));
                pform.SuspendLayout();
                pform.BackColorGradient = 90;
                pform.BackColorGradientAlt = Color.FromArgb(200,Color.Yellow);

                displaycontrol.Add(pform);

                GLPanel p1 = new GLPanel("P3", new Size(200,200), DockingType.BottomRight, 0, Color.Blue);
                p1.DockingMargin = new Margin(50,20,10,20);
                pform.Add(p1);

                GLPanel p2 = new GLPanel("P2", new Size(200, 300), DockingType.LeftTop, 0.15f, Color.Green);
                p2.SetMarginBorderWidth(new Margin(2), 1, Color.Wheat, new OpenTKUtils.GL4.Controls.Padding(2));
                p2.DockingMargin = new Margin(10, 20, 1, 10);
                pform.Add(p2);

                GLGroupBox p3 = new GLGroupBox("GB1", "Group Box", DockingType.Right, 0.15f, Color.Yellow);
                pform.Add(p3);

                if ( testtabcontrol )
                {
                    GLTabControl tc = new GLTabControl("Tabc", new Rectangle(360, 450, 200, 200), Color.DarkCyan);
                    tc.TabStyle = new TabStyleRoundedEdge();
                    tc.TabStyle = new TabStyleSquare();
                    tc.TabStyle = new TabStyleAngled();
                    tc.Font = new Font("Ms Sans Serif", 11);

                    GLTabPage tabp1 = new GLTabPage("tab1", "TAB 1", Color.Blue);
                    tc.Add(tabp1);

                    GLButton tabp1b1 = new GLButton("B1", new Rectangle(5, 5, 80, 40), "Button 1", Color.Gray, Color.Yellow);
                    tabp1.Add(tabp1b1);
                    tabp1b1.Click += (c, ev) => { System.Diagnostics.Debug.WriteLine("On click for " + c.Name + " " + ev.Button); };

                    GLTabPage tabp2 = new GLTabPage("tab1", "TAB Page 2", Color.Yellow);
                    tc.Add(tabp2);

                    GLTabPage tabp3 = new GLTabPage("tab1", "TAB Page 3", Color.Yellow);
                    tc.Add(tabp3);
                    GLTabPage tabp4 = new GLTabPage("tab1", "TAB Page 4", Color.Yellow);
                    tc.Add(tabp4);

                    pform.Add(tc);
                    tc.SelectedTab = 0;
                }

                if (testtable)
                {
                    GLTableLayoutPanel ptable = new GLTableLayoutPanel("tablelayout", new Rectangle(150, 10, 200, 200), Color.Gray);
                    ptable.SuspendLayout();
                    ptable.SetMarginBorderWidth(new Margin(2), 1, Color.Wheat, new OpenTKUtils.GL4.Controls.Padding(2));
                    ptable.Rows = new List<GLTableLayoutPanel.Style> { new GLTableLayoutPanel.Style(GLTableLayoutPanel.Style.SizeTypeEnum.Relative, 50), new GLTableLayoutPanel.Style(GLTableLayoutPanel.Style.SizeTypeEnum.Relative, 50) };
                    ptable.Columns = new List<GLTableLayoutPanel.Style> { new GLTableLayoutPanel.Style(GLTableLayoutPanel.Style.SizeTypeEnum.Relative, 50), new GLTableLayoutPanel.Style(GLTableLayoutPanel.Style.SizeTypeEnum.Relative, 50) };
                    pform.Add(ptable);
                    GLImage pti1 = new GLImage("PTI1", new Rectangle(0, 0, 24, 24), Properties.Resources.dotted);
                    pti1.Column = 0; pti1.Row = 0; pti1.Dock = DockingType.Fill;
                    ptable.Add(pti1);
                    GLImage pti2 = new GLImage("PTI2", new Rectangle(100, 0, 24, 24), Properties.Resources.dotted2);
                    pti2.Column = 1; pti1.Row = 0;
                    ptable.Add(pti2);
                    GLImage pti3 = new GLImage("PTI3", new Rectangle(100, 0, 48, 48), Properties.Resources.ImportSphere);
                    pti3.Column = 0; pti3.Row = 1; pti3.Dock = DockingType.LeftCenter; pti3.ImageStretch = true;
                    ptable.Add(pti3);
                    GLImage pti4 = new GLImage("PTI4", new Rectangle(100, 0, 64, 64), Properties.Resources.Logo8bpp);
                    pti4.Column = 1; pti4.Row = 1; pti4.Dock = DockingType.Center;
                    ptable.Add(pti4);
                    ptable.ResumeLayout();
                }

                if (testflow)
                {
                    GLFlowLayoutPanel ptable = new GLFlowLayoutPanel("flowlayout", new Rectangle(360, 10, 200, 200), Color.Gray);
                    ptable.SuspendLayout();
                    ptable.SetMarginBorderWidth(new Margin(2), 1, Color.Wheat, new OpenTKUtils.GL4.Controls.Padding(2));
                    ptable.FlowPadding = new OpenTKUtils.GL4.Controls.Padding(10, 5, 0, 0);
                    pform.Add(ptable);
                    GLImage pti1 = new GLImage("PTI1", new Rectangle(0, 0, 24, 24), Properties.Resources.dotted);
                    pti1.Column = 0; pti1.Row = 0; 
                    ptable.Add(pti1);
                    GLImage pti2 = new GLImage("PTI2", new Rectangle(100, 0, 32, 32), Properties.Resources.dotted2);
                    pti2.Column = 1; pti1.Row = 0;
                    ptable.Add(pti2);
                    GLImage pti3 = new GLImage("PTI3", new Rectangle(100, 0, 48, 48), Properties.Resources.ImportSphere);
                    pti3.Column = 0; pti3.Row = 1; 
                    ptable.Add(pti3);
                    GLImage pti4 = new GLImage("PTI4", new Rectangle(100, 0, 64, 64), Properties.Resources.Logo8bpp);
                    pti4.Column = 1; pti4.Row = 1; 
                    ptable.Add(pti4);
                    ptable.ResumeLayout();
                }

                if (testtextbox)
                {
                    GLTextBox tb1 = new GLTextBox("TB1", new Rectangle(600, 10, 150, 20), "Text Data Which is a very long string of very many many characters", Color.White);
                    pform.Add(tb1);
                }

                if (testcombobox)
                {
                    List<string> i1 = new List<string>() { "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve" };
                    GLComboBox cb1 = new GLComboBox("CB1", new Rectangle(600, 40, 150, 20), i1, Color.White);
                    cb1.SelectedIndex = 0;
                    cb1.BackColorGradient = 90;
                    cb1.BackColorGradientAlt = Color.Aqua;
                    cb1.Font = new Font("Microsoft Sans Serif", 12f);
                    pform.Add(cb1);
                }

                if (testscrollbar)
                {
                    GLPanel psb = new GLPanel("panelsb", new Rectangle(600, 80, 50, 100), Color.Gray);
                    pform.Add(psb);
                    GLScrollBar sb1 = new GLScrollBar("SB1", new Rectangle(0, 0, 20, 100), 0, 100);
                    psb.Add(sb1);
                }

                if (testvsp)
                {
                    GLVerticalScrollPanel sp1 = new GLVerticalScrollPanel("VSP1", new Rectangle(150, 220, 200, 200), Color.Gray);
                    pform.Add(sp1);
                    GLImage sp1i1 = new GLImage("SP1I1", new Rectangle(10, 10, 100, 100), Properties.Resources.dotted);
                    sp1.Add(sp1i1);
                    GLImage sp1i2 = new GLImage("SP1I22", new Rectangle(10, 120, 100, 100), Properties.Resources.dotted);
                    sp1.Add(sp1i2);
                }

                if (testvsp )
                {
                    GLVerticalScrollPanelScrollBar spb1 = new GLVerticalScrollPanelScrollBar("CSPan", new Rectangle(370, 220, 200, 200), Color.Green);
                    pform.Add(spb1);
                    GLImage spb1i1 = new GLImage("SPB1I1", new Rectangle(10, 10, 100, 100), Properties.Resources.dotted);
                    spb1.Add(spb1i1);
                    GLImage spb1i2 = new GLImage("SPB1I2", new Rectangle(10, 120, 100, 100), Properties.Resources.dotted);
                    spb1.Add(spb1i2);
                }

                if (testlb)
                {
                    List<string> i1 = new List<string>() { "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve" };
                    GLListBox lb1 = new GLListBox("LB1", new Rectangle(580, 220, 200, 200), i1, Color.Gray);
                    lb1.SetMarginBorderWidth(new Margin(2), 1, Color.Wheat, new OpenTKUtils.GL4.Controls.Padding(2));
                    lb1.Font = new Font("Microsoft Sans Serif", 12f);
                    pform.Add(lb1);
                    lb1.SelectedIndexChanged += (s,si) => { System.Diagnostics.Debug.WriteLine("Selected index " + si); };
                }

                if (testbuttons)
                {

                    GLButton b1 = new GLButton("B1", new Rectangle(5, 5, 80, 40), "Button 1", Color.Gray, Color.Yellow);
                    b1.Margin = new Margin(5);
                    b1.Padding = new OpenTKUtils.GL4.Controls.Padding(5);
                    b1.Click += (c, ev) => { System.Diagnostics.Debug.WriteLine("On click for " + c.Name + " " + ev.Button); };
                    p2.Add(b1);

                    GLButton b2 = new GLButton("B2", new Rectangle(5, 50, 0, 0), "Button 2", Color.Gray, Color.Yellow);
                    b2.Image = Properties.Resources.ImportSphere;
                    b2.ImageAlign = ContentAlignment.MiddleLeft;
                    b2.TextAlign = ContentAlignment.MiddleRight;
                    p2.Add(b2);

                    GLCheckBox cb1 = new GLCheckBox("CB1", new Rectangle(5, 100, 100, 20), "Check Box 1", Color.Transparent);
                    cb1.AutoCheck = cb1.GroupRadioButton = true;
                    cb1.CheckChanged += (c) => { System.Diagnostics.Debug.WriteLine("Check 1 changed " + c.Name); };
                    p2.Add(cb1);
                    GLCheckBox cb2 = new GLCheckBox("CB1", new Rectangle(5, 130, 100, 20), "Check Box 2", Color.Transparent);
                    cb2.AutoCheck = cb2.GroupRadioButton = true;
                    cb2.CheckChanged += (c) => { System.Diagnostics.Debug.WriteLine("Check 2 changed " + c.Name); };
                    p2.Add(cb2);
                    GLCheckBox cb3 = new GLCheckBox("CB3", new Rectangle(5, 160, 100, 20), "Radio Box 1", Color.Transparent);
                    cb3.AutoCheck = true;
                    cb3.Appearance = CheckBoxAppearance.Radio;
                    p2.Add(cb3);

                    GLUpDownControl upc1 = new GLUpDownControl("UPC1", new Rectangle(5, 190, 26, 26), Color.AliceBlue);
                    p2.Add(upc1);
                    upc1.ValueChanged += (s, upe) => System.Diagnostics.Debug.WriteLine("Up down control {0} {1}", s.Name, upe.Delta);


                    GLLabel lb1 = new GLLabel("Lab1", new Rectangle(5, 220, 0, 0), "Hello", Color.Red);
                    p2.Add(lb1);

                }

                if ( testdatetime)
                {
                    GLDateTimePicker dtp = new GLDateTimePicker("DTP", new Rectangle(5, 500, 300, 30), DateTime.Now, Color.DarkCyan);
                    dtp.Font = new Font("Ms Sans Serif", 11);
                    dtp.ShowCheckBox = true;
                    dtp.ShowUpDown = true;
                    pform.Add(dtp);
                }


                pform.ResumeLayout();
                displaycontrol.ResumeLayout();
            }

            gl3dcontroller = new Controller3D();    

            gl3dcontroller.MatrixCalc.PerspectiveNearZDistance = 1f;
            gl3dcontroller.MatrixCalc.PerspectiveFarZDistance = 500000f;
            gl3dcontroller.ZoomDistance = 5000F;
            gl3dcontroller.EliteMovement = true;
            gl3dcontroller.PaintObjects = Controller3dDraw;

            gl3dcontroller.KeyboardTravelSpeed = (ms,eyedist) =>
            {
                return (float)ms * 10.0f;
            };

            gl3dcontroller.MatrixCalc.InPerspectiveMode = true;

            if ( displaycontrol != null )
            {
                gl3dcontroller.Start(displaycontrol, new Vector3(0, 0, 10000), new Vector3(140.75f, 0, 0), 0.5F);     // HOOK the 3dcontroller to the form so it gets Form events

                displaycontrol.Paint += (o) =>        // subscribing after start means we paint over the scene, letting transparency work
                {                           // this is because we are at depth 0
                    GLMatrixCalc c = new GLMatrixCalc();
                    ((GLMatrixCalcUniformBlock)items.UB("MCUB")).Set(c,glwfc.Width, glwfc.Height);        // set the matrix unform block to the controller 3d matrix calc.
                    displaycontrol.Render(glwfc.RenderState);
                };

            }
            else
                gl3dcontroller.Start(glwfc, new Vector3(0, 0, 10000), new Vector3(140.75f, 0, 0), 0.5F);     // HOOK the 3dcontroller to the form so it gets Form events

        }


        private void Controller3dDraw(GLMatrixCalc mc, long time)
        {
            ((GLMatrixCalcUniformBlock)items.UB("MCUB")).Set(gl3dcontroller.MatrixCalc, glwfc.Width, glwfc.Height);        // set the matrix unform block to the controller 3d matrix calc.

            rObjects.Render(glwfc.RenderState, gl3dcontroller.MatrixCalc);

            this.Text = "Looking at " + gl3dcontroller.MatrixCalc.TargetPosition + " eye@ " + gl3dcontroller.MatrixCalc.EyePosition + " dir " + gl3dcontroller.Camera.Current + " Dist " + gl3dcontroller.MatrixCalc.EyeDistance + " Zoom " + gl3dcontroller.Zoom.Current;
        }

        private void SystemTick(object sender, EventArgs e)
        {
            if (displaycontrol != null && displaycontrol.RequestRender)
                glwfc.Invalidate();
            var cdmt = gl3dcontroller.HandleKeyboardSlews(true);
            if (cdmt.AnythingChanged )
                glwfc.Invalidate();
        }

    }
}


