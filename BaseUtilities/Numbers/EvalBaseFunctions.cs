/*
 * Copyright 2018-2023 EDDiscovery development team
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

namespace BaseUtils
{
    public class BaseFunctionsForEval : IEvalFunctionHandler
    {

        public virtual object Execute(string name, IEval evaluator, bool noop)
        {
            string[] maths = { "Abs", "Acos" , "Asin", "Atan", "Ceiling", "Cos", "Cosh" ,
                              "Exp","Floor", "Log", "Log10" , "Sin", "Sinh", "Sqrt", "Tan" , "Tanh" , "Truncate" };

            int mathsindex = Array.IndexOf(maths, name);

            if (mathsindex >= 0)
            {
                List<Object> list = evaluator.Parameters(name, 1, new IEvalParaListType[] { IEvalParaListType.NumberOrInteger });

                if (list != null)
                    return BaseUtils.MathFunc.Math(name, typeof(Math), list[0]);
                else
                    return evaluator.Value;
            }

            string[] chars = { "IsControl", "IsDigit","IsLetter","IsLower","IsNumber","IsPunctuation","IsSeparator","IsSurrogate","IsSymbol","IsUpper","IsWhiteSpace",
                                "ToLower","ToUpper" };
            int charsindex = Array.IndexOf(chars, name);

            if (charsindex >= 0)
            {
                List<Object> list = evaluator.Parameters(name, 1, new IEvalParaListType[] { (name == "ToLower" || name == "ToUpper") ? IEvalParaListType.IntegerOrString : IEvalParaListType.Integer });

                if (list != null)
                {
                    if (name == "ToLower" && list[0] is string)
                    {
                        return ((string)list[0]).ToLower(evaluator.Culture);
                    }
                    else if (name == "ToUpper" && list[0] is string)
                    {
                        return ((string)list[0]).ToUpper(evaluator.Culture);
                    }
                    else
                    {
                        var methodarray = typeof(Char).GetMember(name);      // get members given name
                        var method = methodarray.FindMember(new Type[] { list[0] is long ? typeof(char) : typeof(string) }); // find all members which have a single para of this type

                        if (method.ReturnType == typeof(bool))
                        {
                            bool value = (bool)method.Invoke(null, new Object[] { (char)(long)list[0] });
                            return value ? (long)1 : (long)0;
                        }
                        else
                        {
                            char ch = (char)method.Invoke(null, new Object[] { (char)(long)list[0] });
                            return (long)ch;
                        }
                    }
                }
                else
                    return evaluator.Value;
            }

            if (name == "Max" || name == "Min")
            {
                List<Object> list = evaluator.Parameters(name, 2, new IEvalParaListType[] { IEvalParaListType.NumberOrInteger, IEvalParaListType.NumberOrInteger });

                if (list != null)
                {
                    if (list[0] is long && list[1] is long)
                        return name == "Max" ? Math.Max((long)list[0], (long)list[1]) : Math.Min((long)list[0], (long)list[1]);
                    else
                    {
                        double[] array = MathFunc.ToDouble(list);
                        return name == "Max" ? Math.Max(array[0], array[1]) : Math.Min(array[0], array[1]);
                    }
                }
            }
            else if (name == "Pow")
            {
                List<Object> list = evaluator.Parameters(name, 2, new IEvalParaListType[] { IEvalParaListType.Number, IEvalParaListType.Number });

                if (list != null)
                    return Math.Pow((double)list[0], (double)list[1]);
            }
            else if (name == "Round")
            {
                List<Object> list = evaluator.Parameters(name, 1, new IEvalParaListType[] { IEvalParaListType.Number, IEvalParaListType.Integer });

                if (list != null)
                    return (list.Count == 1) ? Math.Round((double)list[0]) : Math.Round((double)list[0], (int)(long)list[1]);
            }
            else if (name == "Sign")
            {
                List<Object> list = evaluator.Parameters(name, 1, new IEvalParaListType[] { IEvalParaListType.NumberOrInteger });

                if (list != null)
                    return (long)(list[0] is long ? Math.Sign((long)list[0]) : Math.Sign((double)list[0]));
            }
            else if (name == "Fp" || name == "double" || name == "float")
            {
                List<Object> list = evaluator.Parameters(name, 1, new IEvalParaListType[] { IEvalParaListType.Number });        // gather a single number 

                if (list != null)
                    return list[0];
            }
            else if (name == "Eval")
            {
                List<Object> list = evaluator.Parameters(name, 1, new IEvalParaListType[] { IEvalParaListType.String });

                if (list != null)
                {
                    Eval ev = new Eval(true, true, true);
                    return ev.Evaluate((string)list[0]);
                }
            }
            else if (name == "ToString")
            {
                List<Object> list = evaluator.Parameters(name, 2, new IEvalParaListType[] { IEvalParaListType.NumberOrInteger, IEvalParaListType.String });

                if (list != null)
                {
                    string output;

                    bool ok = (list[0] is double) ? ((double)list[0]).SafeToString(list[1] as string, out output) : ((long)list[0]).SafeToString(list[1] as string, out output);

                    if (ok)
                        return output;
                    else
                        return new StringParser.ConvertError(name + "() Format is incorrect");
                }
            }
            else if (name == "Unicode")
            {
                List<Object> list = evaluator.Parameters(name, 1, new IEvalParaListType[] { IEvalParaListType.Integer });

                if (list != null)
                    return (string)char.ToString((char)(long)list[0]);
            }
            else if (name == "Compare")
            {
                List<Object> list = evaluator.Parameters(name, 2, new IEvalParaListType[] { IEvalParaListType.All, IEvalParaListType.All });

                if (list != null && list.Count == 2 && (((list[0] is long || list[0] is double) && (list[1] is long || list[1] is double)) || ((list[0] is string) && (list[1] is string))))
                {
                    if ( list[0] is double || list[1] is double)        // either is double
                    {
                        double left = (list[0] is double) ? (double)list[0] : (double)(long)list[0];        // need to go thru long before double
                        double right = (list[1] is double) ? (double)list[1] : (double)(long)list[1];
                        return (long)(left.CompareTo(right));   
                    }
                    else if ( list[0] is long)
                    {
                        long left = (long)list[0];
                        long right = (long)list[1];
                        return (long)(left.CompareTo(right));
                    }
                    else
                    {
                        string left = (string)list[0];
                        string right = (string)list[1];
                        return (long)(left.CompareTo(right));
                    }
                }
                else
                    return new StringParser.ConvertError(name + "() requires two parameters of same basic type (number or string)");
            }
            else
                return new StringParser.ConvertError(name + "() not recognised");

            return evaluator.Value;
        }

    }
}
