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
    public class GLMultiLineTextBox : GLForeDisplayTextBase
    {
        public Action<GLBaseControl> TextChanged { get; set; } = null;      // not fired by programatically changing Text
        public Action<GLBaseControl> ReturnPressed { get; set; } = null;    // not fired by programatically changing Text, only if AllowLF = false
        public bool CRLF { get; set; } = true;                              // set to determine CRLF or LF is used
        public bool AllowLF { get; set; } = true;                           // clear to prevent multiline
        public Margin TextBoundary { get; set; } = new Margin(0);
        public Color HighlightColor { get { return highlightColor; } set { highlightColor = value; Invalidate(); } }       // of text
        public Color LineColor { get { return lineColor; } set { lineColor = value; Invalidate(); } }       // lined text, default off
        private int DisplayableLines { get { return Font != null ? ClientRectangle.Height / Font.Height : 1; } }
        public bool IsSelectionSet { get { return startpos != cursorpos; } }

        public GLMultiLineTextBox(string name, Rectangle pos, string text, Color backcolor = DefaultControlBackColor) : base(name, pos, backcolor)
        {
            Focusable = true;
            this.text = text;
            OnTextSet();
        }

        public GLMultiLineTextBox() : this("TBML?", DefaultWindowRectangle, "")
        {
        }

        public void SetCursorPos(int p)
        {
            if (p >= 0 && p <= Text.Length)
            {
                cursorpos = p;
                if (cursorpos < Text.Length && text[cursorpos] == '\n' && cursorpos > 0 && text[cursorpos - 1] == '\r') // if on a \r\n at \n, need to move back 1 more to disallow
                    cursorpos--;

                OnTextSet();
                Invalidate();
            }
        }

        public void CursorLeft(bool clearstart = false)
        {
            if (cursorpos > 0)
            {
                cursorpos--;

                if (cursorpos < cursorlinecpos)
                {
                    cursorlinecpos -= linelengths[--cursorlineno];
                    if (lineendlengths[cursorlineno] == 2)
                        cursorpos--;
                }

                System.Diagnostics.Debug.WriteLine("Move cpos to line {0} cpos {1} cur {2} off {3} len {4} text '{5}'", cursorlineno, cursorlinecpos, cursorpos, cursorpos - cursorlinecpos, linelengths[cursorlineno], text.EscapeControlChars());
                System.Diagnostics.Debug.WriteLine("Start line {0} cpos {1} cur {2}", startlineno, startlinecpos, startpos);

                if (clearstart)
                    ClearStart();

                EnsureCursorWithinDisplay(true);
            }
        }

        public void CursorRight(bool clearstart = false)
        {
            int nextlinecpos = cursorlinecpos + linelengths[cursorlineno];

            if (cursorpos < nextlinecpos)       // only fails if trailing no /r/n end
            {
                cursorpos++;

                if (cursorpos > nextlinecpos - lineendlengths[cursorlineno])
                {
                    cursorlinecpos = cursorpos = nextlinecpos;
                    cursorlineno++;
                }

                if (clearstart)
                    ClearStart();

                System.Diagnostics.Debug.WriteLine("Move cpos to line {0} cpos {1} cur {2} off {3} len {4} text '{5}'", cursorlineno, cursorlinecpos, cursorpos, cursorpos - cursorlinecpos, linelengths[cursorlineno], text.EscapeControlChars());
                System.Diagnostics.Debug.WriteLine("..Start line {0} cpos {1} cur {2}", startlineno, startlinecpos, startpos);

                EnsureCursorWithinDisplay(true);
            }
        }

        public void CursorDown(bool clearstart = false)
        {
            if (cursorlineno < linelengths.Count() - 1)
            {
                int offsetin = cursorpos - cursorlinecpos;
                cursorlinecpos += linelengths[cursorlineno++];
                cursorpos = cursorlinecpos + Math.Min(offsetin, linelengths[cursorlineno] - lineendlengths[cursorlineno]);

                if (clearstart)
                    ClearStart();

                System.Diagnostics.Debug.WriteLine("Move cpos to line {0} cpos {1} cur {2} off {3} len {4} text '{5}'", cursorlineno, cursorlinecpos, cursorpos, cursorpos - cursorlinecpos, linelengths[cursorlineno], text.EscapeControlChars());
                System.Diagnostics.Debug.WriteLine("..Start line {0} cpos {1} cur {2}", startlineno, startlinecpos, startpos);

                EnsureCursorWithinDisplay(true);
            }
        }

        public void CursorUp(bool clearstart = false)
        {
            if (cursorlineno > 0)
            {
                int offsetin = cursorpos - cursorlinecpos;
                cursorlinecpos -= linelengths[--cursorlineno];
                cursorpos = cursorlinecpos + Math.Min(offsetin, linelengths[cursorlineno] - lineendlengths[cursorlineno]);

                if (clearstart)
                    ClearStart();

                System.Diagnostics.Debug.WriteLine("Move cpos to line {0} cpos {1} cur {2} off {3} len {4} text '{5}'", cursorlineno, cursorlinecpos, cursorpos, cursorpos - cursorlinecpos, linelengths[cursorlineno], text.EscapeControlChars());
                System.Diagnostics.Debug.WriteLine("..Start line {0} cpos {1} cur {2}", startlineno, startlinecpos, startpos);

                EnsureCursorWithinDisplay(true);
                Invalidate();
            }
        }

        public void Home(bool clearstart = false)
        {
            cursorpos = cursorlinecpos;

            if (clearstart)
                ClearStart();

            Invalidate();
        }

        public void End(bool clearstart = false)
        {
            cursorpos = cursorlinecpos + linelengths[cursorlineno] - lineendlengths[cursorlineno];

            if (clearstart)
                ClearStart();

            Invalidate();
        }

        public void InsertTextWithCRLF(string str, bool insertinplace = false)        // any type of lf/cr combo, replaced by selected combo
        {
            DeleteSelectionInt();

            int cpos = 0;
            while (true)
            {
                if (cpos < str.Length)
                {
                    int nextlf = str.IndexOfAny(new char[] { '\r', '\n' }, cpos);

                    if (nextlf >= 0)
                    {
                        InsertTextInt(str.Substring(cpos, nextlf-cpos), insertinplace);
                        InsertCRLFInt();

                        if (str[nextlf] == '\r')
                        {
                            nextlf++;
                        }

                        if (nextlf < str.Length && str[nextlf] == '\n')
                            nextlf++;

                        cpos = nextlf;
                    }
                    else
                    {
                        InsertTextInt(str.Substring(cpos), insertinplace);
                        break;
                    }
                }
            }

            ClearStart();
            System.Diagnostics.Debug.WriteLine("Move cpos to line {0} cpos {1} cur {2} off {3} len {4} text '{5}'", cursorlineno, cursorlinecpos, cursorpos, cursorpos - cursorlinecpos, linelengths[cursorlineno], text.EscapeControlChars());
            OnTextChanged();
            Invalidate();
        }

        public void InsertText(string t, bool insertinplace = false)        // no lf in text
        {
            DeleteSelectionInt();
            InsertTextInt(t,insertinplace);
            System.Diagnostics.Debug.WriteLine("Move cpos to line {0} cpos {1} cur {2} off {3} len {4} text '{5}'", cursorlineno, cursorlinecpos, cursorpos, cursorpos - cursorlinecpos, linelengths[cursorlineno], text.EscapeControlChars());
            ClearStart();
            OnTextChanged();
            Invalidate();
        }

        public void InsertCRLF()        // insert the selected cr/lf pattern
        {
            DeleteSelectionInt();
            InsertCRLFInt();
            ClearStart();
            System.Diagnostics.Debug.WriteLine("Move cpos to line {0} cpos {1} cur {2} off {3} len {4} text '{5}'", cursorlineno, cursorlinecpos, cursorpos, cursorpos - cursorlinecpos, linelengths[cursorlineno], text.EscapeControlChars());
            OnTextChanged();
            Invalidate();
        }

        public void Backspace()
        {
            if (!DeleteSelection())     // if we deleted a selection, no other action
            {
                int offsetin = cursorpos - cursorlinecpos;

                if (offsetin > 0)   // simple backspace
                {
                    //System.Diagnostics.Debug.WriteLine("Text '" + text.EscapeControlChars() + "' cursor text '" + text.Substring(cursorpos).EscapeControlChars() + "'");
                    text = text.Substring(0, cursorpos - 1) + text.Substring(cursorpos);
                    linelengths[cursorlineno]--;
                    cursorpos--;
                    ClearStart();
                    OnTextChanged();
                    Invalidate();
                }
                else if (cursorlinecpos > 0)    // not at start of text
                {
                    cursorlinecpos -= linelengths[--cursorlineno];      // back 1 line
                    int textlen = linelengths[cursorlineno] - lineendlengths[cursorlineno];
                    text = text.Substring(0, cursorpos - lineendlengths[cursorlineno]) + text.Substring(cursorpos); // remove lf/cr from previous line
                    linelengths[cursorlineno] = textlen + linelengths[cursorlineno + 1];
                    lineendlengths[cursorlineno] = lineendlengths[cursorlineno + 1];        // copy end type
                    cursorpos = cursorlinecpos + textlen;
                    linelengths.RemoveAt(cursorlineno + 1);
                    lineendlengths.RemoveAt(cursorlineno + 1);
                    ClearStart();
                    OnTextChanged();
                    Invalidate();
                }

                System.Diagnostics.Debug.WriteLine("Move cpos to line {0} cpos {1} cur {2} off {3} len {4} text '{5}'", cursorlineno, cursorlinecpos, cursorpos, cursorpos - cursorlinecpos, linelengths[cursorlineno], text.EscapeControlChars());
            }
        }

        public void Delete()
        {
            if (!DeleteSelection())      // if we deleted a selection, no other action
            {
                int offsetin = cursorpos - cursorlinecpos;

                if (offsetin < linelengths[cursorlineno] - lineendlengths[cursorlineno])   // simple delete
                {
                    //System.Diagnostics.Debug.WriteLine("Text '" + text.EscapeControlChars() + "' cursor text '" + text.Substring(cursorpos).EscapeControlChars() + "'");
                    text = text.Substring(0, cursorpos) + text.Substring(cursorpos + 1);
                    linelengths[cursorlineno]--;
                    ClearStart();
                    OnTextChanged();
                    Invalidate();
                }
                else if ( cursorpos < Text.Length ) // not at end of text
                {
                    text = text.Substring(0, cursorpos) + text.Substring(cursorpos + lineendlengths[cursorlineno]); // remove lf/cr from out line
                    linelengths[cursorlineno] += linelengths[cursorlineno + 1] - lineendlengths[cursorlineno];   // our line is whole of next less our lf/cr
                    linelengths.RemoveAt(cursorlineno + 1);     // next line disappears
                    lineendlengths.RemoveAt(cursorlineno);  // and we remove our line ends and keep the next one
                    ClearStart();
                    OnTextChanged();
                    Invalidate();
                }

                System.Diagnostics.Debug.WriteLine("Move cpos to line {0} cpos {1} cur {2} off {3} len {4} text '{5}'", cursorlineno, cursorlinecpos, cursorpos, cursorpos - cursorlinecpos, linelengths[cursorlineno], text.EscapeControlChars());
            }
        }


        public void SetSelection(int start, int end)        // set equal to cancel, else set start/end pos
        {
            startpos = Math.Min(start,end);
            cursorpos = Math.Max(start,end);
            OnTextSet();
            Invalidate();
        }

        public void ClearSelection()
        {
            startpos = cursorpos;
            OnTextSet();
            Invalidate();
        }

        public bool DeleteSelection()
        {
            if (DeleteSelectionInt())
            {
                OnTextChanged();
                Invalidate();
                return true;
            }
            else
                return false;
        }

        public string SelectedText {
            get
            {
                if (IsSelectionSet)
                {
                    int min = Math.Min(startpos, cursorpos);
                    int max = Math.Max(startpos, cursorpos);
                    return text.Substring(min, max - min);
                }
                else
                    return null;
            }
        }

        public void Copy()
        {
            string sel = SelectedText;
            if (sel != null)
                System.Windows.Forms.Clipboard.SetText(sel);
        }

        public void Cut()
        {
            string sel = SelectedText;
            if (sel != null)
            {
                System.Windows.Forms.Clipboard.SetText(sel);
                DeleteSelection();
            }
        }

        public void Paste()
        {
            string s = System.Windows.Forms.Clipboard.GetText(System.Windows.Forms.TextDataFormat.UnicodeText);
            if ( !s.IsEmpty() )
                InsertTextWithCRLF(s);
        }


        #region Implementation

        protected override void OnTextSet()     // from text, work out the working parameters
        {
            linelengths.Clear();      
            lineendlengths.Clear();

            if (cursorpos < 0 || cursorpos > Text.Length)
            {
                startpos = cursorpos = Text.Length;
            }

            if (startpos < 0 || startpos > Text.Length)
            {
                startpos = cursorpos;
            }

            int cpos = 0;
            int lineno = 0;

            while (true)
            {
                if (cpos < text.Length)
                {
                    int nextlf = text.IndexOfAny(new char[] { '\r', '\n' }, cpos);

                    if (nextlf == -1)       // no /r/n on last line
                    {
                        nextlf = text.Length;
                        if (cursorpos >= cpos && cursorpos <= nextlf)
                        {
                            cursorlinecpos = cpos;
                            cursorlineno = lineno;
                        }
                        if (startpos >= cpos && startpos <= nextlf)
                        {
                            startlinecpos = cpos;
                            startlineno = lineno;
                        }

                        linelengths.Add(nextlf - cpos);
                        lineendlengths.Add(0);
                        break;
                    }
                    else
                    {
                        int el = 1;

                        if (text[nextlf] == '\r')
                        {
                            nextlf++;
                            el = 2;
                        }

                        if (nextlf < text.Length && text[nextlf] == '\n')
                            nextlf++;

                        if (cursorpos >= cpos && cursorpos < nextlf)
                        {
                            cursorlinecpos = cpos;
                            cursorlineno = lineno;
                        }
                        if (startpos >= cpos && startpos < nextlf)
                        {
                            startlinecpos = cpos;
                            startlineno = lineno;
                        }

                        linelengths.Add(nextlf - cpos);
                        lineendlengths.Add(el);
                        cpos = nextlf;
                    }
                }
                else
                {
                    if (cursorpos == cpos)
                    {
                        cursorlinecpos = cpos;
                        cursorlineno = lineno;
                    }

                    if (startpos == cpos)
                    {
                        startlinecpos = cpos;
                        startlineno = lineno;
                    }

                    linelengths.Add(0);
                    lineendlengths.Add(0);
                    break;
                }

                lineno++;

            }

            EnsureCursorWithinDisplay(false);
        }

        private void ClearStart()
        {
            startlinecpos = cursorlinecpos;
            startlineno = cursorlineno;
            startpos = cursorpos;
        }

        private string LineWithoutCRLF(int startpos, int lineno, int offsetin = 0)
        {
            if (startpos < Text.Length)
            {
                int avtext = linelengths[lineno] - lineendlengths[lineno];
                if (offsetin > avtext)
                    return string.Empty;
                else
                    return text.Substring(startpos + offsetin, avtext - offsetin);
            }
            else
                return string.Empty;
        }

        private void EnsureCursorWithinDisplay(bool invalidate)
        {
            if (Font == null)
                return;

            if (cursorlineno < firstline)
            {
                firstline = cursorlineno;
                invalidate = true;
            }
            else if (cursorlineno >= firstline + DisplayableLines)
            {
                firstline = cursorlineno - DisplayableLines + 1;
                invalidate = true;
            }

            if (invalidate)
                Invalidate();
        }

        private void InsertTextInt(string t, bool insertinplace = false)        // no lf in text
        {
            int offsetin = cursorpos - cursorlinecpos;
            text = text.Substring(0, cursorpos) + t + text.Substring(cursorpos);
            linelengths[cursorlineno] += t.Length;
            if (!insertinplace)
                cursorpos += t.Length;
        }

        private void InsertCRLFInt()
        {
            int offsetin = cursorpos - cursorlinecpos;
            int lineleft = linelengths[cursorlineno] - offsetin;
            string s = CRLF ? "\r\n" : "\n";
            text = text.Substring(0, cursorpos) + s + text.Substring(cursorpos);
            linelengths[cursorlineno] = offsetin + s.Length;
            linelengths.Insert(cursorlineno + 1, lineleft);
            lineendlengths.Insert(cursorlineno + 1, lineendlengths[cursorlineno]);  // copy end down
            lineendlengths[cursorlineno] = CRLF ? 2 : 1;    // and set ours to CR type
            cursorpos = cursorlinecpos += linelengths[cursorlineno++];
        }

        private bool DeleteSelectionInt()
        {
            if (startpos > cursorpos)
            {
                text = text.Substring(0, cursorpos) + text.Substring(startpos);
                System.Diagnostics.Debug.WriteLine("Delete {0} to {1} text '{2}'", startpos, cursorpos, text.EscapeControlChars());
                startpos = cursorpos;
                OnTextSet();
                return true;
            }
            else if (startpos < cursorpos)
            {
                text = text.Substring(0, startpos) + text.Substring(cursorpos);
                System.Diagnostics.Debug.WriteLine("Delete {0} to {1} text '{2}'", startpos, cursorpos, text.EscapeControlChars());
                cursorpos = startpos;
                OnTextSet();
                return true;
            }

            return false;
        }

        public override void OnFontChanged()
        {
            base.OnFontChanged();
            EnsureCursorWithinDisplay(false);
        }

        protected override void Paint(Rectangle clientarea, Graphics gr)
        {
            Rectangle usablearea = new Rectangle(clientarea.Left + TextBoundary.Left, clientarea.Top + TextBoundary.Top, clientarea.Width - TextBoundary.TotalWidth, clientarea.Height - TextBoundary.TotalHeight);

            using (Brush textb = new SolidBrush(Enabled ? this.ForeColor : this.ForeColor.Multiply(DisabledScaling)))
            {
                int lineno = 0;
                int cpos = 0;
                while (lineno < firstline)
                    cpos += linelengths[lineno++];

                int cursoroffset = cursorpos - cursorlinecpos;  // offset into line

                if (startx > cursoroffset)
                    startx = Math.Max(0, cursoroffset - 1);
                else
                {
                    while (true)        // this part sets startx so cursor is visible
                    {
                        string cursorline = LineWithoutCRLF(cursorlinecpos, cursorlineno, startx);

                        if (cursorline.IsEmpty())
                            break;

                        Rectangle ma = usablearea;
                        ma.Width = 7000;        // more due to us trying to find the maximum

                        using (var sfmt = new StringFormat())       // measure where the cursor will be and move startx to make it visible
                        {
                            sfmt.Alignment = StringAlignment.Near;
                            sfmt.LineAlignment = StringAlignment.Near;

                            CharacterRange[] characterRanges = { new CharacterRange(0, cursoroffset - startx) };   // measure where the cursor is..
                            sfmt.SetMeasurableCharacterRanges(characterRanges);
                            var rect = gr.MeasureCharacterRanges(cursorline + "@", Font, ma, sfmt)[0].GetBounds(gr);    // ensure at least 1 char

                            //System.Diagnostics.Debug.WriteLine("{0} {1} {2}", startx, cursoroffset, rect);
                            if ((int)(rect.Width + 1) > usablearea.Width - Font.Height)      // Font.Height is to allow for an overlap  TBD fix this later
                            {
                                System.Diagnostics.Debug.WriteLine("Display start move right");
                                startx++;
                            }
                            else
                                break;
                        }
                    }
                }

                using (var pfmt = new StringFormat())   // for some reasons, using set measurable characters above on fmt screws it up when it comes to paint, vs not using it
                {
                    pfmt.Alignment = StringAlignment.Near;
                    pfmt.LineAlignment = StringAlignment.Near;
                    pfmt.FormatFlags = StringFormatFlags.LineLimit | StringFormatFlags.NoWrap | StringFormatFlags.NoClip;   // still jumps 1 pixel sometimes.. fix later TBD

                    int bottom = usablearea.Bottom;
                    usablearea.Height = Font.Height;        // move the area down the screen progressively
                        
                    while (usablearea.Top < bottom)       // paint each line
                    {
                        if (!lineColor.IsFullyTransparent())        // lined paper
                        {
                            using (Pen p = new Pen(Color.Green))
                            {
                                gr.DrawLine(p, new Point(usablearea.Left, usablearea.Bottom-1), new Point(usablearea.Right-1, usablearea.Bottom-1));
                            }
                        }

                        int highlightstart = 0, highlightend = 0;

                        if (startpos < cursorpos)       // start less than cursor, so estimate s/e this way. 
                        {
                            highlightstart = cpos == startlinecpos ? startpos - startlinecpos : 0;
                            highlightend = (cpos >= startlinecpos && cpos < cursorlinecpos) ? (linelengths[lineno] - lineendlengths[lineno]) : cpos == cursorlinecpos ? (cursorpos - cursorlinecpos) : 0;
                        }
                        else if ( startpos > cursorpos )    // other way
                        {
                            highlightstart = cpos == cursorlinecpos ? cursorpos - cursorlinecpos : 0;
                            highlightend = (cpos >= cursorlinecpos && cpos < startlinecpos) ? (linelengths[lineno] - lineendlengths[lineno]) : cpos == startlinecpos ? (startpos - startlinecpos) : 0;
                        }

                        if (highlightstart != 0 || highlightend != 0)       // if set, we need to offset by startx. this may result in 0,0, turning the highlight off
                        {
                            highlightstart = Math.Max(highlightstart-startx, 0);        // offset by startx, min 0.
                            highlightend = Math.Max(highlightend - startx, 0);          // and the end points
                        }

                        string s = LineWithoutCRLF(cpos, lineno, startx);   // text without cr/lf, empty if none

                        if (highlightstart != 0 || highlightend != 0)       // and highlight if on
                        {
                            //System.Diagnostics.Debug.WriteLine("{0} {1}-{2}", cpos, highlightstart, highlightend);

                            using (var sfmt = new StringFormat())   // new measurer, don't trust reuse
                            {
                                sfmt.Alignment = StringAlignment.Near;
                                sfmt.LineAlignment = StringAlignment.Near;
                                sfmt.FormatFlags = StringFormatFlags.NoWrap;

                                CharacterRange[] characterRanges = { new CharacterRange(highlightstart, highlightend-highlightstart) };
                                sfmt.SetMeasurableCharacterRanges(characterRanges);
                                var rect = gr.MeasureCharacterRanges(s+"@", Font, usablearea, sfmt)[0].GetBounds(gr);    // ensure at least 1 char, need to do it in area otherwise it does not works:

                                using (Brush b1 = new SolidBrush(HighlightColor))
                                {
                                    gr.FillRectangle(b1, rect);
                                }
                            }
                        }

                        if (s.Length>0)
                        {
                            gr.DrawString(s, Font, textb, usablearea, pfmt);        // need to paint to pos not in an area
                        }

                        if (cursorlineno == lineno)
                        {
                            if (Enabled)
                            {
                                int xpos = 0;

                                using (var sfmt = new StringFormat())   
                                {
                                    sfmt.Alignment = StringAlignment.Near;
                                    sfmt.LineAlignment = StringAlignment.Near;
                                    sfmt.FormatFlags = StringFormatFlags.NoWrap;
                                    int offset = cursorpos - cpos - startx;

                                    CharacterRange[] characterRanges = { new CharacterRange(0, Math.Max(1, offset)) };   // if offset=0, 1 char and we use the left pos

                                    string t = s + "a";
                                    //                                    System.Diagnostics.Debug.WriteLine(" Offset '{0}' {1} {2}" ,t, characterRanges[0].First , characterRanges[0].Length);
                                    sfmt.SetMeasurableCharacterRanges(characterRanges);
                                    var rect = gr.MeasureCharacterRanges(t, Font, usablearea, sfmt)[0].GetBounds(gr);    // ensure at least 1 char, need to do it in area otherwise it does not works:

                                    //using (Pen p = new Pen(this.ForeColor)) { gr.DrawRectangle(p, new Rectangle((int)rect.Left, (int)rect.Top, (int)rect.Width, (int)rect.Height)); }

                                    xpos = (int)((offset == 0) ? rect.Left : rect.Right);
                                }

                                using (Pen p = new Pen(this.ForeColor))
                                {
                                    gr.DrawLine(p, new Point(xpos, usablearea.Y), new Point(xpos, usablearea.Y + Font.Height - 2));
                                }
                            }
                        }

                        if (lineno == linelengths.Count() - 1)   // valid line, last entry in lines is the terminating pos
                            break;

                        cpos += linelengths[lineno];
                        usablearea.Y += Font.Height;
                        lineno++;
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
                    CursorLeft(!e.Shift);
                else if (e.KeyCode == System.Windows.Forms.Keys.Right)
                    CursorRight(!e.Shift);
                else if (e.KeyCode == System.Windows.Forms.Keys.Down)
                    CursorDown(!e.Shift);
                else if (e.KeyCode == System.Windows.Forms.Keys.Up)
                    CursorUp(!e.Shift);
                else if (e.KeyCode == System.Windows.Forms.Keys.Delete)
                {
                    if (e.Shift)
                        Cut();
                    else
                        Delete();
                }
                else if (e.KeyCode == System.Windows.Forms.Keys.Home)
                    Home(!e.Shift);
                else if (e.KeyCode == System.Windows.Forms.Keys.End)
                    End(!e.Shift);
                else if (e.KeyCode == System.Windows.Forms.Keys.Insert)
                {
                    if (e.Control)      
                        Copy();
                    else if (e.Shift)
                        Paste();
                }

                else if (e.KeyCode == System.Windows.Forms.Keys.F1)
                    InsertTextWithCRLF("Hello\rThere\rFred");
            }
        }

        public override void OnKeyPress(GLKeyEventArgs e)
        {
            base.OnKeyPress(e);
            if (!e.Handled)
            {
                if (e.KeyChar == 8)
                {
                    Backspace();
                }
                else if (e.KeyChar == 3)    // ctrl-c
                {
                    Copy();
                }
                else if (e.KeyChar == 22)    // ctrl-v
                {
                    Paste();
                }
                else if (e.KeyChar == 24)    // ctrl-x
                {
                    Cut();
                }
                else if (e.KeyChar == 13)
                {
                    if (AllowLF)
                        InsertCRLF();
                    else
                        OnReturnPressed();
                }
                else 
                {
                    InsertText(new string(e.KeyChar, 1));
                }
            }
        }

        protected virtual void OnTextChanged()
        {
            TextChanged?.Invoke(this);
        }

        protected virtual void OnReturnPressed()
        {
            ReturnPressed?.Invoke(this);
        }

        private int FindCursorPos(Point click, out int cpos, out int lineno)
        {
            Rectangle usablearea = new Rectangle(ClientRectangle.Left + TextBoundary.Left, ClientRectangle.Top + TextBoundary.Top, ClientRectangle.Width - TextBoundary.TotalWidth, ClientRectangle.Height - TextBoundary.TotalHeight);
            lineno = cpos = -1;

            if (click.Y < usablearea.Y)
                return -1;

            int lineoffset = (click.Y - usablearea.Y) / Font.Height;

            int lineclicked = Math.Min(firstline + lineoffset,linelengths.Count-1);

            lineno = 0;
            cpos = 0;
            while (lineno < lineclicked)            // setting cpos and lineno
                cpos += linelengths[lineno++];

            string s = LineWithoutCRLF(cpos, lineno, startx);

            if (s.Length == 0)  // no text, means its to the left, so click on end
                return cpos + linelengths[lineno] - lineendlengths[lineno];

            using (Bitmap b = new Bitmap(1, 1))
            {
                using (Graphics gr = Graphics.FromImage(b))
                {
                    for (int i = 0; i < s.Length; i++)    // we have to do it one by one, as the query is limited to 32 char ranges
                    {
                        using (var fmt = new StringFormat())
                        {
                            fmt.Alignment = StringAlignment.Near;
                            fmt.FormatFlags = StringFormatFlags.NoWrap;
                            CharacterRange[] characterRanges = { new CharacterRange(i, 1) };
                            fmt.SetMeasurableCharacterRanges(characterRanges);

                            var rect = gr.MeasureCharacterRanges(s, Font, usablearea, fmt)[0].GetBounds(gr);
                            //System.Diagnostics.Debug.WriteLine("Region " + rect + " char " + i + " vs " + e.Location);
                            if (click.X >= rect.Left && click.X < rect.Right)
                            {
                                return cpos + startx + i;
                            }
                        }
                    }
                }
            }

            return cpos + linelengths[lineno] - lineendlengths[lineno]; ;
        }

        public override void OnMouseDown(GLMouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (!e.Handled)
            {
                int xcursorpos = FindCursorPos(e.Location, out int xlinecpos, out int xlineno);

                if (xcursorpos >= 0)
                {
                    cursorlinecpos = xlinecpos;
                    cursorlineno = xlineno;
                    cursorpos = xcursorpos;
                    if (!e.Shift)
                        ClearStart();

                    Invalidate();
                }
            }
        }

        public override void OnMouseMove(GLMouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!e.Handled && e.Button == GLMouseEventArgs.MouseButtons.Left)
            {
                int xcursorpos = FindCursorPos(e.Location, out int xlinecpos, out int xlineno);

                if (xcursorpos >= 0)
                {
                    cursorlinecpos = xlinecpos;
                    cursorlineno = xlineno;
                    cursorpos = xcursorpos;
                    Invalidate();
                }
            }
        }

        #endregion


        private List<int> linelengths = new List<int>(); // computed on text set
        private List<int> lineendlengths = new List<int>(); // computed on text set, 0 = none, 1 = lf, 2 = cr/lf

        private int cursorpos = int.MaxValue; // set on text set if invalid
        private int cursorlineno;   // computed on text set, updated by all moves/inserts
        private int cursorlinecpos;   // computed on text set, updated by all moves/inserts, start of current line

        private int startpos = int.MaxValue; // set on text set if invalid
        private int startlineno;   // computed on text set, updated by all moves/inserts
        private int startlinecpos;   // computed on text set, updated by all moves/inserts, start of current line

        private int firstline = 0;
        private int startx = 0;

        private Color highlightColor { get; set; } = Color.Red;
        private Color lineColor { get; set; } = Color.Transparent;


    }
}

