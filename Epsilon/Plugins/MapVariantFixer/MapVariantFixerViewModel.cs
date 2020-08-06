using CacheEditor;
using EpsilonLib.Commands;
using Shared;
using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Windows;
using TagTool.BlamFile;
using TagTool.Cache;
using TagTool.IO;

namespace MapVariantFixer
{
    class MapVariantFixerViewModel : Screen
    {
        private IShell _shell;
        private ICacheFile _cacheFile;
        private string _output;
        private bool _inProgress;

        public MapVariantFixerViewModel(IShell shell, ICacheFile cacheFile)
        {
            _shell = shell;
            _cacheFile = cacheFile;

            DisplayName = "Map Variant Fixer";
            StartCommand = new DelegateCommand(Start, () => Files.Count > 0 && !_inProgress);
            ClearCommand = new DelegateCommand(ClearFiles, () => Files.Count > 0 && !_inProgress);

            Files.CollectionChanged += Files_CollectionChanged;

            var sandboxMapsDir = new DirectoryInfo(Path.Combine(_cacheFile.File.Directory.FullName, "..\\mods\\maps"));
            if (sandboxMapsDir.Exists)
                AddFilesRecursive(sandboxMapsDir);
        }

        public ObservableCollection<string> Files { get; } = new ObservableCollection<string>();
        public string CacheFilePath => _cacheFile.File.FullName;
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

        internal async void Start()
        {
            InProgress = true;
            Output = string.Empty;

            using (var progress = _shell.CreateProgressScope())
            {
                try
                {
                    var backupFilePath = Path.Combine(Directory.GetCurrentDirectory(), GetBackupFileName());

                    WriteLog($"creating backup '{backupFilePath}'...");
                    await CreateBackupAsync(backupFilePath);

                    var baseCache = _cacheFile.Cache;

                    for(int i = 0; i < Files.Count; i++)
                    {
                        var filePath = Path.GetFullPath(Files[i]);
                        progress.Report($"Fixing map variant '{filePath}'...", false, (i+1) / (float)Files.Count);
                        await Task.Run(() => FixMapVariantBlocking(baseCache, filePath));
                    }

                    WriteLog("done.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to map variants. Check the output for details.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    WriteLog(ex.ToString());
                }
                finally
                {
                    InProgress = false;
                }
            }
        }

        void FixMapVariantBlocking(GameCache baseCache, string filePath)
        {
            WriteLog($"fixing '{filePath}'...");
            var sandboxMapFile = new FileInfo(filePath);
            using (var stream = sandboxMapFile.Open(FileMode.Open, FileAccess.ReadWrite))
            {
                var blf = new Blf(TagTool.Cache.CacheVersion.HaloOnline106708);
                blf.Read(new EndianReader(stream));

                var reader = new EndianReader(stream);
                var writer = new EndianWriter(stream);

                if (blf.MapVariant == null)
                    return;

                var tagNamesChunk = blf.MapVariantTagNames;
                var palette = blf.MapVariant.MapVariant.Palette;
                for (int i = 0; i < palette.Length; i++)
                {
                    if (palette[i].TagIndex == -1)
                        continue;

                    var name = tagNamesChunk.Names[i].Name;
                    string newName = $"ms30\\{name}";
                    if (baseCache.TagCache.TryGetTag(newName, out CachedTag tag))
                    {
                        tagNamesChunk.Names[i].Name = newName;
                        WriteLog($"prefixed '{tag}'");
                    }
                }

                WriteLog("saving file...");
                stream.Position = 0;
                blf.Write(writer);
            }
        }

        private async Task CreateBackupAsync(string outputPath)
        {
            var zipArchive = new ZipArchive(File.Create(outputPath), ZipArchiveMode.Create);
            var zipEntries = new HashSet<string>();

            foreach (var filePath in Files)
            {
                var sandboxMapFile = new FileInfo(filePath);
                var relativePath = sandboxMapFile.FullName.Replace(sandboxMapFile.Directory.FullName.Replace(sandboxMapFile.Directory.Name, ""), "");
                if (!zipEntries.Add(relativePath))
                    throw new InvalidOperationException($"Could not create backup, conflicting folder names '${relativePath}'");

                var entry = zipArchive.CreateEntry(relativePath);
                using (var entryStream = entry.Open())
                {
                    using (var inputStream = sandboxMapFile.Open(FileMode.Open, FileAccess.Read))
                        await inputStream.CopyToAsync(entryStream);
                }
            }
            zipArchive.Dispose();
        }

        private string GetBackupFileName()
        {
            return $"map_variant_backup_{DateTime.Now.ToFileTime()}.zip";
        }

        internal void ClearFiles()
        {
            Files.Clear();
            Output = string.Empty;
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
            foreach (var file in directory.GetFiles("sandbox.map", SearchOption.AllDirectories))
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
