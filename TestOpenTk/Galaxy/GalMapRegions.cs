using EliteDangerousCore.EDSM;
using OpenTK;
using OpenTKUtils;
using OpenTKUtils.GL4;
using System.Collections.Generic;
using System.Drawing;

namespace TestOpenTk
{
    public class GalMapRegions
    {
        public GalMapRegions()
        {
        }

        static Color[] array = new Color[] { Color.Red, Color.Green, Color.Blue,
                                                    Color.Brown, Color.Crimson, Color.Coral,
                                                    Color.Aqua, Color.Yellow, Color.Violet,
                                                    Color.Sienna, Color.Silver, Color.Salmon,
                                                    Color.Pink , Color.AntiqueWhite , Color.Beige ,
                                                    Color.DarkCyan , Color.DarkGray , Color.ForestGreen , Color.LightSkyBlue ,
                                                    Color.Lime , Color.Maroon, Color.Olive, Color.SteelBlue};

        public void CreateObjects(GLItemsList items, GLRenderProgramSortedList rObjects, GalacticMapping galmap)
        {
            List<Vector4> vertexcolourregions = new List<Vector4>();
            List<Vector4> vertexregionsoutlines = new List<Vector4>();
            List<ushort> vertexregionoutlineindex = new List<ushort>();

            textrenderer = new GLTextRenderer(new Size(256,20),200,depthtest:false);
            StringFormat fmt = new StringFormat(StringFormatFlags.NoWrap) { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            Font fnt = new Font("MS Sans Serif", 8.25F);

            int cindex = 0;

            foreach (GalacticMapObject gmo in galmap.galacticMapObjects)
            {
                if (gmo.galMapType.Enabled && gmo.galMapType.Group == GalMapType.GalMapGroup.Regions)
                {
                    string name = gmo.name;

                    List<Vector2> polygonxz = new List<Vector2>();                              // needs it in x/z and in vector2's
                    foreach (var pd in gmo.points)
                    {
                        polygonxz.Add(new Vector2((float)pd.X, (float)pd.Z));                   // can be concave and wound the wrong way..
                        vertexregionoutlineindex.Add((ushort)(vertexregionsoutlines.Count));
                        vertexregionsoutlines.Add(new Vector4((float)pd.X, 0, (float)pd.Z, 1));
                    }

                    vertexregionoutlineindex.Add(0xffff);       // primitive restart to break polygon

                    List<List<Vector2>> polys = PolygonTriangulator.Triangulate(polygonxz, false);  // cut into convex polygons first - because we want the biggest possible area for naming purposes

                    Vector2 size, avg;
                    Vector2 bestpos = PolygonTriangulator.Centre(polygonxz, out size, out avg);  // default geographic centre (min x/z + max x/z/2) used in case poly triangulate fails (unlikely)
                    Vector2 bestsize = new Vector2(250, 250 / 5);

                    if (polys.Count > 0)                                                      // just in case..
                    {
                        Vector2 centre = PolygonTriangulator.Centroids(polys);                       // weighted mean of the centroids

                        float mindist = float.MaxValue;

                        foreach (List<Vector2> points in polys)                         // now for every poly
                        {
                            if (points.Count == 3)                                    // already a triangle..
                            {
                                vertexcolourregions.Add(points[0].ToVector4XZ(w: cindex));
                                vertexcolourregions.Add(points[2].ToVector4XZ(w: cindex));
                                vertexcolourregions.Add(points[1].ToVector4XZ(w: cindex));
                            }
                            else
                            {
                                List<List<Vector2>> polytri = PolygonTriangulator.Triangulate(points, true);    // cut into triangles not polygons

                                foreach (List<Vector2> pt in polytri)
                                {
                                    vertexcolourregions.Add(pt[0].ToVector4XZ(w: cindex));
                                    vertexcolourregions.Add(pt[2].ToVector4XZ(w: cindex));
                                    vertexcolourregions.Add(pt[1].ToVector4XZ(w: cindex));
                                }
                            }

                            PolygonTriangulator.FitInsideConvexPoly(points, centre, new Vector2(3000, 3000 / 5), new Vector2(200, 200),
                                                                    ref mindist, ref bestpos, ref bestsize, bestsize.X / 2);
                        }

                        cindex = (cindex+1) % array.Length;
                    }

                    textrenderer.Add(null, gmo.name, fnt, Color.White, Color.Transparent, new Vector3(bestpos.X, 0, bestpos.Y), new Vector3(bestsize.X,0,0),new Vector3(0,0,0), fmt);
                }
            }

            fmt.Dispose();
            fnt.Dispose();
            // regions

            var vertregion = new GLPLVertexShaderFixedColourPalletWorldCoords(array.ToVector4(0.1f));
            var fragregion = new GLPLFragmentShaderVSColour();

            regionshader = new GLShaderPipeline(vertregion, fragregion, null, null);

            GLRenderControl rt = GLRenderControl.Tri();
            rt.DepthTest = false;
            var ridisplay = GLRenderableItem.CreateVector4(items, rt, vertexcolourregions.ToArray());
            rObjects.Add(regionshader, "RegionFill", ridisplay);

            // outlines

            var vertoutline = new GLPLVertexShaderWorldCoord();
            var fragoutline = new GLPLFragmentShaderFixedColour(Color.Cyan);

            outlineshader = new GLShaderPipeline(vertoutline, fragoutline, null, null);
            GLRenderControl ro = GLRenderControl.LineStrip();
            ro.DepthTest = false;
            ro.PrimitiveRestart = 0xffff;
            var rioutline = GLRenderableItem.CreateVector4(items, ro, vertexregionsoutlines.ToArray());
            rioutline.CreateElementIndexUShort(items.NewBuffer(), vertexregionoutlineindex.ToArray());

            rObjects.Add(outlineshader, "RegionOutline", rioutline);

            // text renderer

            rObjects.Add(textrenderer.Shader, "RegionText", textrenderer.RenderableItem);
            renderstate = 7;
        }

        public void Toggle()
        {
            renderstate = (renderstate + 1) % 8;
            Set();
        }

        public void SetState(bool regions, bool outline, bool text)
        {
            renderstate = (regions ? 1 : 0) + (outline ? 2 : 0) + (text ? 4 : 0);
            Set();
        }

        public bool Regions { get { return (renderstate & 1) != 0; } set { renderstate = (renderstate & 0x6) | (value ? 1 : 0); Set(); } }
        public bool Outlines { get { return (renderstate & 2) != 0; } set { renderstate = (renderstate & 0x5) | (value ? 2 : 0); Set(); } }
        public bool Text { get { return (renderstate & 4) != 0; } set { renderstate = (renderstate & 0x3) | (value ? 4 : 0); Set(); } }

        private GLShaderPipeline regionshader;
        private GLShaderPipeline outlineshader;
        private GLTextRenderer textrenderer;
        private int renderstate = 0;

        private void Set()
        {
            regionshader.Enabled = (renderstate & 1) != 0;
            outlineshader.Enabled = (renderstate & 2) != 0;
            textrenderer.Shader.Enabled = (renderstate & 4) != 0;
        }

    }

}
