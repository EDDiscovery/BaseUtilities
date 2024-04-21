/*
 * Copyright 2016 - 2023 EDDiscovery development team
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
using System.Linq;

public static class ObjectExtensionsNumbersBool
{
    #region Evaluator - cheap and nasty

    public static bool Eval(this string ins, out string res)        // true, res = eval.  false, res = error
    {
        System.Data.DataTable dt = new System.Data.DataTable();

        res = "";

        try
        {
            var v = dt.Compute(ins, "");
            System.Type t = v.GetType();
            //System.Diagnostics.Debug.WriteLine("Type return is " + t.ToString());
            if (v is double)
                res = ((double)v).ToString(System.Globalization.CultureInfo.InvariantCulture);
            else if (v is System.Decimal)
                res = ((System.Decimal)v).ToString(System.Globalization.CultureInfo.InvariantCulture);
            else if (v is int)
                res = ((int)v).ToString(System.Globalization.CultureInfo.InvariantCulture);
            else
            {
                res = "Expression is Not A Number";
                return false;
            }

            return true;
        }
        catch
        {
            res = "Expression does not evaluate";
            return false;
        }
    }

    #endregion

    #region Int

    static public bool InvariantParse(this string s, out int i)
    {
        return int.TryParse(s, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out i);
    }

    static public int InvariantParseInt(this string s, int def)
    {
        int i;
        return int.TryParse(s, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out i) ? i : def;
    }

    static public int? InvariantParseIntNull(this string s)     // s can be null
    {
        int i;
        if (s != null && int.TryParse(s, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out i))
            return i;
        else
            return null;
    }
    static public int? InvariantParseIntNullIgnoreTextAfter(this string s)     // s can be null. s can have other chars after last digit at start
    {
        int i;
        if (s != null)
        {
            for (int p = 0; p < s.Length; p++)
            {
                bool atend = p == s.Length - 1;
                if (!char.IsDigit(s[p]) || atend)       // if not on digit or at end
                {
                    if (int.TryParse(s.Substring(0, p + (atend?1:0)), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out i))
                        return i;
                }
            }
        }
 
        return null;
    }
    static public int? ParseIntNull(this string s, System.Globalization.CultureInfo culture, System.Globalization.NumberStyles ns = System.Globalization.NumberStyles.None)     // s can be null
    {
        int i;
        if (s != null && int.TryParse(s, System.Globalization.NumberStyles.Integer | ns, culture, out i))
            return i;
        else
            return null;
    }

    static public int? InvariantParseIntNullOffset(this string s, int offset)     // s can be null, can have a +/- in front indicating offset
    {
        int i;
        if (s != null)
        {
            char first = s[0];
            if (first == '-' || first == '+')
                s = s.Substring(1);

            if (int.TryParse(s, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out i))
            {
                if (first == '-')
                    i = offset - i;
                else if (first == '+')
                    i = offset + i;

                return i;
            }
        }
        return null;
    }

    // given a character, return hex value (case insensitive) or null
    static public int? ToHex(this char c)
    {
        if (char.IsDigit(c))
            return c - '0';
        else if ("ABCDEF".Contains(c))
            return c - 'A' + 10;
        else if ("abcdef".Contains(c))
            return c - 'a' + 10;
        else
            return null;
    }

    // given a string, at position p, return hex double byte value ("...A1....") or null if not hex
    static public int? ToHex(this string s, int p)
    {
        if (s.Length > p + 1)
        {
            int? top = ToHex(s[p]);
            int? bot = ToHex(s[p + 1]);
            if (top.HasValue && bot.HasValue)
                return (top << 4) | bot;
        }
        return null;
    }

    // given a string, containing hex only double byte values ("A109A4"), convert.
    static public string FromHexString(this string ascii)
    {
        string s = "";
        for (int i = 0; i < ascii.Length; i += 2)
        {
            int? v = ascii.ToHex(i);
            if (v.HasValue)
                s += Convert.ToChar(v.Value);
            else
                return null;
        }

        return s;
    }

    static public int? ReadDecimalInt(ref string s)
    {
        int i = 0;
        while (i < s.Length && char.IsDigit(s[i]))
            i++;
        int? v = InvariantParseIntNull(s.Substring(0, i));
        if (v != null)
        {
            s = s.Substring(i);
            return v;
        }
        else
            return null;
    }

    #endregion

    #region Double

    static public bool InvariantParse(this string s, out double i)
    {
        return double.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out i);
    }

    static public double InvariantParseDouble(this string s, double def)
    {
        double i;
        return double.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out i) ? i : def;
    }

    static public double? InvariantParseDoubleNull(this string s)
    {
        double i;
        if (s != null && double.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out i))
            return i;
        else
            return null;
    }

    static public double? ParseDoubleNull(this string s)    // current culture
    {
        double i;
        if (s != null && double.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.CurrentCulture, out i))
            return i;
        else
            return null;
    }

    static public double? ParseDoubleNull(this string s, System.Globalization.CultureInfo culture, System.Globalization.NumberStyles ns = System.Globalization.NumberStyles.None)
    {
        double i;
        if (s != null && double.TryParse(s, System.Globalization.NumberStyles.Float | ns, culture, out i))
            return i;
        else
            return null;
    }

    static public double ParseDouble(this string s, double def, System.Globalization.CultureInfo culture, System.Globalization.NumberStyles ns = System.Globalization.NumberStyles.None)
    {
        double i;
        if (s != null && double.TryParse(s, System.Globalization.NumberStyles.Float | ns, culture, out i))
            return i;
        else
            return def;
    }

    #endregion

    #region Float

    static public float InvariantParseFloat(this string s, float def)
    {
        float i;
        return float.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out i) ? i : def;
    }

    static public float? InvariantParseFloatNull(this string s)
    {
        float i;
        if (s != null && float.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out i))
            return i;
        else
            return null;
    }

    static public float? ParseFloatNull(this string s, System.Globalization.CultureInfo culture, System.Globalization.NumberStyles ns = System.Globalization.NumberStyles.None)
    {
        float i;
        if (s != null && float.TryParse(s, System.Globalization.NumberStyles.Float | ns, culture, out i))
            return i;
        else
            return null;
    }
    static public float ParseFloat(this string s, float def, System.Globalization.CultureInfo culture, System.Globalization.NumberStyles ns = System.Globalization.NumberStyles.None)
    {
        float i;
        if (s != null && float.TryParse(s, System.Globalization.NumberStyles.Float | ns, culture, out i))
            return i;
        else
            return def;
    }

    #endregion

    #region Long

    static public bool InvariantParse(this string s, out long i)
    {
        return long.TryParse(s, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out i);
    }

    static public long InvariantParseLong(this string s, long def)
    {
        long i;
        return long.TryParse(s, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out i) ? i : def;
    }

    static public long? InvariantParseLongNull(this string s)
    {
        long i;
        if (s != null && long.TryParse(s, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out i))
            return i;
        else
            return null;
    }

    static public long? ParseLongNull(this string s, System.Globalization.CultureInfo culture, System.Globalization.NumberStyles ns = System.Globalization.NumberStyles.None)
    {
        long i;
        if (s != null && long.TryParse(s, System.Globalization.NumberStyles.Integer | ns, culture, out i))
            return i;
        else
            return null;
    }

    #endregion

    #region ULong

    static public ulong? InvariantParseULongNull(this string s)
    {
        ulong i;
        if (s != null && ulong.TryParse(s, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out i))
            return i;
        else
            return null;
    }

    static public ulong? ParseULongNull(this string s, System.Globalization.CultureInfo culture, System.Globalization.NumberStyles ns = System.Globalization.NumberStyles.None)
    {
        ulong i;
        if (s != null && ulong.TryParse(s, System.Globalization.NumberStyles.Integer | ns, culture, out i))
            return i;
        else
            return null;
    }

    static public ulong InvariantParseULong(this string s, ulong def)
    {
        ulong i;
        return ulong.TryParse(s, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out i) ? i : def;
    }

    #endregion

    #region Bool

    static public bool? InvariantParseBoolNull(this string s)
    {
        bool i;
        if (s != null)
        {
            if (bool.TryParse(s, out i))
            {
                return i;
            }
            else if (s.InvariantParse(out int v))
            {
                return v != 0;
            }
        }

        return null;
    }

    static public bool InvariantParseBool(this string s, bool def)
    {
        bool? i = InvariantParseBoolNull(s);
        return i.HasValue ? i.Value : def;
    }

    #endregion

    #region Version

    // versions are handled as Int Arrays here
    static public int[] VersionFromString(this string s)
    {
        string[] list = s.Split('.');
        return VersionFromStringArray(list);
    }

    static public int[] VersionFromStringArray(this string[] list)
    {
        if (list.Length > 0)
        {
            int[] v = new int[list.Length];

            for (int i = 0; i < list.Length; i++)
            {
                if (!list[i].InvariantParse(out v[i]))
                    return null;
            }

            return v;
        }

        return null;
    }

    static public int CompareVersion(this int[] v1, int[] v2)    // is V1>V2, 1, 0 = equals, -1 less
    {
        for (int i = 0; i < v1.Length; i++)
        {
            if (i >= v2.Length || v1[i] > v2[i])
                return 1;
            else if (v1[i] < v2[i])
                return -1;
        }

        return 0;
    }

    #endregion

    #region Arrays and Lists

    // fill array from comma separ string, with defined length and defined default
    static public int[] RestoreArrayFromString(this string plist, int def, int length)      
    {
        int i = 0;
        string[] parray = plist.Split(',');
        int[] newarray = new int[length];
        for (; i < length; i++)
        {
            if (i >= parray.Length || !parray[i].InvariantParse(out newarray[i]))
                newarray[i] = def;
        }

        return newarray;
    }

    // fill List from comma separ string, with min leng (def if less) and max length
    static public List<int> RestoreIntListFromString(this string plist, int minlength = 0, int def = 0, int maxlength = int.MaxValue)
    {
        List<int> list = new List<int>();

        if (plist.Length > 0)
        {
            string[] parray = plist.Split(',');

            for (int i = 0; i < parray.Length && list.Count < maxlength; i++)
            {
                int v;
                if (parray[i].InvariantParse(out v))
                    list.Add(v);
            }

        }

        while (list.Count < minlength)
            list.Add(def);

        return list;
    }

    // fill array from comma separ string, with min leng (def if less) and max length
    static public bool RestoreArrayFromString(this string plist, out int[] array , int? min = null, int? max = null)   // string of comma values, parse out to array, false if any fail
    {
        string[] parray = plist.Split(',');

        array = new int[parray.Length];

        for (int i = 0; i < array.Length; i++)
        {
            if (!parray[i].InvariantParse(out array[i]) && (!min.HasValue || array[i]>=min) && (!max.HasValue || array[i]<=max))
                return false;
        }

        return true;
    }

    #endregion

    #region Enhanced Compare

    // code is 0 = equal, 1 = v is greater, 2 = v is greater equal, -1, -2
    static public bool CompareTo<T>(this T v, T other, int code) where T : IComparable       
    {
        int compare = v.CompareTo(other);

        if (code == -2)
            return compare <= 0;    //-1 less or 0 equal good
        else if (code == 2)
            return compare >= 0;
        else
            return compare == code; // must be the same
    }

    #endregion

    #region stuff that should have been in Math

    public static int Range(this int a, int min, int max)
    {
        return Math.Min(Math.Max(a, min), max);
    }
    public static int Clamp(this int a, int min, int max)
    {
        return Math.Min(Math.Max(a, min), max);
    }
    public static long Range(this long a, long min, long max)
    {
        return Math.Min(Math.Max(a, min), max);
    }
    public static float Range(this float a, float min, float max)
    {
        return Math.Min(Math.Max(a, min), max);
    }
    public static float Clamp(this float a, float min, float max)   // opengl name
    {
        return Math.Min(Math.Max(a, min), max);
    }
    public static double Range(this double a, double min, double max)
    {
        return Math.Min(Math.Max(a, min), max);
    }

    public static float Radians(this float x)
    {
        return x * (float)(Math.PI / 180.0);
    }

    public static float Degrees(this float x)
    {
        return x * (float)(180.0 / Math.PI);
    }

    public static double Radians(this double x)
    {
        return x * (Math.PI / 180.0);
    }

    public static double Degrees(this double x)
    {
        return x * (180.0 / Math.PI);
    }

    public static float BoundedAngle(this float angle)
    {
        return ((angle + 360 + 180) % 360) - 180;
    }

    public static float BoundedAngle(this float angle, float add)
    {
        return ((angle + add + 360 + 180) % 360) - 180;
    }

    public static float Fract(this float a)
    {
        return a - (float)Math.Floor(a);
    }

    public static float Mix(float a, float b, float mix)
    {
        return a + (b - a) * mix;
    }

    public static float Abs(this float a)
    {
        return (a < 0) ? -a : a;
    }

    // https://en.wikipedia.org/wiki/Gaussian_function
    public static double GaussianDist(double x, double centre, double stddist)     
    {
        return Math.Exp(-(x - centre) * (x - centre) / (2 * stddist * stddist));
    }

    // Wichura 1998, Gentle 2003, https://www.statsdirect.com/help/randomization/generate_random_numbers.htm
    public static double GaussianNoise(double x, double u, double stddist)
    {
        return 1 / Math.Sqrt(2 * Math.PI * stddist) * Math.Exp(-(x - u) * (x - u) / (2 * stddist * stddist));       
    }

    // fron newtonsoft JSON, et al, calculate relative epsilon and compare
    public static bool ApproxEquals(this double left, double right, double epsilon = 2.2204460492503131E-16)       
    {
        if (left == right)
        {
            return true;
        }

        double tolerance = ((Math.Abs(left) + Math.Abs(right)) + 10.0) * epsilon;       // given an arbitary epsilon, scale to magnitude of values
        double difference = left - right;
        //System.Diagnostics.Debug.WriteLine("Approx equal {0} {1}", tolerance, difference);
        return (-tolerance < difference && tolerance > difference);
    }

    static public double Length(double x, double y, double z, double ox, double oy, double oz)
    {
        return Math.Sqrt((x - ox) * (x - ox) + (y - oy) * (y - oy) + (z - oz) * (z - oz));
    }

    static public double Length(double x, double y,  double ox, double oy)
    {
        return Math.Sqrt((x - ox) * (x - ox) + (y - oy) * (y - oy));
    }


    // from lat/long to lat/long, return bearing 0-360
    static public double CalculateBearing(double latitude, double longitude, double targetLat, double targetLong)
    {
        // turn degrees to radians
        double long1 = longitude.Radians();
        double lat1 = latitude.Radians(); 
        double long2 = targetLong.Radians();
        double lat2 = targetLat.Radians();

        double y = Math.Sin(long2 - long1) * Math.Cos(lat2);
        double x = Math.Cos(lat1) * Math.Sin(lat2) -
                    Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(long2 - long1);

        // back to degrees again
        double bearing = Math.Atan2(y, x) / Math.PI * 180;

        //bearing in 0-360, not -180 to 180
        return bearing > 0 ? bearing : 360 + bearing;
    }

    // distance between two points with radius
    static public double CalculateDistance(double latitude, double longitude, double targetLat, double targetLong, double radius)
    {
        // example: https://www.movable-type.co.uk/scripts/latlong.html

        double lat1 = latitude.Radians();
        double lat2 = targetLat.Radians();
        double deltaLong = (targetLong - longitude).Radians();
        double deltaLat = (targetLat - latitude).Radians();

        double a = (Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2)) + Math.Cos(lat1) * Math.Cos(lat2) * (Math.Sin(deltaLong / 2) * Math.Sin(deltaLong / 2));
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return radius * c;
    }

    // Distance between two lat/longs with radius and altitude
    static public double CalculateDistance(double latitude, double longitude, double alitiude, double targetLat, double targetLong, double targetalt, double radius)
    {
        // https://www.mathworks.com/matlabcentral/answers/403262-calculate-distance-between-two-coordinates-with-depth

        double flatdistance = CalculateDistance(latitude, longitude, targetLat, targetLong, radius);
        double altdiff = (alitiude - targetalt);
        return Math.Sqrt(flatdistance * flatdistance + altdiff * altdiff);
    }

    // return slope and back facing result (>=0 means facing backwards)
    static public Tuple<double,double> CalculateGlideslope(double distance, double altitude, double radius)
    {
        double theta = distance / radius;
        double rad = 1 + altitude / radius;
        double dist2 = 1 + rad * rad - 2 * rad * Math.Cos(theta);

        // a = radius, b = distance, c = altitude + radius
        // c^2 = a^2 + b^2 - 2.a.b.cos(C) -> cos(C) = (a^2 + b^2 - c^2) / (2.a.b)
    
        double sintgtslope = (1 + dist2 - rad * rad) / (2 * Math.Sqrt(dist2));

        // a = altitude + radius, b = distance, c = radius
        double slope = -Math.Asin((rad * rad + dist2 - 1) / (2 * rad * Math.Sqrt(dist2))) * 180 / Math.PI;

        return new Tuple<double, double>(slope, sintgtslope);
    }

    #endregion
}

