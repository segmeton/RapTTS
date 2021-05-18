using System;
using System.Collections.Generic;
using System.Text;
using System.Speech.Synthesis;
using System.Speech.AudioFormat;
using System.Linq;

namespace RapTTS
{
    class SpeechGenerator
    {
        public static int vol = 50;
        public static int rateSpeed = 0;
        public static SpeechSynthesizer[] synth = { new SpeechSynthesizer { Volume = vol, Rate = rateSpeed }, new SpeechSynthesizer { Volume = vol, Rate = rateSpeed } };
        public static int sni = 0;
        public static int voiceGender = 1;
        static Boolean isPause = false;
        static List<SpeechSynthesizer> list_sync = new List<SpeechSynthesizer>();

        public static void Initialize()
        {
            //synth[0] = new SpeechSynthesizer { Volume = vol, Rate = rateSpeed };

            SpeechSynthesizer s = new SpeechSynthesizer { Volume = vol, Rate = rateSpeed };

            list_sync.Add(s);
            if (list_sync.Count > 5) { list_sync.First().Dispose(); list_sync.RemoveAt(0); }

            synth[0] = s;

            setVoice(voiceGender);
            if (lang != "") { synth[0].SelectVoice(lang); }
            isPause = false;
        }

        public static void SpeakSync(string s)
        {
            Initialize();
            synth[0].Speak(s);
            synth[0].Dispose();
        }

        public static void speak_cont(string s)
        {
            //synth[0].Rate = rateSpeed;
            Initialize();
            Console.WriteLine("synth[0] rateSpeed : " + synth[0].Rate);
            Console.WriteLine("Word : " + s);
            synth[0].SpeakAsync(s);
        }

        public static void SpeakSyncAlt(string s)
        {
            if (lang != "")
            {
                synth[sni].SelectVoice(lang);
            }
            synth[sni].Rate = rateSpeed;
            Console.WriteLine("synth[" + sni + "] rateSpeed : " + synth[sni].Rate);
            synth[sni].Speak(s);
            sni++; if (sni > 1) { sni = 0; }
        }

        public static void stop()
        {
            synth[0].SpeakAsyncCancelAll();
        }

        public static void pause_resume()
        {
            if (isPause) { synth[0].Resume(); }
            else { synth[0].Pause(); }
            isPause = !isPause;
        }

        public static void setVol(int i)
        {
            vol = i;//cant change during play
        }

        public static void setVoice(int i)
        {
            if (i == 1) { synth[0].SelectVoiceByHints(VoiceGender.Female, VoiceAge.Teen); }
            else if (i == 0) { synth[0].SelectVoiceByHints(VoiceGender.Male, VoiceAge.Adult); }
            else { synth[0].SelectVoiceByHints(VoiceGender.Neutral); }
        }


        //=========================================================================
        public static string err_log = "";

        //https://msdn.microsoft.com/en-us/library/ms586869(v=vs.110).aspx
        public static List<string> getVoiceList()
        {
            List<string> list_s = new List<string>();
            try
            {
                using (SpeechSynthesizer synth = new SpeechSynthesizer())
                {
                    foreach (InstalledVoice voice in synth.GetInstalledVoices())
                    {
                        VoiceInfo info = voice.VoiceInfo;
                        string AudioFormats = "";
                        foreach (SpeechAudioFormatInfo fmt in info.SupportedAudioFormats)
                        {
                            AudioFormats += String.Format("{0}\n",
                            fmt.EncodingFormat.ToString());
                        }
                        list_s.Add(info.Name);
                    }
                }
            }
            catch (Exception ex) { err_log = ex.ToString(); }
            return list_s;
        }

        public static List<string> getVoiceDetails()
        {
            List<string> list_s = new List<string>();
            try
            {
                using (SpeechSynthesizer synth = new SpeechSynthesizer())
                {
                    // Output information about all of the installed voices. 
                    list_s.Add("Installed voices -");
                    foreach (InstalledVoice voice in synth.GetInstalledVoices())
                    {
                        VoiceInfo info = voice.VoiceInfo;
                        string AudioFormats = "";
                        foreach (SpeechAudioFormatInfo fmt in info.SupportedAudioFormats)
                        {
                            AudioFormats += String.Format("{0}\n",
                            fmt.EncodingFormat.ToString());
                        }
                        list_s.Add(" Name:          " + info.Name);
                        list_s.Add(" Culture:       " + info.Culture);
                        list_s.Add(" Age:           " + info.Age);
                        list_s.Add(" Gender:        " + info.Gender);
                        list_s.Add(" Description:   " + info.Description);
                        list_s.Add(" ID:            " + info.Id);
                        list_s.Add(" Enabled:       " + voice.Enabled);
                        if (info.SupportedAudioFormats.Count != 0)
                        {
                            list_s.Add(" Audio formats: " + AudioFormats);
                        }
                        else
                        {
                            list_s.Add(" No supported audio formats found");
                        }

                        string AdditionalInfo = "";
                        foreach (string key in info.AdditionalInfo.Keys)
                        {
                            AdditionalInfo += String.Format("  {0}: {1}\n", key, info.AdditionalInfo[key]);
                        }

                        list_s.Add(" Additional Info - " + AdditionalInfo);
                        list_s.Add("");
                    }
                }
            }
            catch { }
            return list_s;
        }

        public static string lang = "";
        public static void setLanguage(string l)
        {
            lang = l;
            synth[0].SelectVoice(lang);
        }

        public static void setLanguage2()
        {
            var syn = new System.Speech.Synthesis.SpeechSynthesizer();
            syn.SelectVoice("Microsoft Server Speech Text to Speech Voice (ja-JP, Haruka)");
            syn.Speak("こんにちは");
        }
    }
}
