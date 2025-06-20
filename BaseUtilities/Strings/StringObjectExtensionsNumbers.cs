﻿
/*
 * Copyright © 2016 - 2019 EDDiscovery development team
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

public static class ObjectExtensionsStringsNumbers
{
    public static string ToStringInvariant(this int v)
    {
        return v.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
    public static string ToStringInvariant(this int v, string format)
    {
        return v.ToString(format, System.Globalization.CultureInfo.InvariantCulture);
    }
    public static string ToStringInvariant(this uint v)
    {
        return v.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
    public static string ToStringInvariant(this uint v, string format)
    {
        return v.ToString(format, System.Globalization.CultureInfo.InvariantCulture);
    }
    public static string ToStringInvariant(this long v)
    {
        return v.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
    public static string ToStringInvariant(this long v, string format)
    {
        return v.ToString(format, System.Globalization.CultureInfo.InvariantCulture);
    }
    public static string ToStringInvariant(this ulong v)
    {
        return v.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
    public static string ToStringInvariant(this ulong v, string format)
    {
        return v.ToString(format, System.Globalization.CultureInfo.InvariantCulture);
    }
    public static string ToStringIntValue(this bool v)
    {
        return v ? "1" : "0";
    }
    public static string ToStringInvariant(this bool? v)
    {
        return (v.HasValue) ? (v.Value ? "1" : "0") : "";
    }
    public static string ToStringInvariant(this double v, string format)
    {
        return v.ToString(format, System.Globalization.CultureInfo.InvariantCulture);
    }
    public static string ToStringInvariantNAN(this double v, string format)
    {
        return v != double.NaN ? v.ToString(format, System.Globalization.CultureInfo.InvariantCulture) : "";
    }
    public static string ToStringInvariant(this double v)
    {
        return v.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
    public static string ToStringG17InvariantWithDot(this double v)     // G17 is roundtripable, and ensure we have a E or a dot
    {
        string vt = v.ToString("G17", System.Globalization.CultureInfo.InvariantCulture);
        if (vt.IndexOf("E", StringComparison.InvariantCultureIgnoreCase) < 0 && vt.IndexOf('.') < 0)
            vt += ".0";
        else
        { }
        return vt;
    }

    public static string ToStringInvariant(this float v, string format)
    {
        return v.ToString(format, System.Globalization.CultureInfo.InvariantCulture);
    }
    public static string ToStringInvariant(this float v)
    {
        return v.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
    public static string ToStringInvariant(this double? v, string format)
    {
        return (v.HasValue) ? v.Value.ToString(format, System.Globalization.CultureInfo.InvariantCulture) : "";
    }
    public static string ToStringInvariantNAN(this double? v, string format)
    {
        return (v.HasValue && v.Value != double.NaN) ? v.Value.ToString(format, System.Globalization.CultureInfo.InvariantCulture) : "";
    }
    public static string ToStringInvariant(this float? v, string format)
    {
        return (v.HasValue) ? v.Value.ToString(format, System.Globalization.CultureInfo.InvariantCulture) : "";
    }
    public static string ToStringInvariant(this int? v)
    {
        return (v.HasValue) ? v.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : "";
    }
    public static string ToStringInvariant(this int? v, string format)
    {
        return (v.HasValue) ? v.Value.ToString(format, System.Globalization.CultureInfo.InvariantCulture) : "";
    }
    public static string ToStringInvariant(this long? v)
    {
        return (v.HasValue) ? v.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : "";
    }
    public static string ToStringInvariant(this long? v, string format)
    {
        return (v.HasValue) ? v.Value.ToString(format, System.Globalization.CultureInfo.InvariantCulture) : "";
    }

    // safe as fmt can be crap string.. format it.
    // default Invariant but this can be overridden with "CurC"
    // Additional M type for "Minus X"
    // Additional M=text; type for "text X" on minus
    static public bool ToStringExtendedSafe(this double v, string fmt, out string output)
    {
        output = "";

        try
        {
            if (fmt.StartsWith("M="))
            {
                int indexofsemi = fmt.IndexOf(';');
                if (indexofsemi > 0)
                {
                    string text = fmt.Substring(2, indexofsemi - 2);

                    if (v < 0)
                    {
                        output = text + " ";
                        v = -v;
                    }

                    fmt = fmt.Substring(indexofsemi + 1);
                }
                else
                    throw new Exception();
            }
            else if (fmt.StartsWith("M"))
            {
                fmt = fmt.Substring(1);

                if (v < 0)
                {
                    output = "Minus ";
                    v = -v;
                }
            }

            System.Globalization.CultureInfo cl = System.Globalization.CultureInfo.InvariantCulture;

            if (fmt.StartsWith("CurC", StringComparison.InvariantCultureIgnoreCase))
            {
                cl = System.Globalization.CultureInfo.CurrentUICulture;
                fmt = fmt.Substring(4);
            }

            output += v.ToString(fmt, cl);
            return true;
        }
        catch
        {
            output = "Format must be a c# ToString format";
            return false;
        }
    }

    // safe as fmt can be crap string.. format it.
    // default Invariant but this can be overridden with "CurC"
    // Additional M type for "Minus X"
    // Additional M=text; type for "text X" on minus
    static public bool ToStringExtendedSafe(this long v, string fmt, out string output)
    {
        output = "";

        try
        {
            if (fmt.StartsWith("M="))
            {
                int indexofsemi = fmt.IndexOf(';');
                if (indexofsemi > 0)
                {
                    string text = fmt.Substring(2, indexofsemi - 2);

                    if (v < 0)
                    {
                        output = text + " ";
                        v = -v;
                    }

                    fmt = fmt.Substring(indexofsemi + 1);
                }
                else
                    throw new Exception();
            }
            else if (fmt.StartsWith("M"))
            {
                fmt = fmt.Substring(1);

                if (v < 0)
                {
                    output = "Minus ";
                    v = -v;
                }
            }

            if (fmt == "O")
                output += Convert.ToString(v, 8);
            else if (fmt == "B")
                output += Convert.ToString(v, 2);
            else
            {
                System.Globalization.CultureInfo cl = System.Globalization.CultureInfo.InvariantCulture;

                if (fmt.StartsWith("CurC", StringComparison.InvariantCultureIgnoreCase))
                {
                    cl = System.Globalization.CultureInfo.CurrentUICulture;
                    fmt = fmt.Substring(4);
                }

                output += v.ToString(fmt, cl);
            }
            return true;
        }
        catch
        {
            output = "Format must be a c# ToString format";
            return false;
        }
    }
}


