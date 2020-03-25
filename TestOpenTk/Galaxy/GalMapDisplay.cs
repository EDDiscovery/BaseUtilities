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
    class GalMapDisplay
    {
        public GalMapDisplay()
        {
        }

        public void CreateObjects(GLItemsList items, GLRenderProgramSortedList rObjects, GalacticMapping galmap, int bufferfindbinding)
        {
            Bitmap[] images = galmap.galacticMapTypes.Select(x => x.Image as Bitmap).ToArray();

            IGLTexture array2d = items.Add(new GLTexture2DArray(images, mipmaplevel:1, genmipmaplevel:3), "GalObjTex");

            items.Add( new GLMultipleTexturedBlended(false, 0), "ShaderGalObj");

            items.Shader("ShaderGalObj").StartAction += (s) =>
            {
                array2d.Bind(1);
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

            GLRenderControl rt = GLRenderControl.Tri();
            rt.DepthTest = false;

            GLRenderableItem ri = GLRenderableItem.CreateVector4Vector2Vector4(items, rt,
                               GLSphereObjectFactory.CreateTexturedSphereFromTriangles(2, 200.0f),
                               instancepositions.ToArray(), ic: instancepositions.Count, separbuf: false
                               );
            modeltexworldbuffer = items.LastBuffer();
            int modelpos = modeltexworldbuffer.Positions[0];
       
            worldpos = modeltexworldbuffer.Positions[2];

            rObjects.Add(items.Shader("ShaderGalObj"), ri);

            var poivertex = new GLPLVertexShaderModelCoordWithWorldTranslationCommonModelTranslation(); // expecting 0=model, 1 = world, w is ignored for world
            findshader = items.NewShaderPipeline("GEOMAP_FIND", poivertex, null, null, new GLPLGeoShaderFindTriangles(bufferfindbinding, 16), null, null, null);

            rifind = GLRenderableItem.CreateVector4Vector4(items, GLRenderControl.Tri(), modeltexworldbuffer, modelpos, ri.DrawCount, 
                                                                            modeltexworldbuffer, worldpos, null, ic: instancepositions.Count, seconddivisor: 1);

           // UpdateEnables(galmap);
        }

        public GalacticMapObject FindPOI(Point l, GLRenderControl state, Size screensize, GalacticMapping galmap)
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

                int instance = (int)res[0].Y;
                return galmap.galacticMapObjects[instance];
            }

            return null;
        }

        private void UpdateEnables(GalacticMapping galmap)           // update the enable state of each item
        {
            modeltexworldbuffer.StartWrite(worldpos);

            foreach (var o in galmap.galacticMapObjects)
            {
                var ty = galmap.galacticMapTypes.Find(y => o.type == y.Typeid);
                if (ty.Image != null)
                {
                    modeltexworldbuffer.Write(new Vector4(o.points[0].X, o.points[0].Y, o.points[0].Z, ty.Enabled ? ty.Index : -1));
                }
            }

            modeltexworldbuffer.StopReadWrite();
        }

        private GLBuffer modeltexworldbuffer;
        private int worldpos;

        private GLShaderPipeline findshader;
        private GLRenderableItem rifind;


    }

}
