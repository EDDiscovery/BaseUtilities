﻿/*
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
    // Factory created Vector4 shapes..

    static public class GLPointsFactory
    {
        public static Vector3[] RandomStars(int number, float left, float right, float front, float back, float top, float bottom, Random rnd = null, int seed = 23)
        {
            if (rnd == null)
                rnd = new Random(seed);

            Vector3[] array = new Vector3[number];

            for (int s = 0; s < number; s++)
            {
                float x = rnd.Next(100000) * (right - left) / 100000.0f + left;
                float y = rnd.Next(100000) * (top - bottom) / 100000.0f + bottom;
                float z = rnd.Next(100000) * (back - front) / 100000.0f + front;

                array[s] = new Vector3(x, y, z);
            }

            return array;
        }

        public static Vector4[] RandomStars4(int number, float left, float right, float front, float back, float top, float bottom, Random rnd = null, int seed = 23, float w = 1)
        {
            if (rnd == null)
                rnd = new Random(seed);

            Vector4[] array = new Vector4[number];

            for (int s = 0; s < number; s++)
            {
                float x = rnd.Next(100000) * (right - left) / 100000.0f + left;
                float y = rnd.Next(100000) * (top - bottom) / 100000.0f + bottom;
                float z = rnd.Next(100000) * (back - front) / 100000.0f + front;

                array[s] = new Vector4(x, y, z, w);
            }

            return array;
        }

    }
}