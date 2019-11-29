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
using System;

namespace OpenTKUtils
{
    public static class GLStaticsVector3
    {
        static public Vector3 Floor(this Vector3 v)
        {
            return new Vector3((float)Math.Floor(v.X), (float)Math.Floor(v.Y), (float)Math.Floor(v.Z));
        }

        static public Vector3 Fract(this Vector3 v)
        {
            return new Vector3((float)(v.X - Math.Floor(v.X)), (float)(v.Y - Math.Floor(v.Y)), (float)(v.Z - Math.Floor(v.Z)));
        }

        static public Vector3 FloorFract(this Vector3 v, out Vector3 fract) // floor and fract
        {
            float fx = (float)Math.Floor(v.X);
            float fy = (float)Math.Floor(v.Y);
            float fz = (float)Math.Floor(v.Z);
            fract = new Vector3(v.X - fx, v.Y - fy, v.Z - fz);
            return new Vector3(fx, fy, fz);
        }

        public static Vector3 Mix(Vector3 a, Vector3 b, float mix)
        {
            float x = (float)(a.X + (b.X - a.X) * mix);
            float y = (float)(a.Y - (b.Y - a.Y) * mix);
            float z = (float)(a.Z - (b.Z - a.Z) * mix);
            return new Vector3(x, y, z);
        }

        static public Vector3 Abs(this Vector3 v)
        {
            return new Vector3(Math.Abs(v.X), Math.Abs(v.Y), Math.Abs(v.Z));
        }


        public static Vector3 AzEl(this Vector3 curpos, Vector3 target, bool returndegrees)     // az and elevation between curpos and target
        {
            Vector3 delta = Vector3.Subtract(target, curpos);
            //Console.WriteLine("{0}->{1} d {2}", curpos, target, delta);

            float radius = delta.Length;

            if (radius < 0.1)
                return new Vector3(180, 0, 0);     // point forward, level

            float inclination = (float)Math.Acos(delta.Y / radius);
            float azimuth = (float)Math.Atan(delta.Z / delta.X);

            if (delta.X >= 0)      // atan wraps -90 (south)->+90 (north), then -90 to +90 around the y axis, going anticlockwise
                azimuth = (float)(Math.PI / 2) - azimuth;     // adjust
            else
                azimuth = -(float)(Math.PI / 2) - azimuth;

            if (returndegrees)
            {
                inclination = inclination.Degrees();
                azimuth = azimuth.Degrees();
            }

            //System.Diagnostics.Debug.WriteLine("inc " + inclination + " az " + azimuth + " delta" + delta);

            //System.Diagnostics.Debug.WriteLine(" -> inc " + inclination + " az " + azimuth);
            return new Vector3(inclination, azimuth, 0);
        }
    }
}
