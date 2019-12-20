/*
 * Copyright © 2019 Robbyxp1 @ github.com
 * Part of the EDDiscovery Project
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
    // Factory created Vector4 shapes..

    static public class GLTapeObjectFactory
    {
        // tape is segmented, and rotx determines if its flat to Y or not, use with TriangleStrip

        public static Vector4[] CreateTape(Vector3 start, Vector3 end, float width, float segmentlength = 1, float rotationaroundy = 0, bool ensureintegersamples = false)
        {
            float length = (end - start).Length;
            int innersegments = (int)(length / segmentlength);
            if ( ensureintegersamples )
            {
                segmentlength = length / innersegments;
            }

            Vector4[] tape = new Vector4[4 + 2 * innersegments];                // 4 start/end, plus 2 for any inners

            Vector3 vectorto = Vector3.Normalize(end - start);                  // vector between the points, normalised

            double xzangle = Math.Atan2(end.Z - start.Z, end.X - start.X);      // angle on the ZX plane between start/end

            Vector3 leftnormal = Vector3.TransformNormal(vectorto, Matrix4.CreateRotationY(-(float)Math.PI / 2)); // + is clockwise.  Generate normal to vector on left side
            Vector3 rightnormal = Vector3.TransformNormal(vectorto, Matrix4.CreateRotationY((float)Math.PI / 2)); // On right side.

            // the way this works, is that we rotate the l/r normals around Y, to align then with the YZ plane.
            // The rotation is the difference between the facing angle in the ZX plane (xzangle) and the YZ plane itself which is at +90degrees on the ZX plane
            // then we rotate around X
            // then we rotate it back by the facing angle
            // lets just say this took a bit of thinking about!  This is a generic way of rotating around an arbitary plane - rotate it back to a plane.

            double rotatetoyzangle = Math.PI / 2 - (xzangle + Math.PI / 2);           // angle to rotate back to the YZ plane

            leftnormal = Vector3.TransformNormal(leftnormal, Matrix4.CreateRotationY(-(float)rotatetoyzangle));     // rotate back to YZ plane
            leftnormal = Vector3.TransformNormal(leftnormal, Matrix4.CreateRotationX(-(float)rotationaroundy));     // rotate on the YZ plane
            leftnormal = Vector3.TransformNormal(leftnormal, Matrix4.CreateRotationY((float)rotatetoyzangle));      // rotate back to angle on XZ plane

            rightnormal = Vector3.TransformNormal(rightnormal, Matrix4.CreateRotationY(-(float)rotatetoyzangle));
            rightnormal = Vector3.TransformNormal(rightnormal, Matrix4.CreateRotationX(-(float)rotationaroundy));   
            rightnormal = Vector3.TransformNormal(rightnormal, Matrix4.CreateRotationY((float)rotatetoyzangle));

            leftnormal *= width;
            rightnormal *= width;

            Vector4 l = new Vector4(start.X + leftnormal.X, start.Y + leftnormal.Y, start.Z + leftnormal.Z, 1);
            Vector4 r = new Vector4(start.X + rightnormal.X, start.Y + rightnormal.Y, start.Z + rightnormal.Z, 1);
            Vector4 segoff = new Vector4((end.X - start.X) / length * segmentlength, (end.Y - start.Y) / length * segmentlength, (end.Z - start.Z) / length * segmentlength, 0);

            int i;
            for ( i = 0; i <= innersegments; i++ )   // include at least the start
            {
                tape[i * 2] = l;
                tape[i * 2 + 1] = r;
                l += segoff;
                r += segoff;
            }

            tape[i * 2 + 1] = new Vector4(end.X + rightnormal.X, end.Y + rightnormal.Y, end.Z + rightnormal.Z, 1);
            tape[i * 2 + 0] = new Vector4(end.X + leftnormal.X, end.Y + leftnormal.Y, end.Z + leftnormal.Z, 1);

            return tape;
        }


    }
}
