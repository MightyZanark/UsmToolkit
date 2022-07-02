using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using VGMToolbox.format;

namespace UsmToolkit
{
    [Command(Description = "Extract audio and video")]
    public class ExtractCommand
    {
        [Required]
        [FileOrDirectoryExists]
        [Argument(0, Description = "File or folder containing usm files")]
        public string InputPath { get; set; }

        protected int OnExecute(CommandLineApplication app)
        {
            FileAttributes attr = File.GetAttributes(InputPath);
            if (attr.HasFlag(FileAttributes.Directory))
            {
                foreach (var file in Directory.GetFiles(InputPath, "*.usm"))
                    Process(file);
            }
            else
                Process(InputPath);

            return 0;
        }

        private void Process(string fileName)
        {
            Console.WriteLine($"File: {fileName}");
            var usmStream = new CriUsmStream(fileName);

            Console.WriteLine("Demuxing...");
            usmStream.DemultiplexStreams(new MpegStream.DemuxOptionsStruct()
            {
                AddHeader = false,
                AddPlaybackHacks = false,
                ExtractAudio = true,
                ExtractVideo = true,
                SplitAudioStreams = false
            });
        }
    }
    
    [Command(Description = "Convert according to the parameters in config.json")]
    public class ConvertCommand
    {
        [Required]
        [FileOrDirectoryExists]
        [Argument(0, Description = "File or folder containing usm files")]
        public string InputPath { get; set; }

        [Option(CommandOptionType.SingleValue, Description = "Specify output directory", ShortName = "o", LongName = "output-dir")]
        public string OutputDir { get; set; }

        [Option(CommandOptionType.NoValue, Description = "Remove temporary video and audio after converting", ShortName = "c", LongName = "clean")]
        public bool CleanTempFiles { get; set; }

        protected int OnExecute(CommandLineApplication app)
        {
            FileAttributes attr = File.GetAttributes(InputPath);
            if (attr.HasFlag(FileAttributes.Directory))
            {
                foreach (var file in Directory.GetFiles(InputPath, "*.usm"))
                    Process(file);
            }
            else
                Process(InputPath);

            return 0;
        }

        private void Process(string fileName)
        {
            Console.WriteLine($"File: {fileName}");
            var usmStream = new CriUsmStream(fileName);

            Console.WriteLine("Demuxing...");
            usmStream.DemultiplexStreams(new MpegStream.DemuxOptionsStruct()
            {
                AddHeader = false,
                AddPlaybackHacks = false,
                ExtractAudio = true,
                ExtractVideo = true,
                SplitAudioStreams = false
            });

            if (!string.IsNullOrEmpty(OutputDir) && !Directory.Exists(OutputDir))
                Directory.CreateDirectory(OutputDir);

            JoinOutputFile(usmStream);
        }

        private void JoinOutputFile(CriUsmStream usmStream)
        {
            string configPath = Path.Combine(AppContext.BaseDirectory, "config.json");
            if (!File.Exists(configPath))
            {
                Console.WriteLine("ERROR: config.json not found!");
                return;
            }

            // var audioFormat = usmStream.FinalAudioExtension;
            var pureFileName = Path.GetFileNameWithoutExtension(usmStream.FilePath);

            // if (audioFormat == ".adx")
            // {
            //     //ffmpeg can not handle .adx from 0.2 for whatever reason
            //     //need vgmstream to format that to wav
            //     // if (!Directory.Exists("vgmstream"))
            //     // {
            //     //     Console.WriteLine("ERROR: vgmstream folder not found!");
            //     //     return;
            //     // }
            //     

            //     Console.WriteLine("adx audio detected, convert to wav...");
            //     Helpers.ExecuteProcess("vgmstream/test.exe", $"\"{Path.ChangeExtension(usmStream.FilePath, usmStream.FinalAudioExtension)}\" -o \"{Path.ChangeExtension(usmStream.FilePath, "wav")}\"");

            //     usmStream.FinalAudioExtension = ".wav";
            // }
            // Zanark's comment: FFmpeg now works with .adx files again, so vgmstream is not required

            Console.WriteLine($"Converting file: {pureFileName}");
            Helpers.ExecuteProcess("ffmpeg", Helpers.CreateFFmpegParameters(usmStream, pureFileName, OutputDir));

            if (CleanTempFiles)
            {
                Console.WriteLine($"Cleaning up temporary files from {pureFileName}");

                string tempFilePath = Path.GetDirectoryName(Path.GetFullPath(usmStream.FilePath));

                //Deletes temp video files created after usm extraction
                foreach (var tempVideo in Directory.GetFiles(tempFilePath, $"{pureFileName}*{usmStream.FileExtensionVideo}"))
                    File.Delete(tempVideo);

                //Deletes temp audio files created after usm extraction
                if (usmStream.HasAudio)
                {
                    foreach (var tempAudio in Directory.GetFiles(tempFilePath, $"{pureFileName}*{usmStream.FinalAudioExtension}"))
                    File.Delete(tempAudio);
                }

                //Deletes the source .usm file
                File.Delete(usmStream.FilePath);
            }

            Console.WriteLine("Cleaning up completed!");
        }
    }

    [Command(Description = "Setup ffmpeg needed for conversion")]
    public class GetDependenciesCommand
    {
        protected int OnExecute(CommandLineApplication app)
        {
            string depsPath = Path.Combine(AppContext.BaseDirectory, "deps.json");
            DepsConfig conf = JsonConvert.DeserializeObject<DepsConfig>(File.ReadAllText(depsPath));
            WebClient client = new WebClient();
            
            string ffmpegZipPath = Path.Combine(AppContext.BaseDirectory, "ffmpeg.zip");
            string ffmpegExePath = Path.Combine(AppContext.BaseDirectory, "ffmpeg.exe");
            Console.WriteLine($"Downloading FFmpeg from {conf.FFmpeg}\nThis action may take a while");
            client.DownloadFile(conf.FFmpeg, ffmpegZipPath);

            Console.WriteLine($"Extracting ffmpeg...");
            using (ZipArchive archive = ZipFile.OpenRead(ffmpegZipPath))
            {
                var ent = archive.Entries.FirstOrDefault(x => x.Name == "ffmpeg.exe");
                if (ent != null)
                {
                    ent.ExtractToFile(ffmpegExePath, true);
                }
            }
            File.Delete(ffmpegZipPath);
            Console.WriteLine("Extraction complete and deleted unnecessary files");

            // Console.WriteLine($"Downloading vgmstream from {conf.Vgmstream}");
            // client.DownloadFile(conf.Vgmstream, "vgmstream.zip");

            // Console.WriteLine("Extracting vgmstream...");
            // ZipFile.ExtractToDirectory("vgmstream.zip", "vgmstream", true);
            // File.Delete("vgmstream.zip");

            return 0;
        }
    }
}
