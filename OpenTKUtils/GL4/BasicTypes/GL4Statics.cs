/*
 * Copyright 2019-2020 Robbyxp1 @ github.com
 * 
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

using OpenTK.Graphics.OpenGL4;
using System;
using System.Runtime.CompilerServices;

namespace OpenTKUtils
{
    public static class GL4Statics
    {
        public static DrawElementsType DrawElementsTypeFromMaxEID(uint eid)
        {
            if (eid < byte.MaxValue)
                return DrawElementsType.UnsignedByte;
            else if (eid < ushort.MaxValue)
                return DrawElementsType.UnsignedShort;
            else
                return DrawElementsType.UnsignedInt;
        }

        public static uint DrawElementsRestartValue(DrawElementsType t)
        {
            if (t == DrawElementsType.UnsignedByte)
                return 0xff;
            else if (t == DrawElementsType.UnsignedShort)
                return 0xffff;
            else
                return 0xffffffff;
        }
    }
}

