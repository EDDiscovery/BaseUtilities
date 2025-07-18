﻿/*
 * Copyright 2017-2023 EDDiscovery development team
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

using QuickJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BaseUtils
{
    public class FunctionsBasic : FunctionHandlers
    {
        public FunctionsBasic(Functions c, Variables v, FunctionPersistentData h, int recd) : base(c, v, h, recd)
        {
            if (functions == null)        // one time init, done like this cause can't do it in {}
            {
                functions = new Dictionary<string, FuncEntry>();

                #region Variables
                functions.Add("exist", new FuncEntry(Exist, 1, 20, FuncEntry.PT.M)); // no macros, all literal, can be strings
                functions.Add("existsdefault", new FuncEntry(ExistsDefault, 2, FuncEntry.PT.M, FuncEntry.PT.MSE, FuncEntry.PT.MSE, FuncEntry.PT.MSE, FuncEntry.PT.MSE, FuncEntry.PT.MSE));   // first is a macro but can not exist, second etc is a string or macro which must exist
                functions.Add("expand", new FuncEntry(Expand, 1, 20, FuncEntry.PT.ME)); // check var, can be string (if so expanded)
                functions.Add("expandarray", new FuncEntry(ExpandArray, 4, FuncEntry.PT.M, FuncEntry.PT.MESE, FuncEntry.PT.ImeSE, FuncEntry.PT.ImeSE, FuncEntry.PT.LS, FuncEntry.PT.MESE, FuncEntry.PT.MESE));
                functions.Add("expandvars", new FuncEntry(ExpandVars, 4, FuncEntry.PT.M, FuncEntry.PT.MESE, FuncEntry.PT.ImeSE, FuncEntry.PT.ImeSE, FuncEntry.PT.LS));   // var 1 is text root/string, not var, not string, var 2 can be var or string, var 3/4 is integers or variables, checked in function
                functions.Add("findarray", new FuncEntry(FindArray, 2, FuncEntry.PT.M, FuncEntry.PT.MESE, FuncEntry.PT.MESE));
                functions.Add("findinarray", new FuncEntry(FindInArray, 5, FuncEntry.PT.M, FuncEntry.PT.M, FuncEntry.PT.MESE, FuncEntry.PT.ImeSE, FuncEntry.PT.ImeSE, FuncEntry.PT.LS));
                functions.Add("indirect", new FuncEntry(Indirect, 1, 20, FuncEntry.PT.ME));   // check var
                functions.Add("i", new FuncEntry(IndirectI, FuncEntry.PT.ME, FuncEntry.PT.LS));   // first is a macro name, second is literal or string
                functions.Add("ispresent", new FuncEntry(Ispresent, 2, FuncEntry.PT.M, FuncEntry.PT.MESE, FuncEntry.PT.LmeSE)); // 1 may not be there, 2 either a macro or can be string. 3 is optional and a var or literal
                functions.Add("variable", new FuncEntry(Variable, 1, 20, FuncEntry.PT.ME));   // check var
                #endregion

                #region Numbers
                functions.Add("abs", new FuncEntry(Abs, FuncEntry.PT.FmeSE, FuncEntry.PT.LmeSE));  // first is macro or lit. second is macro or literal
                functions.Add("int", new FuncEntry(Int, FuncEntry.PT.ImeSE, FuncEntry.PT.LmeSE));  // first is macro or lit, second is macro or lit
                functions.Add("eval", new FuncEntry(Eval, 1, FuncEntry.PT.LmeSE, FuncEntry.PT.LS, FuncEntry.PT.LmeSE));   // can be string, can be variable, can be literal p2 is not a variable, and can't be a string. p3 can be string, can be variable
                functions.Add("floor", new FuncEntry(Floor, FuncEntry.PT.FmeSE, FuncEntry.PT.LmeSE));     // first is macros or lit, second is macro or literal
                functions.Add("hnum", new FuncEntry(Hnum, FuncEntry.PT.FmeSE, FuncEntry.PT.LmeSE));   // para 1 literal or var, para 2 string, literal or var
                functions.Add("tostring", new FuncEntry(ToString, FuncEntry.PT.FmeSE, FuncEntry.PT.LmeSE));  // first is macro or lit. second is macro or literal

                functions.Add("iftrue", new FuncEntry(Iftrue, 2, FuncEntry.PT.ImeSE, FuncEntry.PT.ms, FuncEntry.PT.ms));   // check var1-3, allow strings var1-3
                functions.Add("iffalse", new FuncEntry(Iffalse, 2, FuncEntry.PT.ImeSE, FuncEntry.PT.ms, FuncEntry.PT.ms));   // check var1-3, allow strings var1-3
                functions.Add("ifzero", new FuncEntry(Ifzero, 2, FuncEntry.PT.FmeSE, FuncEntry.PT.ms, FuncEntry.PT.ms));   // check var1-3, allow strings var1-3
                functions.Add("ifnonzero", new FuncEntry(Ifnonzero, 2, FuncEntry.PT.FmeSE, FuncEntry.PT.ms, FuncEntry.PT.ms));   // check var1-3, allow strings var1-3

                functions.Add("ifeq", new FuncEntry(Ifnumequal, 3, FuncEntry.PT.FmeSEBlk, FuncEntry.PT.FmeSE, FuncEntry.PT.ms, FuncEntry.PT.ms, FuncEntry.PT.ms));
                functions.Add("ifne", new FuncEntry(Ifnumnotequal, 3, FuncEntry.PT.FmeSEBlk, FuncEntry.PT.FmeSE, FuncEntry.PT.ms, FuncEntry.PT.ms, FuncEntry.PT.ms));
                functions.Add("ifgt", new FuncEntry(Ifnumgreater, 3, FuncEntry.PT.FmeSEBlk, FuncEntry.PT.FmeSE, FuncEntry.PT.ms, FuncEntry.PT.ms, FuncEntry.PT.ms));
                functions.Add("iflt", new FuncEntry(Ifnumless, 3, FuncEntry.PT.FmeSEBlk, FuncEntry.PT.FmeSE, FuncEntry.PT.ms, FuncEntry.PT.ms, FuncEntry.PT.ms));
                functions.Add("ifge", new FuncEntry(Ifnumgreaterequal, 3, FuncEntry.PT.FmeSEBlk, FuncEntry.PT.FmeSE, FuncEntry.PT.ms, FuncEntry.PT.ms, FuncEntry.PT.ms));
                functions.Add("ifle", new FuncEntry(Ifnumlessequal, 3, FuncEntry.PT.FmeSEBlk, FuncEntry.PT.FmeSE, FuncEntry.PT.ms, FuncEntry.PT.ms, FuncEntry.PT.ms));

                functions.Add("random", new FuncEntry(Random, FuncEntry.PT.ImeSE));
                functions.Add("seedrandom", new FuncEntry(SeedRandom, FuncEntry.PT.ImeSE));
                functions.Add("round", new FuncEntry(RoundCommon, FuncEntry.PT.FmeSE, FuncEntry.PT.ImeSE, FuncEntry.PT.LmeSE));
                functions.Add("roundnz", new FuncEntry(RoundCommon, FuncEntry.PT.FmeSE, FuncEntry.PT.ImeSE, FuncEntry.PT.LmeSE, FuncEntry.PT.ImeSE));
                functions.Add("roundscale", new FuncEntry(RoundCommon, FuncEntry.PT.FmeSE, FuncEntry.PT.ImeSE, FuncEntry.PT.LmeSE, FuncEntry.PT.ImeSE, FuncEntry.PT.FmeSE));
                #endregion

                #region Strings
                functions.Add("alt", new FuncEntry(Alt, 2, 20, FuncEntry.PT.MESE));  // string/var.. repeated
                functions.Add("escapechar", new FuncEntry(EscapeChar, FuncEntry.PT.MESE));   // check var, can be string
                functions.Add("ifnotempty", new FuncEntry(Ifnotempty, 2, FuncEntry.PT.MESE, FuncEntry.PT.ms, FuncEntry.PT.ms));
                functions.Add("ifempty", new FuncEntry(Ifempty, 2, FuncEntry.PT.MESE, FuncEntry.PT.ms, FuncEntry.PT.ms));
                functions.Add("ifcontains", new FuncEntry(Ifcontains, 3, FuncEntry.PT.MESE, FuncEntry.PT.LmeSE, FuncEntry.PT.ms, FuncEntry.PT.ms, FuncEntry.PT.ms));
                functions.Add("ifnotcontains", new FuncEntry(Ifnotcontains, 3, FuncEntry.PT.MESE, FuncEntry.PT.LmeSE, FuncEntry.PT.ms, FuncEntry.PT.ms, FuncEntry.PT.ms));
                functions.Add("ifequal", new FuncEntry(Ifequal, 3, FuncEntry.PT.MESE, FuncEntry.PT.LmeSE, FuncEntry.PT.ms, FuncEntry.PT.ms, FuncEntry.PT.ms));
                functions.Add("ifnotequal", new FuncEntry(Ifnotequal, 3, FuncEntry.PT.MESE, FuncEntry.PT.LmeSE, FuncEntry.PT.ms, FuncEntry.PT.ms, FuncEntry.PT.ms));

                functions.Add("indexof", new FuncEntry(IndexOf, FuncEntry.PT.MESE, FuncEntry.PT.MESE));
                functions.Add("icao", new FuncEntry(Icao, FuncEntry.PT.MESE, FuncEntry.PT.LmeSE));
                functions.Add("join", new FuncEntry(Join, 3, 20, FuncEntry.PT.MESE));
                functions.Add("jsonparse", new FuncEntry(Jsonparse, FuncEntry.PT.MESE, FuncEntry.PT.M));
                functions.Add("length", new FuncEntry(Length, FuncEntry.PT.MESE));
                functions.Add("lowerinvariant", new FuncEntry(LowerInvariant, 1, 20, FuncEntry.PT.MESE));
                functions.Add("lower", new FuncEntry(Lower, 1, 20, FuncEntry.PT.MESE));
                functions.Add("phrasel", new FuncEntry(PhraseL, 1, FuncEntry.PT.MESE));
                functions.Add("phrase", new FuncEntry(Phrase, 1, FuncEntry.PT.MESE, FuncEntry.PT.MESE, FuncEntry.PT.MESE, FuncEntry.PT.MESE));
                functions.Add("tojson", new FuncEntry(ToJson, 1, FuncEntry.PT.M, FuncEntry.PT.ImeSE));

                functions.Add("regex", new FuncEntry(Regex, FuncEntry.PT.MESE, FuncEntry.PT.MESE, FuncEntry.PT.MESE));
                functions.Add("replace", new FuncEntry(Replace, FuncEntry.PT.MESE, FuncEntry.PT.MESE, FuncEntry.PT.MESE));
                functions.Add("replacevar", new FuncEntry(ReplaceVar, FuncEntry.PT.MESE, FuncEntry.PT.LmeSE));
                functions.Add("rv", new FuncEntry(ReplaceVar, FuncEntry.PT.MESE, FuncEntry.PT.LmeSE)); // var/string, literal/var/string
                functions.Add("rs", new FuncEntry(ReplaceVarSC, FuncEntry.PT.MESE, FuncEntry.PT.LmeSE)); // var/string, literal/var/string
                functions.Add("replaceescapechar", new FuncEntry(ReplaceEscapeChar, FuncEntry.PT.MESE));
                functions.Add("replaceifstartswith", new FuncEntry(ReplaceIfStartsWith, 2, FuncEntry.PT.MESE, FuncEntry.PT.MESE, FuncEntry.PT.MESE ));

                functions.Add("sc", new FuncEntry(SplitCaps, FuncEntry.PT.MESE));
                functions.Add("splitcaps", new FuncEntry(SplitCaps, FuncEntry.PT.MESE));
                functions.Add("substring", new FuncEntry(SubString, FuncEntry.PT.MESE, FuncEntry.PT.ImeSE, FuncEntry.PT.ImeSE));
                functions.Add("trim", new FuncEntry(Trim, FuncEntry.PT.MESE));
                functions.Add("upperinvariant", new FuncEntry(UpperInvariant, 1, 20, FuncEntry.PT.MESE));
                functions.Add("upper", new FuncEntry(Upper, 1, 20, FuncEntry.PT.MESE));
                functions.Add("wordcount", new FuncEntry(WordCount, 1, FuncEntry.PT.MESE, FuncEntry.PT.MESE));
                functions.Add("wordfind", new FuncEntry(WordFind, 2, FuncEntry.PT.MESE, FuncEntry.PT.MESE, FuncEntry.PT.MESE, FuncEntry.PT.ImeSE, FuncEntry.PT.ImeSE));
                functions.Add("wordof", new FuncEntry(WordOf, 2, FuncEntry.PT.MESE, FuncEntry.PT.ImeSE, FuncEntry.PT.MESE));
                functions.Add("wordlistcount", new FuncEntry(WordListCount, 1, FuncEntry.PT.MESE, FuncEntry.PT.MESE));
                functions.Add("wordlistentry", new FuncEntry(WordListEntry, 2, FuncEntry.PT.MESE, FuncEntry.PT.ImeSE, FuncEntry.PT.MESE));
                #endregion

                #region File Read Write
                functions.Add("closefile", new FuncEntry(CloseFile, FuncEntry.PT.ImeSE));
                functions.Add("openfile", new FuncEntry(OpenFile, FuncEntry.PT.M, FuncEntry.PT.MESE, FuncEntry.PT.LmeSE));
                functions.Add("seek", new FuncEntry(SeekFile, FuncEntry.PT.ImeSE, FuncEntry.PT.ImeSE));
                functions.Add("tell", new FuncEntry(TellFile, FuncEntry.PT.ImeSE));
                functions.Add("write", new FuncEntry(Write, FuncEntry.PT.ImeSE, FuncEntry.PT.MESE));
                functions.Add("writeline", new FuncEntry(WriteLine, FuncEntry.PT.ImeSE, FuncEntry.PT.MESE));
                #endregion

                #region Files
                functions.Add("direxists", new FuncEntry(DirExists, 1, 20, FuncEntry.PT.MESE));
                functions.Add("fileexists", new FuncEntry(FileExists, 1, 20, FuncEntry.PT.MESE));
                functions.Add("deletefile", new FuncEntry(DeleteFile, 1, 20, FuncEntry.PT.MESE));
                functions.Add("filelist", new FuncEntry(FileList, FuncEntry.PT.MESE, FuncEntry.PT.MESE));
                functions.Add("filelength", new FuncEntry(FileLength, FuncEntry.PT.MESE));
                functions.Add("findline", new FuncEntry(FindLine, FuncEntry.PT.MESE, FuncEntry.PT.MESE));
                functions.Add("mkdir", new FuncEntry(MkDir, FuncEntry.PT.MESE));
                functions.Add("rmdir", new FuncEntry(RmDir, FuncEntry.PT.MESE));
                functions.Add("readline", new FuncEntry(ReadLine, FuncEntry.PT.ImeSE, FuncEntry.PT.M));
                functions.Add("readalltext", new FuncEntry(ReadAllText, FuncEntry.PT.MESE));
                functions.Add("safevarname", new FuncEntry(SafeVarName, FuncEntry.PT.MESE));
                functions.Add("systempath", new FuncEntry(SystemPath, FuncEntry.PT.LmeSE));
                functions.Add("combinepaths", new FuncEntry(CombinePaths, 2, 10, FuncEntry.PT.MESE));
                functions.Add("directoryname", new FuncEntry(DirectoryName, FuncEntry.PT.MESE));
                functions.Add("filename", new FuncEntry(FileName, FuncEntry.PT.MESE));
                functions.Add("extension", new FuncEntry(Extension, FuncEntry.PT.MESE));
                functions.Add("filenamenoextension", new FuncEntry(FileNameNoExtension, FuncEntry.PT.MESE));
                functions.Add("fullpath", new FuncEntry(FullPath, FuncEntry.PT.MESE));
                #endregion

                #region Processes
                functions.Add("closeprocess", new FuncEntry(CloseProcess, FuncEntry.PT.ImeSE));
                functions.Add("findprocess", new FuncEntry(FindProcess, FuncEntry.PT.MESE));
                functions.Add("hasprocessexited", new FuncEntry(HasProcessExited, FuncEntry.PT.ImeSE));
                functions.Add("killprocess", new FuncEntry(KillProcess, FuncEntry.PT.ImeSE));
                functions.Add("listprocesses", new FuncEntry(ListProcesses, FuncEntry.PT.M));
                functions.Add("startprocess", new FuncEntry(StartProcess, FuncEntry.PT.MESE, FuncEntry.PT.MESE));
                functions.Add("waitforprocess", new FuncEntry(WaitForProcess, FuncEntry.PT.ImeSE, FuncEntry.PT.ImeSE));
                #endregion

                #region Time
                functions.Add("date", new FuncEntry(Date, FuncEntry.PT.MESE, FuncEntry.PT.LmeSE));   // first is a var or string, second is literal
                functions.Add("datetimenow", new FuncEntry(DateTimeNow, FuncEntry.PT.LmeSE));     // literal type
                functions.Add("datedelta", new FuncEntry(DateDelta, 2, FuncEntry.PT.MESE, FuncEntry.PT.MESE, FuncEntry.PT.LmeSE));   // date, date, literal
                functions.Add("datedeltaformat", new FuncEntry(DateDeltaFormat, FuncEntry.PT.FmeSE, FuncEntry.PT.MESE, FuncEntry.PT.MESE));   // delta seconds, string, string
                functions.Add("datedeltaformatnow", new FuncEntry(DateDeltaFormatNow, 3, FuncEntry.PT.MESE, FuncEntry.PT.MESE, FuncEntry.PT.MESE, FuncEntry.PT.LmeSE));   // macro,macro,macro,literal
                functions.Add("datedeltadiffformat", new FuncEntry(DateDeltaDiffFormat, 4, FuncEntry.PT.MESE, FuncEntry.PT.MESE, FuncEntry.PT.MESE, FuncEntry.PT.MESE, FuncEntry.PT.LmeSE));
                functions.Add("tickcount", new FuncEntry(TickCount));
                #endregion
            }
        }

        static Dictionary<string, FuncEntry> functions = null;

        protected override FuncEntry FindFunction(string name)
        {
            name = name.ToLowerInvariant();      // case insensitive.
            return functions.ContainsKey(name) ? functions[name] : null;
        }

        #region Variable Functions

        protected bool Exist(out string output)
        {
            foreach (Parameter s in paras)
            {
                if (!vars.Exists(s.Value))      // either s.value is an expanded string, or a literal.. does not matter.
                {
                    output = "0";
                    return true;
                }
            }

            output = "1";
            return true;
        }

        protected bool Alt(out string output)
        {
            output = "";

            foreach (Parameter s in paras)
            {
                if (s.Value.Length > 0)
                {
                    output = s.Value;
                    break;
                }
            }

            return true;
        }

        protected bool ExistsDefault(out string output)
        {
            for (int i = 0; i < paras.Count; i++)
            {
                if (paras[i].IsString)     // first literal string wins
                {
                    //System.Diagnostics.Debug.WriteLine($"ExistsDefault Selected String {paras[i].Value}");
                    output = paras[i].Value;
                    return true;
                }
                else if (vars.Exists(paras[i].Value))   // else its a macro name, if it exists, we stop
                {
                    output = vars[paras[i].Value];
                    //System.Diagnostics.Debug.WriteLine($"ExistsDefault Selected Macro {paras[i].Value} = {output}");
                    return true;
                }
                else
                {
                    //System.Diagnostics.Debug.WriteLine($"ExistsDefault Not found Macro {paras[i].Value}");
                }
            }

            output = "No default value found in ExistsDefault";
            return false;
        }

        protected bool Expand(out string output)
        {
            output = "";

            foreach (Parameter p in paras)          // output been expanded by ME to get macro contents, now expand them out
            {
                if (caller.ExpandStringFull(p.Value, out string res, recdepth + 1) == Functions.ExpandResult.Failed)
                {
                    output = res;
                    return false;
                }
                output += res;
            }

            return true;
        }

        protected bool Variable(out string output)
        {
            output = "";

            foreach (Parameter p in paras)          // output been expanded by ME to get macro names, now expand them out
            {
                if (vars.Exists(p.Value))           // if its a variable, return the contents
                    output += vars[p.Value];
                else
                {
                    output = "No such variable " + p.Value;
                    return false;
                }
            }

            return true;
        }

        protected bool Indirect(out string output)
        {
            output = "";

            foreach (Parameter p in paras)          // output been expanded by ME..
            {
                //System.Diagnostics.Debug.WriteLine($"Indirect {p.Value}");

                if (vars.Exists(p.Value))         // if macro name, expand..
                {
                    if (caller.ExpandStringFull(vars[p.Value], out string res, recdepth + 1) == Functions.ExpandResult.Failed)
                    {
                        output = res;
                        return false;
                    }

                    //System.Diagnostics.Debug.WriteLine($"Variable expanded to {res}");

                    output += res;
                }
                else if (p.IsString)         // in previous versions, i was incorrectly usign indirect instead of expand.. so if its string quoted, allow it thru
                {
                    output += p.Value;         // its already expanded once, no need again..
                }
                else
                {
                    output = "Indirect Variable '" + p.Value + "' not found";
                    return false;
                }
            }

            return true;
        }

        protected bool IndirectI(out string output)
        {
            string mname = paras[0].Value + paras[1].Value;        // first part macro name, expanded. Plus the second literal part

            if (vars.Exists(mname))
            {
                string value = vars[mname];
                Functions.ExpandResult result = caller.ExpandStringFull(value, out output, recdepth + 1);

                return result != Functions.ExpandResult.Failed;
            }
            else
                output = "Indirect Variable '" + mname + "' does not exist";

            return false;
        }

        #endregion

        #region Formatters

        protected bool SplitCaps(out string output)
        {
            output = paras[0].Value.SplitCapsWordFull();
            return true;
        }

        #endregion

        #region Strings

        protected bool SubString(out string output)
        {
            int start = paras[1].Int;
            int length = paras[2].Int;
            string v = paras[0].Value;

            if (start >= 0 && start < v.Length)
            {
                if (start + length > v.Length)
                    length = v.Length - start;

                output = v.Substring(start, length);
            }
            else
                output = "";

            return true;
        }

        protected bool IndexOf(out string output)
        {
            output = paras[0].Value.IndexOf(paras[1].Value).ToString(ct);
            return true;
        }

        protected bool LowerInvariant(out string output)
        {
            return IntLower(out output, System.Globalization.CultureInfo.InvariantCulture);
        }

        protected bool Lower(out string output)
        {
            return IntLower(out output, System.Globalization.CultureInfo.CurrentCulture);
        }

        private bool IntLower(out string output, System.Globalization.CultureInfo ci)
        {
            string value = paras[0].Value;
            string delim = (paras.Count > 1) ? paras[1].Value : "";

            for (int i = 2; i < paras.Count; i++)
                value += delim + paras[i].Value;

            output = value.ToLower(ci);
            return true;
        }

        protected bool UpperInvariant(out string output)
        {
            return IntUpper(out output, System.Globalization.CultureInfo.InvariantCulture);
        }

        protected bool Upper(out string output)
        {
            return IntUpper(out output, System.Globalization.CultureInfo.CurrentCulture);
        }

        private bool IntUpper(out string output, System.Globalization.CultureInfo ci)
        {
            string value = paras[0].Value;
            string delim = (paras.Count > 1) ? paras[1].Value : "";

            for (int i = 2; i < paras.Count; i++)
                value += delim + paras[i].Value;

            output = value.ToUpper(ci);
            return true;
        }

        protected bool Join(out string output)
        {
            string delim = paras[0].Value;
            string value = paras[1].Value;

            for (int i = 2; i < paras.Count; i++)
                value += delim + paras[i].Value;

            output = value;
            return true;
        }

        protected bool Trim(out string output)
        {
            output = paras[0].Value.Trim();
            return true;
        }

        protected bool Length(out string output)
        {
            output = paras[0].Value.Length.ToString(ct);
            return true;
        }

        protected bool EscapeChar(out string output)
        {
            output = paras[0].Value.EscapeControlChars();
            return true;
        }

        protected bool ReplaceEscapeChar(out string output)
        {
            output = paras[0].Value.ReplaceEscapeControlChars();
            return true;
        }

        protected bool ReplaceIfStartsWith(out string output)
        {
            if (paras[0].Value.StartsWith(paras[1].Value, StringComparison.InvariantCultureIgnoreCase))
            {
                output = ((paras.Count >= 3) ? paras[2].Value : "") + paras[0].Value.Substring(paras[1].Value.Length);
            }
            else
                output = paras[0].Value;
            return true;
        }

        protected bool WordCount(out string output)
        {
            string s = paras[0].Value;
            string splitter = (paras.Count >= 2) ? paras[1].Value : ";";
            char splitchar = (splitter.Length > 0) ? splitter[0] : ';';

            string[] split = s.Split(splitchar);
            output = split.Length.ToStringInvariant();

            return true;
        }
        protected bool WordFind(out string output)
        {
            string s = paras[0].Value;
            string t = paras[1].Value;
            string splitter = (paras.Count >= 3) ? paras[2].Value : ";";
            char splitchar = (splitter.Length > 0) ? splitter[0] : ';';

            bool caseinsensitive = (paras.Count >= 4) ? paras[3].Int != 0 : true;
            StringComparison cp = caseinsensitive ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;

            bool contains = (paras.Count >= 5) ? paras[4].Int != 0 : false;

            string[] split = s.Split(splitchar);

            output = "0";

            for (int i = 0; i < split.Length; i++)
            {
                if (contains ? split[i].Contains(t, cp) : split[i].Equals(t, cp))
                {
                    output = (i + 1).ToStringInvariant();
                    break;
                }
            }

            return true;
        }
        protected bool WordOf(out string output)
        {
            string s = paras[0].Value;
            int count = paras[1].Int;
            string splitter = (paras.Count >= 3) ? paras[2].Value : ";";
            char splitchar = (splitter.Length > 0) ? splitter[0] : ';';

            string[] split = s.Split(splitchar);
            count = Math.Max(1, Math.Min(count, split.Length));  // between 1 and split length
            output = split[count - 1];
            return true;
        }

        protected bool WordListCount(out string output)
        {
            BaseUtils.StringParser l = new BaseUtils.StringParser(paras[0].Value);
            string splitchars = (paras.Count >= 2) ? paras[1].Value : ",";
            List<string> ll = l.NextQuotedWordList(separchars: splitchars);
            output = ll != null ? ll.Count.ToStringInvariant() : "0";
            return true;
        }


        protected bool WordListEntry(out string output)
        {
            BaseUtils.StringParser l = new BaseUtils.StringParser(paras[0].Value);
            int count = paras[1].Int;
            string splitchars = (paras.Count >= 3) ? paras[2].Value : ",";

            output = "";

            List<string> ll = l.NextQuotedWordList(separchars: splitchars);
            if (ll != null && count >= 0 && count < ll.Count)
                output = ll[count];
            return true;
        }

        protected bool Regex(out string output)
        {
            string s = paras[0].Value;
            string f1 = paras[1].Value;
            string f2 = paras[2].Value;
            try
            {
                output = System.Text.RegularExpressions.Regex.Replace(s, f1, f2);
                return true;
            }
            catch
            {
                output = "Regular expression failed";
                return false;
            }
        }

        protected bool Replace(out string output)
        {
            string s = paras[0].Value;
            string f1 = paras[1].Value;
            string f2 = paras[2].Value;
            output = s.Replace(f1, f2, StringComparison.InvariantCultureIgnoreCase);
            return true;
        }


        protected bool ReplaceVar(out string output)
        {
            return ReplaceVarCommon(out output, false);
        }

        protected bool ReplaceVarSC(out string output)
        {
            return ReplaceVarCommon(out output, true);
        }

        protected bool ReplaceVarCommon(out string output, bool sc)
        {
            string s = paras[0].Value;
            string varroot = paras[1].Value;

            foreach (string key in vars.NameEnumuerable)          // all vars.. starting with varroot
            {
                if (key.StartsWith(varroot))
                {
                    string[] subs = vars[key].Split(';');
                    if (subs.Length == 2 && subs[0].Length > 0) // standard anywhere pattern
                    {
                        if (s.IndexOf(subs[0], StringComparison.InvariantCultureIgnoreCase) >= 0)
                            s = s.Replace(subs[0], subs[1], StringComparison.InvariantCultureIgnoreCase);
                    }
                    else if (subs.Length == 3 && subs[1].Length > 0 && subs[0].Equals("R", StringComparison.InvariantCultureIgnoreCase))     // regex pattern
                    {
                        System.Text.RegularExpressions.RegexOptions opt = (subs[0] == "r") ? System.Text.RegularExpressions.RegexOptions.IgnoreCase : System.Text.RegularExpressions.RegexOptions.None;
                        s = System.Text.RegularExpressions.Regex.Replace(s, subs[1], subs[2], opt);
                    }
                    else
                    {
                        output = "Missformed replacement pattern : " + vars[key];
                        return false;
                    }
                }
            }

            if (sc)
                output = s.SplitCapsWordFull();
            else
                output = s;

            return true;
        }

        protected bool Phrase(out string output)
        {
            output = paras[0].Value.PickOneOfGroups(rnd, paras.Count > 1 ? paras[1].Value : ";", paras.Count > 2 ? paras[2].Value : "{", paras.Count > 3 ? paras[3].Value : "}");
            return true;
        }

        protected bool PhraseL(out string output)
        {
            output = paras[0].Value.PickOneOfGroups(rnd, "<;>", "{{{", "}}}");
            return true;
        }

        protected bool SafeVarName(out string output)
        {
            output = paras[0].Value.SafeVariableString();
            return true;
        }

        protected bool Ifnotempty(out string output) { return IfCommon(out output, () => { return paras[0].Value.Length > 0; }, 1, true); }
        protected bool Ifempty(out string output) { return IfCommon(out output, () => { return paras[0].Value.Length == 0; }, 1, true); }
        protected bool Ifequal(out string output) { return IfCommon(out output, () => { return paras[0].Value.Equals(paras[1].Value, StringComparison.InvariantCultureIgnoreCase); }, 2); }
        protected bool Ifnotequal(out string output) { return IfCommon(out output, () => { return paras[0].Value.Equals(paras[1].Value, StringComparison.InvariantCultureIgnoreCase) == false; }, 2); }
        protected bool Ifcontains(out string output) { return IfCommon(out output, () => { return paras[0].Value.IndexOf(paras[1].Value, StringComparison.InvariantCultureIgnoreCase) >= 0; }, 2); }
        protected bool Ifnotcontains(out string output) { return IfCommon(out output, () => { return paras[0].Value.IndexOf(paras[1].Value, StringComparison.InvariantCultureIgnoreCase) < 0; }, 2); }

        protected bool Ispresent(out string output)
        {
            if (vars.Exists(paras[0].Value))        // if paras[0] is a macro which exists
            {
                string mvalue = vars[paras[0].Value];
                string cvalue = paras[1].Value;
                output = mvalue.IndexOf(cvalue, StringComparison.InvariantCultureIgnoreCase) >= 0 ? "1" : "0";
            }
            else
            {   // var does not exist..

                if (paras.Count == 3)   // if default is there, return it
                    output = paras[2].Value;
                else
                    output = "0";
            }

            return true;
        }


        protected bool Jsonparse(out string output)
        {
            string json = paras[0].Value;
            string varprefix = paras[1].Value;

            try
            {
                JToken tk = JToken.Parse(json);
                if (tk != null)
                {
                    vars.FromJSON(tk, varprefix);
                    output = "1";
                    return true;
                }
            }
            catch { }

            output = "0";
            return false;
        }

        protected bool ToJson(out string output)
        {
            string json = paras[0].Value;
            bool verbose = paras.Count > 1 ? paras[1].Int != 0 : false;

            try
            {
                JToken tk = vars.ToJSON(json);
                if (tk != null)
                {
                    output = tk.ToString(verbose);
                    return true;
                }
            }
            catch { }

            output = "0";
            return false;
        }

        static string[] IcaoAlphabet =      // nicked from our EDCD friends
        {
                "<phoneme alphabet=\"ipa\" ph=\"ˈzɪərəʊ\">zero</phoneme>",
                "<phoneme alphabet=\"ipa\" ph=\"ˈwʌn\">one</phoneme>",
                "<phoneme alphabet=\"ipa\" ph=\"ˈtuː\">two</phoneme>",
                "<phoneme alphabet=\"ipa\" ph=\"ˈtriː\">tree</phoneme>",
                "<phoneme alphabet=\"ipa\" ph=\"ˈfoʊ.ər\">fawer</phoneme>",
                "<phoneme alphabet=\"ipa\" ph=\"ˈfaɪf\">fife</phoneme>",
                "<phoneme alphabet=\"ipa\" ph=\"ˈsɪks\">six</phoneme>",
                "<phoneme alphabet=\"ipa\" ph=\"ˈsɛvɛn\">seven</phoneme>",
                "<phoneme alphabet=\"ipa\" ph=\"ˈeɪt\">eight</phoneme>",
                "<phoneme alphabet=\"ipa\" ph=\"ˈnaɪnər\">niner</phoneme>",
                "<phoneme alphabet=\"ipa\" ph=\"ˈælfə\">alpha</phoneme>",
                "<phoneme alphabet=\"ipa\" ph=\"ˈbrɑːˈvo\">bravo</phoneme>",
                "<phoneme alphabet=\"ipa\" ph=\"ˈtʃɑːli\">charlie</phoneme>",
                "<phoneme alphabet=\"ipa\" ph=\"ˈdɛltə\">delta</phoneme>",
                "<phoneme alphabet=\"ipa\" ph=\"ˈeko\">echo</phoneme>",
                "<phoneme alphabet=\"ipa\" ph=\"ˈfɒkstrɒt\">foxtrot</phoneme>",
                "<phoneme alphabet=\"ipa\" ph=\"ɡɒlf\">golf</phoneme>",
                "<phoneme alphabet=\"ipa\" ph=\"hoːˈtel\">hotel</phoneme>",
                "<phoneme alphabet=\"ipa\" ph=\"ˈindiˑɑ\">india</phoneme>",
                "<phoneme alphabet=\"ipa\" ph=\"ˈdʒuːliˑˈet\">juliet</phoneme>",
                "<phoneme alphabet=\"ipa\" ph=\"ˈkiːlo\">kilo</phoneme>",
                "<phoneme alphabet=\"ipa\" ph=\"ˈliːmɑ\">lima</phoneme>",
                "<phoneme alphabet=\"ipa\" ph=\"maɪk\">mike</phoneme>",
                "<phoneme alphabet=\"ipa\" ph=\"noˈvembə\">november</phoneme>",
                "<phoneme alphabet=\"ipa\" ph=\"ˈɒskə\">oscar</phoneme>",
                "<phoneme alphabet=\"ipa\" ph=\"pəˈpɑ\">papa</phoneme>",
                "<phoneme alphabet=\"ipa\" ph=\"keˈbek\">quebec</phoneme>",
                "<phoneme alphabet=\"ipa\" ph=\"ˈroːmiˑo\">romeo</phoneme>",
                "<phoneme alphabet=\"ipa\" ph=\"siˈerə\">sierra</phoneme>",
                "<phoneme alphabet=\"ipa\" ph=\"ˈtænɡo\">tango</phoneme>",
                "<phoneme alphabet=\"ipa\" ph=\"ˈjuːnifɔːm\">uniform</phoneme>",
                "<phoneme alphabet=\"ipa\" ph=\"ˈvɪktə\">victor</phoneme>",
                "<phoneme alphabet=\"ipa\" ph=\"ˈwiski\">whiskey</phoneme>",
                "<phoneme alphabet=\"ipa\" ph=\"ˈeksˈrei\">x-ray</phoneme>",
                "<phoneme alphabet=\"ipa\" ph=\"ˈjænki\">yankee</phoneme>",
                "<phoneme alphabet=\"ipa\" ph=\"ˈzuːluː\">zulu</phoneme>",
        };

        protected bool Icao(out string output)
        {
            string str = paras[0].Value.ToLowerInvariant();
            output = "";
            foreach( char c in str )
            {
                if (char.IsDigit(c))
                    output += IcaoAlphabet[c - '0'];
                else if (c >= 'a' && c <= 'z')
                    output += IcaoAlphabet[c - 'a' + 10];
                else if (c == '-' && paras[1].Value.IndexOf("Dash", StringComparison.InvariantCultureIgnoreCase) >=0)
                    output += " dash ";
            }
            return true;
        }

        #endregion

        #region Numbers

        protected bool Hnum(out string output)
        {
            double value = paras[0].Fractional;
            string postfix = paras[1].Value;
            string[] postfixes = postfix.Split(';');

            if (postfixes.Length < 6)
            {
                output = "Need prefixes and postfixes";
            }
            else if (double.IsNaN(value))
            {
                output = "Not a Number";
            }
            else if (double.IsNegativeInfinity(value))
            {
                output = "Negative Infinity";
            }
            else if (double.IsPositiveInfinity(value))
            {
                output = "Positive Infinity";
            }
            else if (value == 0)// like, doh.  can't log zero
            {
                output = "0";
                return true;
            }
            else
            {
                string prefix = "";
                if (value < 0)
                {
                    prefix = postfixes[0] + " ";
                    value = -value;
                }

                double order = Math.Log10(value);

                if ( double.IsInfinity(order))
                {
                    output = "Log10(0) Something badly wrong, should have been caught - report";
                }
                else if (order >= 12)        // trillions, say X.Y trillion
                {
                    value /= 1E12;
                    output = prefix + value.ToStringInvariant("0.##") + " " + postfixes[1];
                }
                else if (order >= 9)        // billions, say X.Y billion
                {
                    value /= 1E9;
                    output = prefix + value.ToStringInvariant("0.##") + " " + postfixes[2];
                }
                else if (order >= 6)        // millions, say X.Y millions
                {
                    value /= 1E6;
                    output = prefix + value.ToStringInvariant("0.##") + " " + postfixes[3];
                }
                else if (order >= 4)        // 10000+ thousands, say X thousands
                {
                    value = Math.Round(value / 1E3 * 10.0) / 10.0;   // in thousands
                    output = prefix + value.ToStringInvariant("0.#") + " " + postfixes[4];
                }
                else if (order >= 3)        // 1000-9999, say xx hundred
                {
                    value = Math.Round(value / 1E2 * 10.0) / 10.0;   // in hundreds
                    int hundreds = (int)value;  // thousand parts
                    output = prefix + (hundreds / 10).ToStringInvariant() + " " + postfixes[4];
                    if (hundreds % 10 != 0) // hundred parts
                        output += " " + (hundreds % 10).ToStringInvariant() + " " + postfixes[5];
                }
                else if (order >= 2)       // 100-999
                {
                    output = prefix + value.ToStringInvariant("0");
                }
                else if (order >= 1)       // 10-99
                {
                    output = prefix + value.ToStringInvariant("0");
                }
                else if (order >= 0)      //1-9
                    output = prefix + value.ToStringInvariant("0.#");
                else
                {                         // order is <0.  so minimum digits is 1
                    int digits = (int)(-order) + 1;
                    string s = "0." + new string('#', digits);
                    output = prefix + value.ToStringInvariant(s);
                }


                return true;
            }

            return false;
        }

        protected bool Abs(out string output)
        {
            double para = paras[0].Fractional;
            string fmt = paras[1].Value;
            para = Math.Abs(para);
            return para.ToStringExtendedSafe(fmt, out output);
        }

        protected bool ToString(out string output)
        {
            double para = paras[0].Fractional;
            string fmt = paras[1].Value;
            return para.ToStringExtendedSafe(fmt, out output);
        }

        protected bool Int(out string output)
        {
            long para = paras[0].Long;
            string fmt = paras[1].Value;
            return para.ToStringExtendedSafe(fmt, out output);
        }

        protected bool Floor(out string output)
        {
            double para = paras[0].Fractional;
            string fmt = paras[1].Value;
            para = Math.Floor(para);
            return para.ToStringExtendedSafe(fmt, out output);
        }

        protected bool RoundCommon(out string output)
        {
            double value = paras[0].Fractional;
            int digits = paras[1].Int;
            string fmt = paras[2].Value;
            int extradigits = (paras.Count >= 4) ? paras[3].Int : 0;
            double scale = (paras.Count >= 5) ? paras[4].Fractional : 1.0;

            value *= scale;
            double res = Math.Round(value, digits);

            if (extradigits > 0 && Math.Abs(res) < 0.0000001)     // if rounded to zero..
            {
                digits += extradigits;
                fmt += new string('#', extradigits);
                res = Math.Round(value, digits);
            }

            return res.ToStringExtendedSafe(fmt, out output);
        }

        protected bool Random(out string output)
        {
            output = rnd.Next(paras[0].Int).ToString(ct);
            return true;
        }

        protected bool SeedRandom(out string output)
        {
            SetRandom(new System.Random(paras[0].Int));
            output = "1";
            return true;
        }

        protected bool Eval(out string output)
        {
            // string, or if macro name use macro value, else literal
            string s = paras[0].Value;
            bool tryit = false;
            if ( paras.Count >= 2 )
            {
                if (paras[1].Value.Equals("Try", StringComparison.InvariantCultureIgnoreCase))
                    tryit = true;
                else if (!paras[1].Value.Equals("Error", StringComparison.InvariantCultureIgnoreCase))
                {
                    output = "Eval parameter 2 invalid";
                    return false;
                }
            }

            string fmt = "";
            if ( paras.Count >= 3)
            {
                fmt = paras[2].Value;
            }

            BaseUtils.Eval ev = new BaseUtils.Eval(vars, new BaseFunctionsForEval(), checkend: true, allowfp: true, allowstrings: true);

            Object ret = ev.Evaluate(s);

            if (ev.InError)
            {
                if (tryit)
                {
                    output = "NAN";
                    return true;
                }
                else
                {
                    output = ((StringParser.ConvertError)ret).ErrorValue;
                    return false;
                }
            }
            else if (fmt == "")
            {
                output = ev.ToString(System.Globalization.CultureInfo.InvariantCulture);        // will work with all types, long, double, string
                return true;
            }
            else
            {
                try
                {
                    return ev.ToSafeString(fmt, out output);        // may not convert, depends if format is correct for the type
                }
                catch
                {
                    output = "Type did not convert to string";
                    return false;
                }
            }
        }

        const double LM = 0.000001;

        protected bool Iftrue(out string output) { return IfCommon(out output, () => { return paras[0].Long != 0; }, 1); }           // V1, VT, [VF]
        protected bool Iffalse(out string output) { return IfCommon(out output, () => { return paras[0].Long == 0; }, 1); }
        protected bool Ifzero(out string output) { return IfCommon(out output, () => { return Math.Abs(paras[0].Fractional) < LM; }, 1); }
        protected bool Ifnonzero(out string output) { return IfCommon(out output, () => { return Math.Abs(paras[0].Fractional) >= LM; }, 1); }
        protected bool Ifnumequal(out string output) { return IfCommon(out output, () => { return Math.Abs(paras[0].Fractional - paras[1].Fractional) < LM; }, 2); }
        protected bool Ifnumnotequal(out string output) { return IfCommon(out output, () => { return Math.Abs(paras[0].Fractional - paras[1].Fractional) >= LM; }, 2); }
        protected bool Ifnumgreater(out string output) { return IfCommon(out output, () => { return paras[0].Fractional > paras[1].Fractional; }, 2); }
        protected bool Ifnumless(out string output) { return IfCommon(out output, () => { return paras[0].Fractional < paras[1].Fractional; }, 2); }
        protected bool Ifnumgreaterequal(out string output) { return IfCommon(out output, () => { return paras[0].Fractional >= paras[1].Fractional; }, 2); }
        protected bool Ifnumlessequal(out string output) { return IfCommon(out output, () => { return paras[0].Fractional <= paras[1].Fractional; }, 2); }

        protected bool IfCommon(out string output, Func<bool> Test, int firstres , bool disablelengthtest = false)
        {
            output = "";

            if (!disablelengthtest && paras[0].Value.Length == 0)   // if we can have an empty first.. its the third para.  If its not there, empty string
                return ExpandMacroStr(out output, firstres + 2);

            int parano = Test() ? firstres : (firstres + 1);

            return ExpandMacroStr(out output, parano);  // pick first or second to expand.  if second does not exist, func returns empty string
        }


        #endregion


        #region Arrays

        protected bool ExpandArray(out string output)
        {
            string separ = paras[1].Value;
            string lastsepar = paras.Count >= 7 ? paras[6].Value : separ;
            string postname = paras.Count >= 6 ? paras[5].Value : "";
            bool splitcaps = paras.Count >= 5 && paras[4].Value.IndexOf("splitcaps", StringComparison.InvariantCultureIgnoreCase) >= 0;
            bool ignoremissing = paras.Count >= 5 && paras[4].Value.IndexOf("ignoremissing", StringComparison.InvariantCultureIgnoreCase) >= 0;
            int start = paras[2].Int;
            int length = paras[3].Int;

            List<string> entries = new List<string>();

            for (int i = start; i < start + length; i++)
            {
                string aname = paras[0].Value + "[" + i.ToString(ct) + "]" + postname;
                //System.Diagnostics.Debug.WriteLine($"FindInArray check {aname}");

                if (vars.Exists(aname))
                {
                    if (splitcaps)
                        entries.Add(vars[aname].SplitCapsWordFull());
                    else
                        entries.Add(vars[aname]);
                }
                else if ( !ignoremissing )
                    break;
            }

            output = "";

            for (int i = 0; i < entries.Count; i++)
            {
                if (i != 0)
                    output += (i < entries.Count - 1) ? separ : lastsepar;

                output += entries[i];
            }

            return true;
        }
        protected bool FindInArray(out string output)
        {
            string findstring = paras[2].Value;
            int start = paras[3].Int;
            int length = paras[4].Int;
            string options = paras.Count >= 6 ? paras[5].Value.ToLowerInvariant() : "";
            StringComparison comparetype = options.Contains("casesensitive") ? StringComparison.InvariantCulture: StringComparison.InvariantCultureIgnoreCase;
            bool startswith = options.Contains("startswith");
            bool contains = options.Contains("contains");
            bool ignoremissing = options.Contains("ignoremissing");

            for (int i = start; i < start + length; i++)
            {
                string aname = paras[0].Value + "[" + i.ToString(ct) + "]" + paras[1].Value;
                //System.Diagnostics.Debug.WriteLine($"FindInArray check {aname}");

                if (vars.Exists(aname))
                {
                    if ( startswith ? vars[aname].StartsWith(findstring, comparetype) : 
                         contains ? vars[aname].Contains(findstring, comparetype) : vars[aname].Equals(findstring, comparetype))
                    {
                        output = i.ToStringInvariant();
                        return true;
                    }
                }
                else if ( !ignoremissing )
                    break;
            }

            output = "-1";
            return true;
        }

        protected bool ExpandVars(out string output)
        {
            string arrayroot = paras[0].Value;
            int start = paras[2].Int;
            int length = paras[3].Int;

            bool splitcaps = paras.Count == 5 && paras[4].Value.IndexOf("splitcaps", StringComparison.InvariantCultureIgnoreCase) >= 0;
            bool nameonly = paras.Count == 5 && paras[4].Value.IndexOf("nameonly", StringComparison.InvariantCultureIgnoreCase) >= 0;
            bool valueonly = paras.Count == 5 && paras[4].Value.IndexOf("valueonly", StringComparison.InvariantCultureIgnoreCase) >= 0;
            bool removecount = paras.Count == 5 && paras[4].Value.IndexOf("removecount", StringComparison.InvariantCultureIgnoreCase) >= 0;

            output = "";


            int index = 0;
            foreach (string key in vars.NameEnumuerable)
            {
                if (key.StartsWith(arrayroot) && (!removecount || !key.EndsWith("_Count")))
                {
                    index++;
                    if (index >= start && index < start + length)
                    {
                        string value = vars[key];

                        string entry = (valueonly) ? vars[key] : (key.Substring(arrayroot.Length) + (nameonly ? "" : (" = " + vars[key])));

                        if (output.Length > 0)
                            output += paras[1].Value;

                        output += (splitcaps) ? entry.SplitCapsWordFull() : entry;
                    }
                }
            }

            return true;
        }


        protected bool FindArray(out string output)
        {
            string arrayroot = paras[0].Value;
            string search = paras[1].Value;
            string searchafter = (paras.Count >= 3) ? paras[2].Value : "";
            bool searchon = searchafter.Length == 0;    // empty searchafter means go immediately

            foreach (string key in vars.NameEnumuerable)
            {
                if (key.StartsWith(arrayroot))
                {
                    if (searchon)
                    {
                        string value = vars[key];

                        if (value.IndexOf(search, StringComparison.InvariantCultureIgnoreCase) >= 0)
                        {
                            output = key;
                            return true;
                        }
                    }
                    else if (key.Equals(searchafter))
                        searchon = true;
                }
            }

            output = "";
            return true;
        }

        #endregion

        #region File Read Write
        protected bool OpenFile(out string output)
        {
            if (persistentdata == null)
            {
                output = "File access not supported";
                return false;
            }

            string handle = paras[0].Value;
            string file = paras[1].Value;
            string mode = paras[2].Value;

            FileMode fm;
            if (Enum.TryParse<FileMode>(mode, true, out fm))
            {
                if (VerifyFileAccess(file, fm))
                {
                    string errmsg;
                    int id = persistentdata.FileHandles.Open(file, fm, fm == FileMode.Open ? FileAccess.Read : FileAccess.Write, out errmsg);
                    if (id > 0)
                        vars[handle] = id.ToStringInvariant();
                    else
                        output = errmsg;

                    output = (id > 0) ? "1" : "0";
                    return true;
                }
                else
                    output = "Permission denied access to " + file;
            }
            else
                output = "Unknown File Mode";

            return false;
        }

        protected bool CloseFile(out string output)
        {
            if (persistentdata != null)
            {
                persistentdata.FileHandles.Close(paras[0].Int);
                output = "1";
                return true;
            }
            else
            {
                output = "File handle not found or invalid";
                return false;
            }
        }

        protected bool ReadLine(out string output)
        {
            if (persistentdata != null)
            {
                if (persistentdata.FileHandles.ReadLine(paras[0].Int, out output))
                {
                    if (output == null)
                        output = "0";
                    else
                    {
                        vars[paras[1].Value] = output;
                        output = "1";
                    }
                    return true;
                }
                else
                    return false;
            }
            else
            {
                output = "File handle not found or invalid";
                return false;
            }
        }

        protected bool WriteLine(out string output)
        {
            return WriteToFileInt(out output, true);
        }
        protected bool Write(out string output)
        {
            return WriteToFileInt(out output, false);
        }
        protected bool WriteToFileInt(out string output, bool lf)
        {
            if (persistentdata != null)
            {
                if (persistentdata.FileHandles.WriteLine(paras[0].Int, paras[1].Value, lf, out output))
                {
                    output = "1";
                    return true;
                }
                else
                    return false;
            }
            else
            {
                output = "File handle not found or invalid";
                return false;
            }
        }

        protected bool SeekFile(out string output)
        {
            if (persistentdata != null)
            {
                if (persistentdata.FileHandles.Seek(paras[0].Int, paras[1].Long, out output))
                {
                    output = "1";
                    return true;
                }
                else
                    return false;
            }
            else
            {
                output = "File handle not found or invalid, or position invalid";
                return false;
            }
        }
        protected bool TellFile(out string output)
        {
            if (persistentdata != null)
            {
                if (persistentdata.FileHandles.Tell(paras[0].Int, out output))
                {
                    return true;
                }
                else
                    return false;
            }
            else
            {
                output = "File handle not found or invalid";
                return false;
            }
        }

        #endregion

        #region File Functions

        protected bool CombinePaths(out string output)
        {
            output = Path.Combine(paras.Select(x => x.Value).ToArray());
            return true;
        }

        protected bool DirectoryName(out string output)
        {
            output = Path.GetDirectoryName(paras[0].Value);
            return true;
        }

        protected bool FileName(out string output)
        {
            output = Path.GetFileName(paras[0].Value);
            return true;
        }
        protected bool Extension(out string output)
        {
            output = Path.GetExtension(paras[0].Value);
            return true;
        }

        protected bool FileNameNoExtension(out string output)
        {
            output = Path.GetFileNameWithoutExtension(paras[0].Value);
            return true;
        }
        protected bool FullPath(out string output)
        {
            output = Path.GetFullPath(paras[0].Value);
            return true;
        }


        protected bool FileExists(out string output)
        {
            foreach (Parameter p in paras)
            {
                if (!System.IO.File.Exists(p.Value))
                {
                    output = "0";
                    return true;
                }
            }

            output = "1";
            return true;
        }

        protected bool DeleteFile(out string output)
        {
            output = "0";

            foreach (Parameter p in paras)
            {
                try
                {
                    if (VerifyFileAction("Delete", p.Value))
                    {
                        System.IO.File.Delete(p.Value);
                    }
                    else
                        return true;
                }
                catch
                {
                    return true;
                }
            }


            output = "1";
            return true;
        }

        protected bool FileList(out string output)
        {
            string path = paras[0].Value;
            string filename = paras[1].Value;
            try
            {
                var filelist = Directory.EnumerateFiles(path, filename, SearchOption.TopDirectoryOnly).ToList();
                for (int i = 0; i < filelist.Count; i++)
                    filelist[i] = "\"" + filelist[i] + "\"";

                output = string.Join(",", filelist);
                return true;
            }
            catch
            {
                output = "Directory not found";
                return false;
            }
        }

        protected bool DirExists(out string output)
        {
            foreach (Parameter p in paras)
            {
                if (!System.IO.Directory.Exists(p.Value))
                {
                    output = "0";
                    return true;
                }
            }

            output = "1";
            return true;
        }


        protected bool FileLength(out string output)
        {
            try
            {
                FileInfo fi = new FileInfo(paras[0].Value);
                output = fi.Length.ToString(ct);
                return true;
            }
            catch { }
            {
                output = "-1";
                return true;
            }
        }


        protected bool MkDir(out string output)
        {
            try
            {
                Directory.CreateDirectory(paras[0].Value);
                output = "1";
                return true;
            }
            catch { }

            output = "0";
            return true;
        }

        protected bool RmDir(out string output)
        {
            output = "0";

            try
            {
                if (VerifyFileAction("remove folder", paras[0].Value))
                {
                    Directory.Delete(paras[0].Value);
                    output = "1";
                }
            }
            catch { }

            return true;
        }

        protected bool SystemPath(out string output)
        {
            Environment.SpecialFolder sf;
            if (Enum.TryParse<Environment.SpecialFolder>(paras[0].Value, true, out sf))
            {
                output = Environment.GetFolderPath(sf);
                return true;
            }
            else
            {
                output = "";
                return false;
            }
        }

        protected bool FindLine(out string output)
        {
            using (System.IO.TextReader sr = new System.IO.StringReader(paras[0].Value))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.IndexOf(paras[1].Value, StringComparison.InvariantCultureIgnoreCase) != -1)
                    {
                        output = line;
                        return true;
                    }
                }
            }

            output = "";
            return true;
        }

        protected bool ReadAllText(out string output)
        {
            try
            {
                output = File.ReadAllText(paras[0].Value);
                return true;
            }
            catch { }

            output = "File not found:" + paras[0].Value;
            return false;
        }


        protected virtual bool VerifyFileAccess(string file, FileMode fm)      // override to provide protection
        {
            return true;
        }

        protected virtual bool VerifyFileAction(string action, string file)      // override to provide protection
        {
            return true;
        }

        protected virtual bool VerifyProcessAllowed(string proc, string cmdline)      // override to provide protection
        {
            return true;
        }

        #endregion


        #region Processes

        protected bool StartProcess(out string output)
        {
            string procname = paras[0].Value;
            string cmdline = paras[1].Value;

            if (persistentdata != null)
            {
                if (VerifyProcessAllowed(procname, cmdline))
                {
                    int pid = persistentdata.Processes.StartProcess(procname, cmdline);

                    if (pid != 0)
                    {
                        output = pid.ToStringInvariant();
                        return true;
                    }

                    output = "Process " + procname + " did not start";
                }
                else
                    output = "Process not allowed";
            }
            else
                output = "No persistency - Error";

            return false;
        }

        protected bool KillProcess(out string output) { return CloseKillProcess(out output, true); }
        protected bool CloseProcess(out string output) { return CloseKillProcess(out output, false); }

        protected bool CloseKillProcess(out string output, bool kill)
        {
            if (persistentdata != null)
            {
                BaseUtils.Processes.ProcessResult r = (kill) ? persistentdata.Processes.KillProcess(paras[0].Int) : persistentdata.Processes.CloseProcess(paras[0].Int);
                if (r == BaseUtils.Processes.ProcessResult.OK)
                {
                    output = "1";
                    return true;
                }
                else
                    output = "No such process found";
            }
            else
                output = "Missing PID";

            return false;
        }

        protected bool HasProcessExited(out string output)
        {
            if (persistentdata != null)
            {
                int exitcode;
                BaseUtils.Processes.ProcessResult r = persistentdata.Processes.HasProcessExited(paras[0].Int, out exitcode);
                if (r == BaseUtils.Processes.ProcessResult.OK)
                {
                    output = exitcode.ToStringInvariant();
                    return true;
                }
                else if (r == BaseUtils.Processes.ProcessResult.NotExited)
                {
                    output = "NOTEXITED";
                    return true;
                }
                else
                    output = "No such process found";
            }
            else
                output = "Missing PID";

            return false;
        }


        protected bool WaitForProcess(out string output)
        {
            if (persistentdata != null )
            {
                BaseUtils.Processes.ProcessResult r = persistentdata.Processes.WaitForProcess(paras[0].Int, paras[1].Int);
                if (r == BaseUtils.Processes.ProcessResult.OK)
                {
                    output = "1";
                    return true;
                }
                else if (r == BaseUtils.Processes.ProcessResult.Timeout)
                {
                    output = "0";
                    return true;
                }
                else
                    output = "No such process found";
            }
            else
                output = "Missing PID or timeout value";

            return false;
        }

        protected bool FindProcess(out string output)
        {
            if (persistentdata != null)
            {
                output = persistentdata.Processes.FindProcess(paras[0].Value).ToStringInvariant();
                return true;
            }
            else
                output = "No persistency - Error";

            return false;
        }

        protected bool ListProcesses(out string output)
        {
            string basename = paras[0].Value;

            string[] proc = BaseUtils.Processes.ListProcesses();
            for (int i = 0; i < proc.Length; i++)
            {
                vars[basename + "[" + (i + 1) + "]"] = proc[i];
            }

            output = "1";
            return true;
        }

        #endregion

        #region Dates

        protected bool Date(out string output)
        {
            DateTime? cnv = paras[0].Value.ParseUSDateTimeNull(paras[1].Value);

            if (cnv != null)
            {
                output = cnv.Value.ToStringFormatted(paras[1].Value);
                return true;
            }
            else
                output = "Date is not in correct en-US format";

            return false;
        }

        protected bool DateTimeNow(out string output)
        {
            output = DateTime.UtcNow.ToStringFormatted(paras[0].Value);
            return true;
        }

        protected bool DateDelta(out string output)
        {
            string formatoptions = (paras.Count >= 3) ? paras[2].Value : "";

            DateTime? v1 = paras[0].Value.ParseUSDateTimeNull(formatoptions);
            DateTime? v2 = paras[1].Value.ParseUSDateTimeNull(formatoptions);

            if (v1 != null && v2 != null)
            {
                if (v1.Value.Kind == DateTimeKind.Local)       // all must be in UTC for the calc..
                    v1 = v1.Value.ToUniversalTime();
                if (v2.Value.Kind == DateTimeKind.Local)       // all must be in UTC for the calc..
                    v2 = v2.Value.ToUniversalTime();

                TimeSpan ts = v2.Value.Subtract(v1.Value);      // does not respect time zones
                output = ts.TotalSeconds.ToStringInvariant();
                return true;
            }
            else
                output = "One of the dates is not in correct en-US format";

            return false;
        }


        protected bool DateDeltaFormat(out string output)
        {
            output = paras[0].Fractional.ToStringTimeDeltaFormatted(paras[1].Value, paras[2].Value);
            return true;
        }

        protected bool DateDeltaFormatNow(out string output)
        {
            string formatoptions = (paras.Count >= 4) ? paras[3].Value : "";

            DateTime? cnv = paras[0].Value.ParseUSDateTimeNull(formatoptions);

            if (cnv != null)
            {
                if (cnv.Value.Kind == DateTimeKind.Local)       // all must be in UTC for the calc..
                    cnv = cnv.Value.ToUniversalTime();

                DateTime cur = DateTime.UtcNow;
                TimeSpan ts = cnv.Value.Subtract(cur);          // does not respect Kind when doing calc.

                output = ts.TotalSeconds.ToStringTimeDeltaFormatted(paras[1].Value, paras[2].Value, cnv.Value, formatoptions);
                return true;
            }
            else
                output = "Date is not in correct en-US format";

            return false;
        }

        protected bool DateDeltaDiffFormat(out string output)
        {
            string formatoptions = (paras.Count >= 5) ? paras[4].Value : "";

            DateTime? v1 = paras[0].Value.ParseUSDateTimeNull(formatoptions);
            DateTime? v2 = paras[1].Value.ParseUSDateTimeNull(formatoptions);

            if (v1 != null && v2 != null)
            {
                if (v1.Value.Kind == DateTimeKind.Local)       // all must be in UTC for the calc..
                    v1 = v1.Value.ToUniversalTime();
                if (v2.Value.Kind == DateTimeKind.Local)       // all must be in UTC for the calc..
                    v2 = v2.Value.ToUniversalTime();

                TimeSpan ts = v2.Value.Subtract(v1.Value);      // does not respect Kind when doing calc.
                output = ts.TotalSeconds.ToStringTimeDeltaFormatted(paras[2].Value, paras[3].Value, v2.Value, formatoptions);
                return true;
            }
            else
                output = "One of the dates is not in correct en-US format";

            return false;
        }

        protected bool TickCount(out string output)
        {
            int tick = Environment.TickCount;
            uint ticku = (uint)tick;
            output = ticku.ToStringInvariant();
            return true;
        }

        #endregion



    }
}
