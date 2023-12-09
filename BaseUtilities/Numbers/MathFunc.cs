/*
 * Copyright 2023-2023 EDDiscovery development team
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
using System.Reflection;

namespace BaseUtils
{
    public static class MathFunc
    {
        // use a Maths function from the math class of type name on an object of type long or double
        static public Object Math(string name, Type mathclass, Object value)        // runs a function name on a class
        {
            if (value is StringParser.ConvertError)
                return value;
            else if (value is string)
                return new StringParser.ConvertError(name + "() supports numbers only");
            else
            {
                MemberInfo[] memberlist = mathclass.GetMember(name);

                if (value is long)
                {
                    var longmethod = Array.Find(memberlist, (z) => ((MethodInfo)z).ReturnType == typeof(long) && ((MethodInfo)z).GetParameters().Count() == 1);     // find the long version

                    if (longmethod != null)
                        return ((MethodInfo)longmethod).Invoke(null, new Object[] { value });
                    else
                        value = (double)(long)value;        // else convert result to double for next try
                }

                var doublemethod = Array.Find(memberlist, (z) => ((MethodInfo)z).ReturnType == typeof(double) && ((MethodInfo)z).GetParameters().Count() == 1);     // find the double version..

                if (doublemethod != null)
                    return ((MethodInfo)doublemethod).Invoke(null, new Object[] { value });
                else
                    return new StringParser.ConvertError(name + "() Internal coding error - no supporting function");
            }
        }

        public static double? AsDouble(Object d)
        {
            if (d is double)
                return (double)d;
            else if (d is long)
                return (double)(long)d;
            else
                return null;
        }

        public static double[] ToDouble(List<Object> list)
        {
            double[] array = new double[list.Count];
            for (int n = 0; n < list.Count; n++)
                array[n] = (list[n] is long) ? (double)(long)list[n] : (double)list[n];
            return array;
        }
    }

}

