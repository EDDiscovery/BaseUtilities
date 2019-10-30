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
    public partial class ShaderTestGeoTest1 : Form
    {
        private Controller3D gl3dcontroller = new Controller3D();

        private Timer systemtimer = new Timer();

        public ShaderTestGeoTest1()
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
        GLRenderProgramSortedList rObjects2 = new GLRenderProgramSortedList();
        GLItemsList items = new GLItemsList();

        public class ShaderT3 : GLShaderStandard
        {
            string vcode = @"

#version 450 core

layout( std140, binding=5) buffer storagebuffer
{
    vec4 vertex[];
};

void main(void)
{
    vec4 p = vertex[gl_VertexID];
    gl_Position = p;
}
";

            string gcode = @"
#version 450 core
" + GLMatrixCalcUniformBlock.GLSL + @"
layout (points) in;
layout (points) out;
layout (max_vertices=2) out;
out vec4 vs_color;

layout (binding = 1, std430) buffer Positions
{
    vec4 rejectedpos[];
};

layout (binding = 2, std430) buffer Count
{
    uint count;
};

void main(void)
{
    int i;
    for( i = 0 ; i < gl_in.length() ; i++)
    {
        if ( (gl_PrimitiveIDIn & 1) != 0)
            gl_PointSize = 5;
        else
            gl_PointSize = gl_PrimitiveIDIn+5;
        gl_Position = mc.ProjectionModelMatrix * gl_in[i].gl_Position;
        vs_color = vec4(gl_PrimitiveIDIn*0.05,0.4,gl_PrimitiveIDIn*0.05,1.0);

        if (gl_PrimitiveIDIn != 9 && gl_PrimitiveIDIn != 13 )
        {
            EmitVertex();
        }
        else
        {
            uint ipos = atomicAdd(count,1);

            if ( ipos < 128 )
                rejectedpos[ipos] = gl_in[i].gl_Position;
        }

        if ( gl_PrimitiveIDIn ==8  )
        {
            gl_PointSize = 50;
            vec4 p = gl_in[i].gl_Position;
            gl_Position = mc.ProjectionModelMatrix *  vec4(p.x-2,p.y,p.z,1.0);
            vs_color = vec4(1.0-gl_PrimitiveIDIn*0.05,0.4,gl_PrimitiveIDIn*0.05,1.0);
            EmitVertex();
        }

    }

    EndPrimitive();
}
";

            string fcode = @"
#version 450 core
out vec4 color;
in vec4 vs_color;

void main(void)
{
    color = vs_color;
}
";


            public ShaderT3()
            {
                CompileLink(vertex: vcode, frag:fcode, geo:gcode);
            }

            public override void Start()
            {
                base.Start();
                OpenTKUtils.GLStatics.PointSize(0);     // ensure program is in charge
            }
            public override void Finish()
            {
                base.Finish();
            }
        }

        // Demonstrate buffer feedback AND geo shader add vertex/dump vertex

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            gl3dcontroller.MatrixCalc.ZoomDistance = 20F;
            gl3dcontroller.MatrixCalc.PerspectiveNearZDistance = 0.1f;
            gl3dcontroller.Start(new Vector3(0, 0, 0), new Vector3(170f, 0, 0f), 1F);

            gl3dcontroller.KeyboardTravelSpeed = (ms) =>
            {
                return (float)ms / 20.0f;
            };

            items.Add("Shader", new ShaderT3());

            items.Add("ResultShader", new GLShaderPipeline(new GLVertexShaderNoTranslation(), new GLFragmentShaderFixedColour(new Color4(0.9f, 0.9f, 0.9f, 1.0f))));

            //rObjects.Add(items.Shader("COS-1L"), GLRenderableItem.CreateVertex4Color4( items,
            //                                            OpenTK.Graphics.OpenGL4.PrimitiveType.Lines,
            //                                            GLShapeObjectFactory.CreateLines(new Vector3(-100, 0, -100), new Vector3(100, 0, -100), new Vector3(0, 0, 10), 21),
            //                                            new Color4[] { Color.Red, Color.Red, Color.Green, Color.Green }));




            GLStorageBlock storagebuffer = new GLStorageBlock(5);           // new storage block on binding index
            Vector4[] vertexes = new Vector4[16];
            for (int v = 0; v < vertexes.Length; v++)
                vertexes[v] = new Vector4(v % 4, 0, v / 4, 1);

            storagebuffer.Set(vertexes);

            vecoutbuffer = new GLStorageBlock(1);           // new storage block on binding index
            vecoutbuffer.Allocate(sizeof(float) * 4 * 128, OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicCopy);       // set size of vec buffer

            countbuffer = new GLStorageBlock(2);           // new storage block on binding index
            countbuffer.Allocate(sizeof(int), OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicRead);       // set size to a int.

            rObjects.Add(items.Shader("Shader"), "T1", new GLRenderableItem(OpenTK.Graphics.OpenGL4.PrimitiveType.Points, vertexes.Length, null, null, 1));

            //Unknown count at this point.. use another buffer for vec4s not a new one
            redraw = GLRenderableItem.CreateVector4(items, OpenTK.Graphics.OpenGL4.PrimitiveType.Points, vecoutbuffer, 0);

            rObjects2.Add(items.Shader("ResultShader"), redraw);

            items.Add("MCUB", new GLMatrixCalcUniformBlock());     // def binding of 0

            Closed += ShaderTest_Closed;
        }

        GLRenderableItem redraw;
        GLStorageBlock countbuffer;
        GLStorageBlock vecoutbuffer;

        private void ShaderTest_Closed(object sender, EventArgs e)
        {
            items.Dispose();
        }

        private void ControllerDraw(MatrixCalc mc, long time)
        {
            // System.Diagnostics.Debug.WriteLine("Draw eye " + gl3dcontroller.MatrixCalc.EyePosition + " to " + gl3dcontroller.Pos.Current);

            countbuffer.ZeroBuffer();

            rObjects.Render(gl3dcontroller.MatrixCalc);
            GL.MemoryBarrier(MemoryBarrierFlags.VertexAttribArrayBarrierBit);

            int count = countbuffer.ReadInt(0);
            Vector4[] d = vecoutbuffer.ReadVector4(0, count);
            for (int i = 0; i < count; i++)
            {
                System.Diagnostics.Debug.WriteLine(i + " = " + d[i]);
            }

            redraw.DrawCount = count;
            OpenTKUtils.GLStatics.PointSize(30);

            GLMatrixCalcUniformBlock mcub = (GLMatrixCalcUniformBlock)items.UB("MCUB");
            mcub.Set(gl3dcontroller.MatrixCalc);

            rObjects2.Render(gl3dcontroller.MatrixCalc);
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


