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


using System;
using System.Collections.Generic;
using OpenTK;

namespace OpenTKUtils.GL4
{
    // implements open GL standards on writing data to a GLBuffer.
    
    public abstract class GLLayoutStandards
    {
        public int CurrentPos { get; protected set; } = 0;
        public IntPtr CurrentPtr { get; protected set; } = IntPtr.Zero;

        public int BufferSize { get; protected set; } = 0;      // 0 means not complete and allocated, otherwise allocated to this size.

        public bool IsAllocated { get { return BufferSize > 0; } }
        public bool NotAllocated { get { return BufferSize == 0; } }

        public bool Std430 { get; set; } = false;               // you can change your mind, in case of debugging etc.

        public List<int> Positions = new List<int>();           // at each alignment during Fill, a position is stored.  Not for map alignments

        public const int Vec4size = 4 * sizeof(float);
        public const int Vec3size = Vec4size;
        public const int Vec2size = 2 * sizeof(float);
        public const int Mat4size = 4 * 4 * sizeof(float);

        // std140 alignment: scalars are N, 2Vec = 2N, 3Vec = 4N, 4Vec = 4N. 
        //                   array alignment is vec4 for all, stride vec4 
        // std430 alignment: scalars are N, 2Vec = 2N, 3Vec = 4N, 4Vec = 4N. 
        //                   array alignment is same as scalar, stride is as per scalar

        public GLLayoutStandards(bool std430 = false)
        {
            CurrentPos = 0;
            BufferSize = 0;
            Std430 = std430;
        }

        protected int AlignScalar(int scalarsize, int datasize)           // align a scalar
        {
            if ( scalarsize > 1 )
                CurrentPos = (CurrentPos + scalarsize - 1) & (~(scalarsize - 1));

            int pos = CurrentPos;
            CurrentPos += datasize;
            Positions.Add(pos);
            System.Diagnostics.Debug.Assert(CurrentPos <= BufferSize);
            return pos;
        }

        protected int AlignArray(int scalarsize, int datasize)
        {
            int arrayalign = Std430 ? scalarsize : Vec4size;

            if (arrayalign > 1)
                CurrentPos = (CurrentPos + arrayalign - 1) & (~(arrayalign - 1));

            int pos = CurrentPos;
            CurrentPos += datasize;
            Positions.Add(pos);
            System.Diagnostics.Debug.Assert(CurrentPos <= BufferSize);
            return pos;
        }

        protected IntPtr AlignScalarPtr(int scalarsize)
        {
            if (scalarsize > 1)
            {
                int newoffset = (CurrentPos + scalarsize - 1) & (~(scalarsize - 1));
                CurrentPtr += newoffset - CurrentPos;
                CurrentPos = newoffset;
            }

            IntPtr r = CurrentPtr;
            CurrentPtr += scalarsize;
            CurrentPos += scalarsize;
            System.Diagnostics.Debug.Assert(CurrentPos <= BufferSize);
            return r;
        }

        protected Tuple<IntPtr,int> AlignArrayPtr(int scalarsize, int count)    // return pos, stride
        {
            int arrayalign = Std430 ? scalarsize : Vec4size;

            if (arrayalign > 1)
            {
                int newoffset = (CurrentPos + arrayalign - 1) & (~(arrayalign - 1));
                CurrentPtr += newoffset - CurrentPos;
                CurrentPos = newoffset;
            }

            IntPtr r = CurrentPtr;
            CurrentPtr += arrayalign * count;
            CurrentPos += arrayalign * count;
            System.Diagnostics.Debug.Assert(CurrentPos <= BufferSize);
            return new Tuple<IntPtr, int>(r, arrayalign);
        }
    }

}