/*
 * Copyright 2025-2025 EDDiscovery development team
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
using System.Text;
using System.Windows.Forms;

public static class TranslatorExtensionsMkII
{
    static public string Tx(this string s)               // given english text translate. Works even if translator has not been initialised
    {
        return BaseUtils.TranslatorMkII.Instance.Translate(s);
    }
    static public string Tx(this string s, bool translate)              
    {
        return translate ? BaseUtils.TranslatorMkII.Instance.Translate(s) : s;
    }
}

public interface ITranslatableControl
{
    bool TranslateDoChildren { get; }
}


namespace BaseUtils
{
    public class TranslatorMkII
    {
        static public TranslatorMkII Instance
        {
            get
            {
                if (instance == null)
                    instance = new TranslatorMkII();
                return instance;
            }
        }

        // translations are loaded
        public bool Translating { get { return translations != null; } }

        // Debugging
        public bool OutputIDs { get; set; } = false;            // Output IDs to log
        public bool CompareTranslatedToCode { get; set; } = false;      // if set, will moan if the translate does not match the code

        // key list
        public IEnumerable<string> EnumerateKeys { get { return translations.Keys; } }
        // must have stored english in load
        public IEnumerable<string> EnumerateEnglish { get { return originalenglish.Values; } }

        public bool IsDefined(string id) => translations != null && translations.ContainsKey(id);

        // recorded in translator list if store source info is on during load
        static public bool IsSourceID(string id) { return id.StartsWith("SOURCE:"); }

        // true if translation is defined, returns translation text (may be null)
        public bool TryGetValue(string id, out string text)
        {
            text = null;
            return translations != null && translations.TryGetValue(id, out text);
        }

        // requires load to have originalenglish
        public bool TryGetOriginalEnglish(string id, out string text)
        {
            text = null;
            return originalenglish != null ? originalenglish.TryGetValue(id, out text) : false;
        }
        public bool IsDefinedEnglish(string english)
        {
            return originalenglish.Values.Contains(english);
        }

        // requires load to have originalsource
        public bool TryGetSource(string id, out string file, out int lineno)
        {
            file = null;
            lineno = 0;
            return originalfile != null ? originalfile.TryGetValue(id, out file) && originalline.TryGetValue(id, out lineno) : false;
        }
        public string[] SourceFileList()
        {
            return originalfile?.Values.Distinct().ToArray() ?? null;
        }

        public void UnDefine(string id) { translations.Remove(id); originalenglish?.Remove(id); originalfile?.Remove(id); originalline?.Remove(id); }
        public void ReDefine(string id, string newdefine) { translations[id] = newdefine; }        // for edtools

        public TranslatorMkII() // only use via debugging
        {
        }

        // You can call this multiple times if required for debugging purposes
        public bool LoadTranslation(string language,
                                    CultureInfo uicurrent,
                                    string[] txfolders,
                                    int includesearchupdepth,
                                    string logdir = null,       // if non null, logger is active
                                    string logfile = null,      // optionally set logfile
                                    bool storeenglish = false,
                                    bool storesourceinfo = false,
                                    string includefolderreject = "\\bin"       // use to reject include files in specific locations - for debugging
                                    )
        {
            if (logdir != null)
            {
                if (logger != null)
                    logger.Dispose();

                logger = new LogToFile();
                logger.SetFile(logdir, logfile ?? $"translator-ids-{language}.log", false);
            }

            translations = null;        // forget any
            originalenglish = null;
            originalfile = null;
            originalline = null;

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

            string tlfile = Path.Combine(langsel.Item1, langsel.Item2);

            // tlx faster load

            if (Path.GetExtension(tlfile).Equals(".tlx", StringComparison.InvariantCultureIgnoreCase))
            {
                return ReadFromFile(tlfile);
            }

            using (LineReader lr = new LineReader())
            {
                if (lr.Open(tlfile))
                {
                    translations = new Dictionary<string, string>();

                    if (storesourceinfo)           // source info needs all of it
                    {
                        originalfile = new Dictionary<string, string>();
                        originalline = new Dictionary<string, int>();
                    }

                    if (storeenglish)
                    {
                        originalenglish = new Dictionary<string, string>();
                    }

                    int commentblankcount = 1;

                    string line = null;
                    while ((line = lr.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (line.Length == 0 || line.StartsWith("//") || line.StartsWith("SECTION ", StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (storesourceinfo)
                            {
                                string id = "SOURCE:" + commentblankcount++;      // record the information
                                translations[id] = line;
                                originalfile[id] = lr.CurrentFile;
                                originalline[id] = lr.CurrentLine;
                            }
                        }
                        else if (line.StartsWith("Include", StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (storesourceinfo)                            // we store these as comments so they can be spitted back out by translatenormalise
                            {
                                string id = "SOURCE:" + commentblankcount++;
                                translations[id] = line;
                                originalfile[id] = lr.CurrentFile;
                                originalline[id] = lr.CurrentLine;
                            }

                            line = line.Mid(7).Trim();

                            DirectoryInfo di = new DirectoryInfo(Path.GetDirectoryName(tlfile));

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
                        else
                        {
                            StringParser s = new StringParser(line);
                            string id = s.NextWord(" :");

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
                                {
                                    foreign = null;
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
                                        // System.Diagnostics.Debug.WriteLine($"{lr.CurrentFile} {lr.CurrentLine} {id} => `{orgenglish}` => {foreign} ");

                                        if (logger != null)
                                            logger?.WriteLine(string.Format("New {0}: \"{1}\" => \"{2}\"", id, orgenglish, foreign));

                                        translations[id] = foreign;

                                        if (storeenglish)
                                            originalenglish[id] = orgenglish;

                                        if (storesourceinfo)
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

                    return true;
                }
                else
                    return false;
            }
        }

        // write keys to a file - much faster to load than normal version
        public bool WriteToFile(string filename)
        {
            StringBuilder sp = new StringBuilder(200000);
            foreach (var v in translations)
            {
                sp.Append(v.Key);
                sp.Append(":");
                if (v.Value != null)
                    sp.Append(v.Value);
                else
                { }
                sp.Append('\0');
            }

            return FileHelpers.TryWriteToFile(filename, sp.ToString());
        }

        // faster load
        public bool ReadFromFile(string filename)
        {
            translations = new Dictionary<string, string>();
            string st = FileHelpers.TryReadAllTextFromFile(filename);
            if (st != null)
            {
                StringParser sp = new StringParser(st);
                while (!sp.IsEOL)
                {
                    string key = sp.NextWord(':');
                    sp.MoveOn(1);
                    string value = sp.NextWord('\0');
                    sp.MoveOn(1);
                    if (value.Length > 0)
                        translations[key] = value;
                    else
                        translations[key] = null;
                }
                return true;
            }
            return false;
        }

        static public List<Tuple<string, string>> EnumerateLanguages(string[] txfolders)        // return folder, language file, without repeats
        {
            List<Tuple<string, string>> languages = new List<Tuple<string, string>>();

            foreach (string folder in txfolders)
            {
                try
                {
                    List<FileInfo> allFiles = Directory.EnumerateFiles(folder, "*.tlf", SearchOption.TopDirectoryOnly).Select(f => new FileInfo(f)).OrderBy(p => p.LastWriteTime).ToList();
                    allFiles.AddRange(Directory.EnumerateFiles(folder, "*.tlx", SearchOption.TopDirectoryOnly).Select(f => new FileInfo(f)).OrderBy(p => p.LastWriteTime).ToList());
                    System.Diagnostics.Debug.WriteLine("Translator Check folder " + folder);
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
            List<Tuple<string, string>> languages = BaseUtils.TranslatorMkII.EnumerateLanguages(txfolders);
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

        static public string SHAKey(string english)
        {
            return english.CalcSha8();
        }

        // Translate string. Normal is the english text, ID is the ID to look up.
        // if translator is off english is returned.
        // any <code> items are returned as is.

        public string Translate(string english)
        {
            if (translations != null && !english.StartsWith("<code"))
            {
                string key = english.CalcSha8();
                if (OutputIDs)
                {
                    string tx = "ID lookup " + key + " Value " + (translations.ContainsKey(key) ? (translations[key] ?? "Null") : "Missing");
                    logger?.WriteLine(tx);
                }

                if (!translations.ContainsKey(key))     // if we don't have the key, try the full version, in case we ever have a clash
                {
                    key = english.CalcSha();
                }

                if (translations.ContainsKey(key))
                {
                    if (CompareTranslatedToCode && originalenglish != null && originalenglish.ContainsKey(key) && originalenglish[key] != english)
                    {
                        logger?.WriteLine($"Difference Key {key} code `{english}` translation file had `{originalenglish[key]}`");
                    }

#if DEBUG
                    return translations[key] ?? english.QuoteFirstAlphaDigit();     // debug more we quote them to show its not translated, else in release we just print
#else
                    return translations[key] ?? english;
#endif
                }
                else
                {
                    logger?.WriteLine($"{key}: {english.EscapeControlChars().AlwaysQuoteString()} @");
                    System.Diagnostics.Trace.WriteLine($"*** MKII Missing Translate ID:\r\n{english.CalcSha8()}: {english.EscapeControlChars().AlwaysQuoteString()} @");
                    string errtext = "! " + english + " !";          // no id at all, use ! to indicate
                    translations[key] = errtext;
                    return errtext;
                }
            }
            else
                return english;
        }

        // translate controls, verify control name is in enumset.
        // controls can be marked <code to say don't translate

        //[System.Diagnostics.DebuggerHidden]
        public void TranslateControls(Control ctrl, int minlen = 2, bool traceit = false)
        {
            bool translatable = ctrl is ITranslatableControl || ctrl is Label || ctrl is TabPage;

            if (translatable)      // these are translatable
            {
                // ignore if there is nothing to translate or starts with <code

                if (ctrl.Text?.Length >= minlen && ctrl.Text.HasLetterChars() && !ctrl.Text.StartsWith("<code"))
                {
                    string txtext = Translate(ctrl.Text);
                    if (traceit) System.Diagnostics.Debug.WriteLine($"TX `{ctrl.Text}` -> `{txtext}`");

                    ctrl.Text = txtext;
                }
            }


            // if datagrid view, we need to deal with headers
            if (ctrl is DataGridView)
            {
                DataGridView v = ctrl as DataGridView;
                foreach (DataGridViewColumn col in v.Columns)
                {
                    if (col.HeaderText != null && col.HeaderText.Length >= minlen && col.HeaderText.HasLetterChars())
                    {
                        string txtext = Translate(col.HeaderText);

                        col.HeaderText = txtext;
                    }
                }
            }

            // splitterpanel is a panel
            bool dochildren = ctrl is ITranslatableControl tc ? tc.TranslateDoChildren : (ctrl is Form || ctrl is UserControl || ctrl is Panel || ctrl is SplitContainer);

            if (traceit) System.Diagnostics.Debug.WriteLine($"Tx {ctrl.Name} {ctrl.GetType().Name}  translateable {translatable} dochildren {dochildren}");

            if (dochildren)
            {
                // do sub controls
                foreach (Control c in ctrl.Controls)
                {
                    TranslateControls(c, minlen, traceit);
                }
            }
        }

        //[System.Diagnostics.DebuggerHidden]
        public void TranslateTooltip(ToolTip tt, Control ctrl)
        {
            string defaulttooltiptext = tt.GetToolTip(ctrl);

            if (defaulttooltiptext != null && defaulttooltiptext.Length > 1 && defaulttooltiptext.HasLetterChars() && !defaulttooltiptext.StartsWith("<code"))
            {
                string txtext = Translate(defaulttooltiptext);
                tt.SetToolTip(ctrl, txtext);
            }

            foreach (Control c in ctrl.Controls)
            {
                TranslateTooltip(tt, c);
            }
        }

        //[System.Diagnostics.DebuggerHidden]
        public void TranslateToolstrip(ToolStrip ctrl)
        {
            foreach (ToolStripItem msi in ctrl.Items)
            {
                TranslateToolstrip(msi);
            }
        }

        private void TranslateToolstrip(ToolStripItem msi)
        {
            string itemname = msi.Name;

            if (msi.Text != null && msi.Text.Length > 1 && msi.Text.HasLetterChars() && !msi.Text.StartsWith("<code"))
            {
                string txtext = Translate(msi.Text);
                msi.Text = txtext;
            }

            var ddi = msi as ToolStripDropDownItem;

            if (ddi != null)
            {
                foreach (ToolStripItem dd in ddi.DropDownItems)
                {
                    TranslateToolstrip(dd);
                }
            }
        }


        private LogToFile logger = null;

        private Dictionary<string, string> translations = null;         // translation id -> translation. Translation result can be null, which means, use the in-game english string
        private Dictionary<string, string> originalenglish = null;      // optional load - translation id -> english
        private Dictionary<string, string> originalfile = null;         // optional load - translation id -> file
        private Dictionary<string, int> originalline = null;            // optional load - translation id -> line
        private static TranslatorMkII instance;
    }
}
