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
using System.Drawing;
using System.Windows.Forms;

namespace OpenTKUtils
{
    public class GLMouseEventArgs
    {
        [System.Flags]
        public enum MouseButtons { None = 0, Left = 1, Middle = 2, Right = 4, };

        public GLMouseEventArgs(Point l) { Button = MouseButtons.None; Location = l; Clicks = 0; Delta = 0; Handled = false; Area = AreaType.Client; Alt = Control = Shift = false; }
        public GLMouseEventArgs(MouseButtons b, Point l, int c, int delta, bool alt, bool ctrl, bool sh) { Button = MouseButtons.None; Location = l; Clicks = c; Delta = delta; Handled = false; Area = AreaType.Client; Alt = alt; Shift = sh; Control = ctrl; }
        public GLMouseEventArgs(MouseButtons b, Point l, int c, bool alt, bool ctrl, bool sh) { Button = b; Location = l; Clicks = c;Delta = 0; Handled = false; Area = AreaType.Client; Alt = alt; Shift = sh; Control = ctrl; }

        public MouseButtons Button { get; set; }
        public Point Location { get; set; }
        public int X { get { return Location.X; } }
        public int Y { get { return Location.Y; } }
        public enum AreaType { Client, Left, Top, Right, Bottom , NWSE };
        public AreaType Area { get; set; }
        public int Clicks { get; set; }
        public int Delta { get; set; }
        public bool Handled { get; set; }
        public bool Alt { get; private set; }
        public bool Control { get; private set; }
        public bool Shift { get; private set; }
    }

    public class GLKeyEventArgs     // class so passed by ref
    {
        public bool Alt { get; private set; }
        public bool Control { get; private set; }
        public bool Shift { get; private set; }
        public Keys KeyCode { get; private set; }
        public int KeyValue { get; private set; }
        public char KeyChar { get; private set; }      // only on key press, others are not on key press
        public Keys Modifiers { get; private set; }
        public bool Handled { get; set; }

        public GLKeyEventArgs(bool a, bool c, bool s, Keys kc, int kv, Keys mod)
        {
            Alt = a; Control = c; Shift = s; KeyCode = kc; KeyValue = kv; Modifiers = mod; Handled = false; KeyChar = '0';
        }
        public GLKeyEventArgs(char key)
        {
            Alt = false; Control = false; Shift = false; KeyCode = Keys.None; KeyValue = 0; Modifiers = Keys.None; Handled = false; KeyChar = key;
        }
    }

    public enum GLCursorType { Normal, Wait, NS, EW, Move , NWSE};

    public interface GLWindowControl
    {
        Action<Object> Resize { get; set; }
        Action<Object> Paint { get; set; }
        Action<Object, GLMouseEventArgs> MouseDown { get; set; }
        Action<Object, GLMouseEventArgs> MouseUp { get; set; }
        Action<Object, GLMouseEventArgs> MouseMove { get; set; }
        Action<Object, GLMouseEventArgs> MouseEnter { get; set; }
        Action<Object, GLMouseEventArgs> MouseLeave { get; set; }
        Action<Object, GLMouseEventArgs> MouseClick { get; set; }
        Action<Object, GLMouseEventArgs> MouseWheel { get; set; }
        Action<Object, GLKeyEventArgs> KeyDown { get; set; }
        Action<Object, GLKeyEventArgs> KeyUp { get; set; }
        Action<Object, GLKeyEventArgs> KeyPress { get; set; }

        void Invalidate();
        Rectangle ClientScreenPos { get; }
        int Width { get; }
        int Height { get; }
        bool Focused { get; }
        void SetCursor(GLCursorType t);
    }
}
