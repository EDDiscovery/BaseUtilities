/*
 * Copyright © 2020 EDDiscovery development team
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
using System.Globalization;
using System.IO;
using System.Linq;

public static class TranslatorExtensions
{
    static public string Tx(this string s)              // use the text, alpha numeric only, as the translation id
    {
        return BaseUtils.Translator.Instance.Translate(s, s.FirstAlphaNumericText());
    }

    static public string Tx(this string s, Object c, string id)     // use the type plus an id
    {
        return BaseUtils.Translator.Instance.Translate(s, c.GetType().Name, id);
    }

    static public string Txb(this string s, Object c, string id)     // use the base type plus an id
    {
        return BaseUtils.Translator.Instance.Translate(s, c.GetType().BaseType.Name, id);
    }

    static public string Tx(this string s, Type c)    // use a type definition using the string as the id
    {
        return BaseUtils.Translator.Instance.Translate(s, c.Name, s.FirstAlphaNumericText());
    }

    static public string Tx(this string s, Object c)              // use the object type with string as id
    {
        return BaseUtils.Translator.Instance.Translate(s, c.GetType().Name, s.FirstAlphaNumericText());
    }

    static public string Txb(this string s, Object c)     // use the base type using the string as the id
    {
        return BaseUtils.Translator.Instance.Translate(s, c.GetType().BaseType.Name, s.FirstAlphaNumericText());
    }

    static public string Tx(this string s, Type c, string id)    // use a type definition with id
    {
        return BaseUtils.Translator.Instance.Translate(s, c.Name, id);
    }

}

namespace BaseUtils
{
    // specials : if text in a control = <code> its presumed its a code filled in entry and not suitable for translation
    // in translator file, .Label means use the previous first word prefix stored, for shortness
    // using Label: "English" @ means for debug, replace @ with <english> as the foreign word in the debug build. In release, just use the in-code text

    public class Translator
    {
        static public Translator Instance { get
            {
                if (instance == null)
                    instance = new Translator();
                return instance; }
        }

        static Translator instance;

        public bool OutputIDs { get; set; } = false;             // for debugging

        protected LogToFile logger = null;

        protected Dictionary<string, string> translations = null;         // translation result can be null, which means, use the in-game english string
        protected Dictionary<string, string> originalenglish = null;      // optional load
        protected Dictionary<string, string> originalfile = null;         // optional load
        protected List<Type> ExcludedControls = new List<Type>();

        public IEnumerable<string> EnumerateKeys { get { return translations.Keys; } }

        public Translator() // only use via debugging
        {
        }

        public bool Translating { get { return translations != null; } }
        public bool IsDefined(string fullid) => translations != null && translations.ContainsKey(fullid);
        public string GetTranslation(string fullid) => translations[fullid];         // ensure its there first!
        public string GetOriginalEnglish(string fullid) => originalenglish[fullid];         // ensure its there first!
        public string GetOriginalFile (string fullid) => originalfile[fullid];         // ensure its there first!
        public void UnDefine(string fullid) { translations.Remove(fullid); }        // debug
        public bool IsExcludedControl(Type type) => ExcludedControls.Contains(type);
        // You can call this multiple times if required for debugging purposes

        public void LoadTranslation(string language, CultureInfo uicurrent, 
                                    string[] txfolders, int includesearchupdepth, 
                                    string logdir, 
                                    string includefolderreject = "\\bin",       // use to reject include files in specific locations - for debugging
                                    bool loadorgenglish = false,                // optional load original english and store
                                    bool loadfile = false                       // remember file where it came from
                                    )       
        {
#if DEBUG
            if (logger != null)
                logger.Dispose();

            logger = new LogToFile();
            logger.SetFile(logdir, "translator-ids.log", false);
#endif
            translations = null;        // forget any
            originalenglish = null;
            originalfile = null;

            List<Tuple<string, string>> languages = EnumerateLanguages(txfolders);

           //  uicurrent = CultureInfo.CreateSpecificCulture("it"); // debug

            Tuple<string,string> langsel = null;

            if (language == "Auto")
            {
                langsel = FindISOLanguage(languages, uicurrent.Name);

                if (langsel == null)
                    langsel = FindISOLanguage(languages, uicurrent.TwoLetterISOLanguageName);
            }
            else
            {
                langsel = languages.Find(x => Path.GetFileNameWithoutExtension(x.Item2).Equals(language, StringComparison.InvariantCultureIgnoreCase));
            }

            if (langsel == null)
                return;

            System.Diagnostics.Debug.WriteLine("Load Language " + langsel.Item2);
            logger?.WriteLine("Read " + langsel.Item2 + " from " + langsel.Item1);

            using (LineReader lr = new LineReader())
            {
                string tlffile = Path.Combine(langsel.Item1, langsel.Item2);

                if (lr.Open(tlffile))
                {
                    translations = new Dictionary<string, string>();
                    originalenglish = new Dictionary<string, string>();
                    originalfile = new Dictionary<string, string>();

                    string prefix = "";

                    string line = null;
                    while ((line = lr.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (line.StartsWith("Include", StringComparison.InvariantCultureIgnoreCase))
                        {
                            line = line.Mid(7).Trim();

                            DirectoryInfo di = new DirectoryInfo(Path.GetDirectoryName(tlffile));

                            string filename = null;

                            string fileinroot = Path.Combine(di.FullName, line);

                            if (File.Exists(fileinroot))   // first we prefer files in the same folder..
                                filename = fileinroot;
                            else
                            {
                                di = di.GetDirectoryAbove(includesearchupdepth);        // then search the tree, first jump up search depth amount

                                try
                                {
                                    FileInfo[] allFiles = Directory.EnumerateFiles(di.FullName, line, SearchOption.AllDirectories).Select(f => new FileInfo(f)).OrderBy(p => p.LastWriteTime).ToArray();

                                    if (allFiles.Length > 0)
                                    {
                                        var selected = allFiles.Where((x) => !x.DirectoryName.Contains(includefolderreject));       // reject folders with this pattern..files
                                        if (selected.Count() > 0)
                                            filename = selected.First().FullName;
                                    }
                                }
                                catch { }
                            }

                            if (filename == null || !lr.Open(filename))     // if no file found, or can't open..
                                logger?.WriteLine(string.Format("*** Cannot include {0}", line));
                            else
                                logger?.WriteLine("Read " + filename);
                        }
                        else if (line.Length > 0 && !line.StartsWith("//"))
                        {
                            StringParser s = new StringParser(line);

                            string id = s.NextWord(" :");

                            if ( id.Equals("SECTION"))
                            {
                                prefix = s.NextQuotedWord(" /");
                            }
                            else
                            { 
                                if (id.StartsWith(".") && prefix.HasChars())
                                    id = prefix + id;
                                else
                                    prefix = id.Word(new char[] { '.' });

                                if (s.IsCharMoveOn(':'))
                                {
                                    string orgenglish = s.NextQuotedWord(replaceescape: true);  // first is the original english version

                                    string foreign = null;
                                    bool err = false;

                                    if (s.IsStringMoveOn("=>"))
                                    {
                                        foreign = s.NextQuotedWord(replaceescape: true);
                                        err = foreign == null;
                                    }
                                    else if (s.IsCharMoveOn('@'))
                                        foreign = null;
                                    else
                                        err = false;

                                    if (err == true)
                                    {
                                        logger?.WriteLine(string.Format("*** Translator ID but no translation {0}", id));
                                        System.Diagnostics.Debug.WriteLine("*** Translator ID but no translation {0}", id);
                                    }
                                    else
                                    {
                                        if (!translations.ContainsKey(id))
                                        {
                                            //logger?.WriteLine(string.Format("New {0}: \"{1}\" => \"{2}\"", id, english, foreign));
                                            translations[id] = foreign;
                                            if (loadorgenglish)
                                                originalenglish[id] = orgenglish;
                                            if (loadfile)
                                                originalfile[id] = lr.CurrentFile;
                                        }
                                        else
                                        {
                                            logger?.WriteLine(string.Format("*** Translator Repeat {0}", id));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void AddExcludedControls(Type [] s)
        {
            ExcludedControls.AddRange(s);
        }

        static public List<Tuple<string, string>> EnumerateLanguages(string[] txfolders)        // return folder, language file, without repeats
        {
            List<Tuple<string, string>> languages = new List<Tuple<string, string>>();

            foreach (string folder in txfolders)
            {
                try
                {
                    FileInfo[] allFiles = Directory.EnumerateFiles(folder, "*.tlf", SearchOption.TopDirectoryOnly).Select(f => new FileInfo(f)).OrderBy(p => p.LastWriteTime).ToArray();
                    System.Diagnostics.Debug.WriteLine("TX Check folder " + folder);
                    foreach ( FileInfo f in allFiles)
                    {
#if !DEBUG
                        if ( f.Name.Contains("example-ex"))
                            continue;
#endif

                        if (languages.Find(x => x.Item2.Equals(f.Name)) == null)        // if not already found this language, add
                            languages.Add(new Tuple<string, string>(folder, f.Name));   // folder and name
                    }
                }
                catch { }
            }

            return languages;
        }

        static public List<string> EnumerateLanguageNames(string[] txfolders)
        {
            List<Tuple<string, string>> languages = BaseUtils.Translator.EnumerateLanguages(txfolders);
            return (from x in languages select Path.GetFileNameWithoutExtension(x.Item2)).ToList();
        }

        static public Tuple<string,string> FindISOLanguage(List<Tuple<string,string>> lang, string isoname)    // take filename part only, see if filename text-<iso> is present
        {
            return lang.Find(x =>
            {
                string filename = Path.GetFileNameWithoutExtension(x.Item2);
                int dash = filename.IndexOf('-');
                return dash != -1 && filename.Substring(dash + 1).Equals(isoname);
            });
        }

        public string Translate(string normal, string id)
        {
            if (translations != null && !normal.Equals("<code>") )
            {
                string key = id;
                if (OutputIDs)
                {
                    string tx = "ID lookup " + key + " Value " + (translations.ContainsKey(key) ? (translations[key]??"Null") : "Missing");
                    System.Diagnostics.Debug.WriteLine(tx);
                    logger?.WriteLine(tx);
                }

                if (translations.ContainsKey(key))
                {
#if DEBUG
                    return translations[key] ?? normal.QuoteFirstAlphaDigit();     // debug more we quote them to show its not translated, else in release we just print
#else
                    return translations[key] ?? normal;
#endif
                }
                else
                {
                    logger?.WriteLine(string.Format("{0}: {1} @", id, normal.EscapeControlChars().AlwaysQuoteString()));
                    normal = "! " + normal + " !";          // no id at all, use ! to indicate
                    translations.Add(key, normal);
                    //System.Diagnostics.Debug.WriteLine("*** Missing Translate ID: {0}: \"{1}\" => \"{2}\"", id, normal.EscapeControlChars(), "<" + normal.EscapeControlChars() + ">");
                    return normal;
                }
            }
            else
                return normal;
        }

        public string Translate(string normal, string root, string id)
        {
            return Translate(normal, root + "." + id);
        }

     }
}
