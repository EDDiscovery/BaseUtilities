/*
 * Copyright 2019-2020 Robbyxp1 @ github.com
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
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKUtils.GL4.Controls
{
    public class GLTextBox : GLForeDisplayTextBase
    {
        public Action<GLBaseControl> TextChanged { get; set; } = null;     // not fired by programatically changing Text
        public Action<GLBaseControl> ReturnPressed { get; set; } = null;     // not fired by programatically changing Text

        public new ContentAlignment TextAlign { get { return ContentAlignment.MiddleLeft; } }

        public GLTextBox(string name, Rectangle pos, string text, Color backcolor) : base(name,pos,backcolor)
        {
            Focusable = true;
            this.text = text;
        }

        public GLTextBox() : this("TB?", DefaultWindowRectangle, "", DefaultBackColor)
        {
        }

        protected override void Paint(Rectangle area, Graphics gr)
        {
            Rectangle drawbox = area;

            if (Text.HasChars())
            {
                using (var fmt = new StringFormat())
                {
                    fmt.Alignment = StringAlignment.Near;
                    fmt.LineAlignment = StringAlignment.Near;
                    fmt.FormatFlags = StringFormatFlags.NoWrap;        // need to tell it not to wrap for estimate

                    int widthavailable = drawbox.Width - 6;

                    if (displaystart == -1)     //not set, move to show end of string
                    {
                        cursorpos = Text.Length;

                        if (cursorpos > 0)
                        {
                            displaystart = Math.Max(0, cursorpos - 1);

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
                        else
                            displaystart = 0;
                    }
                    else if (displaystart >= cursorpos)    // if we are beyond or AT the cursor pos, reset to cursor pos. the >= means we don't do the below at the cursor pos.
                    {
                        displaystart = cursorpos;
                    }
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

                    string s = Text.Substring(displaystart);

                    using (var fmt2 = new StringFormat())   // for some reasons, using set measurable characters above on fmt screws it up when it comes to paint, vs not using it
                    {
                        fmt2.Alignment = StringAlignment.Near;
                        fmt2.LineAlignment = StringAlignment.Near;

                        using (Brush textb = new SolidBrush(Enabled ? this.ForeColor : this.ForeColor.Multiply(DisabledScaling)))
                        {
                            // System.Diagnostics.Debug.WriteLine("String " + s + " " + drawbox + " " + fmt2 + " " + Font + " "+ cursorpos + " " +displaystart + fmt2.FormatFlags );

                            // we draw at point, and let the clipping box deal with it. using the rectangle with no wrap caused movement when different chars were present.
                            gr.DrawString(s, Font, textb, new Point(drawbox.Left, (drawbox.Top + drawbox.Bottom) / 2 - Font.Height / 2 - 1), fmt2);
                        }
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
        }

        public override void OnMouseClick(GLMouseEventArgs e)
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
                                    fmt.FormatFlags = StringFormatFlags.NoWrap;
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

        public override void OnKeyDown(GLKeyEventArgs e)
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
                        OnTextChanged();
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

        public override void OnKeyPress(GLKeyEventArgs e)
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
                        OnTextChanged();
                    }
                }
                else
                {
                    Text = Text.Substring(0, cursorpos) + e.KeyChar + Text.Substring(cursorpos);
                    cursorpos++;
                    OnTextChanged();
                }
            }
        }

        protected virtual void OnTextChanged()
        {
            TextChanged?.Invoke(this);
        }

        // tbd
        // shift-hightlight/copy
        // make text stay in same place as you move cursor
        //public int StartPos { get { return startpos; } set { if (cursorpos == startpos) cursorpos = value; startpos = value; Invalidate(); } }
        //public int EndPos { get { return endpos; } set { if (cursorpos == endpos) cursorpos = value;  endpos = value; Invalidate(); } }
        //private int startpos = 0;
        //private int endpos = 0;

        private int cursorpos = -1; // not set
        private int displaystart = -1; // its either at startpos, or endpos. -1 means not set so set the string to display to end

    }
}
