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
    class ISystem
    {
        public double X, Y, Z;
        public string Name;
        public ISystem(string n, double x, double y, double z) { Name = n;X = x;Y = y;Z = z; }
    }

    class TravelPath
    {
        private GLTexturedShaderTriangleStripWithWorldCoord tapeshader;
        private GLShaderPipeline sunshader;
        private GLPLVertexShaderModelCoordWithWorldTranslationCommonModelTranslation sunvertex;
        private GLRenderableItem ritape;
        private GLBuffer tapepointbuf;
        private GLBuffer starposbuf;

        public void CreatePath(GLItemsList items, GLRenderProgramSortedList rObjects, List<ISystem> pos, float sunsize, float tapesize)
        {
            var positionsv4 = pos.Select(x => new Vector4((float)x.X, (float)x.Y, (float)x.Z, 0)).ToArray();
            float seglen = tapesize * 10;

            var tape = GLTapeObjectFactory.CreateTape(positionsv4, tapesize, seglen, 0F.Radians(), ensureintegersamples: true, margin: sunsize*1.2f);

            if ( ritape == null ) // first time..
            {
                // create shaders
                items.Add("tapelogo", new GLTexture2D(Properties.Resources.chevron));
                items.Tex("tapelogo").SetSamplerMode(OpenTK.Graphics.OpenGL4.TextureWrapMode.Repeat, OpenTK.Graphics.OpenGL4.TextureWrapMode.Repeat);
                tapeshader = new GLTexturedShaderTriangleStripWithWorldCoord(true);
                items.Add("tapeshader", tapeshader );

                GLRenderControl rts = GLRenderControl.TriStrip(tape.Item3, cullface: false);
                rts.DepthTest = false;

                ritape = GLRenderableItem.CreateVector4(items, rts, tape.Item1.ToArray(), new GLRenderDataTexture(items.Tex("tapelogo")));
                tapepointbuf = items.LastBuffer();

                ritape.CreateElementIndex(items.NewBuffer(), tape.Item2, tape.Item3);

                rObjects.Add(items.Shader("tapeshader"), "traveltape", ritape);

                // now the stars

                starposbuf = items.NewBuffer();
                starposbuf.AllocateFill(positionsv4);

                sunvertex = new GLPLVertexShaderModelCoordWithWorldTranslationCommonModelTranslation();
                sunshader = new GLShaderPipeline(sunvertex, new GLPLStarSurfaceFragmentShader());
                items.Add("STAR-PATH-SUNS", sunshader);

                var shape = GLSphereObjectFactory.CreateSphereFromTriangles(3, sunsize);

                GLRenderControl rt = GLRenderControl.Tri();
                rt.DepthTest = false;
                GLRenderableItem rs = GLRenderableItem.CreateVector4Vector4(items, rt, shape, starposbuf, 0, null, pos.Count, 1);

                rObjects.Add(items.Shader("STAR-PATH-SUNS"), "star-path-suns", rs);

                // tbd now names - maybe autoscale the suns
            }
            else
            {
                tapepointbuf.AllocateFill(tape.Item1.ToArray());        // replace the points with a new one
                ritape.CreateElementIndex(ritape.ElementBuffer, tape.Item2, tape.Item3);       // update the element buffer

                starposbuf.AllocateFill(positionsv4);
            }
        }

        public bool Enabled()
        {
            return tapeshader.Enabled;
        }

        public void EnableToggle(bool? on = null)
        {
            bool beon = on.HasValue ? on.Value : !Enabled();
            sunshader.Enabled = tapeshader.Enabled = beon;
        }

        public void Update(long time)
        {
            const int rotperiodms = 10000;
            time = time % rotperiodms;
            float fract = (float)time / rotperiodms;
            float angle = (float)(2 * Math.PI * fract);
            sunvertex.ModelTranslation = Matrix4.CreateRotationY(-angle);
            tapeshader.TexOffset = new Vector2(-(float)(time%2000)/2000, 0);
        }
    }

}
