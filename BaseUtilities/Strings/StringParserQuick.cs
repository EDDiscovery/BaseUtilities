/*
 * Copyright © 2018-2020 EDDiscovery development team
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
using System.Linq;
using System.Text;

namespace BaseUtils
{
    // Quicker version of StringParser.

    [System.Diagnostics.DebuggerDisplay("Action {line.Substring(pos)} : ({line})")]
    public class StringParser2
    {
        private int pos;        // always left after an operation on the next non space char
        private char[] line;

        #region Init and basic status

        public StringParser2(string l, int p = 0)
        {
            line = l.ToCharArray();
            pos = p;
            SkipSpace();
        }

        public int Position { get { return pos; } }
        public string Line { get { return new string(line,0,line.Length); } }
        public string LineLeft { get { return new string(line,pos,line.Length-pos); } }
        public bool IsEOL { get { return pos == line.Length; } }
        public int Left { get { return Math.Max(line.Length - pos,0); } }

        #endregion

        #region Character or String related functions

        public void SkipSpace()
        {
            while (pos < line.Length && char.IsWhiteSpace(line[pos]))
                pos++;
        }

        public void SkipCharAndSkipSpace()
        {
            pos++;
            while (pos < line.Length && char.IsWhiteSpace(line[pos]))
                pos++;
        }

        public char PeekChar()
        {
            return (pos < line.Length) ? line[pos] : ' ';
        }

        public char GetChar(bool skipspace = false)       // minvalue if at EOL.. Default no skip for backwards compat
        {
            if (pos < line.Length)
            {
                char ch = line[pos++];
                if ( skipspace )
                    SkipSpace();
                return ch;
            }
            else
                return char.MinValue;
        }

        public bool IsStringMoveOn(string s)
        {
            for( int i = 0; i < s.Length; i++)
            {
                if (line[pos + i] != s[i])
                    return false;
            }

            pos += s.Length;
            SkipSpace();

            return true;
        }

        public bool IsCharMoveOn(char t, bool skipspace = true)
        {
            if (pos < line.Length && line[pos] == t)
            {
                pos++;
                if (skipspace)
                    SkipSpace();
                return true;
            }
            else
                return false;
        }


        #endregion

        #region WORDs bare

        // Your on a " or ' quoted string, extract it

        public StringBuilder NextQuotedWord(bool replaceescape = false)
        {
            if (pos >= line.Length)     // null if there is nothing..
                return null;
            else
            {
                char quote = line[pos++];

                StringBuilder b = new StringBuilder(line.Length - pos);

                while (line[pos] != quote)
                {
                    if (line[pos] == '\\' && pos < line.Length - 1) // 2 chars min
                    {
                        pos++;
                        char esc = line[pos++];     // grab escape and move on

                        if (esc == quote)
                        {
                            b.Append(esc);      // place in the character
                        }
                        else if (replaceescape)
                        {
                            switch (esc)
                            {
                                case '\\':
                                    b.Append('\\');
                                    break;
                                case '/':
                                    b.Append('/');
                                    break;
                                case 'b':
                                    b.Append('\b');
                                    break;
                                case 'f':
                                    b.Append('\f');
                                    break;
                                case 'n':
                                    b.Append('\n');
                                    break;
                                case 'r':
                                    b.Append('\r');
                                    break;
                                case 't':
                                    b.Append('\t');
                                    break;
                                case 'u':
                                    if (pos < line.Length - 4)
                                    {
                                        int? v1 = line[pos++].ToHex();
                                        int? v2 = line[pos++].ToHex();
                                        int? v3 = line[pos++].ToHex();
                                        int? v4 = line[pos++].ToHex();
                                        if (v1 != null && v2 != null && v3 != null && v4 != null)
                                        {
                                            char c = (char)((v1 << 12) | (v2 << 8) | (v3 << 4) | (v4 << 0));
                                            b.Append(c);
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                    else
                        b.Append(line[pos++]);
                        
                    if (pos == line.Length)  // no end quote, wrong
                        return null;
                }

                pos++; //skip end quote
                while (pos < line.Length && char.IsWhiteSpace(line[pos]))
                    pos++;
                        
                return b;
            }
        }

        #endregion

        #region Numbers and Bools

        static char[] decchars = new char[] { '.', 'e', 'E', '+', '-' };

        public object NextLongULongBigIntegerOrDouble()      // value or null
        {
            var ty = NextLongULongBigIntegerOrDouble(out string part, out ulong ulv, out int sign);

            if (ty == StringParser2.ObjectType.Ulong)
            {
                if (sign == -1)
                    return -(long)ulv;
                else if (ulv <= long.MaxValue)
                    return (long)ulv;
                else
                    return ulv;
            }
            else if (ty == StringParser2.ObjectType.Double)
            {
                if (double.TryParse(part, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double dv))
                    return dv;
            }
            else if (ty == StringParser2.ObjectType.BigInt)
            {
                if (System.Numerics.BigInteger.TryParse(part, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out System.Numerics.BigInteger bv))
                    return bv;
            }

            return null;
        }

        public enum ObjectType { Failed, Double, Ulong, BigInt };

        public ObjectType NextLongULongBigIntegerOrDouble(out string part, out ulong ulv, out int sign)        
        {
            part = null; ulv = 0; sign = 1;
            if (pos >= line.Length)     // null if there is nothing..
                return ObjectType.Failed;
            else
            {
                int start = pos;

                if (line[pos] == '+')
                {
                    pos++;
                }
                else if (line[pos] == '-')
                {
                    pos++;
                    sign = -1;
                }

                bool bigint = false;

                while (pos < line.Length && line[pos] >= '0' && line[pos] <= '9')
                {
                    if (ulv > ulong.MaxValue / 10)  // if going to overflow, bit int. continue and ignore acc
                        bigint = true;

                    ulv = (ulv * 10) + (ulong)(line[pos++] - '0');
                }

                if (pos < line.Length && line[pos] == '.' || line[pos] == 'E' || line[pos] == 'e')
                {
                    while (pos < line.Length && ((line[pos] >= '0' && line[pos] <= '9') || decchars.Contains(line[pos])))
                        pos++;

                    part = new string(line, start, pos - start);

                    while (pos < line.Length && char.IsWhiteSpace(line[pos]))
                        pos++;

                    return ObjectType.Double;
                }
                else if (bigint == true)
                {
                    part = new string(line, start, pos - start);

                    while (pos < line.Length && char.IsWhiteSpace(line[pos]))
                        pos++;

                    return ObjectType.BigInt;
                }
                else
                {
                    while (pos < line.Length && char.IsWhiteSpace(line[pos]))
                        pos++;

                    return ObjectType.Ulong;
                }
            }
        }

        #endregion

    }
}
