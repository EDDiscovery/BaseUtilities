/*
 * Copyright 2019-2020 Robbyxp1 @ github.com
 * 
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software distributed under
 * the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF
 * ANY KIND, either express or implied. See the License for the specific language
 * governing permissions and limitations under the License.
 */

using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace OpenTKUtils
{
    public class GLMeshVertices     // Vertex store for verts, normals, textures
    {
        public List<Vector4> Vertices { get; set; }

        public List<Vector3> TextureVertices { get; set; }
        public List<Vector2> TextureVertices2 { get { return TextureVertices.Select(x => new Vector2(x.X, x.Y)).ToList(); } }
        public Vector2[] TextureVertices2Array { get { return TextureVertices.Select(x => new Vector2(x.X, x.Y)).ToArray(); } }

        public List<Vector3> Normals { get; set; }

        public GLMeshVertices(List<Vector4> verts, List<Vector3> texvert, List<Vector3> norms,
                        List<uint> vertindex, List<uint> texindex, List<uint> normindex)
        {
            Vertices = verts;
            TextureVertices = texvert;
            Normals = norms;
        }

        public GLMeshVertices()
        {
            Create();
        }

        public void Create()
        {
            Vertices = new List<Vector4>();
            TextureVertices = new List<Vector3>();
            Normals = new List<Vector3>();
        }
    }

    public class GLMeshIndices // indices store for vertex, normals, textures
    {
        public List<uint> VertexIndices { get; set; }
        public uint[] VertexIndicesArray { get { return VertexIndices.ToArray(); } }
        public List<uint> TextureIndices { get; set; }
        public List<uint> NormalIndices { get; set; }

        public GLMeshIndices(List<Vector4> verts, List<Vector3> texvert, List<Vector3> norms,
                        List<uint> vertindex, List<uint> texindex, List<uint> normindex)
        {
            VertexIndices = vertindex;
            TextureIndices = texindex;
            NormalIndices = normindex;
        }

        public GLMeshIndices()
        {
            Create();
        }

        public void Create()
        {
            VertexIndices = new List<uint>();
            TextureIndices = new List<uint>();
            NormalIndices = new List<uint>();
        }

        // adjust polygons into triangles

        public void RefactorVertexIndiciesIntoTriangles(bool ccw = true)
        {
            var newVertexIndices = new List<uint>();
            var newTextureIndices = new List<uint>();
            var newNormalIndices = new List<uint>();

            for (int i = 1; i <= VertexIndices.Count - 2; i++)
            {
                int f = i, s = i + 1;
                if (!ccw)
                {
                    f = i + 1; s = i;
                }

                newVertexIndices.Add(VertexIndices[0]);
                newVertexIndices.Add(VertexIndices[f]);
                newVertexIndices.Add(VertexIndices[s]);

                if (TextureIndices.Count > 0)
                {
                    newTextureIndices.Add(TextureIndices[0]);
                    newTextureIndices.Add(TextureIndices[f]);
                    newTextureIndices.Add(TextureIndices[s]);
                }

                if (NormalIndices.Count > 0)
                {
                    newNormalIndices.Add(NormalIndices[0]);
                    newNormalIndices.Add(NormalIndices[f]);
                    newNormalIndices.Add(NormalIndices[s]);
                }
            }

            VertexIndices = newVertexIndices;
            TextureIndices = newTextureIndices;
            NormalIndices = newNormalIndices;
        }

    }

    public class GLMesh
    {
        public GLMeshVertices Vertices;
        public GLMeshIndices Indices;
    }
}
