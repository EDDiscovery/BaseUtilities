/*
 * Copyright © 2015 - 2016 EDDiscovery development team
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
using System;
using System.Diagnostics;

namespace OpenTKUtils.Common
{
    public class Fov
    {
        public float Current { get { return cameraFov; } }
        public float FovDeg { get { return (float)(cameraFov / Math.PI * 180); } }

        private const float fovdef = (float)(Math.PI / 2.0f);
        private float cameraFov = fovdef;     // Camera, in radians, 180/2 = 90 degrees

        public float FovFactor = 1.258925F;      // scaling

        public void Set(float radians)       // 0 = means default fov
        {
            cameraFov = radians > 0 ? radians : fovdef;
        }

        public bool Scale(bool direction)        // direction true is scale up FOV - need to tell it its changed
        {
            float curfov = cameraFov;

            if (direction)
                cameraFov = (float)Math.Min(cameraFov * FovFactor, Math.PI * 0.8);
            else
                cameraFov /= (float)FovFactor;

            return curfov != cameraFov;
        }
    }
}
