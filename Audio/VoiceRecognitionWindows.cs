/*
 * Copyright 2017-2022 EDDiscovery development team
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
using BaseUtils;

namespace AudioExtensions
{
#if !NO_SYSTEM_SPEECH
    using System.Speech.Recognition;

    public class VoiceRecognitionWindows : IVoiceRecognition
    {
        public float Confidence { get; set; } = 0.96F;

        // WARNING Engine must be started, and they may except if out of range.

        public event SpeechRecognised SpeechRecognised;
        public event SpeechRecognised SpeechNotRecognised;

        public bool IsOpen { get { return recognizer != null; } }

        private SpeechRecognitionEngine recognizer;
        private System.Globalization.CultureInfo ct;
        private bool completed;
        private bool winform;
        private Dictionary<string, List<Grammar>> currentgrammar = new Dictionary<string, List<Grammar>>();
        private Dictionary<string, List<Grammar>> updategrammar;
        private bool updatinggrammar = false;

        public bool Open(System.Globalization.CultureInfo ctp, bool winform)
        {
            Close();

            this.winform = winform;
            ct = ctp;

            try
            {
                recognizer = new SpeechRecognitionEngine(ct);       // may except if ct is not there on machine
                recognizer.SpeechHypothesized += Engine_SpeechHypothesized;
                recognizer.SpeechRecognized += Engine_SpeechRecognized;
                recognizer.SpeechRecognitionRejected += Engine_SpeechRecognitionRejected;
                recognizer.RecognizerUpdateReached += Recognizer_RecognizerUpdateReached;
                recognizer.RecognizeCompleted += Engine_RecognizeCompleted;
                //recognizer.RecognizerUpdateReached += (s, a) => { System.Diagnostics.Debug.WriteLine("Rec update"); };
                //recognizer.EmulateRecognizeCompleted += (s, a) => { System.Diagnostics.Debug.WriteLine("Emu Rec update"); };
                //recognizer.AudioStateChanged += (s, a) => { System.Diagnostics.Debug.WriteLine($"Audio state {a.AudioState}"); };
                recognizer.MaxAlternates = 2;
                recognizer.SetInputToDefaultAudioDevice(); // crashes if no default device..

                GrammarBuilder builder = new GrammarBuilder();      // must have loaded 1 grammar otherwise we can't start, so we keep this dummy one in at all times
                builder.Append("XXXXXXXXXXX");
                builder.Culture = ct;
                Grammar gr = new Grammar(builder);
                gr.Name = "**Default";
                recognizer.LoadGrammar(gr);

                recognizer.RecognizeAsync(RecognizeMode.Multiple);        // got a grammar, start..

                System.Diagnostics.Debug.WriteLine($"{AppTicks.MSd} VR Open");
            }
            catch
            {
                recognizer = null;
                return false;
            }

            //System.Diagnostics.Debug.WriteLine("Engine {0}", engine.RecognizerInfo.Description);
            //foreach (var x in engine.RecognizerInfo.AdditionalInfo)
            //    System.Diagnostics.Debug.WriteLine(".. " + x.Key + "=" + x.Value);
            return true;
        }

        public void Close()
        {
            if (recognizer != null)
            {
                Stop();

                recognizer.SpeechHypothesized -= Engine_SpeechHypothesized;
                recognizer.SpeechRecognized -= Engine_SpeechRecognized;
                recognizer.SpeechRecognitionRejected -= Engine_SpeechRecognitionRejected;
                recognizer.RecognizeCompleted -= Engine_RecognizeCompleted;
                recognizer.Dispose();
                recognizer = null;

                currentgrammar = new Dictionary<string, List<Grammar>>();  // must clear the current list in case reopens!
            }
        }

        private void Stop()
        {
            completed = false;

            System.Diagnostics.Debug.WriteLine($"{AppTicks.MSd} VR Recognition Stopping with {recognizer?.Grammars.Count}");

            recognizer.RecognizeAsyncCancel();

            while (!completed)
            {
                if (winform)        // this one cost about three hours - callbacks must be being done on a winform thread, they won't happen if you just block
                    System.Windows.Forms.Application.DoEvents();
                System.Threading.Thread.Sleep(10);
            }

            System.Diagnostics.Debug.WriteLine($"{AppTicks.MSd} VR Stopped");
        }

        // begin update grammar
        public void BeginGrammarUpdate()
        {
            while (updatinggrammar)       // recogniser update is async - so we may be stuck waiting for it. We can't double update, we need to pause
            {
                if (winform)
                    System.Windows.Forms.Application.DoEvents();
                System.Threading.Thread.Sleep(10);
            }

            updatinggrammar = true;

            updategrammar = new Dictionary<string, List<Grammar>>();
        }

        public void AddGrammar(string s)
        {
            System.Diagnostics.Debug.Assert(updatinggrammar == true);

            if (!currentgrammar.ContainsKey(s))
            {
                var grammar = new List<Grammar>();

#if true
                BaseUtils.StringCombinations sb = new BaseUtils.StringCombinations();

                foreach (List<List<string>> groups in sb.ParseGroup(s))     // for each semicolon group, give me the word combinations
                {
                    GrammarBuilder builder = new GrammarBuilder();

                    string gname = "";

                    foreach (List<string> wordlist in groups)     // for each vertical word list
                    {
                        if (wordlist.Count == 1)      // single entry, must be simple, just add
                        {
                            gname = gname.AppendPrePad(wordlist[0], " ");
                            builder.Append(wordlist[0]);
                            //    System.Diagnostics.Debug.Write("Append " + wordlist[0]  + ", ");
                        }
                        else
                        {                             // conditional list..
                            List<GrammarBuilder> sub = new List<GrammarBuilder>();

                            // if we have an optional entry [], then the parser writes an empty entry after the selection

                            int emptycount = (from x in wordlist where x.Length == 0 select x).Count();   // is there any empty ones indicating optionality

                            if (emptycount > 0)
                                gname = gname.AppendPrePad("[", " ");

                            gname = gname.AppendPrePad(string.Join("|", wordlist.Where(x=>x.Length>0) ), " ");

                            foreach (string o in wordlist)
                            {
                                if (o.Length > 0)
                                {
                                    sub.Add(new GrammarBuilder());
                                    sub.Last().Append(o, (emptycount > 0) ? 0 : 1, 1);      // indicate number of times, either 1:1 or 0,1 if we have an optional entry
                                                                                            // System.Diagnostics.Debug.Write("Choice " + o + (emptycount>0 ? " (opt), " : ", "));
                                }
                            }

                            if (emptycount > 0)
                                gname += "]";

                            Choices c = new Choices(sub.ToArray());
                            builder.Append(c);
                        }
                    }


                    System.Diagnostics.Debug.WriteLine($"Grammar {s} -> {gname}");

                    builder.Culture = ct;
                    Grammar gr = new Grammar(builder);
                    gr.Name = gname;
                    grammar.Add(gr);
                }
#else
                GrammarBuilder builder = new GrammarBuilder();
                builder.Append(s);
                builder.Culture = ct;
                Grammar gr = new Grammar(builder);
                gr.Name = s;
                grammar.Add(gr);

#endif
                updategrammar[s] = grammar;
               // System.Diagnostics.Debug.WriteLine($"{AppTicks.MSd} VR Update Grammar new {s}");
            }
            else
            {
                updategrammar[s] = null;
                //System.Diagnostics.Debug.WriteLine($"{AppTicks.MSd} VR Update Grammar dup of {s}");
            }

        }

        public void EndGrammarUpdate()
        {
            System.Diagnostics.Debug.Assert(updatinggrammar == true);
            System.Diagnostics.Debug.WriteLine($"{AppTicks.MSd} VR Request update with current {currentgrammar.Count} new {updategrammar.Count}");
            recognizer.RequestRecognizerUpdate();
        }

        // callback when recogniser is ready for grammar updates
        private void Recognizer_RecognizerUpdateReached(object sender, RecognizerUpdateReachedEventArgs e)
        {
            var todelete = new List<string>();

            foreach (var kvp in currentgrammar)
            {
                if (!updategrammar.ContainsKey(kvp.Key))        // any keys in currentgrammar not in update list, remove
                {
                    //System.Diagnostics.Debug.WriteLine($"{AppTicks.MSd} VR Remove {kvp.Key}");

                    foreach (var g in kvp.Value)
                    {
                        recognizer.UnloadGrammar(g);
                    }

                    todelete.Add(kvp.Key);
                }
                else
                {
                    //System.Diagnostics.Debug.WriteLine($"{AppTicks.MSd} VR Keep {kvp.Key}");
                }
            }

            foreach (var d in todelete)     // clean up dictionary
                currentgrammar.Remove(d);

            foreach (var kvp in updategrammar)
            {
                if (kvp.Value != null)     // may be null, meaning its already there, if not, add
                {
                    //System.Diagnostics.Debug.WriteLine($"{AppTicks.MSd} VR Add {kvp.Key}");

                    foreach (var g in kvp.Value)
                    {
                        recognizer.LoadGrammar(g);
                    }

                    currentgrammar.Add(kvp.Key, kvp.Value);
                }
            }

            {
                System.Diagnostics.Debug.WriteLine($"{AppTicks.MSd} VR List");
                List<Grammar> glist = new List<Grammar>(recognizer.Grammars);
                foreach (var g in glist)
                    System.Diagnostics.Debug.Write($"{g.Name}, ");
                System.Diagnostics.Debug.WriteLine($"");
            }

            updatinggrammar = false;
        }

        public void UpdateParas(int babbletimeout, int endsilencetimeout, int endsilencetimeoutambiguous, int initialsilencetimeout)
        {
            Stop();     // tried doing it in recognizer update reached, but it won't let you change babble or initial

            recognizer.BabbleTimeout = new TimeSpan(0, 0, 0, 0, babbletimeout);
            recognizer.EndSilenceTimeout = new TimeSpan(0, 0, 0, 0, endsilencetimeout);
            recognizer.EndSilenceTimeoutAmbiguous = new TimeSpan(0, 0, 0, 0, endsilencetimeoutambiguous);
            recognizer.InitialSilenceTimeout = new TimeSpan(0, 0, 0, 0, initialsilencetimeout);

            System.Diagnostics.Debug.WriteLine($"{AppTicks.MSd} VR Request paras {babbletimeout} {endsilencetimeout} {endsilencetimeoutambiguous} {initialsilencetimeout}");

            recognizer.RecognizeAsync(RecognizeMode.Multiple);        // and restart
        }


        private void Engine_RecognizeCompleted(object sender, RecognizeCompletedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"{AppTicks.MSd} VR Recogniser complete");
            completed = true;
        }

        private void Engine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            //DumpInfo("Recognised", e.Result);
            //System.Diagnostics.Debug.WriteLine("VR Confidence {0} vs threshold {1} for {2}", e.Result.Confidence, Confidence, e.Result.Text);
            if (e.Result.Confidence >= Confidence)
                SpeechRecognised?.Invoke(e.Result.Text, e.Result.Confidence);
            else
                SpeechNotRecognised?.Invoke(e.Result.Text, e.Result.Confidence);
        }

        private void Engine_SpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            //DumpInfo("Rejected", e.Result);
            SpeechNotRecognised?.Invoke(e.Result.Text, e.Result.Confidence);
        }

        private void Engine_SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            //DumpInfo("Hypothesised", e.Result);
        }

        void DumpInfo(string t, RecognitionResult r)
        {
            System.Diagnostics.Debug.WriteLine((AppTicks.MSd) + ":" + t + " " + r.Text + " " + r.Confidence.ToString("#.00"));
            foreach (RecognizedPhrase p in r.Alternates)
                System.Diagnostics.Debug.WriteLine("... alt " + p.Text + p.Confidence.ToString("#.00"));

            foreach (KeyValuePair<String, SemanticValue> child in r.Semantics)
            {
                System.Diagnostics.Debug.WriteLine("    The {0} city is {1}",
                  child.Key, child.Value.Value ?? "null");
            }
            foreach (RecognizedWordUnit word in r.Words)
            {
                System.Diagnostics.Debug.WriteLine(
                  "    Lexical form ({1})" +
                  " Pronunciation ({0})" +
                  " Display form ({2})",
                  word.Pronunciation, word.LexicalForm, word.DisplayAttributes);
            }

        }

    }
#endif
}
