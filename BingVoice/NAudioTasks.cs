using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Xunit;
using Xunit.Sdk;

namespace BingVoice
{
    public class NAudioTasks
    {
        [Fact]
        public void ConvertAllMp3Files()
        {
            var samplesMp3 = @"..\..\..\samples\mp3\";
            var files = new DirectoryInfo(samplesMp3).GetFiles("*.mp3");
            foreach (var file in files)
            {
                Console.WriteLine("Converting file: {0}", file.FullName);
                ConvertMp3ToWav(file);
            }
        }


        public void ConvertMp3ToWav(FileInfo inputFile)
        {
            var samplesOutWav = @"..\..\..\samples\wav\";
            using (var mp3 = new Mp3FileReader(inputFile.FullName))
            {
                using (var reader = new AudioFileReader(inputFile.FullName))
                {
                    var filename = Path.Combine(samplesOutWav, inputFile.Name.Replace(inputFile.Extension, string.Empty) + ".wav");
                    var outRate = 16000;
                    {
                        var resampler = new WdlResamplingSampleProvider(reader, outRate);
                        WaveFileWriter.CreateWaveFile16(filename, resampler);
                    }
                }
            }
        }

        [Fact]
        public void SplitTo10SecondBlocks()
        {
            var samplesMp3 = @"..\..\..\samples\wav\";
            var files = new DirectoryInfo(samplesMp3).GetFiles("*.wav");
            foreach (var file in files)
            {
                ConvertWavTo10SecondWavs(file);
            }
        }

        private void ConvertWavTo10SecondWavs(FileInfo inputFile)
        {
            var samplesOutWav = @"..\..\..\samples\wav10seconds\";
            using (var inAudio = new WaveFileReader(inputFile.FullName))
            {
                //Calculate required byte[] buffer.
                var buffer = new byte[10*inAudio.WaveFormat.AverageBytesPerSecond];//Assume average will be constant for WAV format.
                int index = 0;
                do
                {
                    var outFile = string.Format("{0}{1}.{2:0000}.wav",
                    samplesOutWav, inputFile.Name.Replace(inputFile.Extension, string.Empty), index);
                    int bytesRead = 0;
                    do
                    {
                        bytesRead = inAudio.Read(buffer, 0, buffer.Length - bytesRead);
                    } while (bytesRead > 0 && bytesRead < buffer.Length);
                    //Write new file
                    using (var waveWriter = new WaveFileWriter(outFile, inAudio.WaveFormat))
                    {
                        waveWriter.Write(buffer, 0, buffer.Length);
                    }
                    index++;
                } while (inAudio.Position < inAudio.Length);
            }
        }
    }
}
