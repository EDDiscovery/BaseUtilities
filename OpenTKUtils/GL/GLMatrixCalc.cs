/*
 * Copyright 2015 - 2020 EDDiscovery development team
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
using System.Diagnostics;
using System.Drawing;

namespace OpenTKUtils
{
    // GL           World Space                         View Space                                  Clip Space
    //           p5----------p6		                  p5----------p6                     Zfar     p5----------p6 1 = far clip
    //          /|           /|                      /|           /|                             /|           /|
    //         / |          / | 	                / |          / |                            / |          / | 
    //        /  |         /  |                    /  |         /  |                           /  |         /  |
    //       /   p4-------/--p7  Z++              /   p4-------/--p7	                      /   p4-------/--p7	
    //      /   /        /   /                   /   /        /   /                          /   / +1     /   /
    //     p1----------p2   /	x ModelView     p1----------p2   /	    x Projection        p1----------p2   /	
    //     |  /         |  /                    |  /         |  /                           |  /         |  /  
    //     | /          | /			            | /          | /		                 -1 | /          | / +1		
    //     |/	        |/                      |/	         |/                             |/	         |/
    //     p0----------p3			            p0----------p3	  Viewer Pos      ZNear     p0----------p3  0 = near clip
    //     p0-p7 are in world coords                                                               -1
    // https://learnopengl.com/Getting-started/Coordinate-Systems

    public class GLMatrixCalc
    {
        public bool InPerspectiveMode { get; set; } = true;                     // perspective mode

        public float PerspectiveFarZDistance { get; set; } = 100000.0f;         // Model maximum z (corresponding to GL viewport Z=1)
                                                                                // Model minimum z (corresponding to GL viewport Z=0) 
        public float PerspectiveNearZDistance { get; set; } = 1f;               // Don't set this too small othersize depth testing stars going wrong as you exceed the depth buffer resolution

        public float OrthographicDistance { get; set; } = 5000.0f;              // Orthographic, give scale

        public float Fov { get; set; } = (float)(Math.PI / 2.0f);               // field of view, radians
        public float FovDeg { get { return Fov.Degrees(); } }
        public float FovFactor { get; set; } = 1.258925F;                       // scaling

        // after Calculate model matrix
        public Vector3 EyePosition { get; private set; }                       
        public Vector3 TargetPosition { get; private set; }                    
        public float EyeDistance { get; private set; }                         
        public Size ScreenSize { get; set; }
        public Matrix4 ModelMatrix { get; private set; }
        public Matrix4 ProjectionMatrix { get; private set; }
        public Matrix4 ProjectionModelMatrix { get; private set; }
        public Matrix4 GetResMat { get { return ProjectionModelMatrix; } }      // used for calculating positions on the screen from pixel positions.  Remembering Apollo

        // we want the axis orientation with +z away from us, +x to right, +y upwards.
        // this means we need to rotate the normal opengl model (+y down) 180 degrees around x - therefore flip y

        Vector3 cameranormal = new Vector3(0, 0, 1);            // normal to the camera (camera vector is (0,1,0))
        Matrix4 perspectiveflipaxis = Matrix4.CreateScale(new Vector3(1, -1, 1));   // flip y

        // notice flipping y affects the order of vertex for winding.. the vertex models need to have a opposite winding order
        // to make the ccw cull test work.

        // Calculate the model matrix, which is the model translated to world space then to view space..

        public void CalculateModelMatrix(Vector3 position, Vector3 eyeposition, Vector2 cameradirection, float camerarotation)       // We compute the model matrix, not opengl, because we need it before we do a Paint for other computations
        {
            TargetPosition = position;      // record for shader use
            EyePosition = eyeposition;
            EyeDistance = (position - eyeposition).Length;

            if (InPerspectiveMode)
            {
                Matrix3 transform = Matrix3.Identity;                   // identity nominal matrix, dir is in degrees

                transform *= Matrix3.CreateRotationX((float)(cameradirection.X.Radians()));     // rotate around cameradir
                transform *= Matrix3.CreateRotationY((float)(cameradirection.Y.Radians()));
                transform *= Matrix3.CreateRotationZ((float)(camerarotation.Radians()));

                Vector3 cameranormalrot = Vector3.Transform(cameranormal, transform);       // move cameranormal to rotate around current direction

                ModelMatrix = Matrix4.LookAt(EyePosition, position, cameranormalrot);   // from eye, look at target, with normal giving the rotation of the look
            }
            else
            {
                float orthoheight = (OrthographicDistance / 5.0f) * ScreenSize.Height / ScreenSize.Width;  // this comes from the projection calculation, and allows us to work out the scale factor the eye vs lookat has

                Matrix4 scale = Matrix4.CreateScale(orthoheight/EyeDistance);    // create a scale based on eyedistance compensated for by the orth projection scaling

                Matrix4 mat = Matrix4.CreateTranslation(-position.X, 0, -position.Z);        // we offset by the negative of the position to give the central look
                mat = Matrix4.Mult(mat, scale);          // translation world->View = scale + offset

                Matrix4 rotcam = Matrix4.CreateRotationX((float)((90) * Math.PI / 180.0f));        // flip 90 along the x axis to give the top down view
                ModelMatrix = Matrix4.Mult(mat, rotcam);
            }

            //System.Diagnostics.Debug.WriteLine("MM\r\n{0}", ModelMatrix);

            ProjectionModelMatrix = Matrix4.Mult(ModelMatrix, ProjectionMatrix);        // order order order ! so important.
        }

        // Projection matrix - projects the 3d model space to the 2D screen

        public void CalculateProjectionMatrix(out float znear)
        {
            if (InPerspectiveMode)
            {                                                                   // Fov, perspective, znear, zfar
                ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(Fov, (float)ScreenSize.Width / ScreenSize.Height, PerspectiveNearZDistance, PerspectiveFarZDistance);
                znear = PerspectiveNearZDistance;
            }
            else
            {
                znear = -OrthographicDistance;
                float orthoheight = (OrthographicDistance / 5.0f) * ScreenSize.Height / ScreenSize.Width;
                ProjectionMatrix = Matrix4.CreateOrthographic(OrthographicDistance * 2.0f / 5.0f, orthoheight * 2.0F, -OrthographicDistance, OrthographicDistance);

                Matrix4 zoffset = Matrix4.CreateTranslation(0,0,0.5f);     // we ensure all z's are based around 0.5f.  W = 1 in clip space, so Z must be <= 1 to be visible
                ProjectionMatrix = Matrix4.Mult(ProjectionMatrix, zoffset); // doing this means that a control can display with a Z around low 0.
            }

            //System.Diagnostics.Debug.WriteLine("PM\r\n{0}", ProjectionMatrix);

            ProjectionMatrix = ProjectionMatrix * perspectiveflipaxis;

            ProjectionModelMatrix = Matrix4.Mult(ModelMatrix, ProjectionMatrix);
        }

        public bool FovScale(bool direction)        // direction true is scale up FOV - need to tell it its changed
        {
            float curfov = Fov;

            if (direction)
                Fov = (float)Math.Min(Fov * FovFactor, Math.PI * 0.8);
            else
                Fov /= (float)FovFactor;

            return curfov != Fov;
        }

        public Vector4 WorldToScreen(Vector4 pos, string debug = null)           // return.W = 1 if inside screen co-ord
        {
            Vector4 m = Vector4.Transform(pos, ModelMatrix);
            Vector4 p = Vector4.Transform(m, ProjectionMatrix);
            Vector4 c = p / p.W;
            bool outside = c.X <= -1 || c.X >= 1 || c.Y <= -1 || c.Y >= 1 || c.Z >= 1;    // in screen range and P.W must be less than p.Z far z clipping

            Vector4 s = new Vector4((c.X + 1) / 2 * ScreenSize.Width, (-c.Y + 1) / 2 * ScreenSize.Height, 0, outside ? 0:1 );
            if ( debug != null )
                System.Diagnostics.Debug.WriteLine("{0} {1} ->m {2} ->p {3} ->c {4} ->s {5}", debug, pos.ToStringVec(), m.ToStringVec("10:0.00"), p.ToStringVec("10:0.00"), c.ToStringVec("10:0.00"), s.ToStringVec("10:0.00") );
            return s;
        }
    }
}
