using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OpenTKUtils
{
    public struct MouseEventArgs
    {
        [System.Flags]
        public enum MouseButtons { None = 0, Left = 1, Middle = 2, Right = 4, };

        public MouseEventArgs(Point l) { Button = MouseButtons.None; Location = l; Clicks = 0; Delta = 0; }
        public MouseEventArgs(MouseButtons b, Point l, int c, int delta) { Button = MouseButtons.None; Location = l; Clicks = c; Delta = delta; }
        public MouseEventArgs(MouseButtons b, Point l, int c) { Button = b; Location = l; Clicks = c;Delta = 0; }

        public MouseButtons Button { get; set; }
        public Point Location { get; set; }
        public int X { get { return Location.X; } }
        public int Y { get { return Location.Y; } }
        public int Clicks { get; set; }
        public int Delta { get; set; }
    }

    public struct KeyEventArgs
    {
        public bool Alt { get; private set; }
        public bool Control { get; private set; }
        public bool Shift { get; private set; }
        public Keys KeyCode { get; private set; }
        public int KeyValue { get; private set; }
        public Keys Modifiers { get; private set; }

        public KeyEventArgs(bool a, bool c, bool s, Keys kc, int kv, Keys mod)
        {
            Alt = a;Control = c;Shift = s;KeyCode = kc;KeyValue = kv;Modifiers = mod;
        }
    }

    public interface GLWindowControl
    {
        Action<Object> Resize { get; set; }
        Action<Object> Paint { get; set; }
        Action<Object, MouseEventArgs> MouseDown { get; set; }
        Action<Object, MouseEventArgs> MouseUp { get; set; }
        Action<Object, MouseEventArgs> MouseMove { get; set; }
        Action<Object, MouseEventArgs> MouseEnter { get; set; }
        Action<Object, MouseEventArgs> MouseLeave { get; set; }
        Action<Object, MouseEventArgs> MouseClick { get; set; }
        Action<Object, MouseEventArgs> MouseWheel { get; set; }
        Action<Object, KeyEventArgs> KeyDown { get; set; }
        Action<Object, KeyEventArgs> KeyUp { get; set; }

        void Invalidate();
        int Width { get; }
        int Height { get; }
        bool Focused { get; }
    }
}
