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
    // Factory created Sphere Faces for use

    static public class GLSphereObjectFactory
    {
        public struct Face
        {
            public Vector3 V1;
            public Vector3 V2;
            public Vector3 V3;

            public Face(Vector3 v1, Vector3 v2, Vector3 v3)
            {
                V1 = v1;
                V2 = v2;
                V3 = v3;
            }
        }

        public static List<Face> CreateSphereFaces(int recursion, float size)
        {
            var middlePointIndexCache = new Dictionary<long, int>();
            List<Vector3> points = new List<Vector3>();

            float t = (float)((1.0 + Math.Sqrt(5.0)) / 2.0);
            float s = 1;

            AddVertex(points, new Vector3(-s, t, 0));
            AddVertex(points, new Vector3(s, t, 0));
            AddVertex(points, new Vector3(-s, -t, 0));
            AddVertex(points, new Vector3(s, -t, 0));

            AddVertex(points, new Vector3(0, -s, t));
            AddVertex(points, new Vector3(0, s, t));
            AddVertex(points, new Vector3(0, -s, -t));
            AddVertex(points, new Vector3(0, s, -t));

            AddVertex(points, new Vector3(t, 0, -s));
            AddVertex(points, new Vector3(t, 0, s));
            AddVertex(points, new Vector3(-t, 0, -s));
            AddVertex(points, new Vector3(-t, 0, s));

            var faces = new List<Face>();

            // 5 faces around point 0
            faces.Add(new Face(points[0], points[11], points[5]));
            faces.Add(new Face(points[0], points[5], points[1]));
            faces.Add(new Face(points[0], points[1], points[7]));
            faces.Add(new Face(points[0], points[7], points[10]));
            faces.Add(new Face(points[0], points[10], points[11]));

            // 5 adjacent faces 
            faces.Add(new Face(points[1], points[5], points[9]));
            faces.Add(new Face(points[5], points[11], points[4]));
            faces.Add(new Face(points[11], points[10], points[2]));
            faces.Add(new Face(points[10], points[7], points[6]));
            faces.Add(new Face(points[7], points[1], points[8]));

            // 5 faces around point 3
            faces.Add(new Face(points[3], points[9], points[4]));
            faces.Add(new Face(points[3], points[4], points[2]));
            faces.Add(new Face(points[3], points[2], points[6]));
            faces.Add(new Face(points[3], points[6], points[8]));
            faces.Add(new Face(points[3], points[8], points[9]));

            // 5 adjacent faces 
            faces.Add(new Face(points[4], points[9], points[5]));
            faces.Add(new Face(points[2], points[4], points[11]));
            faces.Add(new Face(points[6], points[2], points[10]));
            faces.Add(new Face(points[8], points[6], points[7]));
            faces.Add(new Face(points[9], points[8], points[1]));

            // refine triangles
            for (int i = 0; i < recursion; i++)
            {
                var faces2 = new List<Face>();

                foreach (var tri in faces)
                {
                    // replace triangle by 4 triangles
                    int a = GetMiddlePoint(points, middlePointIndexCache, tri.V1, tri.V2);
                    int b = GetMiddlePoint(points, middlePointIndexCache, tri.V2, tri.V3);
                    int c = GetMiddlePoint(points, middlePointIndexCache, tri.V3, tri.V1);

                    faces2.Add(new Face(tri.V1, points[a], points[c]));
                    faces2.Add(new Face(tri.V2, points[b], points[a]));
                    faces2.Add(new Face(tri.V3, points[c], points[b]));
                    faces2.Add(new Face(points[a], points[b], points[c]));
                }

                faces = faces2;
            }

            size *= 0.5f;   // bacause its produced to the unit vector length on one side.. so 2 wide

            for (int i = 0; i < faces.Count; i++)
                faces[i] = new Face(faces[i].V1 * size, faces[i].V2 * size, faces[i].V3 * size);

            return faces;
        }

        public static Vector4[] CreateSphereFromTriangles(int recursion, float size, Vector3? pos = null)
        {
            var faces = CreateSphereFaces(recursion,size);
            List<Vector4> vertices = new List<Vector4>();

            foreach (var tri in faces)
            {
                vertices.Add(new Vector4(tri.V1, 1.0f));
                vertices.Add(new Vector4(tri.V2, 1.0f));
                vertices.Add(new Vector4(tri.V3, 1.0f));
            }

            var array = vertices.ToArray();
            if ( pos != null )
                array.Translate(pos.Value);

            return array;
        }

        static public Tuple<Vector4[],Vector2[]> CreateTexturedSphereFromTriangles(int recursionLevel, float size, Vector3? pos = null)
        {
            var faces = CreateSphereFaces(recursionLevel, size);
            Vector4[] coords = new Vector4[faces.Count * 3];
            Vector2[] texcoords = new Vector2[faces.Count * 3];

            int p = 0;
            foreach (var tri in faces)
            {
                var uv1 = GetSphereCoord(tri.V1);
                var uv2 = GetSphereCoord(tri.V2);
                var uv3 = GetSphereCoord(tri.V3);
                FixColorStrip(ref uv1, ref uv2, ref uv3);

                coords[p] = new Vector4(tri.V1, 1);
                texcoords[p++] = uv1;
                coords[p] = new Vector4(tri.V2, 1);
                texcoords[p++] = uv2;
                coords[p] = new Vector4(tri.V3, 1);
                texcoords[p++] = uv3;
            }

            return new Tuple<Vector4[], Vector2[]>(coords, texcoords);
        }


        private static int AddVertex(List<Vector3> _points, Vector3 p)
        {
            p.Normalize();
            _points.Add(p);
            return _points.Count - 1;
        }


        // return index of point in the middle of p1 and p2
        static private int GetMiddlePoint(List<Vector3> _points, Dictionary<long, int> _middlePointIndexCache, Vector3 point1, Vector3 point2)
        {
            long i1 = _points.IndexOf(point1);
            long i2 = _points.IndexOf(point2);
            // first check if we have it already
            var firstIsSmaller = i1 < i2;
            long smallerIndex = firstIsSmaller ? i1 : i2;
            long greaterIndex = firstIsSmaller ? i2 : i1;
            long key = (smallerIndex << 32) + greaterIndex;

            int ret;
            if (_middlePointIndexCache.TryGetValue(key, out ret))
            {
                return ret;
            }

            // not in cache, calculate it

            var middle = new Vector3(
                (point1.X + point2.X) / 2.0f,
                (point1.Y + point2.Y) / 2.0f,
                (point1.Z + point2.Z) / 2.0f);

            // add vertex makes sure point is on unit sphere
            int i = AddVertex(_points, middle);

            // store it, return index
            _middlePointIndexCache.Add(key, i);
            return i;
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