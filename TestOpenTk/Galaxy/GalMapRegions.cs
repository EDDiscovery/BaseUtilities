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

        public void CreateObjects(GLItemsList items, GLRenderProgramSortedList rObjects, GalacticMapping galmap, float sizeofname = 5000)
        {
            List<Vector4> vertexcolourregions = new List<Vector4>();
            List<Vector4> vertexregionsoutlines = new List<Vector4>();
            List<ushort> vertexregionoutlineindex = new List<ushort>();

            textrenderer = new GLTextRenderer(new Size(250,22),200,depthtest:false);
            StringFormat fmt = new StringFormat(StringFormatFlags.NoWrap) { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            Font fnt = new Font("MS Sans Serif", 12F);

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

                    Vector2 avgcentroid = new Vector2(0, 0);
                    int pointsaveraged = 0;

                    if (polys.Count > 0)                                                      // just in case..
                    {
                        Vector2 centre = PolygonTriangulator.Centroids(polys);                       // weighted mean of the centroids

                        foreach (List<Vector2> points in polys)                         // now for every poly
                        {
                            List<List<Vector2>> polytri;
                            if (points.Count == 3)                                    // already a triangle..
                                polytri = new List<List<Vector2>>() { new List<Vector2>() { points[0], points[1], points[2] } };
                            else
                                polytri = PolygonTriangulator.Triangulate(points, true);    // cut into triangles not polygons

                            foreach (List<Vector2> pt in polytri)
                            {
                                vertexcolourregions.Add(pt[0].ToVector4XZ(w: cindex));
                                vertexcolourregions.Add(pt[2].ToVector4XZ(w: cindex));
                                vertexcolourregions.Add(pt[1].ToVector4XZ(w: cindex));

                                var cx = (pt[0].X + pt[1].X + pt[2].X) / 3;
                                var cy = (pt[0].Y + pt[1].Y + pt[2].Y) / 3;
                                avgcentroid = new Vector2(avgcentroid.X + cx, avgcentroid.Y + cy);
                                pointsaveraged++;

                                //foreach (var pd in pt) // debug
                                //{
                                //    vertexregionoutlineindex.Add((ushort)(vertexregionsoutlines.Count));
                                //    vertexregionsoutlines.Add(new Vector4((float)pd.X, 0, (float)pd.Y, 1));
                                //}
                                //vertexregionoutlineindex.Add(0xffff);       // primitive restart to break polygon
                            }
                        }

                        cindex = (cindex+1) % array.Length;
                    }

                    Vector3 bestpos = new Vector3(avgcentroid.X / pointsaveraged + (float)gmo.textadjustx, 0, avgcentroid.Y / pointsaveraged + (float)gmo.textadjusty);

                    textrenderer.Add(null, gmo.name, fnt, Color.White, Color.Transparent, bestpos, new Vector3(sizeofname,0,0),new Vector3(0,0,0), fmt, alphascale:10000, alphaend:1000);
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
