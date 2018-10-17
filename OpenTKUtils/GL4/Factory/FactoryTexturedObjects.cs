/*
 * Copyright © 2015 - 2018 EDDiscovery development team
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
 * 
 * EDDiscovery is not affiliated with Frontier Developments plc.
 */

using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;

namespace OpenTKUtils.GL4
{
    // Factory created GLTexturedObjects for you

    public class GLTexturedObjectFactory
    {
        public static GLVertexTextured[] CreateTexturedCubeFromTriangles(float side, Vector3? pos = null)
        {
            return CreateTexturedCubeFromTriangles(side, new GLCubeObjectFactory.Sides[] { GLCubeObjectFactory.Sides.All }, pos);
        }
        public static GLVertexTextured[] CreateTexturedCubeFromTriangles(float side, GLCubeObjectFactory.Sides[] sides, Vector3? pos = null)
        {
            float w = 1.0f, h = 1.0f;       // presume bitmap is square since cube is square

            Vector2[] coords = new Vector2[]
            {
                new Vector2(w, h),      // lower right triangle
                new Vector2(w, 0),
                new Vector2(0, h),
                new Vector2(0, h),      // upper left triangle
                new Vector2(w, 0),
                new Vector2(0, 0),
            };

            var vertexes = GLCubeObjectFactory.CreateSolidCubeFromTriangles(side, sides, pos);     // get cube from factory

            var array = Array.ConvertAll<Vector4, GLVertexTextured>(vertexes, (a) => { return new GLVertexTextured(a); });

            for (int index = 0; index < vertexes.Length; index++)       // fill in texture co-ords, they repeat per face.
                array[index].TextureCoordinate = coords[index % 6];
            return array;
        }

        // presumed to use the whole bitmap

        public static GLVertexTextured[] CreateTexturedQuad(float width, int bitmapwidth, int bitmapheight, Vector3 rotation, Vector3? pos = null, float scale = 1.0f)
        {
            return CreateTexturedQuad(width, width * (float)bitmapheight / (float)bitmapwidth, rotation, pos, scale);
        }

        public static GLVertexTextured[] CreateTexturedQuad(float widthheight, Vector3 rotation, Vector3? pos = null, float scale = 1.0f)
        {
            return CreateTexturedQuad(widthheight, widthheight, rotation, pos, scale);
        }

        public static GLVertexTextured[] CreateTexturedQuad(float width, float height, Vector3 rotation, Vector3? pos = null, float scale = 1.0f)
        {
            width = width / 2.0f * scale;
            height = height / 2.0f * scale;

            GLVertexTextured[] vertices =
            {
                new GLVertexTextured(new Vector4(-width, 0, -height, 1.0f),   new Vector2(0, 1.0f)),
                new GLVertexTextured(new Vector4(+width, 0, -height, 1.0f),    new Vector2(1.0f, 1.0f)),
                new GLVertexTextured(new Vector4(+width, 0, +height, 1.0f),    new Vector2(1.0f, 0)),
                new GLVertexTextured(new Vector4(-width, 0, +height, 1.0f),    new Vector2(0, 0)),
            };

            if (pos != null)
                vertices.Translate(pos.Value);

            if (rotation.Length > 0)
            {
                Matrix4 transform = Matrix4.Identity;                   // identity nominal matrix, dir is in degrees
                transform *= Matrix4.CreateRotationX((float)(rotation.X * Math.PI / 180.0f));
                transform *= Matrix4.CreateRotationY((float)(rotation.Y * Math.PI / 180.0f));
                transform *= Matrix4.CreateRotationZ((float)(rotation.Z * Math.PI / 180.0f));
                vertices.Transform(transform);
            }

            return vertices;
        }

        static public GLVertexTextured[] CreateTexturedSphereFromTriangles(int recursionLevel, float size, Vector3? pos = null)
        {
            var faces = GLSphereObjectFactory.CreateSphereFaces(recursionLevel,size);

            // done, now add triangles to mesh
            var vertices = new List<GLVertexTextured>();

            foreach (var tri in faces)
            {
                var uv1 = GetSphereCoord(tri.V1);
                var uv2 = GetSphereCoord(tri.V2);
                var uv3 = GetSphereCoord(tri.V3);
                FixColorStrip(ref uv1, ref uv2, ref uv3);
                vertices.Add(new GLVertexTextured(new Vector4(tri.V1, 1), uv1));
                vertices.Add(new GLVertexTextured(new Vector4(tri.V2, 1), uv2));
                vertices.Add(new GLVertexTextured(new Vector4(tri.V3, 1), uv3));
            }

            var array = vertices.ToArray();

            if (pos != null)
                array.Translate(pos.Value);

            return array;
        }

        static private void FixColorStrip(ref Vector2 uv1, ref Vector2 uv2, ref Vector2 uv3)
        {
            if ((uv1.X - uv2.X) >= 0.8f)
                uv1.X -= 1;
            if ((uv2.X - uv3.X) >= 0.8f)
                uv2.X -= 1;
            if ((uv3.X - uv1.X) >= 0.8f)
                uv3.X -= 1;

            if ((uv1.X - uv2.X) >= 0.8f)
                uv1.X -= 1;
            if ((uv2.X - uv3.X) >= 0.8f)
                uv2.X -= 1;
            if ((uv3.X - uv1.X) >= 0.8f)
                uv3.X -= 1;
        }

        public static Vector2 GetSphereCoord(Vector3 i)
        {
            var len = i.Length;
            Vector2 uv;
            uv.Y = (float)(Math.Acos(i.Y / len) / Math.PI);
            uv.X = -(float)((Math.Atan2(i.Z, i.X) / Math.PI + 1.0f) * 0.5f);
            return uv;
        }
    }

}
