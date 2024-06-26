﻿/*
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
 * EDDiscovery is not affiliated with Frontier Developments plc.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static partial class ObjectExtensionsStrings
{
    public static string ToNullSafeString(this object obj)
    {
        return (obj ?? string.Empty).ToString();
    }
 
    public static string Alt(this string obj, string alt)
    {
        return (obj == null || obj.Length == 0) ? alt : obj;
    }

    public static void AppendPrePad(this System.Text.StringBuilder sb, string data, string prepad = " ")
    {
        if (data != null && data.Length > 0)
        {
            if (sb.Length > 0)
                sb.Append(prepad);
            sb.Append(data);
        }
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
}

