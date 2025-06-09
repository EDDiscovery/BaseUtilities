using AudioExtensions;
using BaseUtils;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace AudioTest
{
    public partial class Form1 : Form
    {
        AudioDriverCSCore audiodriver;
        AudioQueue audioqueue;

        SpeechSynthesizer speechsynthesiser;
        WindowsSpeechEngine winspeech;
        SpeechSynthesizerWindowsMedia windowsmediaspeechsynth;

        public Form1()
        {
            InitializeComponent();
            audiodriver = new AudioDriverCSCore("Default");
            audioqueue = new AudioQueue(audiodriver);

            winspeech = new WindowsSpeechEngine();
            windowsmediaspeechsynth = new SpeechSynthesizerWindowsMedia();

            speechsynthesiser = new SpeechSynthesizer(new ISpeechEngine[] { winspeech, windowsmediaspeechsynth });
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            var voices = speechsynthesiser.GetVoiceNames();
            richTextBoxLog.AppendText($"Voices: {voices.Count}:");
            richTextBoxLog.AppendText(string.Join(",", voices) + Environment.NewLine);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            audioqueue.StopAll();
            audiodriver.Stop();
            audiodriver.Dispose();

            winspeech.Dispose();
            windowsmediaspeechsynth.Dispose();

        }
        private void button1_Click(object sender, EventArgs e)
        {
            Variables vars = new Variables();
            SoundEffectSettings ses = SoundEffectSettings.Create(vars, vars);        // work out the settings
            AudioQueue.AudioSample audio = audioqueue.Generate(@"c:\code\examples\pack\hulldamage.mp3", ses);
            audioqueue.Submit(audio, 60, AudioQueue.Priority.Normal);
            AudioQueue.AudioSample audio2 = audioqueue.Generate(@"c:\code\examples\pack\sampledockingrequest.mp3", ses);
            audioqueue.Submit(audio2, 60, AudioQueue.Priority.Normal);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            AudioQueue.AudioSample audio = audioqueue.Tone(256.0, 50.0, 1000.0);
            audioqueue.Submit(audio, 60, AudioQueue.Priority.Normal);
            AudioQueue.AudioSample audio2 = audioqueue.Tone(512.0, 100.0, 500.0);
            audioqueue.Submit(audio2, 100, AudioQueue.Priority.Normal);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ToneWaveSource tws = new ToneWaveSource(1000, 50, 10000);

            EnvelopeWaveSource envx = new EnvelopeWaveSource(tws, 1000, 500, 2000, 2000, 100, 90);

        //    audioWaveform1.Draw(envx, (int)(tws.WaveFormat.SampleRate*10));

            AudioQueue.AudioSample audio = audioqueue.Tone(256.0, 50.0, 10000.0);
            var ap = audioqueue.Envelope(audio, 1000, 500, 2000, 2000, 100, 90);


            //AudioQueue.AudioSample audio = audioqueue.Tone(256.0, 50.0, 2000.0);
            //AudioQueue.AudioSample audio2 = audioqueue.Tone(512.0, 100.0, 50000.0);

            //var ap = audioqueue.Append(audio, audio2);

            audioqueue.Submit(ap, 60, AudioQueue.Priority.Normal);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var stream = speechsynthesiser.Speak("Hello sailors", "en", "Ivona 2 Emma OEM", -5);
            var sample = audioqueue.Generate(stream);
            audioqueue.Submit(sample, 100, AudioQueue.Priority.Normal);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var stream = speechsynthesiser.Speak("Hello sailors How are you 10 < 20", "en", "WM/Microsoft Ravi", -5);
            if (stream != null)
            {
                var sample = audioqueue.Generate(stream);
                audioqueue.Submit(sample, 100, AudioQueue.Priority.Normal);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            var speakBody = "Hello <phoneme alphabet='ipa' ph = 'ˈakɜːnɑ'>Not ssml Achenar</phoneme> and <phoneme alphabet='ipa' ph = 'paɪ'>Pai</phoneme> or <phoneme alphabet='ipa' ph = 'ʃuː'>woose Szu</phoneme> the end";

            var stream = speechsynthesiser.Speak(speakBody, "en", "WM/Microsoft Ravi", 0);

            if (stream != null)
            {
                var sample = audioqueue.Generate(stream);
                audioqueue.Submit(sample, 100, AudioQueue.Priority.Normal);
            }
        }
    }
}
