/*
 * Copyright © 2018-2023 EDDiscovery development team
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
    // user defined functions with a backup of BaseFunctions
    public class UserDefinedFunctions : IEvalFunctionHandler                              
    {
        public IEvalFunctionHandler BaseFunctions { get; set; } = null;

        private class FuncDef
        {
            public FuncDef(string n, string[] p , string f) { name = n;parameters = p;funcdef = f; }
            public string name;
            public string[] parameters;
            public string funcdef;
        }

        private Dictionary<string, FuncDef> functions;
        private StackOfDictionaries<string, Object> symbols;

        public UserDefinedFunctions(StackOfDictionaries<string, Object> slist)    
        {
            symbols = slist;
            functions = new Dictionary<string, FuncDef>();
        }

        public void Add(string name, string[] plist , string f )
        {
            functions[name] = new FuncDef(name, plist, f);
        }
        public object Execute(string name, IEval evaluator, bool noop)
        {
            Object ret = BaseFunctions != null ? BaseFunctions.Execute(name, evaluator, noop) : new StringParser.ConvertError("() not recognised");

            StringParser.ConvertError ce = ret as StringParser.ConvertError;

            if (ce != null && ce.ErrorValue.Contains("() not recognised"))
            {
                if ( functions.ContainsKey(name))
                {
                    FuncDef fd = functions[name];

                    bool hasparas = fd.parameters.Length > 0;

                    if (hasparas)
                    {
                        IEvalParaListType[] plist = new IEvalParaListType[fd.parameters.Length];
                        plist = Enumerable.Repeat(IEvalParaListType.All, fd.parameters.Length).ToArray();

                        List<Object> paras = evaluator.Parameters(name, fd.parameters.Length, plist);
                        if ( paras == null )            // if error, stop and return error
                            return evaluator.Value;

                        symbols.Push();
                        for (int i = 0; i < fd.parameters.Length; i++)          // add to symbol list on next level the parameter names and values
                        {
                            System.Diagnostics.Debug.WriteLine(symbols.Levels + " " + name + " Push " + fd.parameters[i] + "=" + paras[i]);
                            symbols.Add(fd.parameters[i], paras[i]);
                        }
                    }

                    Eval evfunc = (Eval)evaluator.Clone();                      // we need a fresh one just to evaluate this, but with the same configuration
                    Object res = evfunc.Evaluate(fd.funcdef);                   // and evaluate

                    if ( hasparas )
                        symbols.Pop();                                          // clear back stack..

                    return res;
                }
            }

            return ret;
        }

    }
}
