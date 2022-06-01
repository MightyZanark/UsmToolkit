using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using VGMToolbox.format;

namespace UsmToolkit
{
    public static class Helpers
    {
        public static void ExecuteProcess(string fileName, string arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false,
            };

            Process process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();
            process.WaitForExit();
        }

        public static string CreateFFmpegParameters(CriUsmStream usmStream, string pureFileName, string outputDir)
        {
            string configPath = Path.Combine(AppContext.BaseDirectory, "config.json");
            JoinConfig conf = JsonConvert.DeserializeObject<JoinConfig>(File.ReadAllText(configPath));

            string vaDir = Path.GetDirectoryName(Path.GetFullPath(usmStream.FilePath));
            int i = 0;

            StringBuilder sb = new StringBuilder();
            sb.Append("-hide_banner -loglevel quiet -y ");

            foreach (var video in Directory.GetFiles(vaDir, $"{pureFileName}*{usmStream.FileExtensionVideo}"))
                sb.Append($"-i \"{video}\" ");

            if (usmStream.HasAudio)
                foreach (var audio in Directory.GetFiles(vaDir, $"{pureFileName}*{usmStream.FinalAudioExtension}"))
                {
                    sb.Append($"-i \"{audio}\" ");
                    i++;
                }

            if (i >= 2)
                sb.Append($"-filter_complex amix=inputs={i} ");

            sb.Append($"{conf.VideoParameter} ");

            if (usmStream.HasAudio)
                sb.Append($"{conf.AudioParameter} ");

            sb.Append($"\"{Path.Combine(outputDir ?? string.Empty, $"{pureFileName}.{conf.OutputFormat}")}\"");

            return sb.ToString();
        }
    }
}
