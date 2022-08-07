/*
 * Copyright © 2022-2022 EDDiscovery development team
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

namespace BaseUtils
{
    // brings together eval and variables
    public class EvalVariables
    {
        public Variables Values { get; set; }               // current var set to eval withj
        public HashSet<string>[] VarsInUse { get; set; }      // external use  - useful to hold to know what vars to extract in Variables.AddProperties..

        public EvalVariables(bool checkend = true, bool allowfp = true, bool allowstrings = true, bool allowmembers = true, bool allowarrays = true, 
                      bool installbasefunc = true, Variables var = null)
        {
            eval = new Eval(checkend, allowfp, allowstrings, allowmembers, allowarrays);

            if (var!=null)      // if passed a values set, set it
                Values = var;

            if ( installbasefunc )  // if asked for base functions, install
                eval.ReturnFunctionValue = BaseFunctionsForEval.BaseFunctions;       // allow functions

            eval.ReturnSymbolValue += (str) =>       // on symbol lookup
            {
                string qualname = Values.Qualify(str);

                if (Values.Exists(qualname))        //  if we have a variable
                {
                    string text = Values[qualname];
                    if (double.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double d))
                    {
                        if (long.TryParse(text, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out long v))    // if its a number, return number
                            return v;
                        else
                            return d;
                    }
                    else
                        return text;    // else its a string
                }
                else
                    return new StringParser.ConvertError("Unknown symbol " + qualname);
            };
        }

        public Object EvaluateQuickCheck(string s)
        {
            return eval.EvaluateQuickCheck(s);
        }
        public Object Evaluate(string s)
        {
            return eval.Evaluate(s);
        }

        public bool InError { get { return eval.InError; } }  // if in error
        public Object Value { get { return eval.Value; } }    // current value

        private Eval eval;
    }
}
