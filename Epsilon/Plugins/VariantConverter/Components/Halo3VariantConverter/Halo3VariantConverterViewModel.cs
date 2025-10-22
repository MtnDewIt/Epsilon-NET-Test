using CacheEditor;
using EpsilonLib.Commands;
using EpsilonLib.Dialogs;
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
using TagTool.BlamFile.Chunks;
using TagTool.BlamFile.Chunks.Metadata;
using TagTool.Cache;
using TagTool.Common;
using TagTool.IO;
using TagTool.Tags;

namespace VariantConverter.Components.Halo3VariantConverter
{
    class Halo3VariantConverterViewModel : Screen
    {
        private IShell _shell;
        private ICacheFile _cacheFile;
        private string _output;
        private bool _inProgress;
        private string _cacheInputPath;
        private string _outputPath;
        private Stopwatch _stopWatch = new Stopwatch();
        private List<ulong> _uniqueIdTable = new List<ulong>();
        private int _errorCount;

        private static readonly string[] ValidExtensions =
        {
            ".assault",
            ".bin",
            ".clip",
            ".ctf",
            ".film",
            ".jugg",
            ".koth",
            ".map",
            ".mvar",
            ".oddball",
            ".sandbox",
            ".shot",
            ".slayer",
            ".terries",
            ".vip",
            ".zombiez",
        };

        private static readonly Dictionary<int, string> MapIdToFilename = new Dictionary<int, string>()
        {
            [030] = "zanzibar",
            [300] = "construct",
            [310] = "deadlock",
            [320] = "guardian",
            [330] = "isolation",
            [340] = "riverworld",
            [350] = "salvation",
            [360] = "snowbound",
            [380] = "chill",
            [390] = "cyberdyne",
            [400] = "shrine",
            [410] = "bunkerworld",
            [440] = "docks",
            [470] = "sidewinder",
            [480] = "warehouse",
            [490] = "descent",
            [500] = "spacecamp",
            [520] = "lockout",
            [580] = "armory",
            [590] = "ghosttown",
            [600] = "chillout",
            [720] = "midship",
            [730] = "sandbox",
            [740] = "fortress",
        };

        private static readonly Dictionary<ContentItemMetadata.ContentItemType, string> ContentTypeToFileExtension = new Dictionary<ContentItemMetadata.ContentItemType, string>()
        {
            [ContentItemMetadata.ContentItemType.None] = ".bin",
            [ContentItemMetadata.ContentItemType.CTF] = ".ctf",
            [ContentItemMetadata.ContentItemType.Slayer] = ".slayer",
            [ContentItemMetadata.ContentItemType.Oddball] = ".oddball",
            [ContentItemMetadata.ContentItemType.King] = ".koth",
            [ContentItemMetadata.ContentItemType.Juggernaut] = ".jugg",
            [ContentItemMetadata.ContentItemType.Territories] = ".terries",
            [ContentItemMetadata.ContentItemType.Assault] = ".assault",
            [ContentItemMetadata.ContentItemType.Infection] = ".zombiez",
            [ContentItemMetadata.ContentItemType.VIP] = ".vip",
            [ContentItemMetadata.ContentItemType.Usermap] = ".map",
        };

        public Halo3VariantConverterViewModel(IShell shell, ICacheFile cacheFile) 
        {
            _shell = shell;
            _cacheFile = cacheFile;

            _outputPath = "";

            DisplayName = "Halo 3 Variant Converter";
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

        public string CacheInputPath 
        {
            get => _cacheInputPath;
            set 
            {
                SetAndNotify(ref _cacheInputPath, value);
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
                if (ValidateCacheInputPath(CacheInputPath))
                {
                    var mapsDirectory = new DirectoryInfo(CacheInputPath);

                    _stopWatch.Start();

                    for (int i = 0; i < Files.Count; i++)
                    {
                        var filePath = Path.GetFullPath(Files[i]);
                        progress.Report($"Converting Variant '{filePath}'...", false, (i + 1) / (float)Files.Count);
                        await Task.Run(() => ConvertFileAsync(filePath, mapsDirectory));
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
                }

                InProgress = false;
            }
        }

        private void ConvertFileAsync(string filePath, DirectoryInfo mapsDirectory)
        {
            var input = new FileInfo(filePath);
            var blf = new Blf(CacheVersion.Halo3Retail, CachePlatform.Original);

            string variantName = "";
            ulong uniqueId = 0;
            ContentItemMetadata.ContentItemType contentType = ContentItemMetadata.ContentItemType.None;

            try
            {
                using (var stream = input.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var reader = new EndianReader(stream);

                    blf.Read(reader);

                    if (blf.MapVariant != null)
                    {
                        var sourceCache = GetMapCache(mapsDirectory, blf.MapVariant.MapVariant.MapId);

                        blf.MapVariantTagNames = new BlfMapVariantTagNames()
                        {
                            Signature = new Tag("tagn"),
                            Length = (int)TagStructure.GetStructureSize(typeof(BlfMapVariantTagNames), blf.Version, CachePlatform.Original),
                            MajorVersion = 1,
                            MinorVersion = 0,
                            Names = Enumerable.Range(0, 256).Select(x => new BlfMapVariantTagNames.TagName()).ToArray(),
                        };
                        blf.ContentFlags |= Blf.BlfFileContentFlags.MapVariantTagNames;

                        // The main issue is the scenario object placements. If objects still aren't appearing on the map, try decreasing the scenario object count

                        for (int i = 0; i < blf.MapVariant.MapVariant.Quotas.Length; i++)
                        {
                            var quota = blf.MapVariant.MapVariant.Quotas[i];

                            var quotaObject = sourceCache.TagCache.GetTag((uint)quota.ObjectDefinitionIndex);

                            if (quotaObject != null)
                            {
                                blf.MapVariantTagNames.Names[i].Name = $"{quotaObject.Name}.{quotaObject.Group.Tag}";
                                quota.ObjectDefinitionIndex = 0;
                                quota.MaximumCount = 255;
                                quota.MaxAllowed = -1;
                            }
                            else
                            {
                                quota.ObjectDefinitionIndex = 0;
                                quota.MaxAllowed = -1;
                                quota.Cost = -1;
                            }
                        }

                        var newObjectIndexes = new short[16];

                        for (int i = 0; i < blf.MapVariant.MapVariant.ObjectTypeStartIndex.Length; i++)
                        {
                            newObjectIndexes[i] = blf.MapVariant.MapVariant.ObjectTypeStartIndex[i];
                        }

                        blf.MapVariant.MapVariant.ObjectTypeStartIndex = newObjectIndexes;
                    }

                    blf.Version = CacheVersion.EldoradoED;
                    blf.CachePlatform = CachePlatform.Original;
                    blf.Format = EndianFormat.LittleEndian;

                    uniqueId = blf.ContentHeader?.Metadata?.UniqueId ?? 0;
                    variantName = blf.ContentHeader?.Metadata?.Name ?? "";
                    contentType = blf.ContentHeader?.Metadata?.ContentType ?? ContentItemMetadata.ContentItemType.None;
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

        private GameCache GetMapCache(DirectoryInfo mapsDirectory, int mapId)
        {
            var mapFile = new FileInfo(Path.Combine(mapsDirectory.FullName, $"{MapIdToFilename[mapId]}.map"));
            if (!mapFile.Exists)
            {
                throw new Exception($"'{MapIdToFilename[mapId]}.map' could not be found.");
            }
            return GameCache.Open(mapFile);
        }

        private string GetOutputPath(string variantName, ContentItemMetadata.ContentItemType contentType, ulong uniqueId)
        {
            var filteredName = Regex.Replace($"{variantName.TrimStart().TrimEnd().TrimEnd('.')}", @"[<>:""/\|?*]", "_");

            string outputPath = contentType == ContentItemMetadata.ContentItemType.Usermap ? Path.Combine(OutputPath, $@"map_variants", filteredName, $@"sandbox{ContentTypeToFileExtension[contentType]}") : Path.Combine(OutputPath, $@"game_variants", filteredName, $@"variant{ContentTypeToFileExtension[contentType]}");

            if (Path.Exists(outputPath) && _uniqueIdTable.Contains(uniqueId))
            {
                throw new Exception("Duplicate Variant");
            }
            else
            {
                return outputPath;
            }
        }

        private bool ValidateCacheInputPath(string path) 
        {
            if (string.IsNullOrEmpty(path)) 
            {
                var alert = new AlertDialogViewModel
                {
                    AlertType = Alert.Error,
                    Message = $"Halo 3 cache file path has not been defined!"
                };
                _shell.ShowDialog(alert);

                return false;
            }

            return true;
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
