/*
 * Copyright © 2020-2024 EDDiscovery development team
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

public static class TranslatorExtensions
{
    static public string TxID(this string s, Enum id)               // given english text and enumeration, translate
    {
        return BaseUtils.Translator.Instance.Translate(s, id.ToString().Replace("_", "."));
    }
    static public bool TxDefined(Enum id)                           // is it defined?
    {
        return BaseUtils.Translator.Instance.IsDefined(id.ToString().Replace("_", "."));
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
        static public Translator Instance
        {
            get
            {
                if (instance == null)
                    instance = new Translator();
                return instance;
            }
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

        public bool CompareTranslatedToCode { get; set; } = false;      // if set, will moan if the translate does not match the code
        public bool Translating { get { return translations != null; } }

        public bool IsDefined(string fullid) => translations != null && translations.ContainsKey(fullid);
        public string GetTranslation(string fullid) => translations[fullid];         // ensure its there first!
        public string GetOriginalEnglish(string fullid) => originalenglish?[fullid] ?? "?";         // ensure its there first!
        public string GetOriginalFile(string fullid) => originalfile?[fullid] ?? "?";         // ensure its there first!
        public int GetOriginalLine(string fullid) => originalline?[fullid] ?? -1;         // ensure its there first!
        public void UnDefine(string fullid) { translations.Remove(fullid); }        // debug
        public void ReDefine(string fullid, string newdefine) { translations[fullid] = newdefine; }        // for edtools
        public string FindTranslation(string text) { var txlist = translations.ToList(); var txpos = txlist.FindIndex(x => x.Value == text); return txpos >= 0 ? txlist[txpos].Key : null; }
        public string FindTranslation(string text, string primaryfile, string currentfile, bool rootonlynamesprimary)
        {
            var txlist = translations.ToList();
            foreach (var kvp in txlist)
            {
                if (kvp.Value == text)
                {
                    if (currentfile != primaryfile && originalfile[kvp.Key] == primaryfile && (!rootonlynamesprimary || !kvp.Key.Contains(".")))
                        return kvp.Key;
                    else if (originalfile[kvp.Key] == currentfile)
                        return kvp.Key;
                }
            }
            return null;
        }

        public string[] FileList()
        {
            return originalfile.Values.Distinct().ToArray();
        }

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

            Tuple<string, string> langsel = null;

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
                    if (loadorgenglish)
                        originalenglish = new Dictionary<string, string>();
                    if (loadfile)
                    {
                        originalfile = new Dictionary<string, string>();
                        originalline = new Dictionary<string, int>();
                    }
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

                            if (id.Equals("SECTION", StringComparison.InvariantCultureIgnoreCase))
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
                                        if (err)
                                        {
                                            logger?.WriteLine(string.Format("*** Translator ID but no translation text {0}", id));
                                            System.Diagnostics.Debug.WriteLine("*** Translator ID but no translation text {0}", id);
                                        }
                                    }
                                    else if (s.IsCharMoveOn('@'))
                                        foreign = null;
                                    else if (s.IsCharMoveOn('='))
                                    {
                                        string keyword = s.NextWord();
                                        if (translations.ContainsKey(keyword))
                                        {
                                            foreign = translations[keyword];
                                        }
                                        else
                                        {
                                            logger?.WriteLine(string.Format("*** Translator ID with reference = but no reference found {0}", id));
                                            System.Diagnostics.Debug.WriteLine("*** Translator ID reference = but no reference found {0}", id);
                                            err = true;
                                        }

                                    }
                                    else
                                    {
                                        logger?.WriteLine(string.Format("*** Translator ID bad format {0}", id));
                                        System.Diagnostics.Debug.WriteLine("*** Translator ID bad format {0}", id);
                                        err = true;
                                    }

                                    if (err != true)
                                    {
                                        if (!translations.ContainsKey(id))
                                        {
                                            //                                            System.Diagnostics.Debug.WriteLine($"{lr.CurrentFile} {lr.CurrentLine} {id} => {orgenglish} => {foreign} ");
                                            if (debugout)
                                                logger?.WriteLine(string.Format("New {0}: \"{1}\" => \"{2}\"", id, orgenglish, foreign));

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
                                            string errt = $"Translator Repeat {lr.CurrentFile}:{lr.CurrentLine} '{id}";
                                            logger?.WriteLine(errt);
                                            System.Diagnostics.Debug.WriteLine(errt);
                                        }
                                    }
                                }
                                else
                                {
                                    string errt = $"Line misformat {lr.CurrentFile}:{lr.CurrentLine} '{line}";
                                    logger?.WriteLine(errt);
                                    System.Diagnostics.Debug.WriteLine(errt);
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

        public void AddExcludedControls(Type[] s)
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
                    foreach (FileInfo f in allFiles)
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

        static public Tuple<string, string> FindISOLanguage(List<Tuple<string, string>> lang, string isoname)    // take filename part only, see if filename text-<iso> is present
        {
            return lang.Find(x =>
            {
                string filename = Path.GetFileNameWithoutExtension(x.Item2);
                int dash = filename.IndexOf('-');
                return dash != -1 && filename.Substring(dash + 1).Equals(isoname);
            });
        }

        // Translate string. Normal is the english text, ID is the ID to look up.
        // if translator is off english is returned.
        // any <code> items are returned as is.

        public string Translate(string english, string id)
        {
            if ( id == "DataGridView.WordWrap")
            {

            }
            if (translations != null && !english.StartsWith("<code"))
            {
                string key = id;
                if (OutputIDs)
                {
                    string tx = "ID lookup " + key + " Value " + (translations.ContainsKey(key) ? (translations[key] ?? "Null") : "Missing");
                    System.Diagnostics.Debug.WriteLine(tx);
                    logger?.WriteLine(tx);
                }

                if (translations.ContainsKey(key))
                {
                    if (inuse != null)
                        inuse[key] = true;


                    if (CompareTranslatedToCode && originalenglish != null && originalenglish.ContainsKey(key) && originalenglish[key] != english)
                    {
                        var orgeng = originalenglish[key];
                        logger?.WriteLine($"Difference Key {key} code `{english}` translation `{orgeng}`");
                    }

#if DEBUG
                    return translations[key] ?? english.QuoteFirstAlphaDigit();     // debug more we quote them to show its not translated, else in release we just print
#else
                    return translations[key] ?? english;
#endif
                }
                else
                {
                    logger?.WriteLine($"{id}: {english.EscapeControlChars().AlwaysQuoteString()} @");
                    english = "! " + english + " !";          // no id at all, use ! to indicate
                    translations.Add(key, english);
                    System.Diagnostics.Trace.WriteLine($"*** Missing Translate ID: {id}: {english.EscapeControlChars().AlwaysQuoteString()} @" );
                    return english;
                }
            }
            else
                return english;
        }


        // translate controls, verify control name is in enumset.
        // all controls to translate must be in the enumset and enumset must have exactly that list. Else assert in debug
        // controls can be marked <code> to say don't translate, or use %id% to indicate to use an ID
        // We must go thru this procedure even if translations are off due to the embedded IDs such as %OK%

        [System.Diagnostics.DebuggerHidden]
        public void TranslateControls(Control ctrl, Enum[] enumset, Control[] ignorelist = null, string subname = null, bool debugit = false)
        {
            System.Diagnostics.Debug.Assert(enumset != null);       // for now, disable ability. comment this out during development

            var elist = enumset == null ? null : enumset.Select(x => x.ToString()).ToList();
            var errlist = Tx(ctrl, elist, subname != null ? subname : ctrl.GetType().Name, ignorelist, debugit);
            if (errlist.HasChars())
            {
                System.Diagnostics.Debug.WriteLine($"        var enumlist = new Enum[] {{{errlist.Replace(",", ", ").WordWrap(160)}}};");
                System.Diagnostics.Debug.WriteLine($"{errlist.Split(",").Join(",\n").Replace("EDTx.", "    ")};");
            }
            if (enumset != null)
            {
                System.Diagnostics.Debug.Assert(errlist.IsEmpty(), "Missing enumerations: " + errlist);
                System.Diagnostics.Debug.Assert(elist.Count == 0, "Enum set contains extra Enums: " + string.Join(",", elist));
            }
        }

        private string Tx(Control ctrl, List<string> enumset, string subname, Control[] ignorelist, bool debugit = false)
        {
            string errlist = "";

            if ((ignorelist == null || !ignorelist.Contains(ctrl)) && !ExcludedControls.Contains(ctrl.GetType()))
            {
                // if text is valid, not a single char, has letters, and not <code>, try

                if (ctrl.Text != null && ctrl.Text.Length > 1 && ctrl.Text.HasLetterChars() && !ctrl.Text.StartsWith("<code"))
                {
                    // if embedded id, try and translate the ID

                    if (ctrl.Text.StartsWith("%") && ctrl.Text.EndsWith("%"))
                    {
                        string id = ctrl.Text.Substring(1, ctrl.Text.Length - 2);
                        ctrl.Text = Translate(id, id);
                        if (debugit)
                            System.Diagnostics.Debug.WriteLine($" {id} -> {ctrl.Text} ({GetOriginalFile(id)} {GetOriginalLine(id)})");
                    }
                    else
                    {
                        // else make ID for control

                        string id = (ctrl is GroupBox || ctrl is TabPage) ? (subname + "." + ctrl.Name) : subname;

                        // make sure enumset has it, else add to errlist
                        string enumid = id.Replace(".", "_");
                        if (enumset == null || !enumset.Contains(enumid))
                            errlist = errlist.AppendPrePad("EDTx." + enumid, ", ");
                        else
                            enumset.Remove(enumid);

                        // translate
                        ctrl.Text = Translate(ctrl.Text, id);

                        if (debugit)
                            System.Diagnostics.Debug.WriteLine($" {id} -> {ctrl.Text} ({GetOriginalFile(id)} {GetOriginalLine(id)})");
                    }
                }

                // if datagrid view, we need to deal with headers
                if (ctrl is DataGridView)
                {
                    DataGridView v = ctrl as DataGridView;
                    foreach (DataGridViewColumn c in v.Columns)
                    {
                        if (c.HeaderText != null && c.HeaderText.Length > 1 && c.HeaderText.HasLetterChars())
                        {
                            string id = subname.AppendPrePad(c.Name, ".");

                            string enumid = id.Replace(".", "_");

                            if (enumset == null || !enumset.Contains(enumid))
                                errlist = errlist.AppendPrePad("EDTx." + enumid, ", ");
                            else
                                enumset.Remove(enumid);

                            c.HeaderText = Translate(c.HeaderText, id);
                        }
                    }
                }

                // if tabpage, add on the page name
                if (ctrl is TabPage)
                    subname = subname.AppendPrePad(ctrl.Name, ".");

                // do sub controls
                foreach (Control c in ctrl.Controls)
                {
                    string name = subname;

                    if (InsertName(c))
                        name = name.AppendPrePad(c.Name, ".");

                    errlist = errlist.AppendPrePad(Tx(c, enumset, name, ignorelist, debugit), ", ");
                }
            }
            else
            {
                //logger?.WriteLine("Rejected " + subname + " of " + ctrl.GetType().Name);
            }

            return errlist;
        }

        // translate tooltips.  Does not support %id%.  <code> is ignored.  Does check for non used enums now

        [System.Diagnostics.DebuggerHidden]
        public void TranslateTooltip(ToolTip tt, Enum[] enumset, Control parent, string subname = null, bool debugit = false)
        {
            System.Diagnostics.Debug.Assert(enumset != null);       // for now, disable ability. comment this out during development

            var elist = enumset == null ? null : enumset.Select(x => x.ToString()).ToList();
            var elistremoved = elist != null ? new List<string>(elist) : new List<string>(); // either duplicate or empty
            var errlist = Tx(tt, parent, elist, elistremoved, subname != null ? subname : parent.GetType().Name, debugit);

            if (errlist.HasChars())
            {
                System.Diagnostics.Debug.WriteLine($"        var enumlisttt = new Enum[] {{{errlist.WordWrap(160)}}};");
                System.Diagnostics.Debug.WriteLine($"{errlist.Split(",").Join(",\n").Replace("EDTx.", "    ")};");
            }
            if (elistremoved.Count>0)
            {
                System.Diagnostics.Debug.WriteLine("Extra unused tooltip enums: " + string.Join(",",elistremoved));
                System.Diagnostics.Debug.Assert(false, "Tooltip Enum set contains extra Enums: " + string.Join(",", elistremoved));
            }
        }

        private string Tx(ToolTip tt, Control ctrl, List<string> enumset, List<string> enumsetremoved, string subname, bool debugit)
        {
            string errlist = "";

            string s = tt.GetToolTip(ctrl);
            //System.Diagnostics.Debug.WriteLine($"Tooltip {ctrl.Name} = {s}");
            if (s != null && s.Length > 1 && s.HasLetterChars() && !s.StartsWith("<code"))
            {
                string id = subname.AppendPrePad("ToolTip", ".");

                string enumid = id.Replace(".", "_");

                // if we do have it, unlike controls, we can't just remove the ID from enumset, because some controls (Exttextbox, extcombobox) copy
                // down their tooltips to their subcontrols and they end up being present multiple times
                // we do however remove them from the enumsetremoved to keep count

                if (enumset == null || !enumset.Contains(enumid))
                    errlist = errlist.AppendPrePad("EDTx." + enumid, ", ");
                else
                    enumsetremoved.Remove(enumid);

                tt.SetToolTip(ctrl, Translate(s, id));

                if (debugit)
                    System.Diagnostics.Debug.WriteLine($" {id} -> {ctrl.Text} ({GetOriginalFile(id)} {GetOriginalLine(id)})");
            }

            foreach (Control c in ctrl.Controls)
            {
                string id = subname;

                if (InsertName(c))      // containers don't send thru 
                    id = id.AppendPrePad(c.Name, ".");

                errlist = errlist.AppendPrePad(Tx(tt, c, enumset, enumsetremoved, id, debugit), ", ");
            }

            return errlist;
        }

        // translate toolstrips.  Does not support %id%.  <code> is ignored.
        public void TranslateToolstrip(ToolStrip ctrl, Enum[] enumset, Control parent)
        {
            TranslateToolstrip(ctrl, enumset, parent.GetType().Name);
        }

        [System.Diagnostics.DebuggerHidden]
        public void TranslateToolstrip(ToolStrip ctrl, Enum[] enumset, string subname)
        {
            System.Diagnostics.Debug.Assert(enumset != null);       // for now, disable ability. comment this out during development

            var elist = enumset == null ? null : enumset.Select(x => x.ToString()).ToList();

            string errlist = "";

            foreach (ToolStripItem msi in ctrl.Items)
            {
                errlist = errlist.AppendPrePad(Tx(msi, elist, subname), ", ");
            }

            if (errlist.HasChars())
            {
                System.Diagnostics.Debug.WriteLine($"        var enumlistcms = new Enum[] {{{errlist.WordWrap(160)}}};");
                System.Diagnostics.Debug.WriteLine($"{errlist.Split(",").Join("\n,").Replace("EDTx.", "    ")};");
            }

            if (enumset != null)
            {
                System.Diagnostics.Debug.Assert(errlist.IsEmpty(), "Missing enumerations: " + errlist.WordWrap(80));
                System.Diagnostics.Debug.Assert(elist.Count == 0, "Enum set contains extra Enums: " + string.Join(",", elist));
            }
        }

        private string Tx(ToolStripItem msi, List<string> enumset, string subname)
        {
            string errlist = "";

            string itemname = msi.Name;

            if (msi.Text != null && msi.Text.Length > 1 && msi.Text.HasLetterChars() && !msi.Text.StartsWith("<code"))
            {
                string id = subname.AppendPrePad(itemname, ".");

                string enumid = id.Replace(".", "_");

                if (enumset == null || !enumset.Contains(enumid))
                    errlist = errlist.AppendPrePad("EDTx." + enumid, ", ");
                else
                    enumset.Remove(enumid);

                msi.Text = Translate(msi.Text, id);
            }

            var ddi = msi as ToolStripDropDownItem;
            if (ddi != null)
            {
                foreach (ToolStripItem dd in ddi.DropDownItems)
                {
                    errlist = errlist.AppendPrePad(Tx(dd, enumset, subname.AppendPrePad(itemname, ".")), ", ");
                }
            }

            return errlist;
        }

        // helper
        private bool InsertName(Control c)
        {
            return c.GetType().Name == "PanelNoTheme" || !(c is Panel || c is DataGridView || c is GroupBox || c is SplitContainer);
        }


    }
}
