using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Google.Cloud.Speech.V1;
using NAudio.Wave;
using static Google.Cloud.Speech.V1.RecognitionConfig.Types;

namespace SpeechToTextAgent
{
    class SpeechToText
    {
        static void Main(string[] args)
        {
            /*if (args.Length != 2) return;
            int sampleRate = int.Parse(args[0]);
            string binaryPath = args[1];*/

            string binaryPath = "/Users/ishaan/Desktop/ECE Senior Design/Headset Pi Scripts/C# Google Cloud Scripts/SpeechToTextAgent/audioOut.binary";
            int sampleRate = 8000;
            string wavPath = "/Users/ishaan/Desktop/ECE Senior Design/Headset Pi Scripts/C# Google Cloud Scripts/SpeechToTextAgent/audioOut.wav";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "/Users/ishaan/Desktop/ECE Senior Design/Headset Pi Scripts/C# Google Cloud Scripts/SpeechToTextAgent/SECRET-senior-design-project-377818-3bb7983a6c1d.json");

            WriteWavFile(ReadBinaryFile(binaryPath), sampleRate, wavPath);
            string text = GetTextFromGoogle(wavPath, sampleRate);
            Console.WriteLine(text);
        }

        static string GetTextFromGoogle(string filePath, int sampleRate)
        {
            RecognitionAudio audio = RecognitionAudio.FromFile(filePath);

            SpeechClient client = SpeechClient.Create();
            RecognitionConfig config = new RecognitionConfig
            {
                Encoding = AudioEncoding.EncodingUnspecified,
                SampleRateHertz = sampleRate,
                LanguageCode = LanguageCodes.English.UnitedStates,
            };
            RecognizeResponse response = client.Recognize(config, audio);
            string responseString = response.Results.ToString();

            string text = string.Empty;
            if (responseString.Contains("\"transcript\": \""))
            {
                text = responseString.Split("\"transcript\": \"")[1].Split("\"")[0];
            }
            return text;
        }

        static List<float> ReadBinaryFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                FileStream stream = File.Open(filePath, FileMode.Open);
                BinaryReader reader = new BinaryReader(stream);
                List<float> audioValues = new List<float>();
                while (stream.Position < stream.Length)
                {
                    float audioVal = reader.ReadSingle();
                    audioValues.Add(audioVal);
                }

                stream.Dispose();

                return audioValues;
            }
            return null;
        }

        static void WriteWavFile(List<float> audioValues, int sampleRate, string outputPath)
        {
            WaveFormat waveFormat = new WaveFormat(sampleRate, 16, 1);
            WaveFileWriter writer = new WaveFileWriter(outputPath, waveFormat);

            foreach (float audioValue in audioValues)
            {
                writer.WriteSample(audioValue/10);
            }
            writer.Flush();
        }
    }
}
