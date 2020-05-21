using EliteDangerousCore.EDSM;
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
    public class GalMapObjects
    {
        public GalMapObjects()
        {
        }

        public bool Enable { get { return objectshader.Enable; } set { objectshader.Enable = value; } }

        public void CreateObjects(GLItemsList items, GLRenderProgramSortedList rObjects, GalacticMapping galmap, int bufferfindbinding)
        {
            Bitmap[] images = galmap.RenderableMapTypes.Select(x => x.Image as Bitmap).ToArray();
            IGLTexture texarray = items.Add(new GLTexture2DArray(images, mipmaplevel:1, genmipmaplevel:3), "GalObjTex");

            var vert = new GLPLVertexScaleLookat(true, false, false, 1000, 0.1f, 2f);
            var tcs = new GLPLTesselationControl(10f);
            tes = new GLPLTesselationEvaluateSinewave(0.2f,1f);
            var frag = new GLPLFragmentShaderTexture2DDiscard(1);

            objectshader = new GLShaderPipeline(vert, tcs, tes, null, frag);
            items.Add( objectshader, "ShaderGalObj");

            objectshader.StartAction += (s) =>
            {
                texarray.Bind(1);
            };

            renderablegalmapobjects = galmap.RenderableMapObjects;       // Only images and enables

            List<Vector4> instancepositions = new List<Vector4>();
            foreach (var o in renderablegalmapobjects)
                instancepositions.Add(new Vector4(o.points[0].X, o.points[0].Y, o.points[0].Z, o.galMapType.Enabled ? o.galMapType.Index : -1));

            GLRenderControl rt = GLRenderControl.Patches(4);
            rt.DepthTest = false;

            ridisplay = GLRenderableItem.CreateVector4Vector4(items, rt,
                                GLShapeObjectFactory.CreateQuad2(50.0f, 50.0f), instancepositions.ToArray(),
                                ic: renderablegalmapobjects.Length, seconddivisor: 1);

            rObjects.Add(objectshader, "GalObj", ridisplay);

            // find

            modelworldbuffer = items.LastBuffer();
            int modelpos = modelworldbuffer.Positions[0];
            worldpos = modelworldbuffer.Positions[1];

            var geofind = new GLPLGeoShaderFindTriangles(bufferfindbinding, 16);        // pass thru normal vert/tcs/tes then to geoshader for results
            findshader = items.NewShaderPipeline("GEOMAP_FIND", vert, tcs, tes, new GLPLGeoShaderFindTriangles(bufferfindbinding, 16), null, null, null);

            rifind = GLRenderableItem.CreateVector4Vector4(items, GLRenderControl.Patches(4), modelworldbuffer, modelpos, ridisplay.DrawCount, 
                                                                            modelworldbuffer, worldpos, null, ic: renderablegalmapobjects.Length, seconddivisor: 1);

            GLStatics.Check();
        }

        public GalacticMapObject FindPOI(Point l, GLRenderControl state, Size screensize, GalacticMapping galmap)
        {
            if (!objectshader.Enable)
                return null;

            var geo = findshader.Get<GLPLGeoShaderFindTriangles>(OpenTK.Graphics.OpenGL4.ShaderType.GeometryShader);
            geo.SetScreenCoords(l, screensize);

            GLStatics.Check();
            rifind.Execute(findshader, state, null, true); // execute, discard

            var res = geo.GetResult();
            if (res != null)
            {
                for (int i = 0; i < res.Length; i++)
                {
                    System.Diagnostics.Debug.WriteLine(i + " = " + res[i]);
                }

                int instance = (int)res[0].Y;
                return renderablegalmapobjects[instance];
            }

            return null;
        }

        public void UpdateEnables(GalacticMapping galmap)           // rewrite 
        {
            modelworldbuffer.StartWrite(worldpos);

            renderablegalmapobjects = galmap.RenderableMapObjects; // update

            foreach (var o in renderablegalmapobjects)
                modelworldbuffer.Write(new Vector4(o.points[0].X, o.points[0].Y, o.points[0].Z, o.galMapType.Enabled ? o.galMapType.Index : -1));

            modelworldbuffer.StopReadWrite();

            ridisplay.InstanceCount = rifind.InstanceCount = renderablegalmapobjects.Length;
        }

        public void Update(long time, float eyedistance)
        {
            const int rotperiodms = 5000;
            time = time % rotperiodms;
            float fract = (float)time / rotperiodms;
     //       System.Diagnostics.Debug.WriteLine("Time " + time + "Phase " + fract);
            tes.Phase = fract;
        }


        private GLPLTesselationEvaluateSinewave tes;
        private GLShaderPipeline objectshader;
        private GLBuffer modelworldbuffer;
        private int worldpos;
        private GLRenderableItem ridisplay;

        private GLShaderPipeline findshader;
        private GLRenderableItem rifind;

        GalacticMapObject[] renderablegalmapobjects;        // ones which render..


    }

}
