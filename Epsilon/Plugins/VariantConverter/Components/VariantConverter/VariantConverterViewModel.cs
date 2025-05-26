using CacheEditor;
using EpsilonLib.Commands;
using EpsilonLib.Dialogs;
using Newtonsoft.Json;
using Shared;
using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using TagTool.BlamFile;
using TagTool.Cache;
using TagTool.Common;
using TagTool.IO;
using TagTool.Serialization;
using TagTool.Tags;

namespace VariantConverter.Components.VariantConverter
{
    class VariantConverterViewModel : Screen
    {
        private IShell _shell;
        private ICacheFile _cacheFile;
        private string _output;
        private bool _inProgress;
        private Dictionary<int, string> _061_TagRemapping;
        private string _outputPath;
        private Stopwatch _stopWatch = new Stopwatch();
        private List<ulong> _uniqueIdTable = new List<ulong>();
        private int _errorCount;

        private static readonly string[] ValidExtensions =
        {
            ".assault",
            ".ctf",
            ".jugg",
            ".koth",
            ".oddball",
            ".slayer",
            ".terries",
            ".vip",
            ".sandbox",
            ".zombiez",
            ".map"
        };

        private static readonly Dictionary<ContentItemType, string> ContentTypeToFileExtension = new Dictionary<ContentItemType, string>()
        {
            [ContentItemType.None] = ".bin",
            [ContentItemType.CtfVariant] = ".ctf",
            [ContentItemType.SlayerVariant] = ".slayer",
            [ContentItemType.OddballVariant] = ".oddball",
            [ContentItemType.KingOfTheHillVariant] = ".koth",
            [ContentItemType.JuggernautVariant] = ".jugg",
            [ContentItemType.TerritoriesVariant] = ".terries",
            [ContentItemType.AssaultVariant] = ".assault",
            [ContentItemType.InfectionVariant] = ".zombiez",
            [ContentItemType.VipVariant] = ".vip",
            [ContentItemType.SandboxMap] = ".map",
        };

        public VariantConverterViewModel(IShell shell, ICacheFile cacheFile)
        {
            _shell = shell;
            _cacheFile = cacheFile;

            _outputPath = "";

            DisplayName = "0.6 Variant Converter";
            StartCommand = new DelegateCommand(Start, () => Files.Count > 0 && !_inProgress);
            ClearCommand = new DelegateCommand(ClearFiles, () => Files.Count > 0 && !_inProgress);

            Files.CollectionChanged += Files_CollectionChanged;
        }

        public ObservableCollection<string> Files { get; } = new ObservableCollection<string>();
        public DelegateCommand StartCommand { get; }
        public DelegateCommand ClearCommand { get; }

        public string Output
        {
            get => _output;
            set
            {
                SetAndNotify(ref _output, value);
            }
        }

        public bool InProgress
        {
            get => _inProgress;
            set
            {
                if (SetAndNotify(ref _inProgress, value))
                {
                    StartCommand.RaiseCanExecuteChanged();
                    ClearCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string OutputPath 
        {
            get => _outputPath;
            set 
            {
                SetAndNotify(ref _outputPath, value);
            }
        }

        internal async void Start()
        {
            InProgress = true;
            Output = string.Empty;

            _stopWatch.Reset();
            _errorCount = 0;
            _uniqueIdTable.Clear();

            using (var progress = _shell.CreateProgressScope())
            {
                _stopWatch.Start();

                for (int i = 0; i < Files.Count; i++)
                {
                    var filePath = Path.GetFullPath(Files[i]);
                    progress.Report($"Converting Variant '{filePath}'...", false, (i + 1) / (float)Files.Count);
                    await Task.Run(() => ConvertFileAsync(filePath));
                }

                _stopWatch.Stop();

                var alertType = Alert.Standard;

                if (_errorCount == 0)
                {
                    alertType = Alert.Success;
                }
                else if (_errorCount == Files.Count) 
                {
                    alertType = Alert.Error;
                }
                else if (_errorCount > 0)
                {
                    alertType = Alert.Warning;
                }

                var alert = new AlertDialogViewModel
                {
                    AlertType = alertType,
                    Message = $"{Files.Count - _errorCount}/{Files.Count} Variants Converted Successfully in {_stopWatch.ElapsedMilliseconds.FormatMilliseconds()}{(_errorCount == 0 ? "." : $" with {_errorCount} {(_errorCount == 1 ? "error" : "errors")}. Check the output for details.")}"
                };
                _shell.ShowDialog(alert);

                InProgress = false;
            }
        }

        private void ConvertFileAsync(string filePath)
        {
            var input = new FileInfo(filePath);
            var blf = new Blf(_cacheFile.Cache.Version, _cacheFile.Cache.Platform);

            string variantName = "";
            ulong uniqueId = 0;
            ContentItemType contentType = ContentItemType.None;

            try
            {
                using (var stream = input.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    FixBlfEndianness(stream, blf);
                    
                    if (blf.MapVariantTagNames == null && blf.MapVariant != null)
                    {
                        Convert06Blf(blf);
                    }

                    if (blf.EndOfFile == null)
                    {
                        blf.EndOfFile = new BlfChunkEndOfFile()
                        {
                            Signature = new Tag("_eof"),
                            Length = (int)TagStructure.GetStructureSize(typeof(BlfChunkEndOfFile), blf.Version, _cacheFile.Cache.Platform),
                            MajorVersion = 1,
                            MinorVersion = 1,
                        };
                        blf.ContentFlags |= BlfFileContentFlags.EndOfFile;
                    }

                    uniqueId = blf.ContentHeader?.Metadata?.UniqueId ?? 0;
                    variantName = blf.ContentHeader?.Metadata?.Name ?? "";
                    contentType = blf.ContentHeader?.Metadata?.ContentType ?? ContentItemType.None;
                }

                var output = GetOutputPath(variantName, contentType, uniqueId);

                Directory.CreateDirectory(Path.GetDirectoryName(output));

                using (var stream = new FileInfo(output).Create())
                {
                    var writer = new EndianWriter(stream);
                    blf.Write(writer);
                }

                if (uniqueId != 0)
                {
                    _uniqueIdTable.Add(uniqueId);
                }
            }
            catch (Exception e)
            {
                WriteLog($"Error converting \"{filePath}\" : {e.Message}");
                _errorCount++;
            }
        }

        private void FixBlfEndianness(FileStream stream, Blf blf)
        {
            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);

            using (var memoryStream = new MemoryStream(buffer))
            {
                var deserializer = new TagDeserializer(CacheVersion.HaloOnlineED, CachePlatform.Original);
                var serializer = new TagSerializer(CacheVersion.HaloOnlineED, CachePlatform.Original);

                var reader = new EndianReader(memoryStream, EndianFormat.BigEndian);
                var writer = new EndianWriter(memoryStream, EndianFormat.LittleEndian);
                var readerContext = new DataSerializationContext(reader);
                var writerContext = new DataSerializationContext(writer);

                if (reader.ReadTag() != "_blf")
                {
                    memoryStream.Position = 0;

                    ReadBlf(memoryStream, blf);
                }

                reader.BaseStream.Position = 0;

                while (true)
                {
                    var pos = reader.BaseStream.Position;
                    var header = (BlfChunkHeader)deserializer.Deserialize(readerContext, typeof(BlfChunkHeader));

                    writer.BaseStream.Position = pos;
                    serializer.Serialize(writerContext, header);
                    if (header.Signature == "_eof")
                        break;

                    reader.BaseStream.Position += header.Length - (int)TagStructure.GetStructureSize(typeof(BlfChunkHeader), _cacheFile.Cache.Version, _cacheFile.Cache.Platform);
                }

                memoryStream.Position = 0xC;
                writer.Format = EndianFormat.LittleEndian;
                writer.Write((short)-2);
                memoryStream.Position = 0;

                ReadBlf(memoryStream, blf);
            }
        }

        private void ReadBlf(Stream stream, Blf blf)
        {
            var memoryReader = new EndianReader(stream);

            if (!blf.Read(memoryReader))
                throw new Exception("Unable to parse BLF data");
        }

        private void Convert06Blf(Blf blf)
        {
            if (_061_TagRemapping == null)
            {
                var jsonData = File.ReadAllText($@"{AppContext.BaseDirectory}\Tools\mappings\061_mapping.json");
                _061_TagRemapping = JsonConvert.DeserializeObject<Dictionary<int, string>>(jsonData);
            }

            blf.MapVariantTagNames = new BlfMapVariantTagNames()
            {
                Signature = new Tag("tagn"),
                Length = (int)TagStructure.GetStructureSize(typeof(BlfMapVariantTagNames), blf.Version, CachePlatform.Original),
                MajorVersion = 1,
                MinorVersion = 0,
                Names = Enumerable.Range(0, 256).Select(x => new TagName()).ToArray(),
            };
            blf.ContentFlags |= BlfFileContentFlags.MapVariantTagNames;

            for (int i = 0; i < blf.MapVariant.MapVariant.Quotas.Length; i++)
            {
                var objectIndex = blf.MapVariant.MapVariant.Quotas[i].ObjectDefinitionIndex;

                if (objectIndex != -1 && _061_TagRemapping.TryGetValue(objectIndex, out string name))
                {
                    blf.MapVariantTagNames.Names[i] = new TagName() { Name = name };
                }
            }
        }

        private string GetOutputPath(string variantName, ContentItemType contentType, ulong uniqueId)
        {
            var filteredName = Regex.Replace($"{variantName.TrimStart().TrimEnd().TrimEnd('.')}", @"[<>:""/\|?*]", "_");

            string outputPath = contentType == ContentItemType.SandboxMap ? Path.Combine(OutputPath, $@"map_variants", filteredName, $@"sandbox{ContentTypeToFileExtension[contentType]}") : Path.Combine(OutputPath, $@"game_variants", filteredName, $@"variant{ContentTypeToFileExtension[contentType]}");

            if (Path.Exists(outputPath) && _uniqueIdTable.Contains(uniqueId))
            {
                throw new Exception("Duplicate Variant");
            }
            else
            {
                return outputPath;
            }
        }

        internal void ClearFiles()
        {
            Files.Clear();
            Output = string.Empty;
            _stopWatch.Reset();
            _errorCount = 0;
            _uniqueIdTable.Clear();
        }

        internal void AddFiles(string[] files)
        {
            foreach (var filePath in files)
            {
                FileAttributes attr = File.GetAttributes(filePath);
                if (attr.HasFlag(FileAttributes.Directory))
                {
                    AddFilesRecursive(new DirectoryInfo(filePath));
                }
                else
                {
                    AddFile(new FileInfo(filePath));
                }
            }
        }

        private void AddFilesRecursive(DirectoryInfo directory)
        {
            var files = directory.EnumerateFiles("*.*", SearchOption.AllDirectories).Where(file => ValidExtensions.Contains(Path.GetExtension(file.FullName).ToLower())).ToList();

            foreach (var file in files)
                AddFile(file);
        }

        private void AddFile(FileInfo file)
        {
            if (Files.Contains(file.FullName))
                return;

            Files.Add(file.FullName);
            StartCommand.RaiseCanExecuteChanged();
        }

        private void Files_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ClearCommand.RaiseCanExecuteChanged();
            StartCommand.RaiseCanExecuteChanged();
        }

        private void WriteLog(string output)
        {
            Application.Current.Dispatcher.Invoke(() => Output += $"{output}\n");
        }
    }
}
