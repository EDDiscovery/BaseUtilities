/*
 * Copyright 2019-2020 Robbyxp1 @ github.com
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
using OpenTK.Graphics;
using System;
using System.Collections.Generic;

namespace OpenTKUtils.GL4
{
    // Factory created Vector4 shapes..  Rotations are in radians

    static public class GLShapeObjectFactory
    {
        public static Vector4[] CreateLines(Vector3 startpos, Vector3 endpos, Vector3 offset, int lines)
        {
            Vector4[] vertices = new Vector4[lines * 2];

            for (int i = 0; i < lines; i++)
            {
                vertices[i * 2] = new Vector4(new Vector4(startpos.X, startpos.Y, startpos.Z, 1.0f));
                vertices[i * 2 + 1] = new Vector4(new Vector4(endpos.X, endpos.Y, endpos.Z, 1.0f));
                startpos += offset;
                endpos += offset;
            }

            return vertices;
        }

        public static Vector4[] CreateBox(float width, float depth, float height, Vector3 pos , Vector3 ? rotationradians = null)
        {
            Vector4[] botvertices = CreateQuad(width, depth, pos: new Vector3(pos.X, pos.Y - height / 2, pos.Z));
            Vector4[] topvertices = CreateQuad(width, depth, pos: new Vector3(pos.X, pos.Y + height / 2, pos.Z));

            Vector4[] box = new Vector4[24];
            box[0] = botvertices[0];            box[1] = botvertices[1];            box[2] = botvertices[1];            box[3] = botvertices[2];
            box[4] = botvertices[2];            box[5] = botvertices[3];            box[6] = botvertices[3];            box[7] = botvertices[0];
            box[8] = topvertices[0];            box[9] = topvertices[1];            box[10] = topvertices[1];            box[11] = topvertices[2];
            box[12] = topvertices[2];            box[13] = topvertices[3];            box[14] = topvertices[3];            box[15] = topvertices[0];
            box[16] = botvertices[0];            box[17] = topvertices[0];            box[18] = botvertices[1];            box[19] = topvertices[1];
            box[20] = botvertices[2];            box[21] = topvertices[2];            box[22] = botvertices[3];            box[23] = topvertices[3];

            GLStaticsVector4.RotPos(ref box, rotationradians);

            return box;
        }

        public static Vector4[] CreateQuad(float width, int bitmapwidth, int bitmapheight, Vector3? rotationradians = null, Vector3? pos = null, float scale = 1.0f)
        {
            return CreateQuad(width, width * (float)bitmapheight / (float)bitmapwidth, rotationradians, pos, scale);
        }

        public static Vector4[] CreateQuad(float widthheight, Vector3? rotationradians = null, Vector3? pos = null, float scale = 1.0f)
        {
            return CreateQuad(widthheight, widthheight, rotationradians, pos, scale);
        }

        public static Vector4[] CreateQuad(float width, float height, Vector3? rotationradians = null, Vector3? pos = null, float scale = 1.0f)        
        {
            width = width / 2.0f * scale;
            height = height / 2.0f * scale;

            Vector4[] vertices1 =
            {
                new Vector4(-width, 0, -height, 1.0f),          // -, -
                new Vector4(+width, 0, -height, 1.0f),          // +, -
                new Vector4(+width, 0, +height, 1.0f),          // +, +
                new Vector4(-width, 0, +height, 1.0f),          // -, +
            };

            GLStaticsVector4.RotPos(ref vertices1, rotationradians, pos);

            return vertices1;
        }

        static public Vector2[] TexQuad = new Vector2[]
        {
            new Vector2(0, 1.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(1.0f, 0),
            new Vector2(0, 0),
        };

        public static Vector4[] CreateQuad2(float width, float height, Vector3? rotationradians = null, Vector3? pos = null, float scale = 1.0f)
        {
            width = width / 2.0f * scale;
            height = height / 2.0f * scale;

            Vector4[] vertices2 =
            {
               new Vector4(-width, 0, -height, 1.0f),      //BL
               new Vector4(+width, 0, -height, 1.0f),      //BR
               new Vector4(-width, 0, +height, 1.0f),      //TL
               new Vector4(+width, 0, +height, 1.0f),      //TR
            };

            GLStaticsVector4.RotPos(ref vertices2, rotationradians, pos);

            return vertices2;
        }


    }
}