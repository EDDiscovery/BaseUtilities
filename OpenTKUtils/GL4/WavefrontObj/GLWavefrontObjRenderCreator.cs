using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKUtils.GL4
{
    public class GLWavefrontObjCreator
    {
        public Color DefaultColour { get; set; } = Color.Transparent;

        private GLItemsList items;
        private GLRenderProgramSortedList rlist;
        private GLUniformColourShaderWithObjectTranslation shadercolour = null;
        private GLTexturedShaderWithObjectTranslation shadertexture = null;

        public GLWavefrontObjCreator(GLItemsList itemsp, GLRenderProgramSortedList rlistp)
        {
            items = itemsp;
            rlist = rlistp;
        }

        public bool Create(List<GLWaveformObject> objects, Vector3 worldpos, Vector3 rotp, float scale = 1.0f)      
        {
            if (objects == null)
                return false;

            GLBuffer vert = null;
            GLRenderControl rts = GLRenderControl.Tri();
            bool okay = false;

            foreach (var obj in objects)
            {
                if (obj.Material.HasChars() && obj.Vertices.Vertices.Count > 0)
                {
                    if (vert == null)
                    {
                        vert = items.NewBuffer();
                        vert.AllocateFill(obj.Vertices.Vertices.ToArray(), obj.Vertices.TextureVertices2.ToArray());    // store all vertices and textures into 
                    }

                    bool textured = obj.Indices.TextureIndices.Count > 0;

                    string name = obj.Objectname != null ? obj.Objectname : obj.Groupname;

                    if (textured)
                    {
                        IGLTexture tex = items.Contains(obj.Material) ? items.Tex(obj.Material) : null;

                        if (tex == null)
                            return false;

                        if (shadertexture == null)
                        {
                            shadertexture = new GLTexturedShaderWithObjectTranslation();
                            items.Add(shadertexture);
                        }

                        obj.Indices.RefactorVertexIndiciesIntoTriangles();

                        var ri = GLRenderableItem.CreateVector4Vector2(items, rts, vert, vert.Positions[0], vert.Positions[1], 0,
                                new GLRenderDataTranslationRotationTexture(tex, worldpos, rotp, scale));           // renderable item pointing to vert for vertexes

                        ri.CreateElementIndex(items.NewBuffer(), obj.Indices.VertexIndices.ToArray(), 0);       // using the refactored indexes, create an index table and use

                        rlist.Add(shadertexture, name, ri);
                        okay = true;
                    }
                    else
                    {
                        Color c = Color.FromName(obj.Material);

                        if (c.A == 0 && c.R == 0 && c.G == 0 && c.B == 0)
                        {
                            if (DefaultColour != Color.Transparent)
                                c = DefaultColour;
                            else
                                return false;
                        }

                        if (shadercolour == null)
                        {
                            shadercolour = new GLUniformColourShaderWithObjectTranslation();
                            items.Add(shadercolour);
                        }

                        obj.Indices.RefactorVertexIndiciesIntoTriangles();

                        var ri = GLRenderableItem.CreateVector4(items, rts, vert, 0, 0, new GLRenderDataTranslationRotationColour(c, worldpos, rotp, scale));           // renderable item pointing to vert for vertexes
                        ri.CreateElementIndex(items.NewBuffer(), obj.Indices.VertexIndices.ToArray(), 0);       // using the refactored indexes, create an index table and use

                        rlist.Add(shadercolour, name, ri);
                        okay = true;
                    }

                }
            }

            return okay;
        }
    }
}
