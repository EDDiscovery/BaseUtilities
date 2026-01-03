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
 *
 */

using System;
using System.Collections.Generic;

namespace BaseUtils
{
    public class CommandArgs
    {
        private string[] args;
        private int pos;

        public CommandArgs(string[] a, int index = 0)
        {
            args = a;
            pos = index;
        }

        // Assemble line arguments.
        // the // donotes a comment line
        public CommandArgs(string a)
        {
            StringParser sp = new StringParser(a);
            List<string> argsl = new List<string>();
            while (!sp.IsEOL)
            {
                string t = sp.NextQuotedWord(" \r\n\t");
                if (t == null)
                    break;
                if (t == "//")
                    sp.SkipPastCRLF(true);
                else
                    argsl.Add(t);
            }

            args = argsl.ToArray();
            pos = 0;
        }

        public CommandArgs(CommandArgs other)
        {
            args = other.args;
            pos = other.pos;
        }

        public string Peek { get { return (pos < args.Length) ? args[pos] : null; } }
        public bool PeekAndRemoveIf(string s, StringComparison sc = StringComparison.InvariantCultureIgnoreCase)
        {
            if (pos < args.Length)
            {
                if (args[pos].Equals(s, sc))
                {
                    pos++;
                    return true;
                }
            }
            return false;
        } 
        public string Next() { return (pos < args.Length) ? args[pos++] : null; }
        public string NextLI() { return (pos < args.Length) ? args[pos++].ToLowerInvariant() : null; }
        public string NextEmpty() { return (pos < args.Length) ? args[pos++] : ""; }
        public int Int() { return (pos < args.Length) ? args[pos++].InvariantParseInt(0) : 0; }
        public int? IntNull() { return (pos < args.Length) ? args[pos++].InvariantParseIntNull() : null; }
           
        public long Long() { return (pos < args.Length) ? args[pos++].InvariantParseLong(0) : 0; }
        public long? LongNull() { return (pos < args.Length) ? args[pos++].InvariantParseLongNull() : null; }
        public double Double() { return (pos < args.Length) ? args[pos++].InvariantParseDouble(0) : 0.0; }
        public double? DoubleNull() { return (pos < args.Length) ? args[pos++].InvariantParseDoubleNull() : null; }
        public bool Bool() { return (pos < args.Length) ? args[pos++].InvariantParseBool(false) : false; }
        public bool? BoolNull() { return (pos < args.Length) ? args[pos++].InvariantParseBoolNull() : null; }

        public string Rest(string sep = " ") { return string.Join(sep, args, pos, args.Length - pos); }

        public string Arguments(int pos, int items = -1, string sep = " ") 
        {
            if (items == -1)
                items = args.Length - pos;
            return string.Join(sep, args, pos, items); 
        }

        public string this[int v] { get { int left = args.Length - pos; return (v < left) ? args[pos + v] : null; } }
        public int Pos { get { return pos; } }
        public bool More { get { return args.Length > pos; } }
        public int Left { get { return args.Length - pos; } }
        public void Remove() { if (pos < args.Length) pos++; }
    }
}
