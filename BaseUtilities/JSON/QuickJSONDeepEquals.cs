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

namespace BaseUtils.JSON
{
    public partial class JToken 
    {
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
                            {
                                //System.Diagnostics.Debug.WriteLine("Array count diff");
                                return false;
                            }
                        }
                        else
                        {
                            //System.Diagnostics.Debug.WriteLine("Other array not an array");
                            return false;
                        }
                    }

                case TType.Object:
                    {
                        JObject us = (JObject)this;
                        if (other.TokenType == TType.Object)
                        {
                            JObject ot = (JObject)other;
                            if (ot.Count == us.Count)
                            {
                                //System.Diagnostics.Debug.WriteLine("Check {0} keys", us.Count);
                                foreach (var kvp in us)
                                {
                                    if (!ot.ContainsKey(kvp.Key))
                                    {
                                        //System.Diagnostics.Debug.WriteLine("Cannot find key {0}", kvp.Key);
                                        return false;
                                    }
                                    else
                                    {
                                        //System.Diagnostics.Debug.WriteLine("Key {0} : {1} vs {2}", kvp.Key, kvp.Value, ot[kvp.Key]);
                                        if (!kvp.Value.DeepEquals(ot[kvp.Key]))       // order unimportant to kvp)
                                            return false;
                                    }
                                }
                                //System.Diagnostics.Debug.WriteLine("All obj okay");
                                return true;
                            }
                            else
                            {
                                //System.Diagnostics.Debug.WriteLine("Key count different");
                                return false;
                            }
                        }
                        else
                        {
                            //System.Diagnostics.Debug.WriteLine("Other obj is not an object");
                            return false;
                        }
                    }

                default:
                    // logic here, is that if its read an integer, it always will pick the smallest rep (Long, ulong, Bigint) so they will be of the same type
                    // but if someone has done 30.0, and the other 30, one will be double, one will be an integer, so we need to check that if either is double, 
                    //      try and convert the other to double and compare. Then we are testing for value equality not type equality.
                    // and since we accept 1/0 as true/false, we need to do the same with booleans
                    // strings don't match with anything else.

                    if (other.TokenType == this.TokenType)
                    {
                        bool equals = this.Value.Equals(other.Value);
                        //System.Diagnostics.Debug.WriteLine("same type {0} vs {1} = {2}", this.Value, other.Value, equals);
                        return equals;
                    }
                    else if (this.TokenType == TType.Double || other.TokenType == TType.Double)          // either is double
                    {
                        double? usd = (double?)this;          // try and convert us to double
                        double? otherd = (double?)other;      // try and convert the other to double
                        if (usd != null && otherd != null)          // if we could, compare
                        {
                            bool equals = usd.Value == otherd.Value;
                            //System.Diagnostics.Debug.WriteLine("Convert to double {0} vs {1} = {2}", this.Value, other.Value, equals);
                            return equals;
                        }
                        else
                        {
                            //System.Diagnostics.Debug.WriteLine("{0} vs {1} Not the same token type", this.TokenType, other.TokenType);
                            return false;
                        }
                    }
                    else if (this.TokenType == TType.Boolean || other.TokenType == TType.Boolean)          // either is boolean
                    {
                        bool? usb = (bool?)this;              // try and convert us to bool
                        bool? otherb = (bool?)other;          // try and convert the other to bool
                        if (usb != null && otherb != null)          // if we could, compare
                        {
                            bool equals = usb.Value == otherb.Value;
                            //System.Diagnostics.Debug.WriteLine("Convert to bool {0} vs {1} = {2}", this.Value, other.Value, equals);
                            return equals;
                        }
                        else
                        {
                            //System.Diagnostics.Debug.WriteLine("{0} vs {1} Not the same token type", this.TokenType, other.TokenType);
                            return false;
                        }

                    }
                    else
                    {
                        //System.Diagnostics.Debug.WriteLine("{0} vs {1} Not the same token type", this.TokenType, other.TokenType);
                        return false;
                    }
            }
        }

        static public bool DeepEquals(JToken left, JToken right)
        {
            return left != null && right != null && left.DeepEquals(right);
        }

    }
}



