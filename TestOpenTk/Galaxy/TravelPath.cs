using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTKUtils;
using OpenTKUtils.GL4;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestOpenTk
{
    //class ISystem
    //{
    //    public double X, Y, Z;
    //    public string Name;
    //}

    class TravelPath
    {
        private GLRenderableItem ri;
        private GLBuffer pointbuf;
        
        public void CreatePath(GLItemsList items, GLRenderProgramSortedList rObjects, List<Vector3> pos)
        {
            var tape = GLTapeObjectFactory.CreateTape(pos.ToArray(), 10, 20, 0F.Radians(), ensureintegersamples: true, margin: 20f);

            if ( ri == null ) // first time..
            {
                // create shaders
                items.Add("tapelogo", new GLTexture2D(Properties.Resources.chevron));
                items.Tex("tapelogo").SetSamplerMode(OpenTK.Graphics.OpenGL4.TextureWrapMode.Repeat, OpenTK.Graphics.OpenGL4.TextureWrapMode.Repeat);
                items.Add("tapeshader", new GLTexturedShaderTriangleStripWithWorldCoord(true));

                GLRenderControl rts = GLRenderControl.TriStrip(tape.Item3, cullface: false);
                rts.DepthTest = false;

                ri = GLRenderableItem.CreateVector4(items, rts, tape.Item1.ToArray(), new GLRenderDataTexture(items.Tex("tapelogo")));
                pointbuf = items.LastBuffer();

                ri.CreateElementIndex(items.NewBuffer(), tape.Item2, tape.Item3);

                rObjects.Add(items.Shader("tapeshader"), "traveltape", ri);

                items.Add("STAR", new GLShaderPipeline(new GLPLVertexShaderModelCoordWithObjectTranslation(), new GLPLStarSurfaceFragmentShader()));



            }
            else
            {
                pointbuf.AllocateFill(tape.Item1.ToArray());        // replace the points with a new one
                ri.CreateElementIndex(ri.ElementBuffer, tape.Item2, tape.Item3);       // update the element buffer
            }
        }
    }

}
