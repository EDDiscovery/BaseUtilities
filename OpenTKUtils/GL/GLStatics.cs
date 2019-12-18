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
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace OpenTKUtils
{
    public static class GLStatics
    {
        [System.Diagnostics.Conditional("DEBUG")]
        public static void Check([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            string errmsg = "";

            OpenTK.Graphics.OpenGL4.ErrorCode ec;

            while ((ec = OpenTK.Graphics.OpenGL4.GL.GetError()) != OpenTK.Graphics.OpenGL4.ErrorCode.NoError)     // call until no error
            {
                if (errmsg.IsEmpty())
                    errmsg += sourceFilePath + ":" + sourceLineNumber + Environment.NewLine;

                errmsg += "Error : " + ec.ToString() + Environment.NewLine;
            }

            if (errmsg.HasChars())
                System.Diagnostics.Debug.Assert(false, errmsg);
        }

        static bool? LastCullface = null;

        public static void CullFace(bool on)
        {
            if (LastCullface == null || LastCullface.Value != on)
            {
                if (on)
                    GL.Enable(EnableCap.CullFace);
                else
                    GL.Disable(EnableCap.CullFace);
                LastCullface = on;
            }
        }


        public static void DefaultCullFace()
        {
            CullFace(true);
        }


        static int? LastPatchSize = null;

        public static void PatchSize(int p)          // cache size for speed
        {
            if (LastPatchSize == null || LastPatchSize.Value != p)
            {
                GL.PatchParameter(PatchParameterInt.PatchVertices, p);
                LastPatchSize = p;
            }
        }

        static float? LastPointSize = null;

        public static void PointSizeByProgram()          // cache size for speed - 0 means use shader point size
        {
            PointSize(0);
        }

        public static void PointSize(float p)          // cache size for speed - 0 means use shader point size
        {
            if (LastPointSize == null || LastPointSize.Value != p)
            {
                if (p > 0)
                {
                    if (LastPointSize == null || LastPointSize == 0)  // if last was 0, turn off point size
                        GL.Disable(EnableCap.ProgramPointSize);

                    GL.PointSize(p);
                }
                else
                    GL.Enable(EnableCap.ProgramPointSize);

                LastPointSize = p;
            }
        }

        public static void DefaultPointSize()
        {
            PointSize(1);
        }

        static bool? LastPointSpriteEnable = null;

        public static void DisablePointSprite()          // cache size for speed 
        {
            if (LastPointSpriteEnable == null || LastPointSpriteEnable == true)
            {
                GL.Disable(EnableCap.PointSprite);
                LastPointSpriteEnable = false;
            }
        }

        public static void EnablePointSprite()          // cache size for speed 
        {
            if (LastPointSpriteEnable == null || LastPointSpriteEnable == false)
            {
                GL.Enable(EnableCap.PointSprite);
                LastPointSpriteEnable = true;
            }
        }

        static float? LastLineWidth = null;

        public static void LineWidth(float p)          // cache size for speed
        {
            if (LastLineWidth == null || LastLineWidth.Value != p)
            {
                GL.LineWidth(p);
                LastLineWidth = p;
            }
        }

        static bool? depthteststate = false;

        public static void DepthTest(bool state)
        {
            if ( depthteststate == null || depthteststate.Value != state)
            {
                SetEnable(EnableCap.DepthTest, state);
                depthteststate = state;
            }
        }

        public static void DefaultDepthTest()
        {
            DepthTest(true);
        }

        static BlendingFactor? bfsource = null;
        static BlendingFactor? bfdest= null;
        static bool bfon = false;

        public static void Blend(BlendingFactor source, BlendingFactor dest)
        {
            if (bfsource == null || bfsource.Value != source || bfdest != dest || bfon == false)
            {
                bfsource = source;
                bfdest = dest;
                GL.BlendFunc(source, dest);

                if (!bfon)
                {
                    GL.Enable(EnableCap.Blend);
                    bfon = true;
                }
            }
        }

        public static void BlendOff()
        {
            bfon = false;
            GL.Disable(EnableCap.Blend);
        }

        public static void BlendDefault()
        {
            Blend(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }


        public static void SetEnable( EnableCap c , bool state)
        {
            if (state)
                GL.Enable(c);
            else
                GL.Disable(c);
        }

        public static string[] Extensions()
        {
            return GL.GetString(StringName.Extensions).Split(' ');
        }

        public static bool HasExtensions(string s)
        {
            return Array.IndexOf(Extensions(), s) >= 0;
        }

        // public delegate void DebugProc(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam);

        public static bool EnableDebug(DebugProc p)
        {
            if (HasExtensions("GL_KHR_debug"))
            {
                GL.Enable(EnableCap.DebugOutput);
                GL.Enable(EnableCap.DebugOutputSynchronous);

                GL.DebugMessageCallback(p, IntPtr.Zero);
                GL.DebugMessageControl(DebugSourceControl.DontCare, DebugTypeControl.DontCare, DebugSeverityControl.DontCare, 0, new int[0], true);

                GL.DebugMessageInsert(DebugSourceExternal.DebugSourceApplication, DebugType.DebugTypeMarker, 0, DebugSeverity.DebugSeverityNotification, -1, "Debug output enabled");

                return true;
            }
            else
                return false;
        }
    }
}

