using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Google.Cloud.Speech.V1;
using NAudio.Wave;
using static Google.Cloud.Speech.V1.RecognitionConfig.Types;

namespace SpeechToTextAgent
{
    //dotnet publish --runtime linux-arm --self-contained --framework net6.0
    class SpeechToText
    {
        static void Main()
        {
            string binaryPath = "./audioOut.b";
            int sampleRate = 8750;
            string wavPath = "./audioOut.wav";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "./SECRET-GOOGLE-TOKEN.json");

            WriteWavFile(ReadBinaryFile(binaryPath), sampleRate, wavPath);
            string text = GetTextFromGoogle(wavPath, sampleRate);
            Console.WriteLine(text);
            WriteTextToTxtFile(text, "./ScannedAudio.txt");
        }

        static string GetTextFromGoogle(string filePath, int sampleRate)
        {
            RecognitionAudio audio = RecognitionAudio.FromFile(filePath);

            SpeechClient client = SpeechClient.Create();

            SpeechContext context = new SpeechContext { Phrases = { "elliott hall of music", "elliot", "music", "physics building", "physic", "hovde hall of administration", "hovde", "admin", "brown laboratory of chemistry", "brown", "neil armstrong hall of engineering", "neil", "armstrong", "strong", "heine pharmacy", "heine", "pharmacy", "union club hotel", "club", "hotel", "stewart center", "stew", "heavilon hall", "heavilon", "student health center", "health", "knoy hall", "knoy", "johnson hall of nursing", "johnson", "nurs", "r. b. wetherill laboratory of chemistry", "wetherill", "purdue student union", "purdue", "data science", "data", "grissom hall", "grissom", "hampton hall of civil engineering", "hampton", "civil", "forney hall of chemical engineering", "forney", "chemical", "schleman hall", "schleman", "haas hall", "haas", "has", "potter engineering center", "potter", "armory", "matthews hall", "matthew", "mathew", "psychological sciences building", "psych", "beering hall", "beer", "mathematical sciences building", "math", "stone hall", "stone", "materials science and electrical engineering", "material", "msee", "electrical engineering", "ee", "mechanical engineering", "mechanical", "me", "university hall", "UH", "northwestern parking garage", "northwestern", "parking", "garage", "university lutheran church", "church", "wang hall", "wang", "class of 1950 lecture hall", "1950", "nineteen", "fifty", "pierce hall", "pierce", "wilmeth active learning center", "wilmeth", "active", "walk", "engineering and polytechnic gateway", "polytechnic", "gateway", "chaney hale hall of science", "chaney", "hale", "stanley coulter hall", "stan", "coulter" } };

            RecognitionConfig config = new RecognitionConfig
            {
                Encoding = AudioEncoding.EncodingUnspecified,
                SampleRateHertz = sampleRate,
                LanguageCode = LanguageCodes.English.UnitedStates,
                SpeechContexts = { context }
            };
            RecognizeResponse response = client.Recognize(config, audio);
            string responseString = response.Results.ToString();

            WriteTextToTxtFile(responseString, "./GoogleLog.txt");

            string text = string.Empty;
            if (responseString.Contains("\"transcript\": \""))
            {
                text = responseString.Split("\"transcript\": \"")[1].Split("\"")[0];
            }
            return text;
        }

        static List<float> ReadBinaryFile(string filePath)
        {
            List<float> audioValues = new List<float>();
            if (File.Exists(filePath))
            {
                FileStream stream = File.Open(filePath, FileMode.Open);
                BinaryReader reader = new BinaryReader(stream);
                while (stream.Position < stream.Length)
                {
                    float audioVal = reader.ReadSingle();
                    audioValues.Add(audioVal);
                }

                stream.Dispose();
            }

            return audioValues;
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
            writer.Dispose();
        }

        static void WriteTextToTxtFile(string text, string outputPath)
        {
            StreamWriter fileWriter = File.CreateText(outputPath);
            fileWriter.WriteLine(text);
            fileWriter.Flush();
            fileWriter.Dispose();
        }
    }
}
