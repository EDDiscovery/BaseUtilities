/*
 * Copyright © 2017-2022 EDDiscovery development team
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

#pragma warning disable 0067

namespace AudioExtensions
{
    public delegate void SpeechRecognised(string text, float confidence);

    public interface IVoiceRecognition
    {
        event SpeechRecognised SpeechRecognised;
        event SpeechRecognised SpeechNotRecognised;
        bool IsOpen { get; }
        float Confidence { get; set; }

        bool Open(System.Globalization.CultureInfo ctp, bool winform);
        void Close();   // can close without stop

        void BeginGrammarUpdate();
        void AddGrammar(string s);
        void EndGrammarUpdate();

        void UpdateParas(int babbletimeout, int endsilencetimeout, int endsilencetimeoutambiguous, int initialsilencetimeout);

    }


    public class VoiceRecognitionDummy : IVoiceRecognition
    {
        public event SpeechRecognised SpeechRecognised;
        public event SpeechRecognised SpeechNotRecognised;
        public bool IsOpen { get { return false; } }
        public float Confidence { get; set; } = 0.98F;
 
        public bool Open(System.Globalization.CultureInfo ctp, bool winform) { return false; }       // Dispose to close
        public void Close() { }

        public void BeginGrammarUpdate() { }
        public void AddGrammar(string s) { }
        public void EndGrammarUpdate() { }

        public void UpdateParas(int babbletimeout, int endsilencetimeout, int endsilencetimeoutambiguous, int initialsilencetimeout) { }

    }

}
