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
using System.Collections;
using System.Collections.Generic;

namespace BaseUtils.JSON
{
    // small light JSON decoder and encoder.

    // JToken is the base class, can parse, encode.  JArray/JObject for json structures

    [System.Diagnostics.DebuggerDisplay("{TokenType} {ToString()}")]
    public partial class JToken : IEnumerable<JToken>, IEnumerable
    {
        public enum TType { Null, Boolean, String, Double, Long, ULong, BigInt, Object, Array, EndObject, EndArray }

        public TType TokenType { get; set; }                    // type of token
        public Object Value { get; set; }                       // value of token, if it has one
        public string Name { get; set; }                        // only set if its a property of an object

        public bool HasValue { get { return Value != null;  } } // true for bools/string/double/long/ulong/bigint
        public bool IsString { get { return TokenType == TType.String; } }
        public bool IsInt { get { return TokenType == TType.Long || TokenType == TType.ULong || TokenType == TType.BigInt; } }
        public bool IsLong { get { return TokenType == TType.Long; } }
        public bool IsBigInt { get { return TokenType == TType.BigInt; } }
        public bool IsULong { get { return TokenType == TType.ULong; } }
        public bool IsDouble { get { return TokenType == TType.Double; } }
        public bool IsBool { get { return TokenType == TType.Boolean; } }
        public bool IsArray { get { return TokenType == TType.Array; } }
        public bool IsObject { get { return TokenType == TType.Object; } }
        public bool IsNull { get { return TokenType == TType.Null; } }
        public bool IsProperty { get { return Name != null; } }                     // indicates that the object is an object property
        public bool IsEndObject { get { return TokenType == TType.EndObject; } }    // only seen for TokenReader
        public bool IsEndArray { get { return TokenType == TType.EndArray; } }      // only seen for TokenReader

        #region Construction

        public JToken()
        {
            TokenType = TType.Null;
        }

        public JToken(JToken other)
        {
            TokenType = other.TokenType;
            Value = other.Value;
            Name = other.Name;
        }

        public JToken(TType t, Object v = null)
        {
            TokenType = t; Value = v;
        }

        public static implicit operator JToken(string v)        // autoconvert types to JToken types
        {
            if (v == null)
                return new JToken(TType.Null);
            else
                return new JToken(TType.String, v);
        }
        public static implicit operator JToken(int v)
        {
            return new JToken(TType.Long, (long)v);
        }
        public static implicit operator JToken(uint v)
        {
            return new JToken(TType.Long, (long)v);
        }
        public static implicit operator JToken(long v)
        {
            return new JToken(TType.Long, v);
        }
        public static implicit operator JToken(ulong v)
        {
            return new JToken(TType.ULong, v);
        }
        public static implicit operator JToken(double v)
        {
            return new JToken(TType.Double, v);
        }
        public static implicit operator JToken(float v)
        {
            return new JToken(TType.Double, (double)v);
        }
        public static implicit operator JToken(bool v)
        {
            return new JToken(TType.Boolean, v);
        }
        public static implicit operator JToken(DateTime v)
        {
            return new JToken(TType.String, v.ToStringZulu());
        }
        public static JToken CreateToken(Object o, bool except = true)
        {
            if (o == null)
                return Null();
            else if (o is string)
                return (string)o;
            else if (o is double || o is double?)
                return (double)o;
            else if (o is float || o is float?)
                return (float)o;
            else if (o is long || o is long?)
                return (long)o;
            else if (o is ulong || o is ulong?)
                return (ulong)o;
            else if (o is int || o is int?)
                return (int)o;
            else if (o is uint || o is uint?)
                return (uint)o;
            else if (o is bool || o is bool?)
                return (bool)o;
            else if (o is JArray)
            {
                var ja = o as JArray;
                return ja.Clone();
            }
            else if (o is JObject)
            {
                var jo = o as JObject;
                return jo.Clone();
            }
            else if (o is Enum)
            {
                return o.ToString();
            }
            else if (o is DateTime)
            {
                return ((DateTime)o).ToStringZulu();
            }
            else if (except)
                throw new NotImplementedException();
            else
            {
                System.Diagnostics.Debug.WriteLine("Failed to serialise type " + o.GetType().Name);
                return null;
            }
        }

        public static JToken Null()
        {
            return new JToken(TType.Null);
        }

        public static explicit operator string(JToken t)
        {
            if (t.TokenType == TType.Null || t.TokenType != TType.String)
                return null;
            else
                return (string)t.Value;
        }
        public static explicit operator int? (JToken t)     // doubles get trunced.. as per previous system
        {
            if (t.TokenType == TType.Long)
                return (int)(long)t.Value;
            else if (t.TokenType == TType.Double)
                return (int)(double)t.Value;
            else
                return null;
        }
        public static explicit operator int(JToken t)
        {
            if (t.TokenType == TType.Long)
                return (int)(long)t.Value;
            else if (t.TokenType == TType.Double)
                return (int)(double)t.Value;
            else
                throw new NotSupportedException();
        }
        public static explicit operator uint? (JToken t)
        {
            if (t.TokenType == TType.Long && (long)t.Value >= 0)
                return (uint)(long)t.Value;
            else if (t.TokenType == TType.Double && (double)t.Value >= 0)
                return (uint)(double)t.Value;
            else
                return null;
        }
        public static explicit operator uint(JToken t)
        {
            if (t.TokenType == TType.Long && (long)t.Value >= 0)
                return (uint)(long)t.Value;
            else if (t.TokenType == TType.Double && (double)t.Value >= 0)
                return (uint)(double)t.Value;
            else
                throw new NotSupportedException();
        }
        public static explicit operator long? (JToken t)
        {
            if (t.TokenType == TType.Long)
                return (long)t.Value;
            else if (t.TokenType == TType.Double)
                return (long)(double)t.Value;
            else
                return null;
        }
        public static explicit operator long(JToken t)
        {
            if (t.TokenType == TType.Long)
                return (long)t.Value;
            else if (t.TokenType == TType.Double)
                return (long)(double)t.Value;
            else
                throw new NotSupportedException();
        }
        public static explicit operator ulong? (JToken t)
        {
            if (t.TokenType == TType.Long && (long)t.Value >= 0)
                return (ulong)(long)t.Value;
            else if (t.TokenType == TType.Double && (double)t.Value >= 0)
                return (ulong)(double)t.Value;
            else
                return null;
        }
        public static explicit operator ulong(JToken t)
        {
            if (t.TokenType == TType.Long && (long)t.Value >= 0)
                return (ulong)(long)t.Value;
            else if (t.TokenType == TType.Double && (double)t.Value >= 0)
                return (ulong)(double)t.Value;
            else
                throw new NotSupportedException();
        }
        public static explicit operator double? (JToken t)
        {
            if (t.TokenType == TType.Long)
                return (double)(long)t.Value;
            else if (t.TokenType == TType.ULong)
                return (double)(ulong)t.Value;
            else if (t.TokenType == TType.Double)
                return (double)t.Value;
            else
                return null;
        }
        public static explicit operator double(JToken t)
        {
            if (t.TokenType == TType.Long)
                return (double)(long)t.Value;
            else if (t.TokenType == TType.ULong)
                return (double)(ulong)t.Value;
            else if (t.TokenType == TType.Double)
                return (double)t.Value;
            else
                throw new NotSupportedException();
        }
        public static explicit operator float? (JToken t)
        {
            if (t.TokenType == TType.Long)
                return (float)(long)t.Value;
            else if (t.TokenType == TType.ULong)
                return (float)(ulong)t.Value;
            else if (t.TokenType == TType.Double)
                return (float)(double)t.Value;
            else
                return null;
        }
        public static explicit operator float(JToken t)
        {
            if (t.TokenType == TType.Long)
                return (float)(long)t.Value;
            else if (t.TokenType == TType.ULong)
                return (float)(ulong)t.Value;
            else if (t.TokenType == TType.Double)
                return (float)(double)t.Value;
            else
                throw new NotSupportedException();
        }
        public static explicit operator bool? (JToken t)
        {
            if (t.TokenType == TType.Boolean)
                return (bool)t.Value;
            else if (t.TokenType == TType.Long)       // accept LONG 1/0 as boolean
                return (long)t.Value != 0;
            else
                return null;
        }
        public static explicit operator bool(JToken t)
        {
            if (t.TokenType == TType.Boolean)
                return (bool)t.Value;
            else if (t.TokenType == TType.Long)       // accept LONG 1/0 as boolean
                return (long)t.Value != 0;
            else
                throw new NotSupportedException();
        }
        public static explicit operator DateTime(JToken t)
        {
            if (t.IsString && System.DateTime.TryParse((string)t.Value, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal, out DateTime ret))
                return ret;
            else
                return DateTime.MinValue;
        }

        public JToken Clone()   // make a copy of the token
        {
            switch (TokenType)
            {
                case TType.Array:
                    {
                        JArray copy = new JArray();
                        foreach (JToken t in this)
                        {
                            copy.Add(t.Clone());
                        }

                        return copy;
                    }
                case TType.Object:
                    {
                        JObject copy = new JObject();
                        foreach (var kvp in (JObject)this)
                        {
                            copy[kvp.Key] = kvp.Value.Clone();
                        }
                        return copy;
                    }
                default:
                    return new JToken(this);
            }
        }

        #endregion

        #region Operators and functions

        public bool DeepEquals(JToken other)
        {
            switch (TokenType)
            {
                case TType.Array:
                    {
                        JArray us = (JArray)this;
                        if (other.TokenType == TType.Array)
                        {
                            JArray ot = (JArray)other;
                            if (ot.Count == us.Count)
                            {
                                for (int i = 0; i < us.Count; i++)
                                {
                                    if (!us[i].DeepEquals(other[i]))
                                        return false;
                                }
                                return true;
                            }
                            else
                                return false;
                        }
                        else
                            return false;
                    }

                case TType.Object:
                    {
                        JObject us = (JObject)this;
                        if (other.TokenType == TType.Object)
                        {
                            JObject ot = (JObject)other;
                            if (ot.Count == us.Count)
                            {
                                foreach (var kvp in us)
                                {
                                    if (!ot.ContainsKey(kvp.Key) || !kvp.Value.DeepEquals(ot[kvp.Key]))       // order unimportant to kvp
                                        return false;
                                }
                                return true;
                            }
                            else
                                return false;
                        }
                        else
                            return false;
                    }

                default:
                    return other.TokenType == this.TokenType && this.Value == other.Value;
            }
        }

        static public bool DeepEquals(JToken left, JToken right)
        {
            return left != null && right != null && left.DeepEquals(right);
        }

        // if called on a non indexed object, return JNotPresent().  
        // On an Array/Object, will return JNotPresent if not present, or indexer is not right type
        public virtual JToken this[object key] { get { return null; } set { throw new NotImplementedException(); } }

        public virtual JToken Contains(string[] ids) { throw new NotImplementedException(); } // lookup one of these keys in a JObject

        public IEnumerator<JToken> GetEnumerator()
        {
            return GetSubClassTokenEnumerator();
        }

        public virtual IEnumerator<JToken> GetSubClassTokenEnumerator() { throw new NotImplementedException(); }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetSubClassEnumerator();
        }

        public virtual IEnumerator GetSubClassEnumerator() { throw new NotImplementedException(); }

        public virtual int Count { get { return 0; } }        // number of children
        public virtual void Clear() { throw new NotImplementedException(); }    // clear all children

        #endregion

    }
}



