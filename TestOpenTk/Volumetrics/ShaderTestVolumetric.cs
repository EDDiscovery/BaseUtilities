using OpenTK;
using OpenTK.Graphics;
using OpenTKUtils;
using OpenTKUtils.Common;
using OpenTKUtils.GL4;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

// Demonstrate the volumetric calculations needed to compute a plane facing the user inside a bounding box.

namespace TestOpenTk
{
    public partial class ShaderTestVolumetric : Form
    {
        private Controller3D gl3dcontroller = new Controller3D();

        private Timer systemtimer = new Timer();

        public ShaderTestVolumetric()
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

        public class GLFixedShader : GLShaderPipeline
        {
            public GLFixedShader(Color c, Action<IGLProgramShader> action = null) : base(action)
            {
                AddVertexFragment(new GLVertexShaderNoTranslation(), new GLFragmentShaderFixedColour(c));
            }
        }

        public class GLFixedProjectionShader : GLShaderPipeline
        {
            public GLFixedProjectionShader(Color c, Action<IGLProgramShader> action = null) : base(action)
            {
                AddVertexFragment(new GLVertexShaderProjection(), new GLFragmentShaderFixedColour(c));
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Closed += ShaderTest_Closed;

            gl3dcontroller.MatrixCalc.PerspectiveNearZDistance = 0.1f;
            gl3dcontroller.ZoomDistance = 20F;
            gl3dcontroller.Start(new Vector3(0, 0, 0), new Vector3(90,0,0), 1F);

            gl3dcontroller.KeyboardTravelSpeed = (ms) =>
            {
                return (float)ms / 100.0f;
            };

            items.Add("COS-1L", new GLColourObjectShaderNoTranslation((a) => { GLStatics.LineWidth(1); }));
            items.Add("LINEYELLOW", new GLFixedShader(System.Drawing.Color.Yellow, (a) => { GLStatics.LineWidth(1); }));
            items.Add("LINEPURPLE", new GLFixedShader(System.Drawing.Color.Purple, (a) => { GLStatics.LineWidth(1); }));
            items.Add("LINERED", new GLFixedShader(System.Drawing.Color.Red, (a) => { GLStatics.LineWidth(1); }));
            items.Add("DOTYELLOW", new GLFixedProjectionShader(System.Drawing.Color.Yellow, (a) => { GLStatics.PointSize(10); }));
            items.Add("SURFACEBLUE", new GLFixedProjectionShader(System.Drawing.Color.FromArgb(60,Color.Blue), (a) => { GLStatics.PointSize(20); }));

            items.Add("gal", new GLTexture2D(Properties.Resources.galheightmap7));

           
            rObjects.Add(items.Shader("COS-1L"), "L1",   // horizontal
                         GLRenderableItem.CreateVector4Color4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Lines,
                                                    GLShapeObjectFactory.CreateLines(new Vector3(-100, 0, -100), new Vector3(-100, 0, 100), new Vector3(10, 0, 0), 21),
                                                    new Color4[] { Color.Gray })
                               );


            rObjects.Add(items.Shader("COS-1L"),    // vertical
                         GLRenderableItem.CreateVector4Color4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Lines,
                               GLShapeObjectFactory.CreateLines(new Vector3(-100, 0, -100), new Vector3(100, 0, -100), new Vector3(0, 0, 10), 21),
                                                         new Color4[] { Color.Gray })
                               );

            int hsize = 10, vsize = 5, zsize = 10;
            boundingbox = new Vector4[]
            {
                new Vector4(-hsize,-vsize,zsize,1),
                new Vector4(-hsize,vsize,zsize,1),
                new Vector4(hsize,vsize,zsize,1),
                new Vector4(hsize,-vsize,zsize,1),

                new Vector4(-hsize,-vsize,-zsize,1),
                new Vector4(-hsize,vsize,-zsize,1),
                new Vector4(hsize,vsize,-zsize,1),
                new Vector4(hsize,-vsize,-zsize,1),
            };

            rObjects.Add(items.Shader("LINERED"),
                        GLRenderableItem.CreateVector4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.LineLoop, boundingbox));

            Vector4[] extralines = new Vector4[]
            {
                new Vector4(-hsize,-vsize,zsize,1),
                new Vector4(-hsize,-vsize,-zsize,1),
                new Vector4(-hsize,vsize,zsize,1),
                new Vector4(-hsize,vsize,-zsize,1),
                new Vector4(hsize,vsize,zsize,1),
                new Vector4(hsize,vsize,-zsize,1),
                new Vector4(hsize,-vsize,zsize,1),
                new Vector4(hsize,-vsize,-zsize,1),

                new Vector4(-hsize,-vsize,zsize,1),
                new Vector4(hsize,-vsize,zsize,1),
                new Vector4(-hsize,-vsize,-zsize,1),
                new Vector4(hsize,-vsize,-zsize,1),
            };

            rObjects.Add(items.Shader("LINEYELLOW"),
                        GLRenderableItem.CreateVector4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Lines, extralines));

            indicatorlinebuffer = new GLBuffer();           // new buffer
            indicatorlinebuffer.Allocate(sizeof(float) * 4 * 2, OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicCopy);       // set size of vec buffer
            rObjects.Add(items.Shader("LINEPURPLE"), GLRenderableItem.CreateVector4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Lines, indicatorlinebuffer, 2));

            interceptpointbuffer = new GLBuffer();           // new buffer
            interceptpointbuffer.Allocate(sizeof(float) * 4 * 12, OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicCopy);       // set size of vec buffer
            interceptri = GLRenderableItem.CreateVector4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Points, interceptpointbuffer, 0);
            rObjects.Add(items.Shader("DOTYELLOW"), interceptri);

            surfacebuffer = new GLBuffer();           // new buffer
            surfacebuffer.Allocate(sizeof(float) * 4 * (6+2), OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicCopy);       // set size of vec buffer
            surfaceri = GLRenderableItem.CreateVector4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.TriangleFan, surfacebuffer, 0);
            rObjects.Add(items.Shader("SURFACEBLUE"), surfaceri);

            items.Add("MCUB", new GLMatrixCalcUniformBlock());     // create a matrix uniform block 

        }

        Vector4[] boundingbox;
        GLBuffer indicatorlinebuffer;
        GLBuffer interceptpointbuffer;
        GLRenderableItem interceptri;
        GLBuffer surfacebuffer;
        GLRenderableItem surfaceri;

        private void ShaderTest_Closed(object sender, EventArgs e)
        {
            items.Dispose();
        }

        private void ControllerDraw(MatrixCalc mc, long time)
        {
            ((GLMatrixCalcUniformBlock)items.UB("MCUB")).Set(gl3dcontroller.MatrixCalc);        // set the matrix unform block to the controller 3d matrix calc.


            Vector4[] modelboundingbox = boundingbox.Transform(gl3dcontroller.MatrixCalc.ModelMatrix);

            for (int i = 0; i < boundingbox.Length; i++)
            {
                System.Diagnostics.Debug.WriteLine(i + " = " + modelboundingbox[i].ToStringVec());
            }

            modelboundingbox.MinMaxZ(out int minz, out int maxz);

            System.Diagnostics.Debug.WriteLine("min " + minz + " max " + maxz);
            var p = indicatorlinebuffer.Map(0, sizeof(float) * 4 * 2);
            indicatorlinebuffer.MapWrite(ref p, boundingbox[minz]);
            indicatorlinebuffer.MapWrite(ref p, boundingbox[maxz]);
            indicatorlinebuffer.UnMap();

            float percent = 0.2f;
            float zdist = modelboundingbox[maxz].Z - modelboundingbox[minz].Z;
            {
                float zpoint = modelboundingbox[maxz].Z - zdist * percent;
                //System.Diagnostics.Debug.WriteLine("Zpoint is" + zpoint);

                Vector4[] intercepts = new Vector4[6];
                int count = 0;
                modelboundingbox[0].FindVectorFromZ(modelboundingbox[1], ref intercepts, ref count, zpoint);       
                modelboundingbox[1].FindVectorFromZ(modelboundingbox[2], ref intercepts, ref count, zpoint);
                modelboundingbox[2].FindVectorFromZ(modelboundingbox[3], ref intercepts, ref count, zpoint);
                modelboundingbox[3].FindVectorFromZ(modelboundingbox[0], ref intercepts, ref count, zpoint);

                modelboundingbox[4].FindVectorFromZ(modelboundingbox[5], ref intercepts, ref count, zpoint);
                modelboundingbox[5].FindVectorFromZ(modelboundingbox[6], ref intercepts, ref count, zpoint);
                modelboundingbox[6].FindVectorFromZ(modelboundingbox[7], ref intercepts, ref count, zpoint);
                modelboundingbox[7].FindVectorFromZ(modelboundingbox[4], ref intercepts, ref count, zpoint);

                modelboundingbox[0].FindVectorFromZ(modelboundingbox[4], ref intercepts, ref count, zpoint);
                modelboundingbox[1].FindVectorFromZ(modelboundingbox[5], ref intercepts, ref count, zpoint);
                modelboundingbox[2].FindVectorFromZ(modelboundingbox[6], ref intercepts, ref count, zpoint);
                modelboundingbox[3].FindVectorFromZ(modelboundingbox[7], ref intercepts, ref count, zpoint);

                // texturecoords can be worked out by zpercent and knowing its direction (x,y,z)..

                if (count>=3)
                {
                    Vector4 avg = intercepts.Average();
                    float[] angles = new float[6];
                    for (int i = 0; i < count; i++)
                    {
                        angles[i] =-(float) Math.Atan2(intercepts[i].Y - avg.Y, intercepts[i].X - avg.X);        // all on the same z plane, so x/y only need be considered
                        System.Diagnostics.Debug.WriteLine("C" + intercepts[i].ToStringVec() + " " + angles[i].Degrees());
                    }

                    Array.Sort(angles, intercepts, 0, count);       // sort by angles, sorting intercepts, from 0 to count

                    for (int i = 0; i < count; i++)
                    {
                    //    System.Diagnostics.Debug.WriteLine(intercepts[i].ToStringVec() + " " + angles[i].Degrees());
                    }

                    var p1 = interceptpointbuffer.Map(0, sizeof(float) * 4 * count);
                    int ji = 0;
                    for (; ji < count; ji++)
                        interceptpointbuffer.MapWrite(ref p1, intercepts[ji]);
                    interceptpointbuffer.UnMap();
                    interceptri.DrawCount = count;

                    var p2 = surfacebuffer.Map(0, sizeof(float) * 4 * (2+count));
                    surfacebuffer.MapWrite(ref p2, avg);
                    for (ji = 0; ji < count; ji++)
                        surfacebuffer.MapWrite(ref p2, intercepts[ji]);

                    surfacebuffer.MapWrite(ref p2, intercepts[0]);
                    surfacebuffer.UnMap();

                    surfaceri.DrawCount = count + 2;


                }
            }

            rObjects.Render(gl3dcontroller.MatrixCalc);

            this.Text = "Looking at " + gl3dcontroller.MatrixCalc.TargetPosition + " dir " + gl3dcontroller.Camera.Current + " eye@ " + gl3dcontroller.MatrixCalc.EyePosition + " Dist " + gl3dcontroller.MatrixCalc.EyeDistance;
        }

        private void SystemTick(object sender, EventArgs e )
        {
            var cdmt = gl3dcontroller.HandleKeyboard(true);
            if ( cdmt.AnythingChanged )
                gl3dcontroller.Redraw();
        }
    }
}


