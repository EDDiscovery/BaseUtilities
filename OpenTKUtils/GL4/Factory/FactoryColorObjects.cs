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
    // Factory created GLVertexColours for you

    public class GLColouredObjectFactory
    {
        public static GLVertexColour[] CreateSolidCubeFromTriangles(float side, Color4[] color, Vector3? pos = null)
        {
            return CreateSolidCubeFromTriangles(side, new GLCubeObjectFactory.Sides[] { GLCubeObjectFactory.Sides.All }, color, pos);
        }

        public static GLVertexColour[] CreateSolidCubeFromTriangles(float side, GLCubeObjectFactory.Sides[] sides, Color4[] color, Vector3? pos = null)
        {
            Vector4[] vl = GLCubeObjectFactory.CreateSolidCubeFromTriangles(side, sides, pos);
            List<GLVertexColour> list = new List<GLVertexColour>();

            int cindex = 0;
            foreach (var v in vl)
                list.Add(new GLVertexColour(v, color.ColorFrom(cindex++)));

            return list.ToArray();
        }

        public static GLVertexColour[] CreateVertexPointCube(float side, Color4[] color, Vector3? pos = null)
        {
            var vl = GLCubeObjectFactory.CreateVertexPointCube(side, pos);
            List<GLVertexColour> list = new List<GLVertexColour>();

            int cindex = 0;
            foreach (var v in vl)
                list.Add(new GLVertexColour(v, color.ColorFrom(cindex++)));

            return list.ToArray();
        }

        public static GLVertexColour[] CreateLines(Vector3 startpos, Vector3 endpos, Vector3 offset, int lines, Color4[] colors)
        {
            GLVertexColour[] vertices = new GLVertexColour[lines * 2];

            for (int i = 0; i < lines; i++)
            {
                vertices[i * 2] = new GLVertexColour(new Vector4(startpos.X, startpos.Y, startpos.Z, 1.0f), colors.ColorFrom(i * 2));
                vertices[i * 2 + 1] = new GLVertexColour(new Vector4(endpos.X, endpos.Y, endpos.Z, 1.0f), colors.ColorFrom(i * 2 + 1));
                startpos += offset;
                endpos += offset;
            }

            return vertices;
        }

        public static GLVertexColour[] CreateSphere(int recursion, float size, Color4[] colours, Vector3? pos = null)
        {
            var s = GLSphereObjectFactory.CreateSphereFromTriangles(recursion, size, pos);
            GLVertexColour[] array = new GLVertexColour[s.Length];
            for (int i = 0; i < s.Length; i++)
            {
                array[i].Vertex = s[i];
                array[i].Color = colours.ColorFrom(i);
            }

            return array;
        }

        public static GLVertexColour[] CreateVertexColour(Vector4[] points, Color4[] colours)
        {
            GLVertexColour[] array = new GLVertexColour[points.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i].Vertex = points[i];
                array[i].Color = colours.ColorFrom(i);
            }

            return array;
        }



    }

}
