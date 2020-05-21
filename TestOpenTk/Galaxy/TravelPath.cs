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
        public void CreatePath(GLItemsList items, GLRenderProgramSortedList rObjects, List<ISystem> pos, float sunsize, float tapesize, int bufferfindbinding)
        {
            if (lastpos != -1)
                lastpos = pos.IndexOf(lastlist[lastpos]);       // will be -1 if the system has disappeared from the list.. this keeps the lastpos in the same place

            lastlist = pos;

            var positionsv4 = pos.Select(x => new Vector4((float)x.X, (float)x.Y, (float)x.Z, 0)).ToArray();
            float seglen = tapesize * 10;

            var tape = GLTapeObjectFactory.CreateTape(positionsv4, tapesize, seglen, 0F.Radians(), ensureintegersamples: true, margin: sunsize * 1.2f);

            if (ritape == null) // first time..
            {
                // create shaders
                items.Add(new GLTexture2D(Properties.Resources.chevron), "tapelogo");
                items.Tex("tapelogo").SetSamplerMode(OpenTK.Graphics.OpenGL4.TextureWrapMode.Repeat, OpenTK.Graphics.OpenGL4.TextureWrapMode.Repeat);
                tapeshader = new GLTexturedShaderTriangleStripWithWorldCoord(true);
                items.Add(tapeshader, "tapeshader");

                GLRenderControl rts = GLRenderControl.TriStrip(tape.Item3, cullface: false);
                rts.DepthTest = false;

                ritape = GLRenderableItem.CreateVector4(items, rts, tape.Item1.ToArray(), new GLRenderDataTexture(items.Tex("tapelogo")));
                tapepointbuf = items.LastBuffer();

                ritape.CreateElementIndex(items.NewBuffer(), tape.Item2.ToArray(), tape.Item3);

                rObjects.Add(items.Shader("tapeshader"), "traveltape", ritape);

                // now the stars

                starposbuf = items.NewBuffer();
                starposbuf.AllocateFill(positionsv4);

                sunvertex = new GLPLVertexShaderModelCoordWithWorldTranslationCommonModelTranslation();
                sunshader = new GLShaderPipeline(sunvertex, new GLPLStarSurfaceFragmentShader());
                items.Add(sunshader, "STAR-PATH-SUNS");

                var shape = GLSphereObjectFactory.CreateSphereFromTriangles(3, sunsize);

                GLRenderControl rt = GLRenderControl.Tri();
                rt.DepthTest = false;
                GLRenderableItem rs = GLRenderableItem.CreateVector4Vector4(items, rt, shape, starposbuf, 0, null, pos.Count, 1);

                rObjects.Add(items.Shader("STAR-PATH-SUNS"), "star-path-suns", rs);

                findshader = items.NewShaderPipeline("STAR-PATH_FIND", sunvertex, null, null, new GLPLGeoShaderFindTriangles(bufferfindbinding, 16), null, null, null);

                rifind = GLRenderableItem.CreateVector4Vector4(items, GLRenderControl.Tri(), shape, starposbuf, ic: pos.Count, seconddivisor: 1);

                // tbd now names - maybe autoscale the suns
            }
            else
            {
                tapepointbuf.AllocateFill(tape.Item1.ToArray());        // replace the points with a new one
                ritape.CreateElementIndex(ritape.ElementBuffer, tape.Item2.ToArray(), tape.Item3);       // update the element buffer
                starposbuf.AllocateFill(positionsv4);
            }
        }

        public bool Enable { get { return tapeshader.Enable; } set { tapeshader.Enable = value; } }

        public void Update(long time, float eyedistance)
        {
            const int rotperiodms = 10000;
            time = time % rotperiodms;
            float fract = (float)time / rotperiodms;
            float angle = (float)(2 * Math.PI * fract);
            sunvertex.ModelTranslation = Matrix4.CreateRotationY(-angle);
            float scale = Math.Max(1, Math.Min(4, eyedistance / 5000));
            //System.Diagnostics.Debug.WriteLine("Scale {0}", scale);
            sunvertex.ModelTranslation *= Matrix4.CreateScale(scale);           // scale them a little with distance to pick them out better
            tapeshader.TexOffset = new Vector2(-(float)(time % 2000) / 2000, 0);
        }

        public ISystem FindSystem(Point l, GLRenderControl state, Size screensize)
        {
            var geo = findshader.Get<GLPLGeoShaderFindTriangles>(OpenTK.Graphics.OpenGL4.ShaderType.GeometryShader);
            geo.SetScreenCoords(l, screensize);

            rifind.Execute(findshader, state, null, true); // execute, discard

            var res = geo.GetResult();
            if (res != null)
            {
                for (int i = 0; i < res.Length; i++)
                {
                    System.Diagnostics.Debug.WriteLine(i + " = " + res[i]);
                }
                return lastlist[(int)res[0].Y];
            }

            return null;
        }

        public ISystem NextSystem()
        {
            if (lastlist == null)
                return null;

            if (lastpos == -1)
                lastpos = 0;
            else if (lastpos < lastlist.Count - 1)
                lastpos++;

            return lastlist[lastpos];
        }

        public ISystem PrevSystem()
        {
            if (lastlist == null)
                return null;

            if (lastpos == -1)
                lastpos = lastlist.Count - 1;
            else if (lastpos > 0)
                lastpos--;

            return lastlist[lastpos];
        }

        public void SetSystem(ISystem s)
        {
            if (lastlist != null)
            {
                lastpos = lastlist.IndexOf(s); // -1 if not in list, hence no system
            }
        }

        private GLTexturedShaderTriangleStripWithWorldCoord tapeshader;
        private GLShaderPipeline sunshader;
        private GLPLVertexShaderModelCoordWithWorldTranslationCommonModelTranslation sunvertex;
        private GLRenderableItem ritape;
        private GLBuffer tapepointbuf;
        private GLBuffer starposbuf;
        private GLShaderPipeline findshader;
        private GLRenderableItem rifind;

        private List<ISystem> lastlist;
        private int lastpos = -1;       // -1 no system

    }

}
