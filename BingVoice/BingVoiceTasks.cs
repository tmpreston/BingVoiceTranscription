using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using SpeechSample;
using Xunit;

namespace BingVoice
{
    public class BingVoiceTasks
    {
        [Fact]
        public void ConvertAudioSamplesToTranscript()
        {
            //Input files
            var wavsIn10SecIncrementsFolder = @"..\..\..\samples\wav10seconds\";
            var allFiles = new DirectoryInfo(wavsIn10SecIncrementsFolder).GetFiles("*.wav");
            var commonFileNameRegex = new Regex("(?<Name>.+)\\.(?<Index>\\d+)\\.wav");
            var toTranscribe = from file in allFiles
                let commonName = commonFileNameRegex.Match(file.Name).Groups["Name"].Value
                group file by commonName;
            foreach (var listToProcess in toTranscribe)
            {
                System.Diagnostics.Debug.WriteLine("Processing : " + listToProcess.Key);
                var dataProcessed = new List<string>();
                var fileToProcessInOrder = listToProcess.OrderBy(zz => zz.Name);
                foreach (var fileInfo in fileToProcessInOrder)
                {
                    System.Diagnostics.Debug.WriteLine(".." + fileInfo.Name);
                    var dataSoFar = Program.Convert(fileInfo.FullName);
                    System.Diagnostics.Debug.WriteLine("." + Environment.NewLine + string.Join(" ", dataSoFar.ToArray()) +
                                                       Environment.NewLine);
                    System.Diagnostics.Debug.WriteLine("..success.." + fileInfo.Name);
                    dataProcessed.AddRange(dataSoFar);
                }
                //At the end write the transcription.
                var transcriptionOutFolder = @"..\..\..\samples\transcriptions\";
                var outFile = string.Format("{0}{1}.txt", transcriptionOutFolder, listToProcess.Key);
                using (var sw = new StreamWriter(outFile, false))
                {
                    foreach (var data in dataProcessed)
                    {
                        sw.Write(data);
                    }
                }
            }
        }

        [Fact]
        public void DecomposeResponse()
        {
            var sampleJson = @"{
  ""version"": ""3.0"",
  ""header"": {
                ""status"": ""success"",
    ""scenario"": ""smd"",
    ""name"": ""Hey rock at a time for NBC Oslos again June 15th through 19th in Oslo Norway Richard and I will be there of course."",
    ""lexical"": ""hey rock at a time for nbc oslos again june fifteenth through nineteenth in oslo norway richard and i will be there of course"",
    ""properties"": {
                    ""requestid"": ""c121f991-ffad-4ded-9ce9-bd041cad0ae9"",
      ""HIGHCONF"": ""1""
    }
            },
  ""results"": [
    {
      ""scenario"": ""smd"",
      ""name"": ""Hey rock at a time for NBC Oslos again June 15th through 19th in Oslo Norway Richard and I will be there of course."",
      ""lexical"": ""hey rock at a time for nbc oslos again june fifteenth through nineteenth in oslo norway richard and i will be there of course"",
      ""confidence"": ""0.7632182"",
      ""properties"": { ""HIGHCONF"": ""1"" }
}
  ]
}";
            dynamic stuff = JsonConvert.DeserializeObject(sampleJson);
            var header = stuff["header"];
            var success = header["status"];
            var results = stuff["results"];
            var data = results[0]["name"];
        }
    }
}