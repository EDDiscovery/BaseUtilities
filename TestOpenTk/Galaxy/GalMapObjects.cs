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

        public void CreateObjects(GLItemsList items, GLRenderProgramSortedList rObjects, GalacticMapping galmap, int radius, int height, int bufferfindbinding)
        {
            Bitmap[] images = galmap.galacticMapTypes.Select(x => x.Image as Bitmap).ToArray();

            IGLTexture array2d = items.Add(new GLTexture2DArray(images, mipmaplevel:1, genmipmaplevel:3), "GalObjTex");

            objectshader = new GLTesselationShaderSinewaveInstanced(10,0.2f,1f, rotate: true, rotateelevation: false,
                                                                    autoscale:1000,autoscalemin:0.1f,autoscalemax:2);
            items.Add( objectshader, "ShaderGalObj");

            objectshader.StartAction += (s) =>
            {
                array2d.Bind(1);
                GLStatics.Check();
            };

            List<Vector4> instancepositions = new List<Vector4>();

            foreach (var o in galmap.galacticMapObjects)
            {
                var ty = galmap.galacticMapTypes.Find(y => o.type == y.Typeid);
                if (ty.Image != null)
                {
                    instancepositions.Add(new Vector4(o.points[0].X, o.points[0].Y, o.points[0].Z, ty.Enabled ? ty.Index : -1));
                }
            }

            GLRenderControl rt = GLRenderControl.Patches(4);
            rt.DepthTest = false;

            GLRenderableItem ri = GLRenderableItem.CreateVector4Vector4(items, rt,
                                GLShapeObjectFactory.CreateQuad2(50.0f, 50.0f), instancepositions.ToArray(),
                                ic: instancepositions.Count, seconddivisor: 1);

            //modeltexworldbuffer = items.LastBuffer();
            //int modelpos = modeltexworldbuffer.Positions[0];
       
            //worldpos = modeltexworldbuffer.Positions[1];

            rObjects.Add(objectshader, ri);

            //var poivertex = new GLPLVertexShaderModelCoordWithWorldTranslationCommonModelTranslation(); // expecting 0=model, 1 = world, w is ignored for world
            //findshader = items.NewShaderPipeline("GEOMAP_FIND", poivertex, null, null, new GLPLGeoShaderFindTriangles(bufferfindbinding, 16), null, null, null);

            //rifind = GLRenderableItem.CreateVector4Vector4(items, GLRenderControl.Tri(), modeltexworldbuffer, modelpos, ri.DrawCount, 
            //                                                                modeltexworldbuffer, worldpos, null, ic: instancepositions.Count, seconddivisor: 1);

            GLStatics.Check();
            // UpdateEnables(galmap);
        }

        public GalacticMapObject FindPOI(Point l, GLRenderControl state, Size screensize, GalacticMapping galmap)
        {
            //var geo = findshader.Get<GLPLGeoShaderFindTriangles>(OpenTK.Graphics.OpenGL4.ShaderType.GeometryShader);
            //geo.SetScreenCoords(l, screensize);

            //rifind.Execute(findshader, state, null, true); // execute, discard

            //var res = geo.GetResult();
            //if (res != null)
            //{
            //    for (int i = 0; i < res.Length; i++)
            //    {
            //        System.Diagnostics.Debug.WriteLine(i + " = " + res[i]);
            //    }

            //    int instance = (int)res[0].Y;
            //    return galmap.galacticMapObjects[instance];
            //}

            return null;
        }

        private void UpdateEnables(GalacticMapping galmap)           // update the enable state of each item
        {
            //modeltexworldbuffer.StartWrite(worldpos);

            //foreach (var o in galmap.galacticMapObjects)
            //{
            //    var ty = galmap.galacticMapTypes.Find(y => o.type == y.Typeid);
            //    if (ty.Image != null)
            //    {
            //        modeltexworldbuffer.Write(new Vector4(o.points[0].X, o.points[0].Y, o.points[0].Z, ty.Enabled ? ty.Index : -1));
            //    }
            //}

            //modeltexworldbuffer.StopReadWrite();
        }

        public void Update(long time, float eyedistance)
        {
            const int rotperiodms = 5000;
            time = time % rotperiodms;
            float fract = (float)time / rotperiodms;
            System.Diagnostics.Debug.WriteLine("Time " + time + "Phase " + fract);
            objectshader.Phase = fract;
        }

        private GLTesselationShaderSinewaveInstanced objectshader;

      //  private GLBuffer modeltexworldbuffer;
      //  private int worldpos;

     //   private GLShaderPipeline findshader;
     //   private GLRenderableItem rifind;


    }

}
