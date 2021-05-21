using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Speech;
using System.Speech.Synthesis;
using System.Speech.AudioFormat;
using System.Threading;
using System.IO;
using System.Collections;

namespace RapTTS
{
    class RapGenerator
    {
        private bool isUsingYo = false;
        private bool isSync = false;
        private bool isReadText = true;

        private string commentsFile = "Comments.txt";
        private string beatFileFolder = "beat";
        private string bgmFileFolder = "bgm";

        private int typeMidi = 0;
        private int typeMusic = 0;

        private int setTime = -999;
        private double beat = 900;
        private int setTimeP = 7800;

        private double cMidi = 0;
        private double cMidiFuture = 0;

        private Queue<string> commentQueue;
        private Queue<string> rapQueue;
        private DataSocket socket;
        private Thread thread;

        private string[] midiList;
        private string[] beatList;
        private string[] midi;
        private string[] words;
        private string midiAll;


        private string baseDir = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;


        public RapGenerator()
        {
            midiList = new string[4] { "How Many Mics testing(MIDI).txt", "Dove.txt", "Pras.txt", "Lost Yourself(MIDI).txt" };
            beatList = new string[2] { "FreeBeats.wav", "FreeBeats2.wav" };
            commentQueue = new Queue<string>();
            rapQueue = new Queue<string>();

            SpeechGenerator.setLanguage(SpeechGenerator.getVoiceList()[1]);
        }

        public void StartTTS(bool isRap)
        {

            GetData();

            if (isRap)
            {
                //rap

                SetMusic();
                Thread.Sleep(setTimeP);
                Rap();
            }
            else
            {
                //normal tts
                NormalTTS();
            }
        }

        public void Stop()
        {
            thread.Abort();
        }

        private void GetData()
        {
            if (isReadText)
            {
                Random rnd = new Random();
                int r = rnd.Next(1, 5);
                
                commentsFile = $"sukiyaki test {r}.txt";
                Console.WriteLine($"template {r} {commentsFile}");
                string path = $"{baseDir}/text/{commentsFile}";
                string[] lines = File.ReadAllLines(path);
                foreach (string line in lines)
                {
                    commentQueue.Enqueue(line);
                }
            }
            else
            {
                thread = new Thread(StartConnection);
                thread.Start();
            }
        }

        public void EnqueueData(string text)
        {
            Console.WriteLine("Enqueue : " + text);
            commentQueue.Enqueue(text);
            Console.WriteLine("Queue : " + commentQueue.Count);
            //Speech.speak(text);
        }

        public void EmptyQueue()
        {
            commentQueue.Clear();
            rapQueue.Clear();
        }

        private void StartConnection()
        {
            socket = new DataSocket(this);
        }

        private void NormalTTS()
        {
            SpeechGenerator.rateSpeed = 0;
            while (true)
            {
                if (commentQueue.Count > 0)
                {
                    SpeechGenerator.SpeakSync(commentQueue.Dequeue());
                }
            }
        }

        private void SetMusic()
        {
            //static midi
            typeMidi = 1;
            readMidi("sukiyaki converted.txt");
            //Console.WriteLine("set dove-old");
            //random midi
            //readRandomMidi();

            // static bgm
            typeMusic = 0;
            //180
            //136bpm -> online analyzer https://getsongbpm.com/tools/audio
            //4 beats = 3.529s
            setBeat(250, 875);
            backingBeat("sukiyaki_mod.wav");

            //random bgm
            //randomBackingBeat();

            //Console.WriteLine(beatList.Length);
        }

        private void setBeat(double beatValue, int timeValue)
        {
            beat = beatValue;
            setTimeP = timeValue;
        }

        private void readMidi(string filename)
        {
            string path = $"{baseDir}/{beatFileFolder}/{filename}";
            midiAll = System.IO.File.ReadAllText(path);
            for (int i = 0; i < midiAll.Length; i++)
            {
                midi = midiAll.Split(',', '\n', ' ');
            }
        }

        private void readRandomMidi()
        {
            Random random = new Random();
            typeMidi = random.Next(0, midiList.Length);
            readMidi(midiList[typeMidi]);
        }

        private void chooseBeat()
        {
            if (typeMusic == 0)
            {
                if (typeMidi == 0)
                {
                    setBeat(900, 7000);
                }
                else if (typeMidi == 1)
                {
                    setBeat(900, 7500);
                }
                else if (typeMidi == 2)
                {
                    setBeat(900, 7000);
                }
                else
                {
                    setBeat(900, 7800);
                }
            }
            else if (typeMusic == 1)
            {
                if (typeMidi == 0)
                {
                    setBeat(900, 9000);
                }
                else if (typeMidi == 1)
                {
                    setBeat(900, 9000);
                }
                else if (typeMidi == 2)
                {
                    setBeat(900, 9800);
                }
                else
                {
                    setBeat(900, 9000);
                }
            }
        }

        private void backingBeat(string filename)
        {
            System.Media.SoundPlayer soundP = new System.Media.SoundPlayer();
            soundP.SoundLocation = $"{baseDir}/{bgmFileFolder}/{filename}";
            Console.WriteLine(soundP.SoundLocation);
            soundP.Load();
            soundP.PlayLooping();
        }

        private void randomBackingBeat()
        {
            Random random = new Random();
            typeMusic = random.Next(0, beatList.Length);
            chooseBeat();

            //play rap bgm
            backingBeat(beatList[typeMusic]);

        }

        private void Rap()
        {
            while (true)
            {
                if (commentQueue.Count > 0)
                {
                    readWords();
                }
                else
                {
                    if (rapQueue.Count == 0 && isUsingYo)
                    {
                        //add yo during blank period
                        introBeforeCommnet();
                    }
                }
            }
        }

        private void readWords()
        {
            string rap = commentQueue.Dequeue();

            words = rap.Split(' ', '\n');

            for (int i = 0; i < words.Length; i++)
            {
                rapQueue.Enqueue(words[i]);
            }

            speechWord();
        }

        private void speechWord()
        {
            //rhythm();
            for (int i = 0; i < midi.Length; i++)
            {
                double.TryParse(midi[i], out cMidi);
                if (i < midi.Length - 1)
                {
                    double.TryParse(midi[i + 1], out cMidiFuture);
                }

                if (cMidiFuture < 0)
                {
                    cMidi = cMidi + (cMidiFuture * -1);
                    i++;
                }
                //Console.WriteLine("I= "+i+" cMidi= "+cMidi);
                setTime = Convert.ToInt32(beat * cMidi);
                Console.WriteLine("speechword beat : " + beat);
                Console.WriteLine("speechword cMidi : " + cMidi);
                Console.WriteLine("speechword setTime : " + setTime);
                addSpeed();

            }
            //after read all rythm done
            //cSpeech = 0;
        }

        private void introBeforeCommnet()
        {
            rapQueue.Enqueue("yo");
            Random random = new Random();
            int preCheck = random.Next(0, 3);
            if (preCheck == 0)
            {
                cMidi = 0.25;
            }
            else if (preCheck == 1)
            {
                cMidi = 0.5;
            }
            else
            {
                cMidi = 0.75;
            }
            //Console.WriteLine("I= " + i + " cMidi= " + cMidi);
            setTime = Convert.ToInt32(beat * cMidi);
            addSpeed();

            //after read all rythm done
            //cSpeech = 0;
            Thread.Sleep(700);
        }

        private void addSpeed()
        {
            if (setTime >= (beat * 1)) //1.0
            {
                SpeechGenerator.rateSpeed = 1;
            }
            else if (setTime >= (beat * 0.75)) //0.75
            {
                SpeechGenerator.rateSpeed = 2;
            }
            else if (setTime >= (beat * 0.5)) //0.50
            {
                SpeechGenerator.rateSpeed = 3;
            }
            else if (setTime >= (beat * 0.33)) //0.33
            {
                SpeechGenerator.rateSpeed = 4;
            }
            else if (setTime >= (beat * 0.25))
            {
                SpeechGenerator.rateSpeed = 5;
            }
            else
            {
                SpeechGenerator.rateSpeed = 6;
            }

            checkWord();
        }

        private void checkWord()
        {
            if (rapQueue.Count > 0)
            {
                if (isSync)
                {
                    SpeechGenerator.SpeakSync(rapQueue.Dequeue());
                    //Thread.Sleep(setTime);
                }
                else
                {
                    SpeechGenerator.speak_cont(rapQueue.Dequeue());
                    //Speech.speak_new2(rapQueue.Dequeue());
                    //SpeechGenerator.SpeakSyncAlt(rapQueue.Dequeue());
                    Console.WriteLine("checkWord : " + setTime);
                    //setTime *= 3;
                    Thread.Sleep(setTime);
                }

            }
        }

    }
}
