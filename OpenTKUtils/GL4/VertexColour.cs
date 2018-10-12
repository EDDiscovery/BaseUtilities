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
    public struct VertexColour
    {
        public const int Size = (4 + 4) * 4; // size of struct in bytes

        private Vector4 Position;
        private Color4 Color;

        public VertexColour(Vector4 position, Color4 color)
        {
            Position = position;
            Color = color;
        }

        public void Translate(Vector3 offset)
        {
            Position.X += offset.X;
            Position.Y += offset.Y;
            Position.Z += offset.Z;
        }

        static public void Translate(VertexColour[] vertices, Vector3 pos)
        {
            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Translate(pos);

        }

        static public Color4 ColorFrom(Color4[] array , int index)      // helper for color arrays
        {
            index = index % array.Length;
            return array[index];
        }
    }
}
