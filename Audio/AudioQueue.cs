/*
 * Copyright 2017-2019 EDDiscovery development team
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

namespace AudioExtensions
{
    public class AudioQueue : IDisposable       // Must dispose BEFORE ISoundOut.
    {
        public delegate void SampleStart(AudioQueue sender, Object tag);
        public delegate void SampleOver(AudioQueue sender, Object tag);

        public enum Priority { Low, Normal, High, HighClear , Immediate };
        static public Priority GetPriority(string s) { Priority p; if (Enum.TryParse<AudioQueue.Priority>(s, true, out p)) return p; else return Priority.Normal; }

        public class AudioSample : IDisposable
        {
            public List<System.IO.Stream> Streams { get; set; } = new List<System.IO.Stream>();   // audio samples held in files to free
            public AudioData AudioData { get; set; }         // the audio data, in the driver format
            public int Volume { get; set; }                  // 0-100
            public Priority Priority { get; set; }           // set audio priority.

            public event SampleStart StartEvent;
            public object SampleStartTag { get; set; }

            public void SampleStart(AudioQueue q)
            {
                if (StartEvent != null)
                    StartEvent(q, SampleStartTag);
            }

            public event SampleOver EndEvent;
            public object SampleEndTag { get; set; }

            public void SampleOver(AudioQueue q)
            {
                if (EndEvent != null)
                    EndEvent(q, SampleEndTag);
            }

            public void Dispose()
            {
                foreach (System.IO.Stream i in Streams)
                    i.Dispose();
                Streams.Clear();
            }

            public AudioSample LinkedSample { get; set; }             // link to another sample. if set, we halt the queue if thissample is not t at the top of the list
            public AudioQueue LinkedQueue { get; set; }               // of the other queue, then release them to play together.
        }

        public AudioQueue(IAudioDriver adp)
        {
            ad = adp;
            ad.AudioStoppedEvent += AudioStoppedEvent;
            audioqueue = new List<AudioSample>();
        }

        public IAudioDriver Driver { get { return ad; } }       // gets driver associated with this queue

        public bool SetAudioEndpoint( string dev )
        {
            bool res = ad.SetAudioEndpoint(dev);
            if (res)
                Clear();
            return res;
        }

        public void Clear()     // clear queue, does not call the end functions
        {
            foreach( AudioSample a in audioqueue )
            {
                FinishSample(a,false);
            }

            audioqueue.Clear();
        }

        private void FinishSample(AudioSample a , bool callback )
        {
            if ( callback )
                a.SampleOver(this);     // let callers know a sample is over

            a.Dispose();
            ad.Dispose(a.AudioData);        // tell the driver to clean up
        }

        private void AudioStoppedEvent()            //CScore calls then when audio over.
        {
            //System.Diagnostics.Debug.WriteLine((Environment.TickCount % 10000).ToString("00000") + " Stopped audio");

            //System.Diagnostics.Debug.Assert(System.Windows.Forms.Application.MessageLoop);      // UI thread.

            if (audioqueue.Count > 0) // Normally always have an entry, except on Kill , where queue is gone
            {
                FinishSample(audioqueue[0],true);

                //System.Diagnostics.Debug.WriteLine("Clear audio at 0 depth " + audioqueue.Count);

                audioqueue.RemoveAt(0);
            }

            Queue(null);
        }


        private void Queue(AudioSample newdata)
        {
            //System.Diagnostics.Debug.Assert(System.Windows.Forms.Application.MessageLoop);      // UI thread.

            //for (int q = 0; q < audioqueue.Count; q++) System.Diagnostics.Debug.WriteLine(q.ToStringInvariant() + " " + (audioqueue[q].audiodata.data != null) + " " + audioqueue[q].priority);

            if (newdata != null)
            {
                //System.Diagnostics.Debug.WriteLine("Play " + ad.Lengthms(newdata.audiodata) + " in queue " + InQueuems() + " " + newdata.priority);

                if ( audioqueue.Count > 0 && newdata.Priority > audioqueue[0].Priority )       // if something is playing, and we have priority..
                {
                    //System.Diagnostics.Debug.WriteLine((Environment.TickCount % 10000).ToString("00000") + " Priority insert " + newdata.priority + " front " + audioqueue[0].priority);

                    List<AudioSample> removelist = new List<AudioSample>();

                    if (newdata.Priority == Priority.HighClear || newdata.Priority == Priority.Immediate)   // if high clear or immediate
                    {
                        for (int i = 1; i < audioqueue.Count; i++)                  // remove all after current
                            removelist.Add(audioqueue[i]);
                    }
                    else if (audioqueue[0].Priority == Priority.Low)                 // if low at front, remove all other lows after it
                    {
                        for (int i = 1; i < audioqueue.Count; i++)
                        {
                            if (audioqueue[i].Priority == Priority.Low)
                            {
                                removelist.Add(audioqueue[i]);
                                //System.Diagnostics.Debug.WriteLine("Queue to remove " + i);
                            }
                        }
                    }

                    foreach (AudioSample a in removelist)
                    {
                        FinishSample(a, false);
                        audioqueue.Remove(a);
                    }

                    // immediate, stop this one, insert this as next
                    if (newdata.Priority == Priority.Immediate)
                    {
                        audioqueue.Insert(1, newdata);  // add one past front
                        ad.Stop();                      // stopping makes it stop, does the callback, this gets called again, audio plays
                    }
                    // high priority is playing, play directly after
                    else if (audioqueue[0].Priority == Priority.High || audioqueue[0].Priority == Priority.HighClear || audioqueue[0].Priority == Priority.Immediate)  
                    {
                        audioqueue.Insert(1, newdata);  // add one past front
                    }
                    else
                    {
                        audioqueue.Insert(1, newdata);  // add one past front
                        ad.Stop();                      // stopping makes it stop, does the callback, this gets called again, audio plays
                    }
                    return;
                }
                else
                {
                    //System.Diagnostics.Debug.WriteLine(Environment.TickCount + "AUDIO queue");
                    
                    audioqueue.Add(newdata);
                    if (audioqueue.Count > 1)       // if not the first in queue, no action yet, let stopped handle it
                        return;
                }
            }

            if (audioqueue.Count > 0)
            {
                if (audioqueue[0].LinkedQueue != null && audioqueue[0].LinkedSample != null)       // linked to another audio q, both must be at front to proceed
                {
                    if (!audioqueue[0].LinkedQueue.IsWaiting(audioqueue[0].LinkedSample))        // if its not on top, don't play it yet
                        return;

                    audioqueue[0].LinkedQueue.ReleaseHalt();        // it is waiting, so its stopped.. release halt on other one
                }

                ad.Start(audioqueue[0].AudioData, audioqueue[0].Volume);    // driver, play this
                audioqueue[0].SampleStart(this);     // let callers know a sample started
            }
        }

        public int InQueuems()       // Length of sound in queue.. does not take account of priority.
        {
            //System.Diagnostics.Debug.Assert(System.Windows.Forms.Application.MessageLoop);      // UI thread.

            int len = 0;
            if (audioqueue.Count > 0)
                len = ad.TimeLeftms(audioqueue[0].AudioData);

            for (int i = 1; i < audioqueue.Count; i++)
                len += ad.Lengthms(audioqueue[i].AudioData);
            return len;
        }

        public AudioSample Generate(string file, SoundEffectSettings effects = null)        // from a file (you get a AS back so you have the chance to add events)
        {
            AudioData audio = ad.Generate(file, effects);

            if (audio != null)
            {
                AudioSample a = new AudioSample() { AudioData = audio };
                return a;
            }
            else
                return null;
        }

        public AudioSample Generate(System.IO.Stream audioms, SoundEffectSettings effects = null, bool ensuresomeaudio = false)   // from a memory stream
        {
            if (audioms != null)
            {
                AudioData audio = ad.Generate(audioms, effects, ensuresomeaudio);
                if (audio != null)
                {
                    AudioSample a = new AudioSample() { AudioData = audio };
                    a.Streams.Add(audioms);
                    return a;
                }
            }

            return null;
        }

        public AudioSample Append(AudioSample last, AudioSample next)
        {
            AudioData audio = ad.Append(last.AudioData, next.AudioData);

            if (audio != null)
            {
                last.AudioData = audio;
                last.Streams.AddRange(next.Streams);
                next.Streams.Clear();
                return last;
            }
            else
                return null;
        }

        public AudioSample Mix(AudioSample last, AudioSample mix)
        {
            AudioData audio = ad.Mix(last.AudioData, mix.AudioData);

            if (audio != null)
            {
                last.AudioData = audio;
                last.Streams.AddRange(mix.Streams);
                mix.Streams.Clear();
                return last;
            }
            else
                return null;
        }
        public AudioSample Tone(double frequency, double amplitude, double lengthms, SoundEffectSettings ses = null)
        {
            AudioData audio = ad.Tone(frequency, amplitude, lengthms,ses);

            if (audio != null)
            {
                return new AudioSample() { AudioData = audio };
            }
            else
                return null;
        }

        public AudioSample Envelope(AudioSample last, double attackms, double decayms, double sustainms, double releasems,
                                    double maxamplitude, double sustainamplitude)
        {
            AudioData audio = ad.Envelope(last.AudioData, attackms, decayms, sustainms, releasems, maxamplitude, sustainamplitude);
            if (audio != null)
            {
                return new AudioSample() { AudioData = audio };
            }
            else
                return null;
        }

        public void Submit(AudioSample s, int vol, Priority p)       // submit to queue
        {
            s.Volume = vol;
            s.Priority = p;
            Queue(s);
        }

        public void StopCurrent()   // async
        {
            if (audioqueue.Count > 0)       // if we are playing, stop current
            {
                ad.Stop();
                //System.Diagnostics.Debug.WriteLine((Environment.TickCount % 10000).ToString("00000") + " Stop current");
            }
        }

        public void StopAll() // async
        {
            if ( audioqueue.Count>0)
            {
                 if (audioqueue.Count > 1)
                    audioqueue.RemoveRange(1, audioqueue.Count - 1);

                ad.Stop();  // async stop
            }
        }

        public void Dispose()
        {
            ad.AudioStoppedEvent -= AudioStoppedEvent;
        }

        public bool IsWaiting(AudioSample s)    // is this at the top of my queue?
        {
            if (audioqueue.Count > 0)
                return Object.ReferenceEquals(audioqueue[0], s);
            else
                return false;
        }

        public void ReleaseHalt()       // other stream is ready, release us for play
        {
            if (audioqueue.Count > 0)
            {
                audioqueue[0].LinkedQueue = null;   // make sure queue now ignores the link
                Queue(null);
            }
        }

        private List<AudioSample> audioqueue;
        private IAudioDriver ad;
    }
}
