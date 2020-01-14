using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKUtils.GL4.Controls
{
    public abstract class GLForeDisplayBase : GLBaseControl
    {
        public Color ForeColor { get { return foreColor; } set { foreColor = value; Invalidate(); } }       // of text
        public float DisabledScaling
        {
            get { return disabledScaling; }
            set
            {
                if (float.IsNaN(value) || float.IsInfinity(value))
                    return;
                else if (disabledScaling != value)
                {
                    disabledScaling = value;
                    Invalidate();
                }
            }
        }

        private Color foreColor { get; set; } = Color.White;
        private float disabledScaling = 0.5F;
    }

    public abstract class GLTextDisplayBase : GLForeDisplayBase
    {
        public string Text { get { return text; } set { text = value; Invalidate(); } }
        private string text;
    }

    public class GLTextBox : GLTextDisplayBase
    {
        public Action<GLBaseControl> TextChanged { get; set; } = null;     // not fired by programatically changing Text
        public Action<GLBaseControl> ReturnPressed { get; set; } = null;     // not fired by programatically changing Text

        public GLTextBox()
        {
            Focusable = true;
        }

        public GLTextBox(string name, Rectangle pos, string text, Color backcolor)
        {
            Focusable = true;
            Name = name;
            Position = pos;
            Text = text;
            BackColor = backcolor;
        }

        public override void Paint(Rectangle area, Graphics gr)
        {
            Rectangle drawbox = area;

            if (Text.HasChars())
            {
                var fmt = new StringFormat();
                fmt.Alignment = StringAlignment.Near;

                int widthavailable = drawbox.Width - 6;
                
                if (displaystart == -1)     // move to show end of string
                {
                    displaystart = cursorpos - 1;

                    while (true)
                    {
                        string p = Text.Substring(displaystart);
                        CharacterRange[] characterRanges = { new CharacterRange(0, p.Length) };   
                        fmt.SetMeasurableCharacterRanges(characterRanges);
                        var rect = gr.MeasureCharacterRanges(p, Font, new Rectangle(0, 0, 10000, 1000), fmt)[0].GetBounds(gr);

                        if (rect.Width < widthavailable) // back off until we fill the box
                            displaystart--;
                        else
                            break;
                    }

                    displaystart++; // then move 1 forward.
                }
                else if (displaystart >= cursorpos)    // if we are beyond or AT the cursor pos, reset to cursor pos. the >= means we don't do the below at the cursor pos.
                    displaystart = cursorpos;
                else
                {
                    while (true)
                    {
                        CharacterRange[] characterRanges = { new CharacterRange(0, cursorpos - displaystart) };   // find where cursor pos is..
                        fmt.SetMeasurableCharacterRanges(characterRanges);
                        var rect = gr.MeasureCharacterRanges(Text.Substring(displaystart), Font, new Rectangle(0, 0, 10000, 1000), fmt)[0].GetBounds(gr);
                        //System.Diagnostics.Debug.WriteLine("Measured " + rect + " allowed " + drawbox);
                        if (rect.Width >= widthavailable) // if width > available, move DS on one and try again...
                            displaystart++;
                        else
                            break;
                    }
                }

                fmt.FormatFlags = StringFormatFlags.NoWrap;

                string s = Text.Substring(displaystart);

                using (Brush textb = new SolidBrush(Enabled ? this.ForeColor : this.ForeColor.Multiply(DisabledScaling)))
                {
                    gr.DrawString(s, Font, textb, drawbox, fmt);
                }

                if (Enabled && Focused)
                {
                    int offset = cursorpos - displaystart;
                    CharacterRange[] characterRanges = { new CharacterRange(0, Math.Max(1, offset)) };   // if offset=0, 1 char and we use the left pos
                    fmt.SetMeasurableCharacterRanges(characterRanges);
                    var rect = gr.MeasureCharacterRanges(s + "@", Font, drawbox, fmt)[0].GetBounds(gr);    // ensure at least 1 char

                    int curpos = (int)((offset == 0) ? rect.Left : rect.Right);
                    int botpos = (int)rect.Bottom + 1;

                    using (Pen p = new Pen(this.ForeColor))
                    {
                        gr.DrawLine(p, new Point(curpos, drawbox.Top), new Point(curpos, botpos));
                    }
                }

            }
        }

        public override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if ( !e.Handled)
            {
                string s = Text.Substring(displaystart);

                if (s.Length > 0)
                {
                    using (Bitmap bmp = new Bitmap(1, 1))
                    {
                        using (Graphics gr = Graphics.FromImage(bmp))
                        {
                            int i;
                            for ( i = 0; i < s.Length; i++ )    // we have to do it one by one, as the query is limited to 32 char ranges
                            {
                                using (var fmt = new StringFormat())
                                {
                                    fmt.Alignment = StringAlignment.Near;
                                    CharacterRange[] characterRanges = { new CharacterRange(i, 1) };
                                    fmt.SetMeasurableCharacterRanges(characterRanges);

                                    var rect = gr.MeasureCharacterRanges(s, Font, new Rectangle(0, 0, 10000, 1000), fmt)[0].GetBounds(gr);
                                    //System.Diagnostics.Debug.WriteLine("Region " + rect + " char " + i + " vs " + e.Location);
                                    if (e.X >= rect.Left && e.X <= rect.Right)
                                    {
                                        cursorpos = i + displaystart+1;
                                        Invalidate();
                                        return;
                                    }
                                }
                            }

                            cursorpos = i + displaystart;
                            Invalidate();
                        }
                    }
                }
            }
        }

        public override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (!e.Handled)
            {
                //System.Diagnostics.Debug.WriteLine("KDown " + Name + " " + e.KeyCode);

                if (e.KeyCode == System.Windows.Forms.Keys.Left)
                {
                    if (cursorpos > 0)
                    {
                        //if (!e.Shift)
                        //    startpos = endpos = cursorpos;

                        //if (startpos == cursorpos)
                        //    startpos--;
                        //if (endpos == cursorpos)
                        //    endpos--;

                        cursorpos--;
                        Invalidate();
                    }

                }
                else if (e.KeyCode == System.Windows.Forms.Keys.Right)
                {
                    if (cursorpos < Text.Length)
                    {
                        //if (!e.Shift)
                        //    startpos = endpos = cursorpos;

                        //if (startpos == cursorpos)
                        //    startpos++;
                        //if (endpos == cursorpos)
                        //    endpos++;

                        cursorpos++;
                        Invalidate();
                    }
                }
                else if (e.KeyCode == System.Windows.Forms.Keys.Delete)
                {
                    if (cursorpos < Text.Length)
                    {
                        Text = Text.Substring(0, cursorpos) + Text.Substring(cursorpos + 1);
                        TextChanged?.Invoke(this);
                    }
                }
                else if (e.KeyCode == System.Windows.Forms.Keys.Home)
                {
                    cursorpos = 0;
                    Invalidate();
                }
                else if (e.KeyCode == System.Windows.Forms.Keys.End)
                {
                    cursorpos = Text.Length;
                    Invalidate();
                }
                else if (e.KeyCode == System.Windows.Forms.Keys.Return || e.KeyCode == System.Windows.Forms.Keys.Enter)
                {
                    ReturnPressed?.Invoke(this);
                }
            }
        }

        public override void OnKeyPress(KeyEventArgs e)
        {
            base.OnKeyPress(e);
            if (!e.Handled)
            {
                if (e.KeyChar == 8)
                {
                    if ( cursorpos>0)
                    {
                        Text = Text.Substring(0, cursorpos - 1) + Text.Substring(cursorpos);
                        cursorpos--;
                        TextChanged?.Invoke(this);
                    }
                }
                else
                {
                    Text = Text.Substring(0, cursorpos) + e.KeyChar + Text.Substring(cursorpos);
                    cursorpos++;
                    TextChanged?.Invoke(this);
                }
            }
        }

        
        // later.. tbd
        //public int StartPos { get { return startpos; } set { if (cursorpos == startpos) cursorpos = value; startpos = value; Invalidate(); } }
        //public int EndPos { get { return endpos; } set { if (cursorpos == endpos) cursorpos = value;  endpos = value; Invalidate(); } }

        //private int startpos = 0;
        //private int endpos = 0;
        private int cursorpos = 0; // current cursor pos
        private int displaystart = -1; // its either at startpos, or endpos. -1 means not set so set the string to display to end

    }
}
