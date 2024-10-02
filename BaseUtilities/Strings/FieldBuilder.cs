/*
 * Copyright © 2017-2023 EDDiscovery development team
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

namespace BaseUtils
{
    static public class FieldBuilder
    {
        // first object = format string
        // second object = data value
        //
        //      if data value null, or string is empty (unless showblanks is used) : field is removed and not shown
        //
        //      if data value is string, format = prefix;postfix
        //      if data value is bool, format = false text;true text
        //      if data value is double/float, format = prefix;postfix[;floatformat]    format = "0" if not present    
        //      if data value is int/long, format = prefix;postfix [;int format]        format = "0" if not present
        //      if data value is date time, format = prefix;postfix [;date format]      format = "g" if not present
        //      if data value is enum, format = prefix;postfix 
        //
        //      if prefix starts with a <, no ,<spc> pad
        //
        // or first object = NewPrefix only, define next pad to use, then go back to standard pad


        public class NewPrefix   // indicator class, use this as first item to indicate the next prefix to use.  After one use, its discarded.
        {
            public string prefix;
            public NewPrefix(string s) { prefix = s; }
        }

        static public string Build(params System.Object[] values)
        {
            var sb = new System.Text.StringBuilder(256);
            BuildField(sb, System.Globalization.CultureInfo.CurrentCulture, ", ", false, false,values);
            return sb.ToString();
        }

        static public string BuildSetPad(string padchars, params System.Object[] values)
        {
            var sb = new System.Text.StringBuilder(256);
            BuildField(sb, System.Globalization.CultureInfo.CurrentCulture, padchars, false, false, values);
            return sb.ToString();
        }
        static public string BuildSetPadShowBlanks(string padchars, bool showblanks, params System.Object[] values)
        {
            var sb = new System.Text.StringBuilder(256);
            BuildField(sb,System.Globalization.CultureInfo.CurrentCulture, padchars, showblanks, false, values);
            return sb.ToString();
        }

        /// <summary>
        /// Field Builder, an alternate formatter
        /// </summary>
        /// <param name="sb">buffer</param>
        /// <param name="ct">culture to print numbers in</param>
        /// <param name="padchars">padding between items, unless overriden by NewPrefix </param>
        /// <param name="showblanks">show blank items</param>
        /// <param name="padifbufferfull">if true, and sb is filled, pad first item. Else don't pad first item </param>
        /// <param name="values">Value list</param>
        
        static public void BuildField(System.Text.StringBuilder sb, System.Globalization.CultureInfo ct, string padchars, bool showblanks, bool padifbufferfull = false, params System.Object[] values)
        { 
            string overrideprefix = string.Empty;

            bool printed = padifbufferfull ? sb.Length > 0 : false;             // if padifbufferfull then if there is anything in it, we pad first. else we dont

            for (int indexn = 0; indexn < values.Length;)
            {
                Object first = values[indexn];

                if ( first is NewPrefix )       // first item is special, a new prefix, override
                {
                    overrideprefix = (first as NewPrefix).prefix;
                    indexn++;
                }
                else if ( first is string )     // normal, string
                {
                    System.Diagnostics.Debug.Assert(indexn + 2 <= values.Length,"Field Builder missing parameter");

                    string[] fieldnames = ((string)first).Split(';');

                    object value = values[indexn + 1];

                    string pad = padchars;
                    if (fieldnames[0].Length > 0 && fieldnames[0][0] == '<')
                    {
                        fieldnames[0] = fieldnames[0].Substring(1);
                        pad = "";
                    }

                    if (value != null)
                    {
                        if (value is bool)
                        {
                            if (fieldnames.Length != 2)
                            {
                                sb.AppendPrePad("!!REPORT ERROR IN FORMAT STRING " + first + "!!", (overrideprefix.Length > 0) ? overrideprefix : pad);
                                System.Diagnostics.Trace.WriteLine("*** FIELD BUILDER ERROR" + first);
                            }
                            else
                            {
                                string s = ((bool)value) ? fieldnames[1] : fieldnames[0];
                                sb.AppendPrePad(s, (overrideprefix.Length > 0) ? overrideprefix : pad);
                                overrideprefix = string.Empty;
                            }
                        }
                        else
                        {
                            string format = fieldnames.Length >= 3 ? fieldnames[2] : "0";

                            string output;
                            if (value is string)
                            {
                                output = (string)value;
                            }
                            else if (value is int)
                            {
                                output = ((int)value).ToString(format, ct);
                            }
                            else if (value is long)
                            {
                                output = ((long)value).ToString(format, ct);
                            }
                            else if (value is double)
                            {
                                output = ((double)value).ToString(format, ct);
                            }
                            else if (value is float)
                            {
                                output = ((float)value).ToString(format, ct);
                            }
                            else if (value is ushort)
                            {
                                output = ((ushort)value).ToString(format, ct);
                            }
                            else if (value is short)
                            {
                                output = ((short)value).ToString(format, ct);
                            }
                            else if (value is uint)
                            {
                                output = ((uint)value).ToString(format, ct);
                            }
                            else if (value is ulong)
                            {
                                output = ((ulong)value).ToString(format, ct);
                            }
                            else if (value is double?)
                            {
                                output = ((double?)value).Value.ToString(format,ct);
                            }
                            else if (value is float?)
                            {
                                output = ((float?)value).Value.ToString(format,ct);
                            }
                            else if (value is int?)
                            {
                                output = ((int?)value).Value.ToString(format, ct);
                            }
                            else if (value is uint?)
                            {
                                output = ((uint?)value).Value.ToString(format, ct);
                            }
                            else if (value is ushort?)
                            {
                                output = ((ushort?)value).Value.ToString(format, ct);
                            }
                            else if (value is short?)
                            {
                                output = ((short?)value).Value.ToString(format, ct);
                            }
                            else if (value is long?)
                            {
                                output = ((long?)value).Value.ToString(format, ct);
                            }
                            else if (value is ulong?)
                            {
                                output = ((ulong?)value).Value.ToString(format, ct);
                            }
                            else if (value is DateTime)
                            {
                                format = fieldnames.Length >= 3 ? fieldnames[2] : "g";
                                output = ((DateTime)value).ToString(format,ct);
                            }
                            else
                            {
                                Type t = value.GetType();
                                if (t.BaseType.Name.Equals("Enum"))
                                {
                                    var ev = Activator.CreateInstance(t);
                                    ev = value;
                                    output = ev.ToString();
                                }
                                else
                                {
                                    output = "";
                                    System.Diagnostics.Debug.Assert(false);
                                }
                            }

                            if (output.Length > 0 || showblanks)    // if output not blank, or show blanks
                            {
                                if (printed)      // if not first, separ
                                {
                                    sb.Append(overrideprefix.Length > 0 ? overrideprefix : pad);
                                }

                                sb.Append(fieldnames[0]);       // print first field
                                sb.Append(output);              // print output
                                if (fieldnames.Length >= 2 && fieldnames[1].Length > 0)
                                    sb.Append(fieldnames[1]);

                                overrideprefix = string.Empty;
                                printed = true;
                            }
                        }
                    }

                    indexn += 2;
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false);
                    sb.Clear();
                    sb.Append("!!REPORT ERROR IN FORMAT STRING!!");
                }
            }
        }

    }
}
