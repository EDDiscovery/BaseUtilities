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

namespace OpenTKUtils.GL4
{
    public static class GL4Statics
    {
        static public void Translate(this Vector4 Vertex, Vector3 offset)
        {
            Vertex.X += offset.X;
            Vertex.Y += offset.Y;
            Vertex.Z += offset.Z;
        }

        static public void Translate(this Vector4[] vertices, Vector3 pos)
        {
            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Translate(pos);
        }

        static public void Transform(this Vector4[] vertices, Matrix4 trans)
        {
            for (int i = 0; i < vertices.Length; i++)
                vertices[i] = Vector4.Transform(vertices[i], trans);
        }

        static public Color4 ColorFrom(this Color4[] array, int index)      // helper for color arrays
        {
            index = index % array.Length;
            return array[index];
        }

        static public void Translate(this GLVertexTextured[] vertices, Vector3 pos)
        {
            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Vertex.Translate(pos);
        }

        static public void Transform(this GLVertexTextured[] vertices, Matrix4 trans)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].Vertex = Vector4.Transform(vertices[i].Vertex, trans);
            }
        }

    }
}
