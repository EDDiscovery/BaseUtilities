/*
 * Copyright 2025 EDDiscovery development team
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

using AudioExtensions;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Windows.Media.SpeechSynthesis;

namespace AudioTest
{
    public class SpeechSynthesizerWindowsMedia : ISpeechEngine
    {
        public string PrefixName => "WM/";

        public SpeechSynthesizerWindowsMedia()
        {
            // as informed from EDDI, lets double check their state
            var regfolder = @"SOFTWARE\Microsoft\Speech_OneCore\Voices\Tokens";

            var keys = Registry.LocalMachine.OpenSubKey(regfolder, false);
            if (keys != null)
            {
                var allvoices = Windows.Media.SpeechSynthesis.SpeechSynthesizer.AllVoices;

                foreach (var vi in allvoices)      // for all declared win media voices
                {
                    try
                    {
                        foreach (var sk in keys.GetSubKeyNames())
                        {
                            var skreg = Registry.LocalMachine.OpenSubKey($"{regfolder}\\{sk}"); // may be null
                            string regname = skreg?.GetValue("")?.ToString();
                            if (regname?.Contains(vi.DisplayName) ?? false)  // if default value found, which is the nameits present
                            {
                                lock (synth)
                                {
                                    synth.Voice = vi;
                                    // calling result blocks until task is complete, which is the same as task.Wait/task.Result
                                    var res = synth.SynthesizeTextToStreamAsync("Hello").AsTask().Result;
                                    if (res != null)
                                    {
                                        res.Dispose();      // dispose of the stream, add to 
                                        voices.Add(vi);
                                    }
                                }
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to load and speak voice {vi.DisplayName} {ex}");
                    }
                }
            }
        }
        public List<string> GetVoiceNames()
        {
            return voices.Select(x => PrefixName + x.DisplayName).ToList();
        }

        public Stream Speak(string phrase, string _, string voice, int volume, int rate)
        {
            lock ( synth )
            {
                if (PrefixName + synth.Voice.DisplayName != voice)      // select voice
                {
                    var vi = voices.Find(x => PrefixName + x.DisplayName == voice);
                    if (vi != null)
                        synth.Voice = vi;
                }

                synth.Options.AudioVolume = volume / 100.0;
                // rate is -10 to 10 matching windows synth.  Convert to 0.5 to 6.
                synth.Options.SpeakingRate = rate < 0 ? 0.5 + -rate / 20.0 : 1 + rate * 50.0 / 100.0;

                phrase = phrase.Trim();

                string[] ssmlstart = new string[] { "<say-as ", "<emphasis", "<phoneme", "<sub", "<prosody", "<break", "<voice", "<lexicon" };
                string[] ssmlend = new string[] { "</say-as>", "</emphasis>", "</phoneme>", "</sub>", "</prosody>", "/>", "</voice>", "/>" };

                StringBuilder ssmltext = new StringBuilder();
                ssmltext.Append(@"<?xml version=""1.0"" encoding=""UTF-8""?>"); // xml header
                ssmltext.AppendCR();
                ssmltext.Append($@"<speak version=""1.0"" xmlns=""http://www.w3.org/2001/10/synthesis""");
                ssmltext.Append($@" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"""); 
                ssmltext.Append($@" xsi:schemaLocation=""http://www.w3.org/2001/10/synthesis http://www.w3.org/TR/speech-synthesis/synthesis.xsd""");
                ssmltext.Append($@" xml:lang=""{synth.Voice.Language}""");
                ssmltext.Append(@">");
                ssmltext.AppendCR();

                StringBuilder baretext = new StringBuilder();

                bool ssml = false;

                while (phrase.Length > 0)
                {
                    int foundpos = phrase.IndexOf(ssmlstart, out int ssmlindex);        // find one of the ssml phrases
                    if (foundpos == -1)     // no more, task on rest as normal text
                    {
                        ssmltext.Append(phrase);
                        baretext.Append(phrase);
                        break;
                    }
                    else
                    {
                        ssml = true;        // have ssml

                        if (foundpos > 0)
                        {
                            string textto = phrase.Substring(0, foundpos);
                            ssmltext.Append(textto);       // tack on front
                            baretext.Append(textto);
                            phrase = phrase.Substring(foundpos);
                        }

                        int indexofend = phrase.IndexOf(ssmlend[ssmlindex]);

                        if (indexofend == -1) // allowed as a shortcut to drop the last one
                        {
                            indexofend = phrase.Length;
                            phrase += ssmlend[ssmlindex];
                        }

                        indexofend += ssmlend[ssmlindex].Length; // move to end of it

                        string ssmlcmd = phrase.Substring(0, indexofend).Replace('\'', '"');

                        ssmltext.Append(ssmlcmd);
                        ssmltext.Append(" ");

                        phrase = phrase.Substring(indexofend).Trim();
                    }
                }

                ssmltext.Append(@"</speak>"); 

                try
                {
                    var ssstream = ssml ? synth.SynthesizeSsmlToStreamAsync(ssmltext.ToString()).AsTask().Result : synth.SynthesizeTextToStreamAsync(baretext.ToString()).AsTask().Result;
                    return ssstream.AsStreamForRead();
                }
                catch ( Exception ex)
                {
                    try
                    {
                        if (ssml)
                        {
                            System.Diagnostics.Trace.WriteLine($"Windows media speech synth ssml conversion failed {ssmltext.ToString()}");
                            var ssstream = synth.SynthesizeTextToStreamAsync(baretext.ToString()).AsTask().Result;      // tried SSML, try bare text
                            return ssstream.AsStreamForRead();
                        }
                        else
                            System.Diagnostics.Trace.WriteLine($"Windows media speech synth failed on bare text {ex}");
                    }
                    catch ( Exception ex1)
                    {
                        System.Diagnostics.Debug.WriteLine($"Windows media speech synth failed trying bare text {ex1}");
                    }

                    return null;
                }
            }
        }

        public void Dispose()
        {
            lock (synth)
            {
                synth.Dispose();
                synth = null;
            }
        }

        // ensure we are using media speech engine
        private Windows.Media.SpeechSynthesis.SpeechSynthesizer synth = new Windows.Media.SpeechSynthesis.SpeechSynthesizer();
        private List<VoiceInformation> voices = new List<VoiceInformation>();
    }
}
