/*
 * Copyright © 2020 robby & EDDiscovery development team
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

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BaseUtils.JSON
{
    public class JSONTokenReader
    {
        // Returns a stream of JTokens describing the JSON structure.

        private IStringParserQuick parser;

        public class JProperty : JToken
        {
            public JProperty(string name, JToken t ) { Name = name; Value = t.Value; TokenType = t.TokenType; }
            public string Name { get; set; }
        }

        public JSONTokenReader(string s)
        {
            using (StringReader sr = new StringReader(s))         // read directly from file..
            {
                parser = new StringParserQuickTextReader(sr, 16384);
            }
        }

        public JSONTokenReader(TextReader trx, int chunksize = 16384)
        {
            parser = new StringParserQuickTextReader(trx, chunksize);
        }

        public JSONTokenReader(IStringParserQuick parserp)
        {
            parser = parserp;
        }

        public IEnumerable<JToken> Parse(JToken.ParseOptions flags = JToken.ParseOptions.None, int maxstringlen = 16384 )
        {
            char[] textbuffer = new char[maxstringlen];
            JToken[] stack = new JToken[256];
            int sptr = 0;
            bool comma = false;
            JArray curarray = null;
            JObject curobject = null;

            {
                parser.SkipSpace();

                JToken o = DecodeValue(textbuffer, false);       // grab new value, not array end

                if (o == null)
                {
                    GenError("No Obj/Array");
                    yield break;
                }
                else if (o.TokenType == JToken.TType.Array)
                {
                    stack[++sptr] = o;                      // push this one onto stack
                    curarray = o as JArray;                 // this is now the current array
                    yield return o;
                }
                else if (o.TokenType == JToken.TType.Object)
                {
                    stack[++sptr] = o;                      // push this one onto stack
                    curobject = o as JObject;               // this is now the current object
                    yield return o;
                }
                else
                {
                    yield return o;                               // value only
                    yield break;
                }
            }

            while (true)
            {
                if (curobject != null)      // if object..
                {
                    while (true)
                    {
                        char next = parser.GetChar();

                        if (next == '}')    // end object
                        {
                            parser.SkipSpace();

                            if (comma == true && (flags & JToken.ParseOptions.AllowTrailingCommas) == 0)
                            {
                                GenError("Comma");
                                yield break;
                            }
                            else
                            {
                                yield return new JToken(JToken.TType.EndObject);

                                JToken prevtoken = stack[--sptr];
                                if (prevtoken == null)      // if popped stack is null, we are back to beginning, return this
                                {
                                    yield break;
                                }
                                else
                                {
                                    comma = parser.IsCharMoveOn(',');
                                    curobject = prevtoken as JObject;
                                    if (curobject == null)
                                    {
                                        curarray = prevtoken as JArray;
                                        break;
                                    }
                                }
                            }
                        }
                        else if (next == '"')   // property name
                        {
                            int textlen = parser.NextQuotedString(next, textbuffer, true);

                            if (textlen < 1 || (comma == false && curobject.Count > 0) || !parser.IsCharMoveOn(':'))
                            {
                                GenError("Object :");
                                yield break;
                            }
                            else
                            {
                                string name = new string(textbuffer, 0, textlen);

                                JToken o = DecodeValue(textbuffer, false);      // get value

                                if (o == null)
                                {
                                    GenError("Bad value");
                                    yield break;
                                }

                                yield return new JProperty(name,o);

                                if ( name.Contains("browser_download"))
                                {

                                }

                                if (o.TokenType == JToken.TType.Array) // if array, we need to change to this as controlling object on top of stack
                                {
                                    if (sptr == stack.Length - 1)
                                    {
                                        GenError("Overflow");
                                        yield break;
                                    }

                                    stack[++sptr] = o;          // push this one onto stack
                                    curarray = o as JArray;                 // this is now the current object
                                    curobject = null;
                                    comma = false;
                                    break;
                                }
                                else if (o.TokenType == JToken.TType.Object)   // if object, this is the controlling object
                                {
                                    if (sptr == stack.Length - 1)
                                    {
                                        GenError("Overflow");
                                        yield break;
                                    }

                                    stack[++sptr] = o;          // push this one onto stack
                                    curobject = o as JObject;                 // this is now the current object
                                    comma = false;
                                }
                                else
                                {
                                    comma = parser.IsCharMoveOn(',');
                                }
                            }
                        }
                        else
                        {
                            GenError("Bad format");
                            yield break;
                        }
                    }
                }
                else
                {
                    while (true)
                    {
                        JToken o = DecodeValue(textbuffer, true);       // grab new value

                        if (o == null)
                        {
                            GenError("Bad value");
                            yield break;
                        }
                        else if (o.TokenType == JToken.TType.EndArray)          // if end marker, jump back
                        {
                            if (comma == true && (flags & JToken.ParseOptions.AllowTrailingCommas) == 0)
                            {
                                GenError("Comma");
                                yield break;
                            }
                            else
                            {
                                yield return new JToken(JToken.TType.EndArray);

                                JToken prevtoken = stack[--sptr];
                                if (prevtoken == null)      // if popped stack is null, we are back to beginning, return this
                                {
                                    yield break;
                                }
                                else
                                {
                                    comma = parser.IsCharMoveOn(',');
                                    curobject = prevtoken as JObject;
                                    if (curobject == null)
                                    {
                                        curarray = prevtoken as JArray;
                                    }
                                    else
                                        break;
                                }
                            }
                        }
                        else if ((comma == false && curarray.Count > 0))   // missing comma
                        {
                            GenError("Array comma");
                            yield break;
                        }
                        else
                        {
                            yield return o;

                            if (o.TokenType == JToken.TType.Array) // if array, we need to change to this as controlling object on top of stack
                            {
                                if (sptr == stack.Length - 1)
                                {
                                    GenError("Overflow");
                                    yield break;
                                }

                                stack[++sptr] = o;              // push this one onto stack
                                curarray = o as JArray;         // this is now the current array
                                comma = false;
                            }
                            else if (o.TokenType == JToken.TType.Object) // if object, this is the controlling object
                            {
                                if (sptr == stack.Length - 1)
                                {
                                    GenError("Overflow");
                                    yield break;
                                }

                                stack[++sptr] = o;              // push this one onto stack
                                curobject = o as JObject;       // this is now the current object
                                curarray = null;
                                comma = false;
                                break;
                            }
                            else
                            {
                                comma = parser.IsCharMoveOn(',');
                            }
                        }
                    }
                }

            }
        }

        static JToken jendarray = new JToken(JToken.TType.EndArray);

        private JToken DecodeValue(char[] textbuffer, bool inarray)
        {
            //System.Diagnostics.Debug.WriteLine("Decode at " + p.LineLeft);
            char next = parser.GetChar();
            switch (next)
            {
                case '{':
                    parser.SkipSpace();
                    return new JObject();

                case '[':
                    parser.SkipSpace();
                    return new JArray();

                case '"':
                    int textlen = parser.NextQuotedString(next, textbuffer, true);
                    return textlen >= 0 ? new JToken(JToken.TType.String, new string(textbuffer, 0, textlen)) : null;

                case ']':
                    if (inarray)
                    {
                        parser.SkipSpace();
                        return jendarray;
                    }
                    else
                        return null;

                case '0':       // all positive. JSON does not allow a + at the start (integer fraction exponent)
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    parser.BackUp();
                    return parser.JNextNumber(false);
                case '-':
                    return parser.JNextNumber(true);
                case 't':
                    return parser.IsStringMoveOn("rue") ? new JToken(JToken.TType.Boolean, true) : null;
                case 'f':
                    return parser.IsStringMoveOn("alse") ? new JToken(JToken.TType.Boolean, false) : null;
                case 'n':
                    return parser.IsStringMoveOn("ull") ? new JToken(JToken.TType.Null) : null;

                default:
                    return null;
            }
        }

        private string GenError(string err )
        {
            string s = "JSON Error " + err + " @ " + parser.Line.Substring(0, parser.Position) + " <ERROR> "
                            + parser.Line.Substring(parser.Position);
            System.Diagnostics.Debug.WriteLine(s);
            return s;
        }

    }
}



