using CacheEditor;
using EpsilonLib.Commands;
using Shared;
using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using TagTool.BlamFile.Reach;
using TagTool.BlamFile;
using TagTool.Cache;
using TagTool.Common;
using TagTool.IO;
using TagTool.Tags.Definitions;
using EpsilonLib.Dialogs;

namespace VariantConverter.Components.HaloReachVariantConverter
{
    class HaloReachVariantConverterViewModel : Screen
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
            ".map",
            ".mvar",
        };

        private static readonly Dictionary<int, string> MapIdToFilename = new Dictionary<int, string>()
        {
            [1000] = "20_sword_slayer",
            [1020] = "45_launch_station",
            [1035] = "50_panopticon",
            [1040] = "45_aftship",
            [1055] = "30_settlement",
            [1080] = "70_boneyard",
            [1150] = "52_ivory_tower",
            [1200] = "35_island",
            [1500] = "condemned",
            [1510] = "trainingpreserve",
            [2001] = "dlc_slayer",
            [2002] = "dlc_invasion",
            [2004] = "dlc_medium",
            [3006] = "forge_halo",
            [10010] = "cex_damnation",
            [10020] = "cex_beaver_creek",
            [10030] = "cex_timberland",
            [10050] = "cex_headlong",
            [10060] = "cex_hangemhigh",
            [10070] = "cex_prisoner",
        };

        private static readonly Dictionary<ContentItemType, string> ContentTypeToFileExtension = new Dictionary<ContentItemType, string>()
        {
            [ContentItemType.SandboxMap] = ".map",
        };

        public HaloReachVariantConverterViewModel(IShell shell, ICacheFile cacheFile)
        {
            _shell = shell;
            _cacheFile = cacheFile;

            _outputPath = "";

            DisplayName = "Halo Reach Variant Converter";
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
            Blf convertedBlf = null;

            string variantName = "";
            ulong uniqueId = 0;
            ContentItemType contentType = ContentItemType.None;

            try
            {
                Dictionary<Tag, BlfChunk> blfChunks;
                using (var inputStream = input.OpenRead())
                    blfChunks = BlfReader.ReadChunks(inputStream).ToDictionary(c => c.Tag);

                if (blfChunks.ContainsKey("mvar"))
                {
                    convertedBlf = ConvertMapVariant(mapsDirectory, blfChunks["mvar"]);
                }
                else if (blfChunks.ContainsKey("_cmp"))
                {
                    var decompressed = DecompressChunk(blfChunks["_cmp"]);
                    var chunk = BlfReader.ReadChunks(new MemoryStream(decompressed)).First();
                    if (chunk.Tag != "mvar")
                        throw new Exception("Unsupported input file");

                    convertedBlf = ConvertMapVariant(mapsDirectory, chunk);
                }
                else
                {
                    throw new Exception("Unsupported input file");
                }

                if (convertedBlf == null)
                    throw new Exception("Failed to convert map variant");

                variantName = convertedBlf?.ContentHeader?.Metadata?.Name ?? "";
                uniqueId = convertedBlf?.ContentHeader?.Metadata?.UniqueId ?? 0;
                contentType = convertedBlf?.ContentHeader?.Metadata?.ContentType ?? ContentItemType.None;

                var output = GetOutputPath(variantName, contentType, uniqueId);

                Directory.CreateDirectory(Path.GetDirectoryName(output));

                using (var stream = new FileInfo(output).Create())
                {
                    var writer = new EndianWriter(stream);
                    convertedBlf.Write(writer);
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

        private Blf ConvertMapVariant(DirectoryInfo mapsDirectory, BlfChunk chunk)
        {
            var stream = new MemoryStream(chunk.Data);

            if (chunk.MajorVerson == 31)
            {
                return ConvertReachMapVariant(stream, mapsDirectory);
            }
            else
            {
                throw new Exception("Unsupported Map Variant version");
            }
        }

        private Blf ConvertReachMapVariant(MemoryStream stream, DirectoryInfo mapsDirectory)
        {
            var sourceBlf = new ReachBlfMapVariant();
            sourceBlf.Decode(stream);

            int mapId = sourceBlf.MapVariant.MapId;
            var sourceCache = GetMapCache(mapsDirectory, mapId);
            if (sourceCache == null)
                return null;

            var sourceCacheStream = sourceCache.OpenCacheRead();
            var sourceScenario = sourceCache.Deserialize<Scenario>(sourceCacheStream, sourceCache.TagCache.FindFirstInGroup("scnr"));
            if (sourceScenario.MapId != mapId)
            {
                throw new Exception($"Scenario map id did not match");
            }

            var converter = new ReachMapVariantConverter();

            // hardcode for now
            converter.SubstitutedTags.Add(@"objects\vehicles\human\warthog\warthog.vehi", @"objects\vehicles\warthog\warthog.vehi");
            converter.SubstitutedTags.Add(@"objects\vehicles\human\mongoose\mongoose.vehi", @"objects\vehicles\mongoose\mongoose.vehi");
            converter.SubstitutedTags.Add(@"objects\vehicles\human\scorpion\scorpion.vehi", @"objects\vehicles\scorpion\scorpion.vehi");
            //converter.SubstitutedTags.Add(@"objects\vehicles\human\falcon\falcon.vehi", @"objects\vehicles\hornet\hornet.vehi");
            converter.SubstitutedTags.Add(@"objects\vehicles\covenant\ghost\ghost.vehi", @"objects\vehicles\ghost\ghost.vehi");
            converter.SubstitutedTags.Add(@"objects\vehicles\covenant\wraith\wraith.vehi", @"objects\vehicles\wraith\wraith.vehi");
            converter.SubstitutedTags.Add(@"objects\vehicles\covenant\banshee\banshee.vehi", @"objects\vehicles\banshee\banshee.vehi");
            converter.SubstitutedTags.Add(@"objects\vehicles\human\turrets\machinegun\machinegun.vehi", @"objects\weapons\turret\machinegun_turret\machinegun_turret.vehi");
            converter.SubstitutedTags.Add(@"objects\vehicles\covenant\turrets\plasma_turret\plasma_turret_mounted.vehi", @"objects\weapons\turret\plasma_cannon\plasma_cannon.vehi");
            converter.SubstitutedTags.Add(@"objects\vehicles\covenant\turrets\shade\shade.vehi", @"objects\vehicles\shade\shade.vehi");
            //converter.SubstitutedTags.Add(@"objects\vehicles\covenant\revenant\revenant.vehi", @"objects\vehicles\ghost\ghost.vehi");

            converter.SubstitutedTags.Add(@"objects\equipment\hologram\hologram.eqip", @"objects\equipment\hologram_equipment\hologram_equipment.eqip");
            converter.SubstitutedTags.Add(@"objects\equipment\active_camouflage\active_camouflage.eqip", @"objects\equipment\invisibility_equipment\invisibility_equipment.eqip");
            converter.SubstitutedTags.Add(@"objects\levels\shared\device_controls\health_station\health_station.ctrl", @"objects\powerups\health_pack\health_pack_large.eqip");

            converter.SubstitutedTags.Add(@"objects\weapons\melee\energy_sword\energy_sword.weap", @"objects\weapons\melee\energy_blade\energy_blade.weap");
            converter.SubstitutedTags.Add(@"objects\levels\shared\golf_club\golf_club.weap", @"objects\weapons\melee\gravity_hammer\gravity_hammer.weap");

            converter.SubstitutedTags.Add(@"objects\multi\models\mp_hill_beacon\mp_hill_beacon.bloc", @"objects\multi\koth\koth_hill_static.bloc");
            converter.SubstitutedTags.Add(@"objects\multi\models\mp_flag_base\mp_flag_base.bloc", @"objects\multi\ctf\ctf_flag_spawn_point.bloc");
            converter.SubstitutedTags.Add(@"objects\multi\models\mp_circle\mp_circle.bloc", @"objects\multi\oddball\oddball_ball_spawn_point.bloc");
            converter.SubstitutedTags.Add(@"objects\multi\archive\vip\vip_boundary.bloc", @"objects\multi\vip\vip_destination_static.bloc");

            converter.ExcludedTags.Add(@"objects\multi\spawning\weak_anti_respawn_zone.scen");
            converter.ExcludedTags.Add(@"objects\multi\spawning\weak_respawn_zone.scen");
            converter.ExcludedTags.Add(@"objects\multi\boundaries\soft_safe_volume.scen");
            converter.ExcludedTags.Add(@"objects\multi\boundaries\soft_kill_volume.scen");
            converter.ExcludedTags.Add(@"objects\multi\boundaries\kill_volume.scen");
            //converter.ExcludedTags.Add(@"objects\multi\spawning\respawn_zone.scen");

            converter.ExcludedTags.Add(@"objects\levels\shared\screen_fx_orb\fx\juicy.bloc");
            converter.ExcludedTags.Add(@"objects\levels\shared\screen_fx_orb\fx\colorblind.bloc");
            converter.ExcludedTags.Add(@"objects\levels\shared\screen_fx_orb\fx\dusk.bloc");
            converter.ExcludedTags.Add(@"objects\levels\shared\screen_fx_orb\fx\golden_hour.bloc");
            converter.ExcludedTags.Add(@"objects\levels\shared\screen_fx_orb\fx\gloomy.bloc");
            converter.ExcludedTags.Add(@"objects\levels\shared\screen_fx_orb\fx\olde_timey.bloc");
            converter.ExcludedTags.Add(@"objects\levels\shared\screen_fx_orb\fx\eerie.bloc");
            converter.ExcludedTags.Add(@"objects\levels\shared\screen_fx_orb\fx\pen_and_ink.bloc");

            //converter.ExcludedTags.Add(@"objects\levels\forge\ff_light_flash_yellow\ff_light_flash_yellow.bloc");
            //converter.ExcludedTags.Add(@"objects\levels\forge\ff_light_flash_red\ff_light_flash_red.bloc");
            //converter.ExcludedTags.Add(@"objects\levels\forge\ff_light_red\ff_light_red.bloc");
            //converter.ExcludedTags.Add(@"objects\levels\forge\ff_light_white\ff_light_white.bloc");
            //converter.ExcludedTags.Add(@"objects\levels\forge\ff_light_green\ff_light_green.bloc");
            //converter.ExcludedTags.Add(@"objects\levels\forge\ff_light_yellow\ff_light_yellow.bloc");
            //converter.ExcludedTags.Add(@"objects\levels\forge\ff_light_orange\ff_light_orange.bloc");
            //converter.ExcludedTags.Add(@"objects\levels\forge\ff_light_purple\ff_light_purple.bloc");
            //converter.ExcludedTags.Add(@"objects\levels\forge\ff_light_blue\ff_light_blue.bloc");

            converter.ExcludedTags.Add(@"objects\equipment\jet_pack\jet_pack.eqip");
            converter.ExcludedTags.Add(@"objects\equipment\sprint\sprint.eqip");
            converter.ExcludedTags.Add(@"objects\equipment\evade\evade.eqip");

            converter.ExcludedMegaloLabels.Add("hh_drop_point");
            converter.ExcludedMegaloLabels.Add("inv_cinematic");
            converter.ExcludedMegaloLabels.Add("inv_gates");
            converter.ExcludedMegaloLabels.Add("inv_mancannon");
            converter.ExcludedMegaloLabels.Add("inv_no_core_zone");
            converter.ExcludedMegaloLabels.Add("inv_obj_flag");
            converter.ExcludedMegaloLabels.Add("inv_objective");
            converter.ExcludedMegaloLabels.Add("inv_platform");
            converter.ExcludedMegaloLabels.Add("inv_res_p1");
            converter.ExcludedMegaloLabels.Add("inv_res_p2");
            converter.ExcludedMegaloLabels.Add("inv_res_p3");
            converter.ExcludedMegaloLabels.Add("inv_res_zone");
            converter.ExcludedMegaloLabels.Add("inv_slayer");
            converter.ExcludedMegaloLabels.Add("inv_slayer_drop");
            converter.ExcludedMegaloLabels.Add("inv_slayer_res_zone");
            converter.ExcludedMegaloLabels.Add("inv_vehicle");
            converter.ExcludedMegaloLabels.Add("inv_weapon");
            converter.ExcludedMegaloLabels.Add("invasion");
            converter.ExcludedMegaloLabels.Add("invasion_slayer");
            converter.ExcludedMegaloLabels.Add("race");
            converter.ExcludedMegaloLabels.Add("race_flag");
            converter.ExcludedMegaloLabels.Add("rally");
            converter.ExcludedMegaloLabels.Add("rally_flag");
            converter.ExcludedMegaloLabels.Add("stockpile");
            converter.ExcludedMegaloLabels.Add("stockpile_flag");
            converter.ExcludedMegaloLabels.Add("stp_flag");
            converter.ExcludedMegaloLabels.Add("stp_goal");

            return converter.Convert(sourceScenario, sourceBlf);
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

        private static byte[] DecompressChunk(BlfChunk cmpChunk)
        {
            var stream = new MemoryStream(cmpChunk.Data);
            var reader = new EndianReader(stream, EndianFormat.BigEndian);
            var compressionType = reader.ReadSByte();
            if (compressionType != 0)
                throw new NotSupportedException();

            var size = reader.ReadInt32();
            reader.ReadBytes(2); // skip header
            var compressed = reader.ReadBytes(size - 2);
            return Decompress(compressed);
        }

        static byte[] Decompress(byte[] compressed)
        {
            using (var stream = new DeflateStream(new MemoryStream(compressed), CompressionMode.Decompress))
            {
                var outStream = new MemoryStream();
                stream.CopyTo(outStream);
                return outStream.ToArray();
            }
        }

        private string GetOutputPath(string variantName, ContentItemType contentType, ulong uniqueId)
        {
            var filteredName = Regex.Replace($"{variantName.TrimStart().TrimEnd().TrimEnd('.')}", @"[<>:""/\|?*]", "_");

            string outputPath = Path.Combine(OutputPath, $@"map_variants", filteredName, $@"sandbox{ContentTypeToFileExtension[contentType]}");

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
                    Message = $"Halo Reach cache file path has not been defined!"
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
