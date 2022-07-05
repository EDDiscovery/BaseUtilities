/*
 * Copyright © 2018 EDDiscovery development team
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

namespace BaseUtils
{
    public enum IEvalParaListType { 
        Number,                 // a double
        NumberOrInteger,        // a double or an integer
        Integer,                // integer
        String,                 // string
        IntegerOrString,        // integer or string
        All                     // any
    };

    public interface IEval
    {
        Object Evaluate(bool unary, bool checkend);
        Object EvaluateDouble(bool unary, bool checkend);
        Object EvaluateLong(bool unary, bool checkend);
        List<Object> Parameters(string nameforerrorreport, int minparas, IEvalParaListType[] paratypes);       // gather parameters comma separ
        bool InError { get; }
        Object Value { get; }
        StringParser Parser { get; }
        bool IgnoreCase { get; }
        System.Globalization.CultureInfo Culture { get; }
        IEval Clone();
    }
}
