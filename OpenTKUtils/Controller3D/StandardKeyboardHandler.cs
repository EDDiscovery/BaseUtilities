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
using BaseUtils;
using OpenTK;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace OpenTKUtils.Common
{
    // standard keys used for movement in 3d programs

    static class StandardKeyboardHandler
    {
        static public bool Movement(KeyboardState kbd, Position pos, bool inperspectivemode, Vector3 cameraDir, int msticks, float distance, bool elitemovement)
        {
            Vector3 cameraActionMovement = Vector3.Zero;

            if (kbd.Shift)
                distance *= 2.0F;

            //Console.WriteLine("Distance " + distance + " zoom " + _zoom + " lzoom " + zoomlimited );
            if (kbd.IsAnyPressed(Keys.Left, Keys.A))
            {
                cameraActionMovement.X = -distance;
            }
            else if (kbd.IsAnyPressed(Keys.Right, Keys.D))
            {
                cameraActionMovement.X = distance;
            }

            if (kbd.IsAnyPressed(Keys.PageUp, Keys.R))
            {
                if (inperspectivemode)
                    cameraActionMovement.Z = distance;
            }
            else if (kbd.IsAnyPressed(Keys.PageDown, Keys.F))
            {
                if (inperspectivemode)
                    cameraActionMovement.Z = -distance;
            }

            if (kbd.IsAnyPressed(Keys.Up, Keys.W))
            {
                if (inperspectivemode)
                    cameraActionMovement.Y = distance;
                else
                    cameraActionMovement.Z = distance;
            }
            else if (kbd.IsAnyPressed(Keys.Down, Keys.S))
            {
                if (inperspectivemode)
                    cameraActionMovement.Y = -distance;
                else
                    cameraActionMovement.Z = -distance;
            }

            if (cameraActionMovement.LengthSquared > 0)
            {
                if (!inperspectivemode)
                    elitemovement = false;

                var rotZ = Matrix4.CreateRotationZ(DegreesToRadians(cameraDir.Z));
                var rotX = Matrix4.CreateRotationX(DegreesToRadians(cameraDir.X));
                var rotY = Matrix4.CreateRotationY(DegreesToRadians(cameraDir.Y));

                Vector3 requestedmove = new Vector3(cameraActionMovement.X, cameraActionMovement.Y, (elitemovement) ? 0 : cameraActionMovement.Z);

                var translation = Matrix4.CreateTranslation(requestedmove);
                var cameramove = Matrix4.Identity;
                cameramove *= translation;
                cameramove *= rotZ;
                cameramove *= rotX;
                cameramove *= rotY;

                Vector3 trans = cameramove.ExtractTranslation();

                if (elitemovement)                                   // if in elite movement, Y is not affected
                {                                                   // by ASDW.
                    trans.Y = 0;                                    // no Y translation even if camera rotated the vector into Y components
                    pos.Translate(trans);
                    pos.Y(-cameraActionMovement.Z);
                }
                else
                    pos.Translate(trans);

                return true;
            }
            else
                return false;
        }

        static public bool Zoom(KeyboardState kbd, Zoom zoom, int msticks)
        {
            float adjustment = 1.0f + ((float)msticks * 0.002f);

            bool changed = false;

            if (kbd.IsAnyPressed(Keys.Add, Keys.Z))
            {
                zoom.Multiply(adjustment);
                changed = true;
            }

            if (kbd.IsAnyPressed(Keys.Subtract, Keys.X))
            {
                zoom.Multiply(1.0f/adjustment);
                changed = true;
            }

            float newzoom = 0;

            if (kbd.IsPressed(Keys.D1))
                newzoom = zoom.ZoomMax;
            if (kbd.IsPressed(Keys.D2))
                newzoom = 100;                                                      // Factor 3 scale
            if (kbd.IsPressed(Keys.D3))
                newzoom = 33;
            if (kbd.IsPressed(Keys.D4))
                newzoom = 11F;
            if (kbd.IsPressed(Keys.D5))
                newzoom = 3.7F;
            if (kbd.IsPressed(Keys.D6))
                newzoom = 1.23F;
            if (kbd.IsPressed(Keys.D7))
                newzoom = 0.4F;
            if (kbd.IsPressed(Keys.D8))
                newzoom = 0.133F;
            if (kbd.IsPressed(Keys.D9))
                newzoom = zoom.ZoomMin;

            if (newzoom != 0)
            {
                zoom.GoTo(newzoom, -1);
                changed = true;
            }

            return changed;
        }

        static public bool Camera(KeyboardState kbd, Camera camera, int msticks)
        {
            Vector3 cameraActionRotation = Vector3.Zero;

            var angle = (float)msticks * 0.075f;
            if (kbd.IsPressed(Keys.NumPad4))
            {
                cameraActionRotation.Z = -angle;
            }
            if (kbd.IsPressed(Keys.NumPad6))
            {
                cameraActionRotation.Z = angle;
            }
            if (kbd.IsAnyPressed(Keys.NumPad5, Keys.NumPad2))
            {
                cameraActionRotation.X = -angle;
            }
            if (kbd.IsPressed(Keys.NumPad8))
            {
                cameraActionRotation.X = angle;
            }
            if (kbd.IsAnyPressed(Keys.NumPad7, Keys.Q))
            {
                cameraActionRotation.Y = angle;
            }
            if (kbd.IsAnyPressed(Keys.NumPad9, Keys.E))
            {
                cameraActionRotation.Y = -angle;
            }

            if (cameraActionRotation.LengthSquared > 0)
            {
                camera.Rotate(cameraActionRotation);
                return true;
            }
            else
                return false;
        }

        static private float DegreesToRadians(float angle)
        {
            return (float)(Math.PI * angle / 180.0);
        }
    }
}
