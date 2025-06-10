/*
 * Copyright © 2016 - 2022 EDDiscovery development team
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
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static partial class ObjectExtensionsStrings
{
    public static StringBuilder AppendPrePad(this System.Text.StringBuilder sb, string data, string prepad = " ")
    {
        if (data != null && data.Length > 0)
        {
            if (sb.Length > 0)
                sb.Append(prepad);
            sb.Append(data);
        }
        return sb;
    }
    public static StringBuilder AppendPrePadCS(this System.Text.StringBuilder sb, string data)
    {
        if (data != null && data.Length > 0)
        {
            if (sb.Length > 0)
                sb.Append(", ");
            sb.Append(data);
        }
        return sb;
    }
    public static StringBuilder AppendPrePadCR(this System.Text.StringBuilder sb, string data)
    {
        if (data != null && data.Length > 0)
        {
            if (sb.Length > 0)
                sb.Append(Environment.NewLine);
            sb.Append(data);
        }
        return sb;
    }

    // if data, place a CR in
    public static StringBuilder AppendCR(this System.Text.StringBuilder sb)
    {
        if (sb.Length > 0)
            sb.Append(Environment.NewLine);
        return sb;
    }

    // if data, place a CR in
    public static StringBuilder AppendSPC(this System.Text.StringBuilder sb)
    {
        if (sb.Length > 0)
            sb.Append(' ');
        return sb;
    }
    // if data, place a CR in
    public static StringBuilder AppendBracketed(this System.Text.StringBuilder sb,string str)
    {
        sb.Append('(');
        sb.Append(str);
        sb.Append(')');
        return sb;
    }

    // if data, place a comma space
    public static StringBuilder AppendCS(this System.Text.StringBuilder sb)
    {
        if (sb.Length > 0)
            sb.Append(", ");
        return sb;
    }

    public static StringBuilder AppendColonS(this System.Text.StringBuilder sb)
    {
        if (sb.Length > 0)
            sb.Append(": ");
        return sb;
    }

    public static StringBuilder AppendSemiColonS(this System.Text.StringBuilder sb)
    {
        if (sb.Length > 0)
            sb.Append("; ");
        return sb;
    }

    public static bool AppendPrePad(this System.Text.StringBuilder sb, string data, string prefix, string prepad, bool showblanks )
    {
        if (data != null && (showblanks || data.Length > 0))
        {
            if (sb.Length > 0)
                sb.Append(prepad);
            if (prefix.Length > 0)
                sb.Append(prefix);
            sb.Append(data);
            return true;
        }
        else
            return false;
    }

    public static string AppendPrePad(this string sb, string other, string prepad = " ")
    {
        if (other != null && other.Length > 0)
        {
            if (sb.Length > 0)
                sb += prepad;
            sb += other;
        }
        return sb;
    }

    static public void Build(this StringBuilder sb, params System.Object[] values)
    {
        BaseUtils.FieldBuilder.BuildField(sb, System.Globalization.CultureInfo.CurrentCulture, ", ", false, false, values);
    }
    // build into string. Padding is at front if buffer is already full
    static public void BuildCont(this StringBuilder sb, params System.Object[] values)
    {
        BaseUtils.FieldBuilder.BuildField(sb, System.Globalization.CultureInfo.CurrentCulture, ", ", false, true, values);
    }
    // build into string. Padding is internal only between fields and not at start
    static public void BuildSetPad(this StringBuilder sb, string padchars, params System.Object[] values)
    {
        BaseUtils.FieldBuilder.BuildField(sb, System.Globalization.CultureInfo.CurrentCulture, padchars, false, false, values);
    }
    static public void BuildSetPadCont(this StringBuilder sb, string padchars, params System.Object[] values)
    {
        BaseUtils.FieldBuilder.BuildField(sb, System.Globalization.CultureInfo.CurrentCulture, padchars, false, true, values);
    }
}

