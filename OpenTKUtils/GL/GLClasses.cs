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

        public MouseEventArgs(Point l) { Button = MouseButtons.None; Location = l; Clicks = 0; Delta = 0; Handled = false; NonClientArea = false; }
        public MouseEventArgs(MouseButtons b, Point l, int c, int delta) { Button = MouseButtons.None; Location = l; Clicks = c; Delta = delta; Handled = false; NonClientArea = false; }
        public MouseEventArgs(MouseButtons b, Point l, int c) { Button = b; Location = l; Clicks = c;Delta = 0; Handled = false; NonClientArea = false; }

        public MouseButtons Button { get; set; }
        public Point Location { get; set; }
        public int X { get { return Location.X; } }
        public int Y { get { return Location.Y; } }
        public bool NonClientArea { get; set; }
        public int Clicks { get; set; }
        public int Delta { get; set; }
        public bool Handled { get; set; }
    }

    public struct KeyEventArgs
    {
        public bool Alt { get; private set; }
        public bool Control { get; private set; }
        public bool Shift { get; private set; }
        public Keys KeyCode { get; private set; }
        public int KeyValue { get; private set; }
        public char KeyChar { get; private set; }      // only on key press, others are not on key press
        public Keys Modifiers { get; private set; }
        public bool Handled { get; set; }

        public KeyEventArgs(bool a, bool c, bool s, Keys kc, int kv, Keys mod)
        {
            Alt = a; Control = c; Shift = s; KeyCode = kc; KeyValue = kv; Modifiers = mod; Handled = false; KeyChar = '0';
        }
        public KeyEventArgs(char key)
        {
            Alt = false; Control = false; Shift = false; KeyCode = Keys.None; KeyValue = 0; Modifiers = Keys.None; Handled = false; KeyChar = key;
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
        Action<Object, KeyEventArgs> KeyPress { get; set; }

        void Invalidate();
        int Width { get; }
        int Height { get; }
        bool Focused { get; }
    }
}
