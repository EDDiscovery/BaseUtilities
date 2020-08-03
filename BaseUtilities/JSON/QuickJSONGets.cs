/*
 * Copyright © 2020 robby & EDDiscovery development team
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

namespace BaseUtils.JSON
{
    public static class JTokenExtensionsGet
    {
        // done as extension classes as it allows null to be in tk

        public static string MultiStr( this JToken tk, string[] ids, string def = "")       // multiple lookup in Object of names
        {
            JToken t = tk?.Contains(ids);
            return t != null && t.IsString ? (string)tk.Value : def;
        }

        public static string Str( this JToken tk, string def = "")
        {
            return tk != null && tk.IsString ? (string)tk.Value : def;
        }

        public static string StrNull( this JToken tk)
        {
            return tk != null && tk.IsString ? (string)tk.Value : null;
        }

        public static int Int( this JToken tk, int def = 0)
        {
            return tk != null && tk.IsLong ? (int)(long)tk.Value : def;
        }

        public static int? IntNull( this JToken tk)
        {
            return tk != null && tk.IsLong ? (int)(long)tk.Value : default(int?);
        }

        public static uint UInt( this JToken tk, uint def = 0)
        {
            return tk != null && tk.IsLong && (long)tk.Value >= 0 ? (uint)(long)tk.Value : def;
        }

        public static uint? UIntNull( this JToken tk)
        {
            return tk != null && tk.IsLong && (long)tk.Value >= 0 ? (uint)(long)tk.Value : default(uint?);
        }

        public static long Long( this JToken tk, long def = 0)
        {
            return tk != null && tk.IsLong ? (long)tk.Value : def;
        }

        public static long? LongNull( this JToken tk)
        {
            return tk != null && tk.IsLong ? (long)tk.Value : default(long?);
        }

        public static ulong ULong( this JToken tk, ulong def = 0)
        {
            if (tk == null)
                return def;
            else if (tk.TokenType == JToken.TType.ULong)
                return (ulong)tk.Value;
            else if (tk.IsLong && (long)tk.Value >= 0)
                return (ulong)(long)tk.Value;
            else
                return def;
        }

        public static System.Numerics.BigInteger BigInteger( this JToken tk, System.Numerics.BigInteger def)
        {
            if (tk == null)
                return def;
            else if (tk.TokenType == JToken.TType.ULong)
                return (ulong)tk.Value;
            else if (tk.IsLong && (long)tk.Value >= 0)
                return (ulong)(long)tk.Value;
            else if (tk.TokenType == JToken.TType.BigInt)
                return (System.Numerics.BigInteger)tk.Value;
            else
                return def;
        }

        public static bool Bool( this JToken tk, bool def = false)
        {
            return tk != null && tk.TokenType == JToken.TType.Boolean ? (bool)tk.Value : def;
        }

        public static bool? BoolNull( this JToken tk)
        {
            return tk != null && tk.TokenType == JToken.TType.Boolean ? (bool)tk.Value : default(bool?);
        }

        public static double Double( this JToken tk, double def = 0)
        {
            return tk != null && tk.TokenType == JToken.TType.Double ? (double)tk.Value : (tk.IsLong ? (double)(long)tk.Value : def);
        }

        public static double? DoubleNull( this JToken tk)
        {
            return tk != null && tk.TokenType == JToken.TType.Double ? (double)tk.Value : (tk.IsLong ? (double)(long)tk.Value : default(double?));
        }

        public static DateTime? DateTime( this JToken tk, System.Globalization.CultureInfo ci, System.Globalization.DateTimeStyles ds = System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal)
        {
            if (tk != null && tk.IsString && System.DateTime.TryParse((string)tk.Value, ci, ds, out DateTime ret))
                return ret;
            else
                return null;
        }

        public static DateTime DateTime( this JToken tk, DateTime def, System.Globalization.CultureInfo ci, System.Globalization.DateTimeStyles ds = System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal)
        {
            if (tk != null && tk.IsString && System.DateTime.TryParse((string)tk.Value, ci, ds, out DateTime ret))
                return ret;
            else
                return def;
        }

        public static DateTime DateTimeUTC( this JToken tk)
        {
            if (tk != null && tk.IsString && System.DateTime.TryParse((string)tk.Value, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal, out DateTime ret))
                return ret;
            else
                return new DateTime(2000, 1, 1);
        }

        public static JArray Array( this JToken tk)       // null if not
        {
            return tk as JArray;
        }

        public static JObject Object( this JToken tk)     // null if not
        {
            return tk as JObject;
        }

    }
}



