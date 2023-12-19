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
using System.Linq;

namespace BaseUtils
{
    public enum IEvalParaListType { 
        Double,                 // a double (integer is converted to double)
        DoubleOrLong,           // a double or an integer
        Long,                   // integer
        String,                 // string
        LongOrString,           // integer or string
        All,                    // any
        CollectAsString,        // collect as string don't evaluate
    };

    public interface IEval
    {
        // return StringParser.ConvertError, string, double, long
        Object Evaluate(string s);
        // return StringParser.ConvertError, string, double, long
        Object Evaluate(bool unary, bool checkend);
        bool TryEvaluateDouble(bool unary, bool checkend, out double value);
        bool TryEvaluateLong(bool unary, bool checkend, out long value);
        List<Object> Parameters(string nameforerrorreport, int minparas, IEvalParaListType[] paratypes);       
        void SymbolsFuncsInExpression(string expr, HashSet<string> symnames = null, HashSet<string> funcnames = null);
        void SymbolsFuncsInExpression(string expr, out HashSet<string> symnames, out HashSet<string> funcnames);
        bool InError { get; }
        Object Value { get; }
        StringParser Parser { get; }
        bool IgnoreCase { get; }
        System.Globalization.CultureInfo Culture { get; }
        IEval Clone();
        IEvalFunctionHandler ReturnFunctionValue { get; set; }         // if not null, handler for functions
        IEvalSymbolHandler ReturnSymbolValue { get; set; }             // if not null, handler for symbols
    }
    public interface IEvalFunctionHandler
    {
        Object Execute(string text, IEval eval, bool noop);     // noop means don't do anything persistent such as file ops
    }
    public interface IEvalSymbolHandler
    {
        object Get(string text);    // if not present, return ConvertError
        bool EvalSupportSet { get; }
        object Set(string text, Object e);
    }

}
