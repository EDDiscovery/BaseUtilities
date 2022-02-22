/*
 * Copyright © 2020-2021 EDDiscovery development team
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
using System.Windows.Forms;

public static class TranslatorExtensions
{
    static public string TxID(this string s, Enum id)               // given english text and enumeration, translate
    {
        return BaseUtils.Translator.Instance.Translate(s, id.ToString().Replace("_","."));
    }

    static public string TxID(this string s, Type type, string id)    // given english text, type for base name, and id for rest, translate
    {
        return BaseUtils.Translator.Instance.Translate(s, type.Name + "." + id);
    }

    static public string TxID(this string s, string name, string id)    // given english text, string for base name, and id for rest, translate
    {
        return BaseUtils.Translator.Instance.Translate(s, name + "." + id);
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

        private LogToFile logger = null;

        private Dictionary<string, string> translations = null;         // translation result can be null, which means, use the in-game english string
        private Dictionary<string, string> originalenglish = null;      // optional load
        private Dictionary<string, string> originalfile = null;         // optional load
        private Dictionary<string, int> originalline = null;            // optional load
        private Dictionary<string, bool> inuse = null;                  // optional load
        private List<Type> ExcludedControls = new List<Type>();

        public IEnumerable<string> EnumerateKeys { get { return translations.Keys; } }

        public Translator() // only use via debugging
        {
        }

        public bool Translating { get { return translations != null; } }

        public bool IsDefined(string fullid) => translations != null && translations.ContainsKey(fullid);
        public string GetTranslation(string fullid) => translations[fullid];         // ensure its there first!
        public string GetOriginalEnglish(string fullid) => originalenglish[fullid];         // ensure its there first!
        public string GetOriginalFile(string fullid) => originalfile[fullid];         // ensure its there first!
        public int GetOriginalLine(string fullid) => originalline[fullid];         // ensure its there first!
        public void UnDefine(string fullid) { translations.Remove(fullid); }        // debug
        public List<string> NotUsed()                                               // if track use on, whats not used
        {
            if (inuse != null)
            {
                List<string> res = new List<string>();
                foreach (var kvp in translations)
                {
                    if (!inuse.ContainsKey(kvp.Key))
                        res.Add(kvp.Key);
                }
                return res;
            }
            else
                return null;
        }


        // You can call this multiple times if required for debugging purposes
        public bool LoadTranslation(string language, CultureInfo uicurrent, 
                                    string[] txfolders, int includesearchupdepth, 
                                    string logdir, 
                                    string includefolderreject = "\\bin",       // use to reject include files in specific locations - for debugging
                                    bool loadorgenglish = false,                // optional load original english and store
                                    bool loadfile = false,                      // remember file where it came from
                                    bool trackinuse = false,                    // mark if in use
                                    bool debugout = false                       // list translators in log file
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
            originalline = null;
            inuse = null;

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
                return false;

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
                    originalline = new Dictionary<string, int>();
                    if (trackinuse)
                        inuse = new Dictionary<string, bool>();

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
                            {
                                System.Diagnostics.Debug.WriteLine("*** Cannot include " + line);
                                logger?.WriteLine(string.Format("*** Cannot include {0}", line));
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("Reading file " + filename);
                                logger?.WriteLine("Readingfile " + filename);
                            }
                        }
                        else if (line.Length > 0 && !line.StartsWith("//"))
                        {
                            StringParser s = new StringParser(line);

                            string id = s.NextWord(" :");

                            if (id.Equals("SECTION",StringComparison.InvariantCultureIgnoreCase))
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
                                        err = true;

                                    if (err == true)
                                    {
                                        logger?.WriteLine(string.Format("*** Translator ID but no translation {0}", id));
                                        System.Diagnostics.Debug.WriteLine("*** Translator ID but no translation {0}", id);
                                    }
                                    else
                                    {
                                        if (!translations.ContainsKey(id))
                                        {
                                            //                                            System.Diagnostics.Debug.WriteLine($"{lr.CurrentFile} {lr.CurrentLine} {id} => {orgenglish} => {foreign} ");

                                            if ( debugout )
                                                logger?.WriteLine(string.Format("New {0}: \"{1}\" => \"{2}\"", id , orgenglish, foreign));

                                            translations[id] = foreign;
                                            if (loadorgenglish)
                                                originalenglish[id] = orgenglish;
                                            if (loadfile)
                                            {
                                                originalfile[id] = lr.CurrentFile;
                                                originalline[id] = lr.CurrentLine;
                                            }
                                        }
                                        else
                                        {
                                            logger?.WriteLine(string.Format("*** Translator Repeat {0}", id));
                                            System.Diagnostics.Debug.WriteLine(string.Format("*** Translator Repeat {0}", id));
                                        }
                                    }
                                }
                            }
                        }
                    }

                    return true;
                }
                else
                    return false;
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
                    if (inuse != null)
                        inuse[key] = true;
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

        // translate controls, verify control name is in enumset.

        public void TranslateVerify(Control ctrl, Type enumset, Control[] ignorelist = null, string subname = null, bool debugit = false)
        {
            bool okay = TranslateVerify(ctrl, enumset, subname != null ? subname : ctrl.GetType().Name, ignorelist, debugit);
            System.Diagnostics.Debug.Assert(okay, "Missing control enumerations");
        }

        // Call direct only for debugging, normally use the one above.
        // controls can use %id% in their text for redirection
        private bool TranslateVerify(Control ctrl, Type enumset, string subname, Control[] ignorelist, bool debugit = false)
        {
            bool okay = true;

            if (translations != null)
            {
                if ((ignorelist == null || !ignorelist.Contains(ctrl)) && !ExcludedControls.Contains(ctrl.GetType()))
                {
                    if (ctrl.Text != null && ctrl.Text.Length>1 && ctrl.Text.HasLetterChars() && ctrl.Text != "<code>")
                    {
                        if (ctrl.Text.StartsWith("%") && ctrl.Text.EndsWith("%"))
                        {
                            string id = ctrl.Text.Substring(1, ctrl.Text.Length - 2);
                            ctrl.Text = Translate(id,id);
                            if (debugit)
                                System.Diagnostics.Debug.WriteLine($" {id} -> {ctrl.Text}");
                        }
                        else
                        {
                            string id = (ctrl is GroupBox || ctrl is TabPage) ? (subname + "." + ctrl.Name) : subname;

                            if (!Enum.IsDefined(enumset, id.Replace(".", "_")))
                            {
                                System.Diagnostics.Debug.WriteLine($"    {id.Replace(".", "_")}, // Control '{ctrl.Text.EscapeControlChars()}'");
                                okay = false;
                            }

                            ctrl.Text = Translate(ctrl.Text, id);

                            if (debugit)
                                System.Diagnostics.Debug.WriteLine($" {id} -> {ctrl.Text}");
                        }
                    }

                    if (ctrl is DataGridView)
                    {
                        DataGridView v = ctrl as DataGridView;
                        foreach (DataGridViewColumn c in v.Columns)
                        {
                            if (c.HeaderText != null && c.HeaderText.Length>1 && c.HeaderText.HasLetterChars())
                            {
                                string id = subname.AppendPrePad(c.Name, ".");
                                if (!Enum.IsDefined(enumset, id.Replace(".", "_")))
                                {
                                    System.Diagnostics.Debug.WriteLine($"    {id.Replace(".", "_")}, // Column Header '{c.HeaderText.EscapeControlChars()}'");
                                    okay = false;
                                }

                                c.HeaderText = Translate(c.HeaderText, id );
                            }
                        }
                    }

                    if (ctrl is TabPage)
                        subname = subname.AppendPrePad(ctrl.Name, ".");

                    foreach (Control c in ctrl.Controls)
                    {
                        string name = subname;

                        if (NameControl(c))
                            name = name.AppendPrePad(c.Name, ".");

                        bool subokay = TranslateVerify(c, enumset, name, ignorelist, debugit);
                        okay = okay && subokay;
                    }
                }
                else
                {
                    //logger?.WriteLine("Rejected " + subname + " of " + ctrl.GetType().Name);
                }
            }

            return okay;
        }

        public void TranslateVerify(ToolStrip ctrl, Type enumset, Control parent)
        {
            if (translations != null)
            {
                string subname = parent.GetType().Name;

                bool okay = true;

                foreach (ToolStripItem msi in ctrl.Items)
                {
                    bool subokay = TranslateVerify(msi, enumset, subname);
                    okay = okay && subokay;
                }

                System.Diagnostics.Debug.Assert(okay, "Missing tooltip enumerations");
            }
        }

        private bool TranslateVerify(ToolStripItem msi, Type enumset, string subname)
        {
            bool okay = true;

            string itemname = msi.Name;

            if (msi.Text != null && msi.Text.Length > 1 && msi.Text.HasLetterChars() && msi.Text != "<code>")
            {
                string id = subname.AppendPrePad(itemname, ".");
                if (!Enum.IsDefined(enumset, id.Replace(".", "_")))
                {
                    System.Diagnostics.Debug.WriteLine($"    {id.Replace(".", "_")}, // ToolStrip control '{msi.Text.EscapeControlChars()}'");
                    okay = false;
                }

                msi.Text = Translate(msi.Text, id);
            }

            var ddi = msi as ToolStripDropDownItem;
            if (ddi != null)
            {
                foreach (ToolStripItem dd in ddi.DropDownItems)
                {
                    bool subokay = TranslateVerify(dd, enumset, subname.AppendPrePad(itemname, "."));
                    okay = okay && subokay;
                }
            }

            return okay;
        }


        public void TranslateVerify(ToolTip tt, Type enumset, Control parent, string subname = null)
        {
            bool okay = TranslateVerify(tt, parent, enumset, subname!=null ? subname : parent.GetType().Name);
            System.Diagnostics.Debug.Assert(okay, "Missing toolstrip enumerations");
        }

        private bool TranslateVerify(ToolTip tt, Control ctrl, Type enumset, string subname)
        {
            bool okay = true;

            if (translations != null)
            {
                string s = tt.GetToolTip(ctrl);
                if (s != null && s.Length > 1 && s.HasLetterChars() && s != "<code>")
                {
                    string id = subname.AppendPrePad("ToolTip", ".");

                    if (!Enum.IsDefined(enumset, id.Replace(".", "_")))
                    {
                        System.Diagnostics.Debug.WriteLine($"    {id.Replace(".", "_")}, // ToolTip '{s.EscapeControlChars()}'");
                        okay = false;
                    }

                    tt.SetToolTip(ctrl, Translate(s, id));
                }

                foreach (Control c in ctrl.Controls)
                {
                    string id = subname;

                    if (NameControl(c))      // containers don't send thru 
                        id = id.AppendPrePad(c.Name, ".");

                    bool subokay = TranslateVerify(tt, c, enumset, id);
                    okay = okay && subokay;
                }
            }

            return okay;
        }

        private bool NameControl(Control c)
        {
            return c.GetType().Name == "PanelNoTheme" || !(c is Panel || c is DataGridView || c is GroupBox || c is SplitContainer);
        }

    }
}
