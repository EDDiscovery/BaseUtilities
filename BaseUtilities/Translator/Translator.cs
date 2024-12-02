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
        public void TranslateControls(Control parent, Enum[] enumset, Control[] ignorelist = null, string[] toplevelnames = null, bool debugit = false)
        {
            System.Diagnostics.Debug.Assert(enumset != null);       // for now, disable ability. comment this out during development

            var elist = enumset == null ? null : enumset.Select(x => x.ToString()).ToList();

            if (toplevelnames == null)
                toplevelnames = new string[] { parent.GetType().Name };

            var errlist = Tx(parent, elist, toplevelnames, "", ignorelist, debugit);
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

        private string Tx(Control ctrl, List<string> enumset, string[] toplevelnames, string prefixid, Control[] ignorelist, bool debugit = false)
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
                        bool foundinenumset = false;
                        foreach (var tln in toplevelnames)
                        {
                            string id = tln + ((ctrl is GroupBox || ctrl is TabPage) ? (prefixid + "." + ctrl.Name) : prefixid);
                            string enumid = id.Replace(".", "_");

                            if (enumset.Contains(enumid))     // if found, use this
                            {
                                foundinenumset = true;

                                ctrl.Text = Translate(ctrl.Text, id);

                                if (debugit)
                                    System.Diagnostics.Debug.WriteLine($" {id} -> {ctrl.Text} ({GetOriginalFile(id)} {GetOriginalLine(id)})");

                                enumset.Remove(enumid);
                                break;
                            }

                        }

                        if ( !foundinenumset)
                        {
                            string id = toplevelnames[0] + ((ctrl is GroupBox || ctrl is TabPage) ? (prefixid + "." + ctrl.Name) : prefixid);
                            string enumid = id.Replace(".", "_");
                            errlist = errlist.AppendPrePad("EDTx." + enumid, ", ");
                        }
                    }
                }

                // if datagrid view, we need to deal with headers
                if (ctrl is DataGridView)
                {
                    DataGridView v = ctrl as DataGridView;
                    foreach (DataGridViewColumn col in v.Columns)
                    {
                        if (col.HeaderText != null && col.HeaderText.Length > 1 && col.HeaderText.HasLetterChars())
                        {
                            bool foundinenumset = false;
                            foreach (var tln in toplevelnames)
                            {
                                string idlong = tln + prefixid + "." + ctrl.Name + "." + col.Name;  // new dec 24 include ctrl name in id for multiple grids per control
                                string enumid = idlong.Replace(".", "_");

                                if (enumset.Contains(enumid))     // if found, use this
                                {
                                    foundinenumset = true;
                                    col.HeaderText = Translate(col.HeaderText, idlong);
                                    enumset.Remove(enumid);
                                    break;
                                }
                                else
                                {
                                    string idshort = tln + prefixid + "." + col.Name;  // new dec 24 short older version (EDTx.UserControlJournalGrid_ColumnTime)
                                    enumid = idshort.Replace(".", "_");

                                    if (enumset.Contains(enumid))     // if found, use this
                                    {
                                        foundinenumset = true;
                                        col.HeaderText = Translate(col.HeaderText, idshort);
                                        enumset.Remove(enumid);
                                        break;
                                    }
                                }
                            }

                            if (!foundinenumset)
                            {
                                string id = toplevelnames[0] + prefixid + "." + ctrl.Name + "." + col.Name;  // new dec 24 include ctrl name in id for multiple grids per control
                                string enumid = id.Replace(".", "_");
                                errlist = errlist.AppendPrePad("EDTx." + enumid, ", ");
                            }
                        }
                    }
                }

                // if tabpage, add on the page name
                if (ctrl is TabPage)
                    prefixid = prefixid + "." + ctrl.Name;

                // do sub controls
                foreach (Control c in ctrl.Controls)
                {
                    string nextid = prefixid;

                    if (InsertName(c))
                        nextid = nextid + "." + c.Name;

                    errlist = errlist.AppendPrePad(Tx(c, enumset, toplevelnames, nextid, ignorelist, debugit), ", ");
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
        public void TranslateTooltip(ToolTip tt, Enum[] enumset, Control parent, string[] toplevelnames = null, bool debugit = false)
        {
            System.Diagnostics.Debug.Assert(enumset != null);       // for now, disable ability. comment this out during development

            var elist = enumset.Select(x => x.ToString()).ToList();
            var elistremoved = new List<string>(elist); // either duplicate or empty

            if (toplevelnames == null)
                toplevelnames = new string[] { parent.GetType().Name };

            var errlist = Tx(tt, parent, elist, elistremoved, toplevelnames, "", debugit,0);

            if (errlist.HasChars())
            {
                System.Diagnostics.Debug.WriteLine($"        var enumlisttt = new Enum[] {{{errlist.WordWrap(160)}}};");
                System.Diagnostics.Debug.WriteLine($"{errlist.Split(",").Join(",\n").Replace("EDTx.", "    ")};");
                System.Diagnostics.Debug.Assert(false, "Tooltip has errors:" + errlist);
            }
            if (elistremoved.Count>0)
            {
                System.Diagnostics.Debug.WriteLine("Extra unused tooltip enums: " + string.Join(",",elistremoved));
                System.Diagnostics.Debug.Assert(false, "Tooltip Enum set contains extra Enums: " + string.Join(",", elistremoved));
            }
        }

        private string Tx(ToolTip tt, Control ctrl, List<string> enumset, List<string> enumsetremoved, string[] toplevelnames, string prefixid, bool debugit, int level)
        {
            string errlist = "";

            string defaulttooltiptext = tt.GetToolTip(ctrl);

            if (debugit)
                System.Diagnostics.Debug.WriteLine($"{new string(' ', level * 4)}Tooltip Control at {level} name {ctrl.Name} tooltext `{defaulttooltiptext}`");

            if (defaulttooltiptext != null && defaulttooltiptext.Length > 1 && defaulttooltiptext.HasLetterChars() && !defaulttooltiptext.StartsWith("<code"))
            {
                bool foundinenumset = false;
                foreach (var tln in toplevelnames)
                {
                    string id = tln + prefixid + ".ToolTip";
                    string enumid = id.Replace(".", "_");

                    if (enumset.Contains(enumid))     // if found, use this
                    {
                        foundinenumset = true;

                        string translate = Translate(defaulttooltiptext, id);
                        tt.SetToolTip(ctrl, translate);

                        if (debugit)
                            System.Diagnostics.Debug.WriteLine($"{new string(' ', level * 4)}Set tooltip to id {id} text `{translate}`");

                        enumsetremoved.Remove(enumid);
                        break;
                    }
                }

                if ( !foundinenumset )
                {
                    string id = toplevelnames[0] + prefixid + ".ToolTip";
                    string enumid = id.Replace(".", "_");
                    errlist = errlist.AppendPrePad("EDTx." + enumid, ", ");
                }
            }
            else
            {
                if ( debugit )
                    System.Diagnostics.Debug.WriteLine($"{new string(' ', level * 4)}No tooltip on control");
            }

            foreach (Control c in ctrl.Controls)
            {
                string nextid = prefixid;

                if (InsertName(c))      // containers don't send thru 
                    nextid = "." + c.Name;
                if (debugit)
                    System.Diagnostics.Debug.WriteLine($"{new string(' ', level*4)} -> into {c.Name} with id {nextid}");

                string res = Tx(tt, c, enumset, enumsetremoved, toplevelnames, nextid, debugit, level + 1);

                errlist = errlist.AppendPrePad(res, ", ");
            }

            return errlist;
        }

        // translate toolstrips.  Does not support %id%.  <code> is ignored.
        public void TranslateToolstrip(ToolStrip ctrl, Enum[] enumset, Control parent)
        {
            TranslateToolstrip(ctrl, enumset, new string[] { parent.GetType().Name });
        }

        [System.Diagnostics.DebuggerHidden]
        public void TranslateToolstrip(ToolStrip ctrl, Enum[] enumset, string[] toplevelnames = null)
        {
            System.Diagnostics.Debug.Assert(enumset != null);       // for now, disable ability. comment this out during development

            var elist = enumset.Select(x => x.ToString()).ToList();

            string errlist = "";

            foreach (ToolStripItem msi in ctrl.Items)
            {
                var errl = Tx(msi, elist, toplevelnames, "");
                errlist = errlist.AppendPrePad(errl, ", ");
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

        private string Tx(ToolStripItem msi, List<string> enumset, string[] toplevelnames, string prefixid)
        {
            string errlist = "";

            string itemname = msi.Name;

            if (msi.Text != null && msi.Text.Length > 1 && msi.Text.HasLetterChars() && !msi.Text.StartsWith("<code"))
            {
                bool foundinenumset = false;
                foreach ( var tln in toplevelnames)
                {
                    string id = tln + prefixid + "." + itemname;
                    string enumid = id.Replace(".", "_");

                    if ( enumset.Contains(enumid))
                    {
                        foundinenumset = true;
                        msi.Text = Translate(msi.Text, id);
                        enumset.Remove(enumid);
                        break;
                    }
                }

                if (!foundinenumset)
                {
                    string id = toplevelnames[0] + prefixid + "." + itemname;
                    string enumid = id.Replace(".", "_");
                    errlist = errlist.AppendPrePad("EDTx." + enumid, ", ");
                }
            }

            var ddi = msi as ToolStripDropDownItem;
            if (ddi != null)
            {
                foreach (ToolStripItem dd in ddi.DropDownItems)
                {
                    string nextid = prefixid + "." + itemname;
                    var erl = Tx(dd, enumset, toplevelnames, nextid);
                    errlist = errlist.AppendPrePad(erl, ", ");
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
