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

    public class Eval : IEval
    {
        public Eval(bool checkend = false, bool allowfp = false, bool allowstrings = false, bool allowmembers = false, bool allowarrays = false)
        {
            sp = null;
            CheckEnd = checkend;
            AllowFP = allowfp;
            AllowStrings = allowstrings;
            AllowMemberSymbol = allowmembers;
            AllowArrays = allowarrays;
        }

        // these allow you to set up symbol and func handlers with all options set true by default
        public Eval(IEvalSymbolHandler symh, bool checkend = true, bool allowfp = true, bool allowstrings = true, bool allowmembers = true, bool allowarrays = true) :
                        this(checkend, allowfp, allowstrings, allowmembers, allowarrays)
        {
            ReturnSymbolValue = symh;
        }
        public Eval(IEvalFunctionHandler funch, bool checkend = true, bool allowfp = true, bool allowstrings = true, bool allowmembers = true, bool allowarrays = true) :
                        this(checkend, allowfp, allowstrings, allowmembers, allowarrays)
        {
            ReturnFunctionValue = funch;
        }
        public Eval(IEvalSymbolHandler symh, IEvalFunctionHandler funch, bool checkend = true, bool allowfp = true, bool allowstrings = true, bool allowmembers = true, bool allowarrays = true) :
                        this(checkend, allowfp, allowstrings, allowmembers, allowarrays)
        {
            ReturnSymbolValue = symh;
            ReturnFunctionValue = funch;
        }

        // With a string
        public Eval(string s, bool checkend = false, bool allowfp = false, bool allowstrings = false, bool allowmembers = false, bool allowarrays = false) : this(checkend, allowfp, allowstrings, allowmembers, allowarrays)
        {
            sp = new StringParser(s);
        }

        // With a string parser
        public Eval(StringParser parse, bool checkend = false, bool allowfp = false, bool allowstrings = false, bool allowmembers = false, bool allowarrays = false) : this(checkend, allowfp, allowstrings, allowmembers, allowarrays)
        {
            sp = parse;
        }

        public IEval Clone()     // clone with options, but without parser..
        {
            return new Eval(CheckEnd, AllowFP, AllowStrings)
            {
                DefaultBase = this.DefaultBase,
                ReplaceEscape = this.ReplaceEscape,
                IgnoreCase = this.IgnoreCase,
                Culture = this.Culture,
                ReturnSymbolValue = this.ReturnSymbolValue,
                ReturnFunctionValue = this.ReturnFunctionValue
            };
        }

        public int DefaultBase { get; set; } = 10;              // default base value
        public bool CheckEnd { get; set; } = false;             // after expression, check string is at end
        public bool ReplaceEscape { get; set; } = false;        // in strings, expand escape
        public bool AllowFP { get; set; } = false;              // Allow floating point values
        public bool AllowStrings { get; set; } = false;         // Allow strings
        public bool UnaryEntry { get; set; } = false;           // enter at unary level, requires () to do other operators
        public bool IgnoreCase { get; set; } = false;           // ignore case on string checks
        public bool AllowMemberSymbol { get; set; } = false;    // allow Rings.member syntax on symbols
        public bool AllowArrays { get; set; } = false;     // allow Rings[n] syntax on symbols

        public System.Globalization.CultureInfo Culture { get; set; } = System.Globalization.CultureInfo.InvariantCulture;

        public StringParser Parser { get { return sp; } }       // get parser, can use after use to get rest of string
        public bool InError { get { return value is StringParser.ConvertError; } }  // if in error
        public Object Value { get { return value; } }           // current value
        public IEvalFunctionHandler ReturnFunctionValue { get; set; }         // if not null, handler for functions
        public IEvalSymbolHandler ReturnSymbolValue { get; set; }             // if not null, handler for symbols

        #region Public IF

        // return StringParser.ConvertError, string, double, long
        public Object Evaluate(string s)     
        {
            sp = new StringParser(s);
            return Evaluate(UnaryEntry, CheckEnd);
        }

        // return StringParser.ConvertError, string, double, long.  Quick check for double/long
        public Object EvaluateQuickCheck(string s)     
        {
            if (double.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double resd))
            {
                if (long.TryParse(s, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out long resl))
                {
                    value = resl;       // remember to update value in case its used, and to make sure any previous error is cancelled.
                    return resl;
                }
                else
                {
                    value = resd;
                    return resd;
                }
            }
            sp = new StringParser(s);
            return Evaluate(UnaryEntry, CheckEnd);
        }

        // return StringParser.ConvertError, string, double, long
        public Object Evaluate()     
        {
            Evaluate(UnaryEntry, CheckEnd);
            return value;
        }

        // return StringParser.ConvertError, string, double, long
        public Object Evaluate(string s, bool unary, bool checkend)     
        {
            sp = new StringParser(s);
            return Evaluate(unary, checkend);
        }

        // Allow control of unary entry and check end on a case by case basis
        public Object Evaluate(bool unary, bool checkend)          
        {
            System.Diagnostics.Debug.Assert(sp != null);

            unaryentrytimes = 0;

            if (unary)
                Level0Unary();
            else
                Level12Assign();

            if (!InError && checkend && !sp.IsEOL)
                value = new StringParser.ConvertError("Extra characters after expression: " + sp.LineLeft);

            //System.Diagnostics.Debug.WriteLine("Evaluate Value is " + value + " : of type " + value.GetType().Name);
            return value;
        }

        // Allow control of unary entry and check end on a case by case basis and return double or ConvertError
        public bool TryEvaluateDouble(bool unary, bool checkend, out double dvalue)          
        {
            Evaluate(unary, checkend);

            if (value is long)
                value = (double)(long)value;

            if ( value is double )
            {
                dvalue = (double)value;
                return true;
            }
            else
            {
                dvalue = 0;
                return false;
            }
        }

        // Allow control of unary entry and check end on a case by case basis and return Long or ConvertError
        public bool TryEvaluateLong(bool unary, bool checkend, out long lvalue)          // Allow control of unary entry and check end on a case by case basis
        {
            Evaluate(unary, checkend);

            if (value is long)
            {
                lvalue = (long)value;
                return true;
            }
            else
            {
                lvalue = 0;
                return false;
            }
        }

        // At the current string parse  point, get the next expression as a string.. do not evaluate it, dummy it. Leave position after expression (space removed)
        // used for eval like functionality in functions
        public string GetExpressionText()
        {
            sp.SkipSpace();
            int startpos = sp.Position;
            if (symbolNames != null && functionNames != null)   // we could be recursed into 
            {
                Evaluate(false, false);
            }
            else
            {
                symbolNames = new HashSet<string>();        // define these indicates collecting data only
                functionNames = new HashSet<string>();
                Evaluate(false, false);
                symbolNames = null;
                functionNames = null;
            }
            return sp.Line.Substring(startpos, sp.Position - startpos).Trim();
        }

        // for supporting functions..  given a list of types of parameters, evaluate or collect them, comma separ.
        public List<Object> Parameters(string nameforerrorreport, int minparas, IEvalParaListType[] paratypes)
        {
            List<Object> list = new List<object>();

            for (int n = 0; n < paratypes.Length; n++)
            {
                Object evres;
                bool ok = true;

                if (paratypes[n] == IEvalParaListType.CollectAsString)
                {
                    evres = GetExpressionText();
                }
                else
                {
                    evres = Evaluate(false, false);
                    ok = (paratypes[n] == IEvalParaListType.String && (evres is string)) ||
                        (paratypes[n] == IEvalParaListType.Double && (evres is double || evres is long)) ||
                        (paratypes[n] == IEvalParaListType.DoubleOrLong && (evres is double || evres is long)) ||
                        (paratypes[n] == IEvalParaListType.Long && (evres is long)) ||
                        (paratypes[n] == IEvalParaListType.LongOrString && (evres is string || evres is long)) ||
                        (paratypes[n] == IEvalParaListType.All);

                    if (ok && paratypes[n] is IEvalParaListType.Double && evres is long)
                        evres = (double)(long)evres;
                }

                if (InError)
                    return null;

                if (ok)
                {
                    list.Add(evres);

                    if (n < paratypes.Length - 1)    // if not maximum point
                    {
                        if (!sp.IsCharMoveOn(','))  // and not comma..
                        {
                            if (list.Count() < minparas) // ensure minimum
                            {
                                value = new StringParser.ConvertError(nameforerrorreport + "() requires " + minparas + " parameters minimum");
                                return null;
                            }
                            else
                                return list;        // have min, return it
                        }
                    }
                }
                else
                {
                    value = new StringParser.ConvertError(nameforerrorreport + "() type mismatch in parameter " + (n + 1) + " wanted " + paratypes[n].ToString().SplitCapsWord());
                    return null;
                }
            }

            if (sp.IsChar(','))     // if at max, can't have another
            {
                value = new StringParser.ConvertError(nameforerrorreport + "() takes " + paratypes.Length + " parameters maximum");
                return null;
            }

            return list;
        }

        public string ToString(System.Globalization.CultureInfo ci)
        {
            if (InError)
                return ((StringParser.ConvertError)value).ErrorValue;
            else if (value is double)
                return ((double)value).ToString(ci);
            else if (value is long)
                return ((long)value).ToString(ci);
            else
                return (string)value;
        }

        public bool ToSafeString(string fmt, out string ret)
        {
            if (InError)
            {
                ret = ((StringParser.ConvertError)value).ErrorValue;
                return false;
            }
            else if (value is double)
            {
                return ((double)value).ToStringExtendedSafe(fmt, out ret);
            }
            else if (value is long)
                return ((long)value).ToStringExtendedSafe(fmt, out ret);
            else
            {
                ret = (string)value;
                return true;
            }
        }

        #endregion

        #region Vars Funcs

        // need to have a function handler attatched to get funcnames. Both may be null if you don't want collection of one or the other
        // do not use 
        public void SymbolsFuncsInExpression(string expr, HashSet<string> symnames = null, HashSet<string> funcnames = null)
        {
            if (symbolNames != null && functionNames != null)
            {
                Evaluate(expr);         // recursion
            }
            else
            {
                symbolNames = symnames;     // point to hash set
                functionNames = funcnames;
                Evaluate(expr);
                symbolNames = null;
                functionNames = null;
            }
        }

        // symnames/funcsnames are cleared
        public void SymbolsFuncsInExpression(string expr, out HashSet<string> symnames, out HashSet<string> funcnames)
        {
            symnames = new HashSet<string>();
            funcnames = new HashSet<string>();
            SymbolsFuncsInExpression(expr, symnames, funcnames);
        }

        #endregion

        #region Static Helpers

        // return StringParser.ConvertError, string, double, long
        static public Object Expr(string s, bool checkend = true, bool allowfp = true, bool allowstrings = true)
        {
            Eval v = new Eval(s);
            v.CheckEnd = checkend;
            v.AllowFP = allowfp;
            v.AllowStrings = allowstrings;
            return v.Evaluate();
        }

        #endregion

        #region Level Evaluators

        private void Level0Unary()      // Value left as Error, double, long, string
        {
            unaryentrytimes++;          // counts entry to the unary level.. if 1, and symbolname set, its a symbol def on left side 
            lvaluename = null;

            long sign = 0;

            if (sp.IsCharMoveOn('-'))   // space allowed after unary minus plus to entry
                sign = -1;
            else if (sp.IsCharMoveOn('+'))
                sign = 1;

            if (sp.IsCharMoveOn('('))       // ( value )
            {
                value = Evaluate(false, false);

                if (!(value is StringParser.ConvertError) && !sp.IsCharMoveOn(')'))
                {
                    value = new StringParser.ConvertError("Missing ) at end of expression");
                }
            }
            else
            {
                value = sp.ConvertNumberStringSymbolChar(DefaultBase, AllowFP, AllowStrings, ReplaceEscape, AllowMemberSymbol);

                if (value is StringParser.ConvertSymbol)    // symbol must resolve to a value or Error
                {
                    string symname = (value as StringParser.ConvertSymbol).SymbolValue;

                    if (sp.IsCharMoveOn('('))
                    {
                        if (ReturnFunctionValue != null)
                        {
                            functionNames?.Add(symname);
                            value = ReturnFunctionValue.Execute(symname, this, functionNames!=null);        // indicate no-op if collecting func names

                            if (!(value is StringParser.ConvertError) && !sp.IsCharMoveOn(')'))         // must be ) terminated
                                value = new StringParser.ConvertError("Function not terminated with )");
                        }
                        else
                            value = new StringParser.ConvertError("Functions not supported");
                    }
                    else if (ReturnSymbolValue == null && symbolNames == null)
                    {
                        value = new StringParser.ConvertError("Symbols not supported");
                    }
                    else
                    {
                        while (AllowArrays && sp.IsCharMoveOn('['))            // is it an array symbol..
                        {
                            value = Evaluate(false, false);     // get [] expression

                            if (value is StringParser.ConvertError)     // see what we have back and generate array
                                break;
                            if (value is long)
                                symname += $"[{((long)value).ToStringInvariant()}]";
                            else if (value is double)
                            {
                                value = new StringParser.ConvertError("Cannot use floating point value as array index");
                                break;
                            }
                            else
                                symname += $"[{((string)value).AlwaysQuoteString()}]";

                            if (!sp.IsCharMoveOn(']', false))         // must be ] terminated
                            {
                                value = new StringParser.ConvertError("Array not terminated with ]");
                                break;
                            }

                            if (!sp.IsLetterDigitUnderscoreMember())        // if no following chars, we are done 
                                break;

                            string moresym;         // more symbol text beyond it

                            if (AllowMemberSymbol)
                                moresym = sp.NextWord((c) => { return char.IsLetterOrDigit(c) || c == '_' || c == '.'; });
                            else
                                moresym = sp.NextWord((c) => { return char.IsLetterOrDigit(c) || c == '_'; });

                            symname += moresym;     // add on
                        }

                        if (!(value is StringParser.ConvertError))      // if not in error, see if symbols is there
                        {
                            lvaluename = (sign == 0) ? symname : null;              // pass back symbol name found, only if not signed.

                            if (symbolNames != null)
                                symbolNames.Add(symname);
                            else
                                value = ReturnSymbolValue.Get(symname);             // could be Error
                        }
                    }
                }
            }

            // value now Error, double, long, string

            if (!(value is StringParser.ConvertError) && sign != 0)      // if not error, and signed
            {
                if (value is double)
                    value = (double)value * sign;
                else if (value is long)
                    value = (long)value * sign;
                else
                    value = new StringParser.ConvertError("Unary +/- not allowed in front of string");
            }

            if (symbolNames!=null || functionNames != null)       // if we are collecting names, not executing, remove any error and just return 1
                value = 1L;
        }

        private void Level1Not()
        {
            if (sp.IsCharMoveOn('!'))
            {
                Level1Not();    // allow recurse..

                lvaluename = null;      // clear symbol name, can't use with !

                if (IsLong)
                    value = ((long)value != 0) ? 0L : 1L;
                else if (!InError)
                    value = new StringParser.ConvertError("! only valid with integers");
            }
            else if (sp.IsCharMoveOn('~'))
            {
                Level1Not();    // allow recurse..

                lvaluename = null;      // clear symbol name, can't use with ~

                if (IsLong)
                    value = ~(long)value;
                else if (!InError)
                    value = new StringParser.ConvertError("~ is only valid with integers");
            }
            else
                Level0Unary();
        }

        private void Level2Times()
        {
            Level1Not();        // get left value

            while (!InError && sp.IsCharOneOf("*/%"))
            {
                if (!IsNumeric)
                {
                    value = new StringParser.ConvertError("*/% only valid with numbers");
                    return;
                }

                char operation = sp.GetChar(skipspace: true);

                Object leftside = value;            // remember left side value

                Level1Not();                        // get right side

                if (InError)
                    return;
                else if (!IsNumeric)
                {
                    value = new StringParser.ConvertError("*/% only valid with numbers");
                    return;
                }

                if (leftside is double || value is double)    // either double, its double
                {
                    double left = AsDouble(leftside);
                    double right = AsDouble(value);

                    if (operation == '*')
                        value = left * right;
                    else if (operation == '/')
                    {
                        if (right == 0)
                        {
                            value = new StringParser.ConvertError("Divide by zero");
                            return;
                        }

                        value = left / right;
                    }
                    else
                    {
                        value = new StringParser.ConvertError("Cannot perform modulo with floating point values");
                        return;
                    }
                }
                else
                {
                    long left = (long)(leftside);
                    long right = (long)(value);

                    if (operation == '*')
                        value = left * right;
                    else if (operation == '/')
                    {
                        if (right == 0)
                        {
                            value = new StringParser.ConvertError("Divide by zero");
                            return;
                        }

                        value = left / right;
                    }
                    else
                    {
                        value = left % right;
                    }
                }
            }
        }

        private void Level3Add()
        {
            Level2Times();        // get left value

            while (!InError && sp.IsCharOneOf("+-"))
            {
                char operation = sp.GetChar(skipspace: true);

                if (IsString && operation == '-')
                {
                    value = new StringParser.ConvertError("- is not supported with strings");
                    return;
                }

                Object leftside = value;            // remember left side value

                Level2Times();                      // get right side

                if (InError)
                    return;

                if (leftside is string && IsString) // two strings, +
                {
                    value = (leftside as string) + (value as string);
                }
                else
                {
                    if (leftside is string || IsString)
                    {
                        value = new StringParser.ConvertError("Cannot mix string and number types");
                        return;
                    }
                    else if (leftside is double || value is double)    // either double, its double
                    {
                        double left = AsDouble(leftside);
                        double right = AsDouble(value);

                        if (operation == '+')
                            value = left + right;
                        else
                            value = left - right;
                    }
                    else
                    {
                        long left = (long)(leftside);
                        long right = (long)(value);

                        if (operation == '+')
                            value = left + right;
                        else
                            value = left - right;
                    }
                }
            }
        }

        private void Level4Shift()
        {
            Level3Add();        // get left value

            bool leftshift = false;

            while (!InError && ((leftshift = sp.IsString("<<")) || sp.IsString(">>")))
            {
                sp.MoveOn(2);

                if (CheckLong("<< and >>"))
                    return;

                Object leftside = value;            // remember left side value

                Level3Add();        // get right side

                if (InError || CheckLong("<< and >>", true))
                    return;

                if (leftshift)
                    value = (long)leftside << (int)(long)value;
                else
                    value = (long)leftside >> (int)(long)value;
            }
        }

        private void Level5GreaterLess()
        {
            Level4Shift();        // get left value

            while (!InError && sp.IsCharOneOf("<>"))
            {
                bool left = sp.GetChar() == '<';
                bool equals = sp.IsCharMoveOn('=');
                sp.SkipSpace();

                Object leftside = value;            // remember left side value

                Level4Shift();        // get right side

                if (InError)
                    return;

                bool result;

                if (leftside is string || value is string)
                {
                    if (!(leftside is string) || !(value is string))
                    {
                        value = new StringParser.ConvertError("Cannot mix string and number types in comparisions");
                        return;
                    }
                    else
                    {
                        int cmp = String.Compare(leftside as string, value as string, IgnoreCase, Culture);
                        if (left)
                            result = equals ? (cmp <= 0) : (cmp < 0);
                        else
                            result = equals ? (cmp >= 0) : (cmp > 0);
                    }
                }
                else if (leftside is double || value is double)
                {
                    double l = AsDouble(leftside);
                    double r = AsDouble(value);

                    if (left)
                        result = equals ? (l <= r) : (l < r);
                    else
                        result = equals ? (l >= r) : (l > r);
                }
                else
                {
                    long l = (long)(leftside);
                    long r = (long)(value);

                    if (left)
                        result = equals ? (l <= r) : (l < r);
                    else
                        result = equals ? (l >= r) : (l > r);
                }

                value = result ? (long)1 : (long)0;
            }
        }

        private void Level6Equals()
        {
            Level5GreaterLess();        // get left value

            bool equals = false;

            while (!InError && ((equals = sp.IsString("==")) || sp.IsString("!=")))
            {
                sp.MoveOn(2);

                Object leftside = value;            // remember left side value

                Level5GreaterLess();        // get left value

                if (InError)
                    return;

                bool result;

                if (leftside is string || value is string)
                {
                    if (!(leftside is string) || !(value is string))
                    {
                        value = new StringParser.ConvertError("Cannot mix string and number types in comparisions");
                        return;
                    }
                    else
                    {
                        int cmp = String.Compare(leftside as string, value as string, IgnoreCase, Culture);
                        result = equals ? (cmp == 0) : (cmp != 0);
                    }
                }
                else if (leftside is double || value is double)
                {
                    double l = AsDouble(leftside);
                    double r = AsDouble(value);
                    result = equals ? (l == r) : (l != r);
                }
                else
                {
                    long l = (long)(leftside);
                    long r = (long)(value);
                    result = equals ? (l == r) : (l != r);
                }

                value = result ? (long)1 : (long)0;
            }
        }

        private void Level7BinaryAnd()
        {
            Level6Equals();        // get left value

            while (!InError && (sp.IsChar('&') && !sp.IsNextChar('&')))     // & not &&
            {
                sp.MoveOn(1);

                if (CheckLong("&"))
                    return;

                Object leftside = value;            // remember left side value

                Level6Equals();        // get left value

                if (InError || CheckLong("&", true))
                    return;

                value = (long)((long)leftside & (long)value);
            }
        }

        private void Level8BinaryEor()
        {
            Level7BinaryAnd();        // get left value

            while (!InError && sp.IsChar('^'))     // ^
            {
                sp.MoveOn(1);

                if (CheckLong("^"))
                    return;

                Object leftside = value;            // remember left side value

                Level7BinaryAnd();        // get left value

                if (InError || CheckLong("^", true))
                    return;

                value = (long)((long)leftside ^ (long)value);
            }
        }

        private void Level9BinaryOr()
        {
            Level8BinaryEor();        // get left value

            while (!InError && (sp.IsChar('|') && !sp.IsNextChar('|')))     // | not ||
            {
                sp.MoveOn(1);

                if (CheckLong("|"))
                    return;

                Object leftside = value;            // remember left side value

                Level8BinaryEor();        // get left value

                if (InError || CheckLong("|", true))
                    return;

                value = (long)((long)leftside | (long)value);
            }
        }

        private void Level10And()
        {
            Level9BinaryOr();        // get left value

            while (!InError && sp.IsStringMoveOn("&&"))
            {
                if (CheckLong("&&"))
                    return;

                Object leftside = value;            // remember left side value

                Level9BinaryOr();        // get left value

                if (InError || CheckLong("&&", true))
                    return;

                bool l = (long)leftside != 0;
                bool r = (long)value != 0;
                value = (l && r) ? (long)1 : (long)0;
            }
        }

        private void Level11Or()
        {
            Level10And();        // get left value

            while (!InError && sp.IsStringMoveOn("||"))
            {
                if (CheckLong("||"))
                    return;

                Object leftside = value;            // remember left side value

                Level10And();        // get left value

                if (InError || CheckLong("||", true))
                    return;

                bool l = (long)leftside != 0;
                bool r = (long)value != 0;
                value = (l || r) ? (long)1 : (long)0;
            }
        }

        private void Level12Assign()
        {
            Level11Or();        // get left value

            if (sp.IsCharMoveOn('='))
            {
                if (ReturnSymbolValue?.EvalSupportSet ?? false == true)     // if the return sysbol system supports set
                {
                    StringParser.ConvertError err = value as StringParser.ConvertError;

                    string symbolname;

                    if (unaryentrytimes == 1 && lvaluename != null)    // if 1 entry to unary, and symbol value is set, its a current value
                        symbolname = lvaluename;
                    else
                    {                                                   // else its an equal without an error but not a symbol, so its not an lvalue
                        value = new StringParser.ConvertError("Lvalue required for = operator");
                        return;
                    }

                    Evaluate(false, false);                             // get next expression

                    if (!InError)
                        value = ReturnSymbolValue.Set(symbolname, value);
                }
                else
                {
                    value = new StringParser.ConvertError("= operator not supported");
                }
            }
        }

        #endregion


        #region Helpers and privates

        private StringParser sp;        // string parser
        private string lvaluename;      // symbol saw at unary level..
        private int unaryentrytimes;        // no of times at unary
        private Object value = null;        // this can be Error class, double, long, string
        private HashSet<string> symbolNames;    // when set, collecting symbol names
        private HashSet<string> functionNames;      // when set, collecting func names and nop

        private bool IsNumeric { get { return value is double || value is long; } }
        private bool IsLong { get { return value is long; } }
        private bool IsDouble { get { return value is double; } }
        private bool IsString { get { return value is string; } }

        private double AsDouble(Object v) { if (v is double) return (double)v; else return (double)(long)v; } // must be numeric before call

        private bool CheckLong(string op, bool right = false)
        {
            if (!IsLong)
            {
                value = new StringParser.ConvertError(op + " requires integer on " + ((right) ? "right" : "left") + " side");
                return true;
            }
            else
                return false;
        }

        #endregion
    }
}
