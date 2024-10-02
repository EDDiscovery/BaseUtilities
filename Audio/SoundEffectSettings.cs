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

using BaseUtils;

namespace AudioExtensions
{
    public class SoundEffectSettings
    {
        public Variables Values { get; set; }

        public bool Any { get { return echoenabled || chorusenabled || reverbenabled || distortionenabled || gargleenabled || pitchshiftenabled; } }
        public bool OverrideNone { get { return Values.Exists("NoEffects"); } set { Values["NoEffects"] = (value) ? "1" : "0"; } }
        public bool Merge { get { return Values.Exists("MergeEffects"); } set { Values["MergeEffects"] = (value) ? "1" : "0"; } }
        public bool NoGlobalEffects { get { return Values.Exists("NoGlobalEffects"); } set { Values["NoGlobalEffects"] = (value) ? "1" : "0"; } }

        public bool echoenabled { get { return Values.Exists("EchoMix") || Values.Exists("EchoFeedback") || Values.Exists("EchoDelay"); } }
        public int echomix { get { return Values.GetInt("EchoMix", 50); } set { Values["EchoMix"] = value.ToString(); } }
        public int echofeedback { get { return Values.GetInt("EchoFeedback", 50); } set { Values["EchoFeedback"] = value.ToString(); } }
        public int echodelay { get { return Values.GetInt("EchoDelay", 100); } set { Values["EchoDelay"] = value.ToString(); } }

        public bool chorusenabled { get { return Values.Exists("ChorusMix") || Values.Exists("ChorusFeedback") || Values.Exists("ChorusDelay") || Values.Exists("ChorusDepth"); } }
        public int chorusmix { get { return Values.GetInt("ChorusMix", 50); } set { Values["ChorusMix"] = value.ToString(); } }
        public int chorusfeedback { get { return Values.GetInt("ChorusFeedback", 25); } set { Values["ChorusFeedback"] = value.ToString(); } }
        public int chorusdelay { get { return Values.GetInt("ChorusDelay", 16); } set { Values["ChorusDelay"] = value.ToString(); } }
        public int chorusdepth { get { return Values.GetInt("ChorusDepth", 10); } set { Values["ChorusDepth"] = value.ToString(); } }

        public bool reverbenabled { get { return Values.Exists("ReverbMix") || Values.Exists("ReverbTime") || Values.Exists("ReverbRatio"); } }
        public int reverbmix { get { return Values.GetInt("ReverbMix", 0); } set { Values["ReverbMix"] = value.ToString(); } }
        public int reverbtime { get { return Values.GetInt("ReverbTime", 1000); } set { Values["ReverbTime"] = value.ToString(); } }
        public int reverbhfratio { get { return Values.GetInt("ReverbRatio", 1); } set { Values["ReverbRatio"] = value.ToString(); } }

        public bool distortionenabled { get { return Values.Exists("DistortionGain") || Values.Exists("DistortionEdge") || Values.Exists("DistortionCF") || Values.Exists("DistortionWidth"); } }
        public int distortiongain { get { return Values.GetInt("DistortionGain", -18); } set { Values["DistortionGain"] = value.ToString(); } }
        public int distortionedge { get { return Values.GetInt("DistortionEdge", 15); } set { Values["DistortionEdge"] = value.ToString(); } }
        public int distortioncentrefreq { get { return Values.GetInt("DistortionCF", 2400); } set { Values["DistortionCF"] = value.ToString(); } }
        public int distortionfreqwidth { get { return Values.GetInt("DistortionWidth", 2400); } set { Values["DistortionWidth"] = value.ToString(); } }

        public bool gargleenabled { get { return Values.Exists("GargleFreq"); } }
        public int garglefreq { get { return Values.GetInt("GargleFreq", 20); } set { Values["GargleFreq"] = value.ToString(); } }

        public bool pitchshiftenabled { get { return Values.Exists("PitchShift"); } }
        public int pitchshift { get { return Values.GetInt("PitchShift", 100); } set { Values["PitchShift"] = value.ToString(); } }

        public SoundEffectSettings()
        { Values = new Variables(); }

        public SoundEffectSettings(Variables v )
        { Values = v; }

        public static SoundEffectSettings Create(Variables globals, Variables local)
        {
            SoundEffectSettings ses = new SoundEffectSettings(local);        // use the vars to place effects

            if (ses.OverrideNone)      // if none
            {
                ses = null;             // no speech effects
                //System.Diagnostics.Debug.WriteLine("SES No effects");
            }
            else if (ses.Merge)       // merged
            {
                Variables merged = new Variables(globals, local);   // add global settings (if not null) overridden by vars
                ses = new SoundEffectSettings(merged);
                //System.Diagnostics.Debug.WriteLine($"SES Merged effects {ses.Values.ToString()}");
            }
            else if (!ses.NoGlobalEffects && !ses.Any )     // if SES global effects allowed, and ses does not have any active ones
            {
                ses = (globals != null) ? new SoundEffectSettings(globals) : null;
                //System.Diagnostics.Debug.WriteLine($"SES Global effects {ses?.Values.ToString()}");
            }
            else
            {
                //System.Diagnostics.Debug.WriteLine($"SES Local effects {ses?.Values.ToString()}");
            }

            return ses;
        }
    }
}
