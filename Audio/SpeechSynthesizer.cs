/*
 * Copyright 2017-2025 EDDiscovery development team
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

using BaseUtils.Threads;
using System;
using System.Collections.Generic;
using System.Threading;

namespace AudioExtensions
{
    public interface ISpeechEngine: IDisposable
    {
        string PrefixName { get; }     // string prefixing all voice names, to help select the right voice engine

        List<string> GetVoiceNames();

        /// <summary>
        /// Speak
        /// </summary>
        /// <param name="phrase">text</param>
        /// <param name="culture">en/de etc</param>
        /// <param name="voice">voice name</param>
        /// <param name="volume">volume, 0-100</param>
        /// <param name="rate">-10 to +10 rate</param>
        /// <returns></returns>
        System.IO.Stream Speak(string phrase, string culture, string voice , int volume, int rate);
    }


    public class SpeechSynthesizer
    {
        ISpeechEngine[] speechengines;
        TaskQueue tq;

        public SpeechSynthesizer(ISpeechEngine engine)
        {
            speechengines = new ISpeechEngine[] { engine };
            tq = new TaskQueue();
        }
        public SpeechSynthesizer(ISpeechEngine[] engines)
        {
            speechengines = engines;
            tq = new TaskQueue();
        }

        public List<string> GetVoiceNames()
        {
            List<string> names = new List<string>();
            foreach (var x in speechengines)
                names.AddRange(x.GetVoiceNames());
            return names;
        }

        private System.IO.Stream SelectAndSpeak(string say, string culture, string voice, int rate)     // may return null
        {
            foreach( var x in speechengines)
            {
                if (x.PrefixName.Length > 0 && voice.StartsWith(x.PrefixName))
                    return x.Speak(say, culture, voice, 100, rate);
            }

            return speechengines[0].Speak(say, culture, voice, 100, rate);     // samples are always generated at 100 volume

        }

        public System.IO.Stream Speak(string say, string culture, string voice, int rate)     // may return null
        {
            while (tq.Active)       // just to make sure we are not doing anything with the speech queue
                Thread.Sleep(20);

            return SelectAndSpeak(say, culture, voice, rate);
        }

        public void SpeakQueue(string say, string culture, string voice, int rate, Action<System.IO.Stream> callback)     // may return null
        {
            tq.Enqueue(()=> 
            {
                //System.Diagnostics.Debug.WriteLine($"{Environment.TickCount} SpeechSynth In task run {say}");
                var audio = SelectAndSpeak(say, culture, voice, rate);
                //System.Diagnostics.Debug.WriteLine($"{Environment.TickCount} SpeechSynth audio complete {say}");
                callback(audio);
                //System.Diagnostics.Debug.WriteLine($"{Environment.TickCount} SpeechSynth callback done {say}");
            });
        }
    }
}
