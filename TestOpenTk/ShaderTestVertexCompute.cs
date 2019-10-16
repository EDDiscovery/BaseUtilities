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
    public partial class ShaderTestVertexCompute : Form
    {
        private Controller3D gl3dcontroller = new Controller3D();

        private Timer systemtimer = new Timer();

        public ShaderTestVertexCompute()
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


        public class GLVertexShaderCompute : GLShaderPipelineShadersBase
        {
            public string Code()       // Runs the noise function over the vectors and reports state
            {
                return
    @"
#version 450 core
layout (location = 0) in vec4 position;

layout (binding = 1, std430) buffer Positions
{
    int count;
    float noisebuf[];
};

" + GLShaderFunctionsNoise.NoiseFunctions3 + @"

void write(float v)
{
    uint ipos = atomicAdd(count,1);
    if ( ipos < 1024 )
        noisebuf[ipos] = v;
}

void write(vec4 v)
{
    uint ipos = atomicAdd(count,4);
    if ( ipos < 1024 )
    {
        noisebuf[ipos] = v.x;
        noisebuf[ipos+1] = v.y;
        noisebuf[ipos+2] = v.z;
        noisebuf[ipos+3] = v.w;
    }
}

void main(void)
{
    vec3 position3 = normalize(position.xyz);

    float theta = dot(vec3(0,1,0),position3);    // angle between cur pos and up, modulo equator.  acos(n) would give radians. As both lengths should be modulo 1, no need to divide by |A||B|

    float unRadius = 1;
    vec3 sPosition = position3 * unRadius;

    float s = 0.36;
    float frequency = 1; //0.00001;
    float t1 = snoise(sPosition * frequency) ;
    float t2 = snoise((sPosition + unRadius) * frequency) ;
	float ss = (max(t1, 0.0) * max(t2, 0.0)) * 2.0;

    write(vec4(position3,theta));

}
";
            }

            public GLVertexShaderCompute()
            {
                Program = GLProgram.CompileLink(OpenTK.Graphics.OpenGL4.ShaderType.VertexShader, Code(), GetType().Name);
            }
        }


        // Demonstrate buffer feedback AND geo shader add vertex/dump vertex

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Closed += ShaderTest_Closed;

            gl3dcontroller.MatrixCalc.ZoomDistance = 100F;
            gl3dcontroller.MatrixCalc.PerspectiveNearZDistance = 0.1f;
            gl3dcontroller.BackColour = Color.FromArgb(0, 0, 60);
            gl3dcontroller.Start(new Vector3(0, 0, 0), new Vector3(120f, 0, 0f), 1F);

            gl3dcontroller.TravelSpeed = (ms) =>
            {
                return (float)ms / 20.0f;
            };

            // this bit is eye candy just to show its working

            items.Add("COS-1L", new GLColourObjectShaderNoTranslation((a) => { GLStatics.LineWidth(1); }));
            items.Add("TEX", new GLTexturedObjectShaderSimple());

            rObjects.Add(items.Shader("COS-1L"),
                         GLRenderableItem.CreateVector4Color4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Lines,
                                                    GLShapeObjectFactory.CreateLines(new Vector3(-40, 0, -40), new Vector3(-40, 0, 40), new Vector3(10, 0, 0), 9),
                                                    new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green })
                               );


            rObjects.Add(items.Shader("COS-1L"),
                         GLRenderableItem.CreateVector4Color4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Lines,
                               GLShapeObjectFactory.CreateLines(new Vector3(-40, 0, -40), new Vector3(40, 0, -40), new Vector3(0, 0, 10), 9),
                                                         new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green })
                               );


            items.Add("moon", new GLTexture2D(Properties.Resources.moonmap1k));

            rObjects.Add(items.Shader("TEX"), "sphere7",
                GLRenderableItem.CreateVector4Vector2(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles,
                        GLSphereObjectFactory.CreateTexturedSphereFromTriangles(4, 20.0f),
                        new GLObjectDataTranslationRotationTexture(items.Tex("moon"), new Vector3(0, 0, 0))
                        ));


            // Pass vertex data thru a vertex shader which stores into a block

            items.Add("N1", new GLShaderPipeline(new GLVertexShaderCompute()));

            vecoutbuffer = new GLStorageBlock(1);           // new storage block on binding index
            vecoutbuffer.Allocate(sizeof(float) * 2048 + sizeof(int), OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicCopy);       // set size of vec buffer

            //Vector4[] data = new Vector4[] {
            //    new Vector4(1, 2, 3, 0),
            //    new Vector4(4, 5, 6, 0)
            //};

            Vector4[] data = GLSphereObjectFactory.CreateSphereFromTriangles(0, 1.0f);

            rObjects.Add(items.Shader("N1"), GLRenderableItem.CreateVector4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Points, data));

            for (double ang = -Math.PI/2; ang <= Math.PI/2+0.1; ang += 0.1)
            {
                Vector3 pos = new Vector3((float)Math.Cos(ang), (float)Math.Sin(ang), 0);
                Vector3 up = new Vector3(0, 1, 0);
                float dotp = Vector3.Dot(up, pos);
                float lens = (float)(up.Length * pos.Length);
                double computedang = Math.Acos(dotp / lens);
                System.Diagnostics.Debug.WriteLine(ang.Degrees() +  " " + pos + "-> dotp" + dotp + " " + computedang.Degrees());
            }


        }

        GLStorageBlock vecoutbuffer;


        private void ShaderTest_Closed(object sender, EventArgs e)
        {
            items.Dispose();
        }

        private void ControllerDraw(MatrixCalc mc, long time)
        {
            // System.Diagnostics.Debug.WriteLine("Draw eye " + gl3dcontroller.MatrixCalc.EyePosition + " to " + gl3dcontroller.Pos.Current);

            float zeroone10s = ((float)(time % 10000)) / 10000.0f;
            float zeroone5s = ((float)(time % 5000)) / 5000.0f;
            float zerotwo5s = ((float)(time % 5000)) / 2500.0f;
            float degrees = zeroone10s * 360;
            // matrixbuffer.Write(Matrix4.CreateTranslation(new Vector3(zeroone * 20, 50, 0)),0,true);


            vecoutbuffer.ZeroBuffer();
            rObjects.Render(gl3dcontroller.MatrixCalc);

            int count = vecoutbuffer.ReadInt(0);
            if (count > 0)
            {
                float[] values = vecoutbuffer.ReadFloats(4, Math.Min(2000, count));
                System.Diagnostics.Debug.WriteLine("Count " + count + " min " + values.Min() + " max " + values.Max());
                for (int i = 0; i < count; i = i + 4)
                {
                    Vector3 pos = new Vector3(values[i], values[i + 1], values[i + 2]);
                    System.Diagnostics.Debug.Write("    " + i / 4 + " = " + pos + " : " + values[i + 3]);

                    Vector3 up = new Vector3(0, 1, 0);
                    float value = Vector3.Dot(up, pos);
                    value = 0.0f + value;
                    System.Diagnostics.Debug.WriteLine("        -> dotp" + value);
                }
            }

            this.Text = "Looking at " + gl3dcontroller.MatrixCalc.TargetPosition + " dir " + gl3dcontroller.Camera.Current + " Dist " + gl3dcontroller.MatrixCalc.EyeDistance;

        }

        private void SystemTick(object sender, EventArgs e )
        {
            gl3dcontroller.HandleKeyboard(true, OtherKeys);
            //gl3dcontroller.Redraw();
        }

        private void OtherKeys( BaseUtils.KeyboardState kb )
        {
        }
    }
}


