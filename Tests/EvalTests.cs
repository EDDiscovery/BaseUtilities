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
using BaseUtils;
using NFluent;
using NUnit.Framework;
using System;
using System.Collections.Generic;


namespace EDDiscoveryTests
{
    [TestFixture(TestOf = typeof(Eval))]
    public class EvalTests
    {
        [Test]
        public void EvalTestsFunc()
        {


            {       // new June 22 array syntaxer

                Dictionary<string, Object> symbols = new Dictionary<string, object>();

                Eval ev = new Eval(true, true, true, true, true);

                ev.ReturnSymbolValue = (s) =>
                {
                    System.Diagnostics.Debug.WriteLine($"Symbol {s}");
                    if (symbols.ContainsKey(s))
                        return symbols[s];
                    else
                        return new StringParser.ConvertError("No such symbol '" + s + "'");
                };

                symbols.Add("Level[0].Rings", 20L);
                symbols.Add("Level[1].Rings", 100L);
                symbols.Add("Value[1]", 1L);
                symbols.Add("index", 1L);

                Object ret = null;

                ret = ev.Evaluate("Level[index].Rings*2");
                Check.That(ret).IsNotNull();
                Check.That(ret).Equals(200);

                ret = ev.Evaluate("Level[0].Rings*2");
                Check.That(ret).IsNotNull();
                Check.That(ret).Equals(40);

                ret = ev.Evaluate("Level[1].Rings*2");
                Check.That(ret).IsNotNull();
                Check.That(ret).Equals(200);

                
                ret = ev.Evaluate("Level[index-1].Rings*2");
                Check.That(ret).IsNotNull();
                Check.That(ret).Equals(40);

                ret = ev.Evaluate("Level[Value[1]].Rings*2");
                Check.That(ret).IsNotNull();
                Check.That(ret).Equals(200);

            }

            { 
                Eval ev = new Eval(true, true, true, true, true);           // test Fakes
                ev.Fake = true;
                ev.ReturnSymbolValue += (s) => {
                    System.Diagnostics.Debug.WriteLine($"Fake Symbol {s}");
                    return 1L; };
                var ret = ev.Evaluate("Level[0].Rings*2/fred+jim");
                Check.That(ret).Equals(2);
            }

            {
                Check.That(StringParser.ConvertNumber("0x") == null).IsTrue();

                Object t2 = StringParser.ConvertNumber("0x1a");
                Check.That(t2 != null && (long)t2 == 0x1a).IsTrue();
                Check.That((long)StringParser.ConvertNumber("0xFa1") == 0xfa1).IsTrue();
                Check.That((long)StringParser.ConvertNumber("%0101") == 5).IsTrue();
                Check.That((long)StringParser.ConvertNumber("%01011") == 11).IsTrue();
                Check.That((long)StringParser.ConvertNumber("`1091") == 1091).IsTrue();
                Check.That((long)StringParser.ConvertNumber("012") == 8 * 1 + 2).IsTrue();

                StringParser sp2 = new StringParser("10U20UL30 40LU 50");
                Check.That((long)sp2.ConvertNumber() == 10).IsTrue();
                Check.That((long)sp2.ConvertNumber() == 20).IsTrue();
                Check.That((long)sp2.ConvertNumber() == 30).IsTrue();
                Check.That((long)sp2.ConvertNumber() == 40).IsTrue();
                Check.That((long)sp2.ConvertNumber() == 50).IsTrue();
                Check.That(sp2.IsEOL == true).IsTrue();

                Check.That((double)StringParser.ConvertNumber("1.1", allowfp: true) == 1.1).IsTrue();
                Check.That((double)StringParser.ConvertNumber("1.932", allowfp: true) == 1.932).IsTrue();
                Check.That((double)StringParser.ConvertNumber("1.292A", allowfp: true) == 1.292).IsTrue();
                Check.That((double)StringParser.ConvertNumber("123.456", allowfp: true) == 123.456).IsTrue();
                Check.That((double)StringParser.ConvertNumber("0.456", allowfp: true) == 0.456).IsTrue();
                Check.That((double)StringParser.ConvertNumber(".456", allowfp: true) == 0.456).IsTrue();
                Check.That((double)StringParser.ConvertNumber(".456E2", allowfp: true) == 45.6).IsTrue();
                Check.That(StringParser.ConvertNumber(".456E", allowfp: true) == null).IsTrue();
                Check.That(StringParser.ConvertNumber(".E23", allowfp: true) == null).IsTrue();
                Check.That((double)StringParser.ConvertNumber("1e20", allowfp: true) == 1e20).IsTrue();
                Check.That((double)StringParser.ConvertNumber("1E20", allowfp: true) == 1e20).IsTrue();
                Check.That((double)StringParser.ConvertNumber("1E-20", allowfp: true) == 1e-20).IsTrue();
                Check.That((double)StringParser.ConvertNumber("1E-20.2", allowfp: true) == 1e-20).IsTrue();



                Check.That((string)StringParser.ConvertNumberStringSymbolChar("\"Hello there\"", allowstrings: true) == "Hello there").IsTrue();
                Check.That(((StringParser.ConvertSymbol)StringParser.ConvertNumberStringSymbolChar("_fred", allowstrings: true)).SymbolValue == "_fred").IsTrue();
                Check.That(((StringParser.ConvertSymbol)StringParser.ConvertNumberStringSymbolChar("Fr_ed", allowstrings: true)).SymbolValue == "Fr_ed").IsTrue();

                Check.That((long)StringParser.ConvertNumberStringSymbolChar("1234") == 1234).IsTrue();


                Object t1 = StringParser.ConvertNumberStringSymbolChar("'a'");
                Check.That(t1 != null && (long)t1 == 97).IsTrue();
                Check.That(((StringParser.ConvertError)StringParser.ConvertNumberStringSymbolChar("'a")).ErrorValue == "Incorrectly formatted 'c' expression").IsTrue();
                Check.That(((StringParser.ConvertError)StringParser.ConvertNumberStringSymbolChar("'")).ErrorValue == "Incorrectly formatted 'c' expression").IsTrue();


                // 0
                Check.That((long)Eval.Expr("-1234") == -1234).IsTrue();
                Check.That((double)Eval.Expr("-1234.23", allowfp: true) == -1234.23).IsTrue();
                Check.That((long)Eval.Expr("10") == 10).IsTrue();
                Check.That((long)Eval.Expr("-10") == -10).IsTrue();
                Check.That((double)Eval.Expr("-10.23") == -10.23).IsTrue();

                // 1
                Check.That((long)Eval.Expr("!10") == 0).IsTrue();
                Check.That((long)Eval.Expr("!0") == 1).IsTrue();
                Check.That((long)Eval.Expr("~10") == ~10).IsTrue();
                Check.That((long)Eval.Expr("~0") == ~0).IsTrue();

                //2
                Check.That((long)Eval.Expr(" 10 * 20") == 200).IsTrue();
                Check.That((long)Eval.Expr(" -10 * -20") == 200).IsTrue();
                Check.That((long)Eval.Expr(" 10 * -20") == -200).IsTrue();
                Check.That((long)Eval.Expr(" 10 * -20") == -200).IsTrue();
                Check.That((long)Eval.Expr(" 10 * -20 * 50") == -200 * 50).IsTrue();
                Check.That((long)Eval.Expr(" 10/20") == 0).IsTrue();
                Check.That((double)Eval.Expr(" 10.0/20") == 0.5).IsTrue();
                Check.That((double)Eval.Expr(" -10.0 / 20") == -0.5).IsTrue();
                Check.That((double)Eval.Expr(" -10.0 / 0.01") == -10 / 0.01).IsTrue();
                Check.That((long)Eval.Expr(" 10 % 20") == 10 % 20).IsTrue();
                Check.That((long)Eval.Expr(" 50 % 20") == 50 % 20).IsTrue();

                //3
                Check.That((long)Eval.Expr(" 10 + 20") == 10 + 20).IsTrue();
                Check.That((long)Eval.Expr(" 10 - 20 - 30") == 10 - 20 - 30).IsTrue();
                Check.That((long)Eval.Expr(" 10 - -20") == 30).IsTrue();
                Check.That((double)Eval.Expr(" 10.2 + 20") == 30.2).IsTrue();
                Check.That((double)Eval.Expr(" 10.2 - 10.2") == 0).IsTrue();
                Check.That((double)Eval.Expr(" 5 - 50.5") == 5 - 50.5).IsTrue();
                Check.That((string)Eval.Expr(" \"Hello\" + \"Goodbye\" ") == "HelloGoodbye").IsTrue();
                Check.That(((StringParser.ConvertError)Eval.Expr(" \"Hello\" - \"Goodbye\" ")).ErrorValue.Equals("- is not supported with strings")).IsTrue();
                Check.That(((StringParser.ConvertError)Eval.Expr(" 20 + \"Goodbye\" ")).ErrorValue.Equals("Cannot mix string and number types")).IsTrue();

                //4
                Check.That((long)Eval.Expr(" 10 << 2") == 10 << 2).IsTrue();
                Check.That((long)Eval.Expr(" 1000 >> 2") == 1000 >> 2).IsTrue();
                Check.That(((StringParser.ConvertError)Eval.Expr(" 10.2 << 20")).ErrorValue.Equals("<< and >> requires integer on left side")).IsTrue();
                Check.That(((StringParser.ConvertError)Eval.Expr(" 10 << 20.2")).ErrorValue.Equals("<< and >> requires integer on right side")).IsTrue();
                Check.That((long)Eval.Expr(" 10 - -20") == 30).IsTrue();

                //5
                Check.That((long)Eval.Expr(" 1 > 2 ") == 0).IsTrue();
                Check.That((long)Eval.Expr(" 1 < 2 ") == 1).IsTrue();
                Check.That((long)Eval.Expr(" 1 >= 2 ") == 0).IsTrue();
                Check.That((long)Eval.Expr(" 1 <= 2 ") == 1).IsTrue();
                Check.That((long)Eval.Expr(" 2 >= 2 ") == 1).IsTrue();
                Check.That((long)Eval.Expr(" 3 >= 2 ") == 1).IsTrue();
                Check.That((long)Eval.Expr(" 1.0 > 2 ") == 0).IsTrue();
                Check.That((long)Eval.Expr(" 1.0 < 2 ") == 1).IsTrue();
                Check.That((long)Eval.Expr(" 1.0 >= 2 ") == 0).IsTrue();
                Check.That((long)Eval.Expr(" 1.0 <= 2 ") == 1).IsTrue();
                Check.That((long)Eval.Expr(" 2 >= 2.0 ") == 1).IsTrue();
                Check.That((long)Eval.Expr(" 3 >= 2.0 ") == 1).IsTrue();
                Check.That((long)Eval.Expr(" \"ABC\" > \"AAA\"") == 1).IsTrue();
                Check.That((long)Eval.Expr(" \"AAA\" < \"ABC\"") == 1).IsTrue();
                Check.That((long)Eval.Expr(" \"AAA\" >= \"ABC\"") == 0).IsTrue();
                Check.That((long)Eval.Expr(" \"ABC\" >= \"ABC\"") == 1).IsTrue();

                //6
                Check.That((long)Eval.Expr(" 1 == 2 ") == 0).IsTrue();
                Check.That((long)Eval.Expr(" 2 == 1") == 0).IsTrue();
                Check.That((long)Eval.Expr(" 1 == 1 ") == 1).IsTrue();
                Check.That((long)Eval.Expr(" 1.0 == 1") == 1).IsTrue();
                Check.That((long)Eval.Expr(" 1 == 1.0 ") == 1).IsTrue();
                Check.That((long)Eval.Expr(" \"ABC\" == \"AAA\"") == 0).IsTrue();
                Check.That((long)Eval.Expr(" \"AAA\" != \"ABC\"") == 1).IsTrue();
                Check.That((long)Eval.Expr(" \"AAA\" == \"AAA\"") == 1).IsTrue();
                Check.That((long)Eval.Expr(" \"ABC\" != \"ABC\"") == 0).IsTrue();

                //7
                Check.That((long)Eval.Expr(" 1 & 2 ") == (long)(1 & 2)).IsTrue();
                Check.That((long)Eval.Expr(" 100 & 200 ") == (long)(100 & 200)).IsTrue();
                Check.That(((StringParser.ConvertError)Eval.Expr(" 10.2 & 20")).ErrorValue.Equals("& requires integer on left side")).IsTrue();
                Check.That(((StringParser.ConvertError)Eval.Expr(" 10 & 20.2")).ErrorValue.Equals("& requires integer on right side")).IsTrue();

                //8
                Check.That((long)Eval.Expr(" 1 ^ 2 ") == (long)(1 ^ 2)).IsTrue();
                Check.That((long)Eval.Expr(" 100 ^ 200 ") == (long)(100 ^ 200)).IsTrue();
                Check.That(((StringParser.ConvertError)Eval.Expr(" 10.2 ^ 20")).ErrorValue.Equals("^ requires integer on left side")).IsTrue();
                Check.That(((StringParser.ConvertError)Eval.Expr(" 10 ^ 20.2")).ErrorValue.Equals("^ requires integer on right side")).IsTrue();

                //9
                Check.That((long)Eval.Expr(" 1 | 2 ") == (long)(1 | 2)).IsTrue();
                Check.That((long)Eval.Expr(" 100 | 200 ") == (long)(100 | 200)).IsTrue();
                Check.That(((StringParser.ConvertError)Eval.Expr(" 10.2 | 20")).ErrorValue.Equals("| requires integer on left side")).IsTrue();
                Check.That(((StringParser.ConvertError)Eval.Expr(" 10 | 20.2")).ErrorValue.Equals("| requires integer on right side")).IsTrue();

                //10

                Check.That((long)Eval.Expr(" 1 && 2 ") == 1).IsTrue();
                Check.That((long)Eval.Expr(" 0 && 1 ") == 0).IsTrue();
                Check.That((long)Eval.Expr(" 0 && 0 ") == 0).IsTrue();
                Check.That(((StringParser.ConvertError)Eval.Expr(" 10.0 && 2")).ErrorValue.Equals("&& requires integer on left side")).IsTrue();
                Check.That(((StringParser.ConvertError)Eval.Expr(" 10 && 20.0")).ErrorValue.Equals("&& requires integer on right side")).IsTrue();

                //11

                Check.That((long)Eval.Expr(" 1 || 2 ") == 1).IsTrue();
                Check.That((long)Eval.Expr(" 0 || 1 ") == 1).IsTrue();
                Check.That((long)Eval.Expr(" 0 || 0 ") == 0).IsTrue();
                Check.That(((StringParser.ConvertError)Eval.Expr(" 10.0 || 2")).ErrorValue.Equals("|| requires integer on left side")).IsTrue();
                Check.That(((StringParser.ConvertError)Eval.Expr(" 10 || 20.0")).ErrorValue.Equals("|| requires integer on right side")).IsTrue();

                // Misc

                Check.That(((StringParser.ConvertError)Eval.Expr(" 1 !| 2")).ErrorValue.Equals("Extra characters after expression: !| 2")).IsTrue();
                Check.That(((StringParser.ConvertError)Eval.Expr(" \"string\"", allowstrings: false)).ErrorValue.Equals("Strings not supported")).IsTrue();

                // Brackets

                Check.That((long)Eval.Expr(" 10 * ( 2 + 3 ) ") == 10 * (2 + 3)).IsTrue();
                Check.That(((StringParser.ConvertError)Eval.Expr(" 10 * (2 + 3")).ErrorValue.Equals("Missing ) at end of expression")).IsTrue();
            }

            {
                // Symbols..
                Dictionary<string, Object> symbols = new Dictionary<string, object>();

                Eval ev = new Eval(allowfp: true, allowstrings: true);
                ev.ReturnSymbolValue = (s) =>
                {
                    if (symbols.ContainsKey(s))
                        return symbols[s];
                    else
                        return new StringParser.ConvertError("No such symbol '" + s + "'");
                };

                symbols.Add("d20", (double)20.0);
                symbols.Add("d10", (double)10.0);
                symbols.Add("i20", (long)20);
                symbols.Add("i10", (long)10);

                Check.That((long)ev.Evaluate(" i10 + i20 ") == 30).IsTrue();
                Check.That((double)ev.Evaluate(" i10 + d20 ") == 30.0).IsTrue();
                Check.That((double)ev.Evaluate(" d10 + d20 ") == 30.0).IsTrue();
                Check.That(((StringParser.ConvertError)ev.Evaluate(" i20 * i11 ")).ErrorValue.Equals("No such symbol 'i11'")).IsTrue();

                ev.SetSymbolValue = (s, o) =>
                {
                    symbols[s] = o;
                    return o;
                };

                Check.That((long)ev.Evaluate(" i11 = 20 ") == 20).IsTrue();
                Check.That((long)ev.Evaluate(" i11 ") == 20).IsTrue();
                Check.That((long)ev.Evaluate(" ( i20 + i20 ) * i10 ") == (20 + 20) * 10).IsTrue();
                Check.That(((StringParser.ConvertError)ev.Evaluate(" 20*20 = 20")).ErrorValue.Equals("Lvalue required for = operator")).IsTrue();
                Check.That(((StringParser.ConvertError)ev.Evaluate(" l1 * l1 = 20")).ErrorValue.Equals("No such symbol 'l1'")).IsTrue();
                Check.That(((StringParser.ConvertError)ev.Evaluate(" i20 * 20 = 20")).ErrorValue.Equals("Lvalue required for = operator")).IsTrue();

                ev.ReturnFunctionValue = BaseFunctionsForEval.BaseFunctions;


                Check.That((long)ev.Evaluate(" Abs ( - 20 ) ") == 20).IsTrue();
                Check.That((double)ev.Evaluate(" Abs ( - 20.2 ) ") == 20.2).IsTrue();
                Check.That((double)ev.Evaluate(" Acos ( 1 ) ") == 0).IsTrue();
                Check.That(Math.Abs((double)ev.Evaluate(" Asin ( 1 ) ") - 1.57079) < 1E-4).IsTrue();
                Check.That(Math.Abs((double)ev.Evaluate(" Atan ( 1 ) ") - 0.78539) < 1E-4).IsTrue();
                Check.That(Math.Abs((double)ev.Evaluate(" Ceiling ( 1.99 ) ")) == 2).IsTrue();
                Check.That(Math.Abs((double)ev.Evaluate(" Cos ( 1 ) ") - 0.54030) < 1E-4).IsTrue();
                Check.That(Math.Abs((double)ev.Evaluate(" Cosh ( 1 ) ") - 1.54308) < 1E-4).IsTrue();
                Check.That(Math.Abs((double)ev.Evaluate(" Exp ( 1 ) ") - 2.71828) < 1E-4).IsTrue();
                Check.That(Math.Abs((double)ev.Evaluate(" Floor ( 1.99 ) ")) == 1).IsTrue();
                Check.That(Math.Abs((double)ev.Evaluate(" Log ( 10 ) ") - 2.3025) < 1E-4).IsTrue();
                Check.That(Math.Abs((double)ev.Evaluate(" Log10 ( 10 ) ") - 1) < 1E-4).IsTrue();
                Check.That(Math.Abs((long)ev.Evaluate(" Max ( 1,10 ) ")) == 10).IsTrue();
                Check.That(Math.Abs((long)ev.Evaluate(" Min ( 1,10 ) ")) == 1).IsTrue();
                Check.That(Math.Abs((double)ev.Evaluate(" Max ( 1,10.2 ) ")) == 10.2).IsTrue();
                Check.That(Math.Abs((double)ev.Evaluate(" Min ( 1.3,10 ) ")) == 1.3).IsTrue();
                Check.That(Math.Abs((double)ev.Evaluate(" Pow ( 2,2 ) ") - 4) < 1E-4).IsTrue();
                Check.That(((StringParser.ConvertError)ev.Evaluate(" Pow ( 2 )")).ErrorValue.Equals("Pow() requires 2 parameters minimum")).IsTrue();
                Check.That(((StringParser.ConvertError)ev.Evaluate(" Pow ( 2,3,4 )")).ErrorValue.Equals("Pow() takes 2 parameters maximum")).IsTrue();
                Check.That(Math.Abs((double)ev.Evaluate(" Round ( 1.1 ) ") - 1) < 1E-4).IsTrue();
                Check.That(Math.Abs((double)ev.Evaluate(" Round ( 1.9 ) ") - 2) < 1E-4).IsTrue();
                Check.That(((StringParser.ConvertError)ev.Evaluate(" Round (1.23456, 3.4)")).ErrorValue.Equals("Round() type mismatch in parameter 2 wanted Integer")).IsTrue();
                Check.That(Math.Abs((double)ev.Evaluate(" Round ( 1.123456, 2 ) ") - 1.12) < 1E-4).IsTrue();
                Check.That(Math.Abs((double)ev.Evaluate(" Round ( 1.123456, 4 ) ") - 1.1234) < 1E-4).IsTrue();
                Check.That(Math.Abs((long)ev.Evaluate(" Sign ( 1.123456 ) ") - 1) < 1E-4).IsTrue();
                Check.That(Math.Abs((long)ev.Evaluate(" Sign ( -1.123456 ) ") - (-1)) < 1E-4).IsTrue();
                Check.That(Math.Abs((long)ev.Evaluate(" Sign ( 0 ) ") - (0)) < 1E-4).IsTrue();
                Check.That(Math.Abs((long)ev.Evaluate(" Sign ( 1000 ) ") - (1)) < 1E-4).IsTrue();
                Check.That(Math.Abs((double)ev.Evaluate(" Sin ( 1 ) ") - 0.84147) < 1E-4).IsTrue();
                Check.That(Math.Abs((double)ev.Evaluate(" Sinh ( 1 ) ") - 1.1752) < 1E-4).IsTrue();
                Check.That(Math.Abs((double)ev.Evaluate(" Sqrt ( 10 ) ") - 3.16227) < 1E-4).IsTrue();
                Check.That(Math.Abs((double)ev.Evaluate(" Tan ( 1 ) ") - 1.5574) < 1E-4).IsTrue();
                Check.That(Math.Abs((double)ev.Evaluate(" Tanh ( 1 ) ") - 0.76159) < 1E-4).IsTrue();
                Check.That(Math.Abs((double)ev.Evaluate(" Truncate ( 1.222 ) ") - 1) < 1E-4).IsTrue();
                Check.That(Math.Abs((double)ev.Evaluate(" Fp(2) ")) == 2.0).IsTrue();
                Check.That(Math.Abs((long)ev.Evaluate(" Eval(\"2+2\") ")) == 4.0).IsTrue();

                Check.That((string)ev.Evaluate(" ToString(10,\"N1\") ") == "10.0").IsTrue();
                Check.That((string)ev.Evaluate(" ToString(10.2,\"N1\") ") == "10.2").IsTrue();

                Check.That((long)ev.Evaluate(" IsDigit('2')") == 1).IsTrue();
                Check.That((long)ev.Evaluate(" IsDigit('A')") == 0).IsTrue();
                Check.That((long)ev.Evaluate(" IsUpper('A')") == 1).IsTrue();
                Check.That((long)ev.Evaluate(" IsUpper('a')") == 0).IsTrue();
                Check.That((long)ev.Evaluate(" ToUpper('a')") == 65).IsTrue();
                Check.That((long)ev.Evaluate(" ToLower('A')") == 97).IsTrue();
                Check.That((string)ev.Evaluate(" ToLower(\"Hello\")") == "hello").IsTrue();
                Check.That((string)ev.Evaluate(" ToUpper(\"Hello\")") == "HELLO").IsTrue();
                Check.That((string)ev.Evaluate(" Unicode(65)") == "A").IsTrue();
                Check.That((string)ev.Evaluate(" Unicode(329)") == "ŉ").IsTrue();

                Check.That(((StringParser.ConvertError)ev.Evaluate(" fred ( 20 )")).ErrorValue.Equals("fred() not recognised")).IsTrue();
            }

            {
                Eval ev = new Eval(allowfp: true, allowstrings: true);
                StackOfDictionaries<string, Object> symbols = new StackOfDictionaries<string, Object>();
                UserDefinedFunctions userdef = new UserDefinedFunctions(symbols);
                userdef.BaseFunctions = BaseFunctionsForEval.BaseFunctions;

                ev.ReturnSymbolValue = (s) =>
                {
                    if (symbols.ContainsKey(s))
                        return symbols[s];
                    else
                        return new StringParser.ConvertError("No such symbol '" + s + "'");
                };

                ev.SetSymbolValue = (s, o) =>
                {
                    symbols[s] = o;
                    return o;
                };

                ev.ReturnFunctionValue = userdef.Functions;

                userdef.Add("f1", new string[] { }, "10 + 20");

                //  Check.That((long)ev.Evaluate(" 2*f1()+10") == 70).IsTrue();

                userdef.Add("f2", new string[] { "a" }, "a + 20");

                Check.That((long)ev.Evaluate(" 2*f2( 15 )+10") == 2 * (15 + 20) + 10).IsTrue();

                userdef.Add("f3", new string[] { "a", "b", }, "a + b + 20");

                Check.That((long)ev.Evaluate(" 2*f3( 15, 34 )+10") == 2 * (15 + 34 + 20) + 10).IsTrue();

                userdef.Add("f4", new string[] { "a", "b", }, "f2(a) + b + 20");

                Check.That((long)ev.Evaluate(" 2*f4( 15, 34 )+10") == 2 * ((15+20) + 34 + 20) + 10).IsTrue();
            }

            System.Diagnostics.Debug.WriteLine("ALL EVAL TESTS FINISHED");
        }
    }
}
