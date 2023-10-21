/*
 * Copyright © 2017 EDDiscovery development team
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

using BaseUtils.Threads;
using System;
using System.Threading;

namespace AudioExtensions
{
    public interface ISpeechEngine
    {
        string[] GetVoiceNames();
        System.IO.MemoryStream Speak(string phrase, string culture, string voice , int volume, int rate);
    }


    public class SpeechSynthesizer
    {
        ISpeechEngine speechengine;
        TaskQueue tq;

        public SpeechSynthesizer( ISpeechEngine engine )
        {
            speechengine = engine;
            tq = new TaskQueue();
        }

        public string[] GetVoiceNames()
        {
            return speechengine.GetVoiceNames();
        }

        public System.IO.MemoryStream Speak(string say, string culture, string voice, int rate)     // may return null
        {
            while (tq.Active)       // just to make sure we are not doing anything with the speech queue
                Thread.Sleep(20);

            return speechengine.Speak(say, culture, voice, 100, rate);     // samples are always generated at 100 volume
        }

        public void SpeakQueue(string say, string culture, string voice, int rate, Action<System.IO.MemoryStream> callback)     // may return null
        {
            tq.Enqueue(()=> 
            {
                //System.Diagnostics.Debug.WriteLine($"{Environment.TickCount} SpeechSynth In task run {say}");
                var audio = speechengine.Speak(say, culture, voice, 100, rate);     // samples are always generated at 100 volume
                //System.Diagnostics.Debug.WriteLine($"{Environment.TickCount} SpeechSynth audio complete {say}");
                callback(audio);
                //System.Diagnostics.Debug.WriteLine($"{Environment.TickCount} SpeechSynth callback done {say}");
            });
        }
    }
}
