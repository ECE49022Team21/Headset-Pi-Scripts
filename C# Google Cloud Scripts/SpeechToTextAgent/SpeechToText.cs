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

            string binaryPath = "../audioOut.b";
            int sampleRate = 8750;
            string wavPath = "../audioOut.wav";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "../SECRET-GOOGLE-TOKEN.json");

            WriteWavFile(ReadBinaryFile(binaryPath), sampleRate, wavPath);
            string text = GetTextFromGoogle(wavPath, sampleRate);
            Console.WriteLine(text);
            WriteTextToTxtFile(text, "../OutputText.txt");
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
