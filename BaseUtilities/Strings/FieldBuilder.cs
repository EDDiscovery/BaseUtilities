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
        //     format string = <prefix>;<postfix> [;<format>] [;<condition>] [;<option>]
        //          if prefix starts with a <, no ,<spc> pad
        //     <format> = for string, numbers/datetime/ classes/enum 
        //              either ,<fieldwidth>:<format> or <format> or ,<fieldwidth>
        //              <fieldwidth> is positive left pad, negative right pad.  You must use the comma in front to tell it you want field width
        //              <format> is a standard numeric or datetime c# format https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings
        //              example: string ",20" enums ",20" numbers ",20:N0" or "N0"
        //              default is 0 for numbers or g for datetime
        //     <condition> = only for numbers or strings. Uses Eval engine, must compute to 1 or 0, string/number is passed in as 'Value'
        //     <option> = SCF means split caps full on output. Use comma to separate options
        //
        // or first object = NewPrefix only, define next pad to use, then go back to standard pad
        //
        // second object = data value
        //      if data value null, or result is empty (unless showblanks is used), or condition if given fails : field is removed and not shown

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
                else if ( first is string ctrlstring )     // normal, string
                {
                    System.Diagnostics.Debug.Assert(indexn + 2 <= values.Length,"Field Builder missing parameter");

                    object value = values[indexn + 1];

                    if (value != null)
                    {
                        Type t = value.GetType();
                        // field 3 is format, field 4 is condition.

                        string[] fieldnames = ctrlstring.Split(';');
                        string cond = fieldnames.Length >= 4 && fieldnames[3].Length>0 ? fieldnames[3] : null;
                        string output = null;
                        char fc = t.Name[0];

                        // see if the < in first field changes the padding between fields
                        string pad = padchars;
                        if (fieldnames[0].Length > 0 && fieldnames[0][0] == '<')
                        {
                            fieldnames[0] = fieldnames[0].Substring(1);
                            pad = "";
                        }

                        if (fc == 'S' && t.Name.Equals("String"))
                        {
                            string s = (string)value;
                            if (cond == null || Eval.EvalBool(cond, s))
                            {
                                if (fieldnames.Length >= 3)
                                    output = string.Format("{0" + fieldnames[2] + "}", s);
                                else
                                    output = s;
                            }
                        }
                        else if (fc == 'B' && t.Name.Equals("Boolean"))
                        {
                            if (fieldnames.Length != 2)
                            {
                                System.Diagnostics.Trace.WriteLine("*** FIELD BUILDER ERROR" + first);
                            }
                            else
                            {
                                output = ((bool)value) ? fieldnames[1] : fieldnames[0];
                                fieldnames[0] = fieldnames[1] = "";
                            }
                        }
                        else
                        {
                            string format = null;

                            if (fieldnames.Length >= 3 && fieldnames[2].Length>0)
                            {
                                if (fieldnames[2][0] == ',')
                                    format = "{0" + fieldnames[2] + "}";
                                else
                                    format = "{0:" + fieldnames[2] + "}";
                            }

                            if (t.IsPrimitive)
                            {
                                if (fc == 'I' && t.Name.Equals("Int32"))
                                {
                                    var v = ((int)value);
                                    if (cond == null || Eval.EvalBool(cond, v.ToStringInvariant()))
                                        output = format == null ? v.ToString(ct) : string.Format(ct, format, v);
                                }
                                else if (fc == 'I' && t.Name.Equals("Int64"))
                                {
                                    var v = ((long)value);
                                    if (cond == null || Eval.EvalBool(cond, v.ToStringInvariant()))
                                        output = format == null ? v.ToString(ct) : string.Format(ct, format, v);
                                }
                                else if (fc == 'D' && t.Name.Equals("Double"))
                                {
                                    double v = ((double)value);
                                    if (cond == null || Eval.EvalBool(cond, v.ToStringInvariant()))
                                        output = format == null ? v.ToString(ct) : string.Format(ct, format, v);
                                }
                                else if (fc == 'S' && t.Name.Equals("Single"))
                                {
                                    var v = ((float)value);
                                    if (cond == null || Eval.EvalBool(cond, v.ToStringInvariant()))
                                        output = format == null ? v.ToString(ct) : string.Format(ct, format, v);
                                }
                                else if (t.Name.Equals("UInt32"))
                                {
                                    var v = ((uint)value);
                                    if (cond == null || Eval.EvalBool(cond, v.ToStringInvariant()))
                                        output = format == null ? v.ToString(ct) : string.Format(ct, format, v);
                                }
                                else if (t.Name.Equals("UInt64"))
                                {
                                    var v = ((ulong)value);
                                    if (cond == null || Eval.EvalBool(cond, v.ToStringInvariant()))
                                        output = format == null ? v.ToString(ct) : string.Format(ct, format, v);
                                }
                                else if (t.Name.Equals("UInt16"))
                                {
                                    var v = ((ushort)value);
                                    if (cond == null || Eval.EvalBool(cond, v.ToStringInvariant()))
                                        output = format == null ? v.ToString(ct) : string.Format(ct, format, v);
                                }
                                else if (t.Name.Equals("Int16"))
                                {
                                    var v = ((short)value);
                                    if (cond == null || Eval.EvalBool(cond, v.ToStringInvariant()))
                                        output = format == null ? v.ToString(ct) : string.Format(ct, format, v);
                                }
                                else if (t.Name.Equals("UInt8"))
                                {
                                    var v = ((byte)value);
                                    if (cond == null || Eval.EvalBool(cond, v.ToStringInvariant()))
                                        output = format == null ? v.ToString(ct) : string.Format(ct, format, v);
                                }
                                else
                                    System.Diagnostics.Debug.Assert(false, $"Unknown primitive type {t.Name}");
                            }
                            else if (t.Name.Equals("DateTime"))
                            {
                                if (format == null)
                                    output = ((DateTime)value).ToString(ct);
                                else
                                    output = ((DateTime)value).ToString(format, ct);
                            }
                            else
                            {
                                //System.Diagnostics.Debug.WriteLine($"Fieldbuilder ToString for type {value.GetType().Name}");
                                output = format == null ? value.ToString() : string.Format(ct, format, value);
                            }
                        }

                        if ( output != null && (output.Length > 0 || showblanks) )   // if output not null, and has characters or show blanks
                        {
                            if (printed)      // if not first, separ
                            {
                                sb.Append(overrideprefix.Length > 0 ? overrideprefix : pad);
                            }

                            sb.Append(fieldnames[0]);       // print first field

                            if (fieldnames.Length>=5)
                            {
                                if (fieldnames[4].Contains("SCF"))
                                    output = output.SplitCapsWordFull();
                            }

                            sb.Append(output);              // print output

                            if (fieldnames.Length >= 2 && fieldnames[1].Length > 0)     // print postfix
                                sb.Append(fieldnames[1]);

                            overrideprefix = string.Empty;
                            printed = true;
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
