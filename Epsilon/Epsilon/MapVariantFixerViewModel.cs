﻿using Epsilon.Commands;
using Epsilon.Dialogs;
using Shared;
using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TagTool.BlamFile;
using TagTool.Cache;
using TagTool.Common;
using TagTool.IO;
using TagTool.Serialization;
using TagTool.Tags;

namespace Epsilon
{
	public class MapVariantFixerViewModel : Screen
    {
        private IShell _shell;
        private ICacheFile _cacheFile;
        private string _output;
        private bool _inProgress;
        private Dictionary<int, string> _061_TagRemapping;
        private readonly Dictionary<string, string> ObjeRenames = new Dictionary<string, string>()
        {
            { "objects\\equipment\\instantcover_equipment\\instantcover_equipment.eqip", "objects\\equipment\\instantcover_equipment\\instantcover_equipment_mp.eqip" },
            { "objects\\levels\\multi\\cyberdyne\\cyber_monitor_med\\cyber_monitor.scen", "objects\\levels\\solo\\020_base\\monitor_med\\monitor_med.scen" },
            { "objects\\levels\\multi\\s3d_turf\\s3d_turf_turf_crate_large\\s3d_turf_turf_crate_large.bloc", "objects\\levels\\multi\\s3d_turf\\turf_crate_large\\turf_crate_large.bloc" }
        };

        public MapVariantFixerViewModel(IShell shell, ICacheFile cacheFile)
        {
            _shell = shell;
            _cacheFile = cacheFile;

            DisplayName = "Map Variant Fixer";
            StartCommand = new DelegateCommand(Start, () => Files.Count > 0 && !_inProgress);
            ClearCommand = new DelegateCommand(ClearFiles, () => Files.Count > 0 && !_inProgress);

            Files.CollectionChanged += Files_CollectionChanged;

            var sandboxMapsDir = new DirectoryInfo(Path.Combine(_cacheFile.File.Directory.FullName, "..\\data\\map_variants"));
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
                    var alert = new AlertDialogViewModel
                    {
                        AlertType = Alert.Error,
                        Message = "One or more map variants failed. Check the output for details."
                    };
                    _shell.ShowDialog(alert);

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
                var reader = new EndianReader(stream);
                var writer = new EndianWriter(stream);

                Fix061Endianess(stream);

                var blf = new Blf(baseCache.Version, baseCache.Platform);
                blf.Read(reader);

                if (blf.MapVariant == null)
                    return;

                if (blf.MapVariantTagNames == null)
                {
                    WriteLog("converting from 0.6.1 format...");
                    Convert061MapVariant(blf);
                }

                var palette = blf.MapVariant.MapVariant.Quotas;
                for (int i = 0; i < palette.Length; i++)
                {
                    if (palette[i].ObjectDefinitionIndex == -1)
                        continue;

                    var name = blf.MapVariantTagNames.Names[i].Name;
                    CachedTag tag;

                    if (baseCache.TagCache.TryGetTag(name, out tag))
                    {
                        continue;
                    }
                    else
                    {
                        string newName = "";

                        if (name.StartsWith("ms30"))
                            newName = name.Substring(5);
                        else
                            newName = $"ms30\\{name}";
                        
                        if (baseCache.TagCache.TryGetTag(newName, out tag))
                        {
                            blf.MapVariantTagNames.Names[i].Name = newName;
                            WriteLog($"Fixed name '{newName}'");
                        }
                        else if (ObjeRenames.TryGetValue(name, out var reName))
                        {
                            blf.MapVariantTagNames.Names[i].Name = reName;
                            WriteLog($"Fixed name '{reName}'");
                        }
                        else
                        {
                            throw new Exception($"Missing tag {name} in the base cache. Reach out to a dev for help.");
                        }
                    }
                }

                if(blf.EndOfFile == null)
                {
                    WriteLog("fixing EOF chunk...");
                    blf.EndOfFile = new BlfChunkEndOfFile()
                    {
                        Signature = new Tag("_eof"),
                        Length = (int)TagStructure.GetStructureSize(typeof(BlfChunkEndOfFile), blf.Version, baseCache.Platform),
                        MajorVersion = 1,
                        MinorVersion = 1,
                    };
                    blf.ContentFlags |= BlfFileContentFlags.EndOfFile;
                }

                WriteLog("saving file...");
                stream.Position = 0;
                if (!blf.Write(writer))
                    throw new Exception("failed to write blf");
            }
        }

        private void Fix061Endianess(Stream stream)
        {
            var deserializer = new TagDeserializer(CacheVersion.HaloOnlineED, CachePlatform.Original);
            var serializer = new TagSerializer(CacheVersion.HaloOnlineED, CachePlatform.Original);

            var reader = new EndianReader(stream, EndianFormat.BigEndian);
            var writer = new EndianWriter(stream, EndianFormat.LittleEndian);
            var readerContext = new DataSerializationContext(reader);
            var writerContext = new DataSerializationContext(writer);

            // check this is actually a big endian chunk header (this could also be true for h3 files, but that's their fault)
            if(reader.ReadTag() != "_blf")
            {
                stream.Position = 0;
                return;
            }

            WriteLog("Fixing chunk endianess...");

            reader.BaseStream.Position = 0;
            // fix the chunk headers
            while (true)
            {
                var pos = reader.BaseStream.Position;
                var header = (BlfChunkHeader)deserializer.Deserialize(readerContext, typeof(BlfChunkHeader));
               
                writer.BaseStream.Position = pos;
                serializer.Serialize(writerContext, header);
                if (header.Signature == "_eof")
                    break;

                reader.BaseStream.Position += header.Length - typeof(BlfChunkHeader).GetSize();
            }

            // fix the BOM
            stream.Position = 0xC;
            writer.Format = EndianFormat.LittleEndian;
            writer.Write((short)-2);
            stream.Position = 0;
        }

        private void Convert061MapVariant(Blf blf)
        {
            if(_061_TagRemapping == null)
            {
                _061_TagRemapping = _61MappingCSV
								.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(line => line.Split(','))
                                .Where(delim => delim.Length > 1)
                                .Select(delim => new { TagIndex = System.Convert.ToInt32(delim[0].Trim(), 16), Name = delim[1].Trim() })
                                .ToDictionary(x => x.TagIndex, y => y.Name);
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

            var mapVariant = blf.MapVariant.MapVariant;
            for(int i = 0; i < mapVariant.Quotas.Length; i++)
            {
                var tagIndex = mapVariant.Quotas[i].ObjectDefinitionIndex;
                if (tagIndex == -1)
                    continue;
                if(_061_TagRemapping.TryGetValue(tagIndex, out string name))
                {
                    blf.MapVariantTagNames.Names[i] = new TagName() { Name = name };
                    WriteLog($"added tag name entry 0x{tagIndex:X04} -> {name}");
                }
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

        private const string _61MappingCSV = @"002A, objects\characters\masterchief\mp_masterchief\armor\base.scen
0032, objects\characters\masterchief\mp_masterchief\armor\mp_cobra.scen
003A, objects\characters\masterchief\mp_masterchief\armor\mp_intruder.scen
0042, objects\characters\masterchief\mp_masterchief\armor\mp_ninja.scen
004A, objects\characters\masterchief\mp_masterchief\armor\mp_regulator.scen
0052, objects\characters\masterchief\mp_masterchief\armor\mp_ryu.scen
005A, objects\characters\masterchief\mp_masterchief\armor\mp_marathon.scen
0063, objects\characters\masterchief\mp_masterchief\armor\mp_scout.scen
006B, objects\characters\masterchief\mp_masterchief\armor\mp_odst.scen
0073, objects\characters\masterchief\mp_masterchief\armor\mp_markv.scen
007B, objects\characters\masterchief\mp_masterchief\armor\mp_rogue.scen
007E, objects\characters\masterchief\mp_masterchief\armor\mp_bungie.scen
0081, objects\characters\masterchief\mp_masterchief\armor\mp_katana.scen
00B0, objects\eldewrito\forge\light_object.bloc
00B4, objects\eldewrito\forge\fx_object.bloc
00B8, objects\eldewrito\reforge\hemisphere_2x2x05.bloc
00B9, objects\eldewrito\reforge\hemisphere_2x2x01.bloc
00BA, objects\eldewrito\reforge\hemisphere_1x2x01.bloc
00BB, objects\eldewrito\reforge\hemisphere_1x2x05.bloc
00BC, objects\eldewrito\reforge\hemisphere_1x2x1.bloc
00BD, objects\eldewrito\reforge\hemisphere_5x5x2.bloc
00BE, objects\eldewrito\reforge\hemisphere_5x5x1.bloc
00BF, objects\eldewrito\reforge\hemisphere_5x5x025.bloc
00C0, objects\eldewrito\reforge\hemisphere_2x5x025.bloc
00C1, objects\eldewrito\reforge\hemisphere_2x5x1.bloc
00C2, objects\eldewrito\reforge\hemisphere_2x5x2.bloc
00C3, objects\eldewrito\reforge\hemisphere_10x10x5.bloc
00C4, objects\eldewrito\reforge\hemisphere_10x10x2.bloc
00C5, objects\eldewrito\reforge\hemisphere_10x10x05.bloc
00C6, objects\eldewrito\reforge\hemisphere_5x10x1.bloc
00C7, objects\eldewrito\reforge\hemisphere_5x10x2.bloc
00C8, objects\eldewrito\reforge\hemisphere_5x10x5.bloc
00C9, objects\eldewrito\reforge\hemisphere_20x20x10.bloc
00CA, objects\eldewrito\reforge\hemisphere_20x20x5.bloc
00CB, objects\eldewrito\reforge\hemisphere_20x20x1.bloc
00CC, objects\eldewrito\reforge\hemisphere_10x20x1.bloc
00CD, objects\eldewrito\reforge\hemisphere_10x20x5.bloc
00CE, objects\eldewrito\reforge\hemisphere_10x20x10.bloc
00CF, objects\eldewrito\reforge\hemisphere_2x2x1.bloc
00D0, objects\eldewrito\reforge\glass_01x1x1.bloc
00D1, objects\eldewrito\reforge\glass_01x10x1.bloc
00D2, objects\eldewrito\reforge\glass_01x10x05.bloc
00D3, objects\eldewrito\reforge\glass_01x2x2.bloc
00D4, objects\eldewrito\reforge\glass_01x10x2.bloc
00D5, objects\eldewrito\reforge\cylinder_2x2x01.bloc
00D6, objects\eldewrito\reforge\cylinder_05x05x01.bloc
00D7, objects\eldewrito\reforge\cylinder_1x1x01.bloc
00D8, objects\eldewrito\reforge\cylinder_1x1x05.bloc
00D9, objects\eldewrito\reforge\cylinder_2x2x05.bloc
00DA, objects\eldewrito\reforge\cylinder_5x5x01.bloc
00DB, objects\eldewrito\reforge\cylinder_2x2x1.bloc
00DC, objects\eldewrito\reforge\cylinder_1x1x1.bloc
00DD, objects\eldewrito\reforge\cylinder_05x05x1.bloc
00DE, objects\eldewrito\reforge\cylinder_2x2x2.bloc
00DF, objects\eldewrito\reforge\cylinder_1x1x2.bloc
00E0, objects\eldewrito\reforge\cylinder_05x05x2.bloc
00E1, objects\eldewrito\reforge\cylinder_2x2x4.bloc
00E2, objects\eldewrito\reforge\cylinder_1x1x4.bloc
00E3, objects\eldewrito\reforge\cylinder_05x05x4.bloc
00E4, objects\eldewrito\reforge\cylinder_10x10x4.bloc
00E5, objects\eldewrito\reforge\cylinder_10x10x2.bloc
00E6, objects\eldewrito\reforge\cylinder_10x10x1.bloc
00E7, objects\eldewrito\reforge\cylinder_10x10x05.bloc
00E8, objects\eldewrito\reforge\cylinder_10x10x01.bloc
00E9, objects\eldewrito\reforge\cylinder_5x5x4.bloc
00EA, objects\eldewrito\reforge\cylinder_5x5x2.bloc
00EB, objects\eldewrito\reforge\cylinder_5x5x1.bloc
00EC, objects\eldewrito\reforge\cylinder_5x5x05.bloc
00ED, objects\eldewrito\reforge\cylinder_05x05x05.bloc
00EE, objects\eldewrito\reforge\halfcylinder_2x01x05.bloc
00EF, objects\eldewrito\reforge\halfcylinder_1x01x05.bloc
00F0, objects\eldewrito\reforge\halfcylinder_3x01x05.bloc
00F1, objects\eldewrito\reforge\trianglee_01x10x066.bloc
00F2, objects\eldewrito\reforge\trianglee_01x4x046.bloc
00F3, objects\eldewrito\reforge\trianglee_01x3x06.bloc
00F4, objects\eldewrito\reforge\trianglee_01x2x073.bloc
00F5, objects\eldewrito\reforge\trianglee_01x1x087.bloc
00F6, objects\eldewrito\reforge\trianglee_01x5x033.bloc
00F7, objects\eldewrito\reforge\trianglee_01x05x043.bloc
00F8, objects\eldewrito\reforge\glass_01x05x05.bloc
00F9, objects\eldewrito\reforge\halfcylinder_1x3x05.bloc
00FA, objects\eldewrito\reforge\halfcylinder_1x4x05.bloc
00FB, objects\eldewrito\reforge\halfcylinder_1x10x05.bloc
00FC, objects\eldewrito\reforge\halfcylinder_2x10x05.bloc
00FD, objects\eldewrito\reforge\halfcylinder_3x10x05.bloc
00FE, objects\eldewrito\reforge\halfcylinder_3x1x05.bloc
00FF, objects\eldewrito\reforge\halfcylinder_3x3x05.bloc
0100, objects\eldewrito\reforge\halfcylinder_3x4x05.bloc
0101, objects\eldewrito\reforge\halfcylinder_2x1x05.bloc
0102, objects\eldewrito\reforge\halfcylinder_2x3x05.bloc
0103, objects\eldewrito\reforge\halfcylinder_2x4x05.bloc
0104, objects\eldewrito\reforge\halfcylinder_1x1x05.bloc
0105, objects\eldewrito\reforge\triangle_1x05x05.bloc
0106, objects\eldewrito\reforge\triangle_1x2x2.bloc
0107, objects\eldewrito\reforge\triangle_1x3x3.bloc
0108, objects\eldewrito\reforge\triangle_1x4x4.bloc
0109, objects\eldewrito\reforge\triangle_1x5x5.bloc
010A, objects\eldewrito\reforge\triangle_1x1x1.bloc
010B, objects\eldewrito\reforge\triangle_05x1x1.bloc
010C, objects\eldewrito\reforge\triangle_05x5x5.bloc
010D, objects\eldewrito\reforge\triangle_05x4x4.bloc
010E, objects\eldewrito\reforge\triangle_05x3x3.bloc
010F, objects\eldewrito\reforge\triangle_05x2x2.bloc
0110, objects\eldewrito\reforge\triangle_05x10x10.bloc
0111, objects\eldewrito\reforge\triangle_1x10x10.bloc
0112, objects\eldewrito\reforge\triangle_05x05x05.bloc
0113, objects\eldewrito\reforge\block_1x3x2.bloc
0114, objects\eldewrito\reforge\block_1x3x3.bloc
0115, objects\eldewrito\reforge\block_1x4x3.bloc
0116, objects\eldewrito\reforge\block_1x4x4.bloc
0117, objects\eldewrito\reforge\block_1x5x5.bloc
0118, objects\eldewrito\reforge\block_1x5x2.bloc
0119, objects\eldewrito\reforge\block_1x5x3.bloc
011A, objects\eldewrito\reforge\block_1x5x4.bloc
011B, objects\eldewrito\reforge\block_1x2x1.bloc
011C, objects\eldewrito\reforge\block_1x5x1.bloc
011D, objects\eldewrito\reforge\block_1x3x1.bloc
011E, objects\eldewrito\reforge\block_1x4x1.bloc
011F, objects\eldewrito\reforge\block_1x1x1.bloc
0120, objects\eldewrito\reforge\block_1x10x10.bloc
0121, objects\eldewrito\reforge\block_1x10x5.bloc
0122, objects\eldewrito\reforge\block_1x10x1.bloc
0123, objects\eldewrito\reforge\block_1x10x2.bloc
0124, objects\eldewrito\reforge\block_1x10x3.bloc
0125, objects\eldewrito\reforge\block_1x10x4.bloc
0126, objects\eldewrito\reforge\block_1x4x2.bloc
0127, objects\eldewrito\reforge\block_1x20x20.bloc
0128, objects\eldewrito\reforge\block_05x20x20.bloc
0129, objects\eldewrito\reforge\block_05x4x2.bloc
012A, objects\eldewrito\reforge\block_05x05x05.bloc
012B, objects\eldewrito\reforge\block_05x1x05.bloc
012C, objects\eldewrito\reforge\block_05x10x4.bloc
012D, objects\eldewrito\reforge\block_05x10x3.bloc
012E, objects\eldewrito\reforge\block_05x10x2.bloc
012F, objects\eldewrito\reforge\block_05x10x1.bloc
0130, objects\eldewrito\reforge\block_05x10x5.bloc
0131, objects\eldewrito\reforge\block_05x10x10.bloc
0132, objects\eldewrito\reforge\block_05x1x1.bloc
0133, objects\eldewrito\reforge\block_05x4x1.bloc
0134, objects\eldewrito\reforge\block_05x3x1.bloc
0135, objects\eldewrito\reforge\block_05x5x1.bloc
0136, objects\eldewrito\reforge\block_05x2x1.bloc
0137, objects\eldewrito\reforge\block_05x5x4.bloc
0138, objects\eldewrito\reforge\block_05x5x3.bloc
0139, objects\eldewrito\reforge\block_05x5x2.bloc
013A, objects\eldewrito\reforge\block_05x5x5.bloc
013B, objects\eldewrito\reforge\block_05x4x4.bloc
013C, objects\eldewrito\reforge\block_05x4x3.bloc
013D, objects\eldewrito\reforge\block_05x3x3.bloc
013E, objects\eldewrito\reforge\block_05x3x2.bloc
013F, objects\eldewrito\reforge\block_05x2x2.bloc
0140, objects\eldewrito\reforge\block_1x2x2.bloc
0141, objects\eldewrito\reforge\block_01x4x2.bloc
0142, objects\eldewrito\reforge\block_01x10x4.bloc
0143, objects\eldewrito\reforge\block_01x10x3.bloc
0144, objects\eldewrito\reforge\block_01x10x2.bloc
0145, objects\eldewrito\reforge\block_01x10x1.bloc
0146, objects\eldewrito\reforge\block_01x10x5.bloc
0147, objects\eldewrito\reforge\block_01x10x10.bloc
0148, objects\eldewrito\reforge\block_01x1x1.bloc
0149, objects\eldewrito\reforge\block_01x4x1.bloc
014A, objects\eldewrito\reforge\block_01x3x1.bloc
014B, objects\eldewrito\reforge\block_01x5x1.bloc
014C, objects\eldewrito\reforge\block_01x2x1.bloc
014D, objects\eldewrito\reforge\block_01x5x4.bloc
014E, objects\eldewrito\reforge\block_01x5x3.bloc
014F, objects\eldewrito\reforge\block_01x5x2.bloc
0150, objects\eldewrito\reforge\block_01x5x5.bloc
0151, objects\eldewrito\reforge\block_01x4x4.bloc
0152, objects\eldewrito\reforge\block_01x4x3.bloc
0153, objects\eldewrito\reforge\block_01x3x3.bloc
0154, objects\eldewrito\reforge\block_01x3x2.bloc
0155, objects\eldewrito\reforge\block_01x2x2.bloc
0156, objects\eldewrito\reforge\block_01x2x05.bloc
0157, objects\eldewrito\reforge\block_01x10x05.bloc
0158, objects\eldewrito\reforge\block_01x1x05.bloc
0159, objects\eldewrito\reforge\block_01x4x05.bloc
015A, objects\eldewrito\reforge\block_01x3x05.bloc
015B, objects\eldewrito\reforge\block_01x5x05.bloc
015C, objects\eldewrito\reforge\block_01x05x05.bloc
015D, objects\eldewrito\reforge\block_01x20x20.bloc
015E, objects\eldewrito\reforge\triangle_01x10x10.bloc
015F, objects\eldewrito\reforge\triangle_01x5x5.bloc
0160, objects\eldewrito\reforge\triangle_01x4x4.bloc
0161, objects\eldewrito\reforge\triangle_01x3x3.bloc
0162, objects\eldewrito\reforge\triangle_01x2x2.bloc
0163, objects\eldewrito\reforge\triangle_01x05x05.bloc
0164, objects\eldewrito\reforge\triangle_01x1x1.bloc
01AC, objects\weapons\grenade\frag_grenade\frag_grenade.eqip
01AD, objects\weapons\grenade\frag_grenade\frag_grenade.proj
01AF, objects\weapons\grenade\plasma_grenade\plasma_grenade.eqip
01B0, objects\weapons\grenade\plasma_grenade\plasma_grenade.proj
01B2, objects\weapons\grenade\claymore_grenade\claymore_grenade.eqip
01B3, objects\weapons\grenade\claymore_grenade\claymore_grenade.proj
01B5, objects\weapons\grenade\firebomb_grenade\firebomb_grenade.eqip
01B6, objects\weapons\grenade\firebomb_grenade\projectiles\firebomb_grenade.proj
01DD, objects\characters\masterchief\mp_masterchief\mp_masterchief.bipd
0290, objects\cinematics\cinematic_anchor\cinematic_anchor.scen
0587, objects\weapons\grenade\claymore_grenade\projectiles\claymore_grenade_fragment.proj
06D9, objects\weapons\grenade\firebomb_grenade\projectiles\primary_impact.proj
06DB, objects\weapons\grenade\firebomb_grenade\projectiles\secondary_impact.proj
06DC, objects\weapons\grenade\firebomb_grenade\projectiles\tertiary_impact_spawn.proj
06FB, objects\weapons\grenade\firebomb_grenade\projectiles\tertiary_impact.proj
0C13, objects\equipment\hologram\bipeds\masterchief_hologram.bipd
0C17, objects\props\human\unsc\spartan_knife\spartan_knife.scen
14F7, objects\weapons\pistol\plasma_pistol\plasma_pistol.weap
14F8, objects\weapons\pistol\needler\needler.weap
14F9, objects\weapons\support_high\flak_cannon\flak_cannon.weap
14FB, objects\weapons\turret\plasma_cannon\plasma_cannon.vehi
14FE, objects\weapons\rifle\covenant_carbine\covenant_carbine.weap
14FF, objects\weapons\support_low\brute_shot\brute_shot.weap
1500, objects\weapons\rifle\spike_rifle\spike_rifle.weap
1504, objects\weapons\pistol\excavator\excavator.weap
1509, objects\weapons\rifle\beam_rifle\beam_rifle.weap
150C, objects\weapons\melee\gravity_hammer\gravity_hammer.weap
150E, objects\weapons\turret\plasma_cannon\plasma_cannon.weap
1516, objects\vehicles\shade\shade.vehi
1517, objects\vehicles\ghost\ghost.vehi
1518, objects\vehicles\brute_chopper\brute_chopper.vehi
1519, objects\vehicles\wraith\wraith.vehi
151A, objects\vehicles\banshee\banshee.vehi
151E, objects\weapons\rifle\assault_rifle\assault_rifle.weap
151F, objects\vehicles\warthog\warthog.vehi
1520, objects\vehicles\scorpion\scorpion.vehi
1525, objects\weapons\rifle\plasma_rifle\plasma_rifle.weap
1560, objects\equipment\jammer_equipment\jammer_equipment.eqip
1561, objects\equipment\powerdrain_equipment\powerdrain_equipment.eqip
1563, objects\equipment\invisibility_equipment\invisibility_vehicle_equipment.eqip
1564, objects\equipment\bubbleshield_equipment\bubbleshield_equipment.eqip
1565, objects\equipment\superflare_equipment\superflare_equipment.eqip
1566, objects\equipment\regenerator_equipment\regenerator_equipment.eqip
1567, objects\equipment\tripmine_equipment\tripmine_equipment.eqip
1569, objects\equipment\instantcover_equipment\instantcover_equipment.eqip
156B, objects\equipment\concussiveblast_equipment\concussiveblast_equipment.eqip
156E, objects\equipment\hologram_equipment\hologram_equipment.eqip
156F, objects\equipment\reactive_armor_equipment\reactive_armor_equipment.eqip
1570, objects\equipment\bombrun_equipment\bombrun_equipment.eqip
1573, objects\equipment\lightningstrike_equipment\lightningstrike_equipment.eqip
1577, objects\equipment\vision_equipment\vision_equipment.eqip
157C, objects\weapons\rifle\battle_rifle\battle_rifle.weap
157D, objects\weapons\rifle\smg\smg.weap
157E, objects\weapons\pistol\magnum\magnum.weap
157F, objects\weapons\melee\energy_blade\energy_blade_useless.weap
1580, objects\weapons\rifle\dmr\dmr.weap
1581, objects\weapons\rifle\assault_rifle_damage\assault_rifle_damage.weap
1582, objects\weapons\rifle\assault_rifle_rof\assault_rifle_rof.weap
1583, objects\weapons\rifle\assault_rifle_accuracy\assault_rifle_accuracy.weap
1584, objects\weapons\rifle\assault_rifle_power\assault_rifle_power.weap
1585, objects\weapons\rifle\battle_rifle_ammo\battle_rifle_ammo.weap
1586, objects\weapons\rifle\battle_rifle_damage\battle_rifle_damage.weap
1587, objects\weapons\rifle\battle_rifle_power\battle_rifle_power.weap
1588, objects\weapons\rifle\dmr_accuracy\dmr_accuracy.weap
1589, objects\weapons\rifle\dmr_rof\dmr_rof.weap
158A, objects\weapons\rifle\dmr_damage\dmr_damage.weap
158B, objects\weapons\rifle\dmr_power\dmr_power.weap
158C, objects\weapons\rifle\smg\smg_rof.weap
158D, objects\weapons\rifle\smg\smg_accuracy.weap
158E, objects\weapons\rifle\smg\smg_damage.weap
158F, objects\weapons\rifle\smg\smg_power.weap
1590, objects\weapons\rifle\plasma_rifle\plasma_rifle_power.weap
1591, objects\weapons\rifle\covenant_carbine_power\covenant_carbine_power.weap
1592, objects\weapons\pistol\excavator_power\excavator_power.weap
1593, objects\weapons\pistol\magnum\magnum_damage.weap
1594, objects\weapons\pistol\magnum\magnum_power.weap
1595, objects\weapons\pistol\plasma_pistol\plasma_pistol_power.weap
1596, objects\vehicles\mongoose\mongoose.vehi
1598, objects\vehicles\hornet\hornet.vehi
1599, objects\vehicles\warthog\warthog_snow.vehi
159B, objects\levels\dlc\sidewinder\hornet_lite\hornet_lite.vehi
159E, objects\weapons\melee\energy_blade\energy_blade.weap
15A0, objects\characters\monitor\monitor_editor.bipd
15A1, objects\ui\editor_gizmo\editor_gizmo.scen
15A2, objects\weapons\multiplayer\flag\flag.weap
15A3, objects\weapons\multiplayer\ball\ball.weap
15A4, objects\weapons\multiplayer\assault_bomb\assault_bomb.weap
15A5, objects\multi\vip\vip_boundary.bloc
15A7, objects\weapons\grenade\plasma_grenade\plasma_grenade2.proj
15B1, objects\weapons\rifle\sniper_rifle\sniper_rifle.weap
15B2, objects\weapons\support_high\spartan_laser\spartan_laser.weap
15B3, objects\weapons\support_high\rocket_launcher\rocket_launcher.weap
15B4, objects\vehicles\warthog\turrets\chaingun\weapon\chaingun_turret.weap
15B5, objects\weapons\turret\machinegun_turret\machinegun_turret.weap
15B6, objects\weapons\turret\machinegun_turret\machinegun_turret_integrated.weap
15B7, objects\weapons\turret\plasma_cannon\plasma_cannon_integrated.weap
15B8, objects\vehicles\warthog\turrets\gauss\weapon\gauss_turret.weap
15BB, objects\weapons\rifle\battle_rifle_rof\battle_rifle_rof.weap
15BC, objects\weapons\rifle\battle_rifle_accuracy\battle_rifle_accuracy.weap
15BD, objects\weapons\rifle\battle_rifle_range\battle_rifle_range.weap
15BE, objects\weapons\rifle\dmr_ammo\dmr_ammo.weap
15C0, objects\weapons\rifle\covenant_carbine_rof\covenant_carbine_rof.weap
15C1, objects\weapons\rifle\covenant_carbine_ammo\covenant_carbine_ammo.weap
15C2, objects\weapons\rifle\covenant_carbine_range\covenant_carbine_range.weap
15C3, objects\weapons\rifle\covenant_carbine_damage\covenant_carbine_damage.weap
15C4, objects\weapons\rifle\covenant_carbine_accuracy\covenant_carbine_accuracy.weap
15C8, objects\vehicles\banshee\weapon\banshee_bomb.proj
15C9, objects\weapons\support_high\rocket_launcher\projectiles\rocket.proj
15CA, objects\weapons\turret\missile_pod\projectiles\missile_pod_missile.proj
15CB, objects\vehicles\wraith\turrets\mortar\mortar_turret_charged.proj
15CD, objects\weapons\support_low\brute_shot\projectiles\grenade\grenade.proj
15CE, objects\vehicles\hornet\weapon\hornet_missile.proj
15CF, objects\weapons\support_high\flak_cannon\projectiles\flak_bolt\flak_bolt.proj
15D0, objects\equipment\bombrun\projectiles\bombrun_grenade.proj
15D1, objects\equipment\tripmine\tripmine.eqip
15D2, objects\equipment\powerdrain\powerdrain.eqip
15D3, objects\equipment\superflare\superflare.eqip
18AC, objects\equipment\jammer\jammer.eqip
196E, objects\equipment\bubbleshield\bubbleshield.eqip
19C5, objects\equipment\regenerator\regenerator.eqip
1A02, objects\equipment\tripmine\projectiles\tripmine_material_fx.proj
1A44, objects\vehicles\pelican\pelican_rocket_pod\pelican_rocket_pod.vehi
1A45, objects\weapons\rifle\shotgun\shotgun.weap
1A47, objects\vehicles\pelican\pelican_rocket_pod\weapon\pelican_rocket_pod.weap
1A48, objects\vehicles\scorpion\turrets\anti_infantry\weapon\anti_infantry_turret.weap
1A49, objects\vehicles\wraith\turrets\anti_infantry\weapon\wraith_anti_infantry_turret.weap
1A4A, objects\vehicles\shade\weapon\shade_turret.weap
1A4B, objects\vehicles\ghost\weapon\ghost_gun.weap
1A4C, objects\vehicles\banshee\weapon\banshee_gun.weap
1A4D, objects\vehicles\wraith\turrets\mortar\mortar_turret.weap
1A4E, objects\vehicles\wraith\turrets\anti_air\anti_air_turret.weap
1A4F, objects\vehicles\scorpion\turrets\cannon\weapon\cannon_turret.weap
1A53, objects\vehicles\brute_chopper\weapon\chopper_gun.weap
1A54, objects\weapons\turret\missile_pod\missile_pod.weap
1A55, objects\weapons\turret\flamethrower\flamethrower.weap
1A56, objects\weapons\support_low\sentinel_gun\sentinel_gun.weap
1A57, objects\weapons\turret\missile_pod\missile_pod_integrated.weap
1A58, objects\vehicles\hornet\weapon\hornet_missile.weap
1A6C, objects\equipment\invincibility_equipment\invincibility_equipment.eqip
1A8A, objects\equipment\instantcover\garbage\covshield_base\covshield_base.bloc
1A8B, objects\equipment\instantcover\garbage\covshield_mainflap\covshield_mainflap.bloc
1A8C, objects\equipment\instantcover\garbage\covshield_rightflap\covshield_rightflap.bloc
1B04, objects\equipment\instantcover\instantcover.eqip
1B8E, objects\powerups\unsc_ammo_large\unsc_ammo_large.eqip
1B8F, objects\powerups\unsc_ammo_small\unsc_ammo_small.eqip
1B90, objects\weapons\rifle\assault_rifle\projectiles\assault_rifle_bullet.proj
1CA0, objects\powerups\rocket_launcher_ammo\rocket_launcher_ammo.eqip
1D88, objects\weapons\rifle\battle_rifle\projectiles\battle_rifle_bullet.proj
1DBC, objects\weapons\rifle\covenant_carbine\projectiles\carbine_slug\carbine_slug.proj
1E12, objects\weapons\rifle\dmr\projectiles\dmr_bullet.proj
1E49, objects\weapons\rifle\plasma_rifle\projectiles\plasma_rifle_bolt.proj
1EFF, objects\powerups\shotgun_ammo\shotgun_ammo.eqip
1F00, objects\weapons\rifle\shotgun\projectiles\shotgun_bullet.proj
1F51, objects\weapons\rifle\smg\projectiles\smg_bullet.proj
1F91, objects\powerups\sniper_rifle_ammo\sniper_rifle_ammo.eqip
1F92, objects\weapons\rifle\sniper_rifle\projectiles\sniper_bullet.proj
2058, objects\weapons\rifle\spike_rifle\projectiles\spike_shard\spike_shard.proj
208F, objects\weapons\pistol\excavator\projectiles\excavator_shard.proj
20BD, objects\weapons\pistol\magnum\projectiles\magnum_bullet.proj
20EE, objects\powerups\needler_ammo\needler_ammo.eqip
20EF, objects\weapons\pistol\needler\projectiles\needler_shard\needler_shard.proj
219E, objects\weapons\turret\machinegun_turret\projectiles\machinegun_turret_bullet.proj
225C, objects\weapons\pistol\plasma_pistol\projectiles\plasma_pistol_bolt.proj
225E, objects\weapons\pistol\plasma_pistol\projectiles\plasma_pistol_charged_bolt.proj
2320, objects\weapons\rifle\assault_rifle_damage\projectiles\assault_rifle_bullet.proj
2335, objects\weapons\rifle\assault_rifle_rof\projectiles\assault_rifle_bullet.proj
2343, objects\weapons\rifle\assault_rifle_accuracy\projectiles\assault_rifle_bullet.proj
2354, objects\weapons\rifle\assault_rifle_power\projectiles\assault_rifle_bullet.proj
2362, objects\weapons\rifle\battle_rifle_ammo\projectiles\battle_rifle_bullet.proj
2370, objects\weapons\rifle\battle_rifle_damage\projectiles\battle_rifle_bullet.proj
2382, objects\weapons\rifle\battle_rifle_power\projectiles\battle_rifle_bullet.proj
2393, objects\weapons\rifle\dmr_accuracy\projectiles\dmr_bullet.proj
23A6, objects\weapons\rifle\dmr_rof\projectiles\dmr_bullet.proj
23B2, objects\weapons\rifle\dmr_damage\projectiles\dmr_bullet.proj
23C1, objects\weapons\rifle\dmr_power\projectiles\dmr_bullet.proj
23CD, objects\weapons\rifle\smg_rof\projectiles\smg_bullet.proj
23D8, objects\weapons\rifle\smg_accuracy\projectiles\smg_bullet.proj
23E2, objects\weapons\rifle\smg_damage\projectiles\smg_bullet.proj
23EB, objects\weapons\rifle\smg_power\projectiles\smg_bullet.proj
23F4, objects\weapons\rifle\plasma_rifle_power\projectiles\plasma_rifle_bolt.proj
23FF, objects\weapons\rifle\covenant_carbine_power\projectiles\carbine_slug.proj
240A, objects\weapons\pistol\excavator_power\projectiles\excavator_shard.proj
2413, objects\weapons\pistol\magnum_damage\projectiles\magnum_bullet.proj
2425, objects\weapons\pistol\plasma_pistol_power\projectiles\plasma_pistol_charged_bolt.proj
245A, objects\weapons\rifle\beam_rifle\projectiles\beam_rifle_beam.proj
24AC, objects\weapons\support_high\spartan_laser\projectiles\spartan_laser_tracer.proj
24AE, objects\weapons\support_high\spartan_laser\projectiles\spartan_laser_beam.proj
251B, objects\vehicles\warthog\turrets\chaingun\weapon\bullet.proj
2528, objects\weapons\turret\plasma_cannon\projectiles\plasma_cannon_bolt.proj
2558, objects\vehicles\warthog\turrets\gauss\weapon\gauss_bullet.proj
2582, objects\weapons\rifle\battle_rifle_rof\projectiles\battle_rifle_bullet.proj
258C, objects\weapons\rifle\battle_rifle_accuracy\projectiles\battle_rifle_bullet.proj
2596, objects\weapons\rifle\battle_rifle_range\projectiles\battle_rifle_bullet.proj
259F, objects\weapons\rifle\dmr_ammo\projectiles\dmr_bullet.proj
25B2, objects\weapons\rifle\covenant_carbine_rof\projectiles\carbine_slug.proj
25BB, objects\weapons\rifle\covenant_carbine_ammo\projectiles\carbine_slug.proj
25C4, objects\weapons\rifle\covenant_carbine_range\projectiles\carbine_slug.proj
25CD, objects\weapons\rifle\covenant_carbine_damage\projectiles\carbine_slug.proj
25D6, objects\weapons\rifle\covenant_carbine_accuracy\projectiles\carbine_slug.proj
2603, objects\characters\monitor\garbage\core_back_garbage\core_back_garbage.bloc
2604, objects\characters\monitor\garbage\core_front_garbage\core_front_garbage.bloc
2605, objects\characters\monitor\garbage\eye_garbage\eye_garbage.bloc
2606, objects\characters\monitor\garbage\outer_shell_b_garbage\outer_shell_b_garbage.bloc
2607, objects\characters\monitor\garbage\outer_shell_f_garbage\outer_shell_f_garbage.bloc
2608, objects\characters\monitor\garbage\outer_shell_t_garbage\outer_shell_t_garbage.bloc
27C7, objects\levels\ui\mainmenu\menu_pelican\menu_pelican.scen
27D1, levels\multi\s3d_mainmenu\platform.scen
27EF, objects\levels\multi\s3d_turf\s3d_turf_crate_tech_giant\s3d_turf_crate_tech_giant.bloc
27F4, objects\characters\ambient_life\rat\rat.crea
284B, objects\vehicles\pelican\pelican_rocket_pod02\pelican_rocket_pod02.vehi
284C, objects\weapons\turret\machinegun_turret\machinegun_turret.vehi
2853, objects\vehicles\pelican\pelican_rocket_pod\weapon\projectiles\pelican_rocket.proj
2888, objects\weapons\turret\machinegun_turret\machinegun_turret_vehicle\garbage\machinegun_turret_tripod_mp\machinegun_turret_tripod_mp.bloc
28C6, objects\vehicles\scorpion\turrets\cannon\cannon.vehi
28C7, objects\vehicles\scorpion\turrets\anti_infantry\anti_infantry.vehi
28E1, objects\vehicles\scorpion\turrets\cannon\weapon\tank_shell.proj
2932, objects\vehicles\scorpion\turrets\anti_infantry\weapon\bullet.proj
2941, objects\vehicles\scorpion\turrets\anti_infantry\garbage\anti_gun\anti_gun.bloc
2958, objects\vehicles\scorpion\turrets\cannon\garbage\turret\turret.bloc
2964, objects\vehicles\scorpion\garbage\rf_tread\rf_tread.bloc
2978, objects\vehicles\scorpion\garbage\lb_tread\lb_tread.bloc
297D, objects\vehicles\scorpion\garbage\rb_tread\rb_tread.bloc
2982, objects\vehicles\scorpion\garbage\lf_tread\lf_tread.bloc
2987, objects\vehicles\scorpion\garbage\engine_panel\engine_panel.bloc
2988, objects\vehicles\scorpion\garbage\duffle_bag\duffle_bag.bloc
2989, objects\vehicles\scorpion\garbage\water_can\water_can.bloc
2996, objects\vehicles\scorpion\garbage\driver_hatch\driver_hatch.bloc
29A3, objects\vehicles\scorpion\garbage\l_tread_cover\l_tread_cover.bloc
29A4, objects\vehicles\scorpion\garbage\l_big_panel\l_big_panel.bloc
29A5, objects\vehicles\scorpion\garbage\tread_side_panel\tread_side_panel.bloc
29B6, objects\vehicles\scorpion\garbage\r_tread_cover\r_tread_cover.bloc
29BB, objects\vehicles\scorpion\garbage\lb_tread_cover\lb_tread_cover.bloc
29C0, objects\vehicles\scorpion\garbage\rb_tread_cover\rb_tread_cover.bloc
29EC, objects\vehicles\warthog\turrets\chaingun\chaingun.vehi
29ED, objects\vehicles\warthog\turrets\gauss\gauss.vehi
29EE, objects\vehicles\warthog\turrets\troop\troop.vehi
2A0B, objects\vehicles\warthog\turrets\chaingun\garbage\barrel_garbage\barrel_garbage.bloc
2A14, objects\vehicles\warthog\turrets\chaingun\garbage\shield_garbage\shield_garbage.bloc
2A26, objects\vehicles\warthog\turrets\gauss\garbage\barrel_garbage\barrel_garbage.bloc
2A3C, objects\vehicles\warthog\turrets\gauss\garbage\display_garbage\display_garbage.bloc
2A3D, objects\vehicles\warthog\turrets\gauss\garbage\shield_garbage\shield_garbage.bloc
2A5D, objects\vehicles\warthog\turrets\troop\garbage\can\can.bloc
2A65, objects\vehicles\warthog\turrets\troop\garbage\floodlight\floodlight.bloc
2A76, objects\vehicles\warthog\garbage\tire\tire.bloc
2A8F, objects\vehicles\warthog\garbage\winch\winch.bloc
2A90, objects\vehicles\warthog\garbage\hood\hood.bloc
2AA5, objects\vehicles\warthog\garbage\hubcap\hubcap.bloc
2AAA, objects\vehicles\warthog\garbage\lf_fender\lf_fender.bloc
2AAF, objects\vehicles\warthog\garbage\rf_fender\rf_fender.bloc
2AB4, objects\vehicles\warthog\garbage\lb_fender\lb_fender.bloc
2AB5, objects\vehicles\warthog\garbage\sailpanel\sailpanel.bloc
2ABE, objects\vehicles\warthog\garbage\rb_fender\rb_fender.bloc
2AC3, objects\vehicles\warthog\garbage\bumper\bumper.bloc
2CA4, objects\vehicles\hornet\projectile\hornet_bullet.proj
2CBF, objects\vehicles\hornet\garbage\tail\tail.bloc
2CE0, objects\vehicles\hornet\garbage\landing_gear\landing_gear.bloc
2CE7, objects\vehicles\hornet\garbage\jet_l\jet_l.bloc
2CE8, objects\vehicles\hornet\garbage\thrust_bell\thrust_bell.bloc
2CE9, objects\vehicles\hornet\garbage\rotor\rotor.bloc
2CEA, objects\vehicles\hornet\garbage\flap\flap.bloc
2CFE, objects\vehicles\hornet\garbage\jet_r\jet_r.bloc
2D03, objects\vehicles\hornet\garbage\winch\winch.bloc
2D08, objects\vehicles\hornet\garbage\cover_r\cover_r.bloc
2D0D, objects\vehicles\hornet\garbage\cover_l\cover_l.bloc
2D12, objects\vehicles\hornet\garbage\skid_r\skid_r.bloc
2D17, objects\vehicles\hornet\garbage\skid_l\skid_l.bloc
2D1C, objects\vehicles\hornet\garbage\gun_r\gun_r.bloc
2D21, objects\vehicles\hornet\garbage\gun_l\gun_l.bloc
2E61, objects\characters\ambient_life\rat_garbage\rat_garbage.bloc
2E8F, levels\multi\guardian\sky\sky.scen
2E90, objects\multi\spawning\respawn_point.scen
2E91, objects\multi\ctf\ctf_initial_spawn_point.scen
2E92, objects\multi\slayer\slayer_initial_spawn_point.scen
2E93, objects\multi\ctf\ctf_respawn_zone.scen
2E94, objects\multi\slayer\slayer_respawn_zone.scen
2E95, objects\multi\ctf\ctf_flag_at_home_respawn_zone.scen
2E96, objects\multi\ctf\ctf_flag_away_respawn_zone.scen
2E97, objects\multi\territories\territories_respawn_zone.scen
2E98, objects\multi\assault\assault_initial_spawn_point.scen
2E99, objects\multi\assault\assault_respawn_zone.scen
2E9A, objects\multi\koth\koth_initial_spawn_point.scen
2E9B, objects\multi\koth\koth_respawn_zone.scen
2E9C, objects\multi\territories\territories_initial_spawn_point.scen
2E9D, objects\multi\oddball\oddball_initial_spawn_point.scen
2E9E, objects\multi\vip\vip_initial_spawn_point.scen
2E9F, objects\multi\vip\vip_respawn_zone.scen
2EA4, objects\halograms\130lb_earth\earth_halogram\guardian_halogram.scen
2EA6, objects\multi\spawning\respawn_point_invisible.scen
2EA7, objects\multi\infection\infection_initial_spawn_point.scen
2EA8, objects\multi\infection\infection_respawn_zone.scen
2EA9, objects\equipment\gravlift_equipment\gravlift_equipment.eqip
2EAA, objects\multi\powerups\powerup_blue\powerup_blue.eqip
2EAB, objects\multi\powerups\powerup_red\powerup_red.eqip
2EAC, objects\multi\powerups\powerup_yellow\powerup_yellow.eqip
2EAD, levels\multi\guardian\fx\holy_light_lift\holy_light_lift.efsc
2EAE, levels\multi\guardian\fx\mancannon\mancannon.efsc
2EAF, objects\gear\human\military\hu_mil_radio_big\hu_mil_radio_big.bloc
2EB0, objects\gear\human\military\hu_mil_rucksack\rucksack.bloc
2EB1, objects\gear\human\military\drum_55gal\drum_55gal.bloc
2EB2, objects\gear\human\military\camping_stool_mp\camping_stool_mp.bloc
2EB3, objects\gear\human\military\crate_packing\crate_packing.bloc
2EB4, objects\gear\human\military\crate_packing_giant\crate_packing_giant_mp.bloc
2EB5, objects\gear\human\industrial\h_barrel_rusty\h_barrel_rusty.bloc
2EB6, objects\gear\human\industrial\h_barrel_rusty_small\h_barrel_rusty_small.bloc
2EB7, objects\gear\human\industrial\pallet_large\pallet_large.bloc
2EB8, objects\gear\human\industrial\sawhorse\sawhorse.bloc
2EB9, objects\gear\human\industrial\street_cone\street_cone.bloc
2EBA, objects\gear\covenant\military\battery\battery.bloc
2EBB, objects\gear\forerunner\power_core_for\power_core_for.bloc
2EBC, objects\gear\human\military\case_ap_turret\case_ap_turret.bloc
2EBD, objects\gear\human\military\generator\generator.bloc
2EBE, objects\gear\covenant\military\cov_sword_holder\cov_sword_holder.bloc
2EBF, objects\equipment\gravlift_permanent\gravlift_permanent.eqip
2EC0, objects\multi\teleporter_sender\teleporter_sender.bloc
2EC1, objects\multi\teleporter_reciever\teleporter_reciever.bloc
2EC2, objects\multi\teleporter_2way\teleporter_2way.bloc
2EC3, objects\multi\ctf\ctf_flag_spawn_point.bloc
2EC4, objects\multi\ctf\ctf_flag_return_area.bloc
2EC5, objects\multi\assault\assault_bomb_spawn_point.bloc
2EC6, objects\multi\assault\assault_bomb_goal_area.bloc
2EC7, objects\multi\juggernaut\juggernaut_destination_static.bloc
2EC8, objects\multi\koth\koth_hill_static.bloc
2EC9, objects\multi\oddball\oddball_ball_spawn_point.bloc
2ECA, objects\multi\territories\territory_static.bloc
2ECB, objects\multi\vip\vip_destination_static.bloc
2ECC, objects\multi\oddball\oddball_respawn_zone.scen
2ED9, objects\levels\multi\guardian\holy_light_guardian\holy_light_guardian.bloc
2EDA, objects\levels\multi\guardian\man_cannon_guardian_iii\man_cannon_guardian_iii.bloc
2EDB, objects\levels\multi\guardian\man_cannon_guardian_ii\man_cannon_guardian_ii.bloc
2EDC, objects\multi\infection\infection_haven_static.bloc
2EDD, objects\levels\multi\guardian\minilift\minilift.bloc
2EDF, objects\levels\multi\salvation\jittery_holo_01\jittery_holo_01.bloc
2EE0, objects\levels\multi\salvation\jittery_holo_02\jittery_holo_02.bloc
2EE2, objects\levels\multi\salvation\jittery_holo_04\jittery_holo_04.bloc
2EE3, objects\levels\multi\salvation\jittery_holo_05\jittery_holo_05.bloc
2EE5, objects\levels\multi\salvation\large_field\large_field.bloc
2EE6, objects\levels\solo\010_jungle\foliage\plant_vine_horizontal_01\plant_vine_horizontal_01_noshadow.bloc
2EE7, objects\levels\solo\010_jungle\foliage\plant_vine_horizontal_03\plant_vine_horizontal_03_noshadow.bloc
2EE8, objects\levels\solo\010_jungle\foliage\plant_vine_horizontal_short\plant_vine_horizontal_short_noshadow.bloc
2EE9, objects\multi\box_l\box_l.bloc
2EEA, objects\multi\box_m\box_m.bloc
2EEB, objects\multi\box_xl\box_xl.bloc
2EEC, objects\multi\box_xxl\box_xxl.bloc
2EED, objects\multi\box_xxxl\box_xxxl.bloc
2EEE, objects\multi\wall_l\wall_l.bloc
2EEF, objects\multi\wall_m\wall_m.bloc
2EF0, objects\multi\wall_xl\wall_xl.bloc
2EF1, objects\multi\wall_xxl\wall_xxl.bloc
2EF2, objects\multi\wall_xxxl\wall_xxxl.bloc
2EF5, objects\characters\ambient_life\bird_quadwing\bird_quadwing.crea
2EF6, objects\levels\multi\guardian\glowbug\glowbug.crea
2EF7, objects\levels\multi\guardian\glowfly\glowfly.crea
3096, objects\weapons\turret\plasma_cannon\plasma_cannon_vehicle\garbage\garbage_tripod_back\garbage_tripod_back.bloc
3097, objects\weapons\turret\plasma_cannon\plasma_cannon_vehicle\garbage\garbage_tripod_front\garbage_tripod_front.bloc
30AA, objects\vehicles\shade\weapon\shade_turret.proj
30C5, objects\vehicles\shade\garbage\ring\ring.bloc
30D3, objects\vehicles\shade\garbage\gun\gun.bloc
30E1, objects\vehicles\shade\garbage\door\door.bloc
30FF, objects\vehicles\mongoose\weapon\mongoose_horn.weap
311D, objects\vehicles\mongoose\garbage\wheel\wheel.bloc
312D, objects\vehicles\mongoose\garbage\ramp\ramp.bloc
3135, objects\vehicles\mongoose\garbage\cargo_shelf\cargo_shelf.bloc
313E, objects\vehicles\mongoose\garbage\cowl_left\cowl_left.bloc
313F, objects\vehicles\mongoose\garbage\cowl_right\cowl_right.bloc
314E, objects\vehicles\mongoose\garbage\lb_fender\lb_fender.bloc
3154, objects\vehicles\mongoose\garbage\rb_fender\rb_fender.bloc
317F, objects\equipment\gravlift\gravlift.eqip
31C6, objects\weapons\turret\flamethrower\projectiles\flamethrower_stream.proj
322D, objects\weapons\support_low\sentinel_gun\projectiles\sentinel_gun_beam.proj
3298, objects\gear\human\military\crate_packing_lid\crate_packing_lid.bloc
32CF, objects\gear\human\industrial\pallet\pallet_broken01\pallet_broken01.bloc
32D0, objects\gear\human\industrial\pallet\pallet_broken02\pallet_broken02.bloc
32D1, objects\gear\human\industrial\pallet\pallet_broken03\pallet_broken03.bloc
32D2, objects\gear\human\industrial\pallet\pallet_broken04\pallet_broken04.bloc
32D3, objects\gear\human\industrial\pallet\pallet_broken05\pallet_broken05.bloc
32D5, objects\gear\human\industrial\pallet\pallet_broken06\pallet_broken06.bloc
3301, objects\gear\human\industrial\sawhorse\garbage\sawhorse_leg.bloc
332B, objects\gear\covenant\military\battery\battery_fragment01\battery_fragment01.bloc
332C, objects\gear\covenant\military\battery\battery_fragment03\battery_fragment03.bloc
332D, objects\gear\covenant\military\battery\battery_fragment02\battery_fragment02.bloc
3359, objects\gear\forerunner\power_core_junk01\power_core_junk01.bloc
335A, objects\gear\forerunner\power_core_junk02\power_core_junk02.bloc
33C3, objects\multi\territories\territory_flag.scen
345A, objects\characters\ambient_life\bird_quadwing\garbage\bird_quadwing_dead\bird_quadwing_dead.bloc
345B, objects\characters\ambient_life\bird_quadwing\garbage\wing_big\wing_big.bloc
345C, objects\characters\ambient_life\bird_quadwing\garbage\wing_small\wing_small.bloc
348D, levels\ui\mainmenu\sky\menu_sky.scen
348E, objects\gear\human\military\resupply_capsule_fired\resupply_capsule_fired.scen
348F, levels\multi\riverworld\fx\tower_pulse\tower_pulse.scen
3490, levels\multi\riverworld\fx\man_cannon\man_cannon.scen
3491, objects\gear\human\military\resupply_capsule_ground_scar\ground_scar.scen
3492, objects\scenery\human\military\warthog_drop_palette\warthog_drop_palette.scen
3493, objects\scenery\human\military\mongoose_drop_palette\mongoose_drop_palette.scen
3494, levels\multi\riverworld\fx\man_cannon\mini_man_cannon.scen
3495, levels\multi\riverworld\fx\waterfall\waterfall_base.scen
3496, levels\multi\riverworld\fx\waterfall\waterfall_top.scen
3497, levels\multi\riverworld\fx\waterfall\waterfall_mid.scen
3498, sound\levels\riverworld\sound_scenery\water_creek\water_creek.ssce
3499, sound\levels\riverworld\sound_scenery\waterfall_far\waterfall_far.ssce
349A, sound\levels\riverworld\sound_scenery\riverworld_waterfall_close\riverworld_waterfall_close.ssce
349B, sound\levels\riverworld\sound_scenery\water_against_surface\water_against_surface.ssce
349C, sound\levels\riverworld\sound_scenery\water_brook\water_brook.ssce
349D, sound\levels\riverworld\sound_scenery\waterfall_close_churning\waterfall_close_churning.ssce
349E, sound\levels\riverworld\sound_scenery\backward_eagle\backward_eagle.ssce
349F, sound\levels\riverworld\sound_scenery\open_breeze\open_breeze.ssce
34A0, sound\materials\gear\generator_flood\generator_flood.ssce
34A1, sound\materials\gear\flood_lamp\flood_lamp.ssce
34A2, sound\levels\riverworld\sound_scenery\riverworld_rapids\riverworld_rapids.ssce
34A3, sound\levels\riverworld\sound_scenery\inside_waterfall\inside_waterfall.ssce
34A4, objects\gear\human\military\fusion_coil\fusion_coil.bloc
34A5, objects\gear\human\military\antennae_mast\antennae_mast.bloc
34A6, objects\gear\human\military\case\case.bloc
34A7, objects\gear\human\military\hu_mil_radio_small\hu_mil_radio_small.bloc
34A8, objects\gear\human\military\resupply_capsule_unfired\resupply_capsule_unfired.bloc
34A9, objects\gear\human\military\resupply_capsule_panel\resupply_capsule_panel.bloc
34AA, objects\gear\human\military\barricade_large\barricade_large.bloc
34AB, objects\gear\human\medical\medical_crate\medical_crate.bloc
34BE, objects\levels\multi\riverworld\man_cannon_river\man_cannon_river.bloc
34BF, objects\levels\multi\riverworld\man_cannon_river_short\man_cannon_river_short.bloc
34C0, objects\gear\covenant\military\crate_space\crate_space.bloc
34C1, objects\gear\human\military\drum_12gal\drum_12gal.bloc
34C2, objects\gear\human\military\blitzcan\blitzcan.bloc
34C3, objects\levels\solo\100_citadel\foliage\plant_tree_pine\plant_tree_pine.bloc
34C5, objects\levels\solo\100_citadel\foliage\plant_pine_tree_large\plant_pine_tree_large.bloc
34C6, objects\physics\nutblocker_1x1x2\nutblocker_1x1x2.bloc
34C8, objects\characters\ambient_life\butterfly_b\butterfly_b.crea
34C9, objects\characters\ambient_life\butterfly_c\butterfly_c.crea
34CA, objects\characters\ambient_life\butterfly_d\butterfly_d.crea
34CB, objects\characters\ambient_life\fish_elephantnose\fish_elephantnose.crea
360D, levels\multi\riverworld\fx\tower_pulse\damage_pulse.proj
360E, levels\multi\riverworld\fx\tower_pulse\tower_pulse.proj
3647, objects\vehicles\warthog\weapon\warthog_horn.weap
3677, objects\vehicles\banshee\weapon\banshee_bolt.proj
36AC, objects\vehicles\banshee\garbage\cab_garbage\cab_garbage.bloc
36BF, objects\vehicles\banshee\garbage\fin_top_garbage\fin_top_garbage.bloc
36C0, objects\vehicles\banshee\garbage\fin_bottom_garbage\fin_bottom_garbage.bloc
36C9, objects\vehicles\banshee\garbage\l_wing_garbage\l_wing_garbage.bloc
36CE, objects\vehicles\banshee\garbage\r_wing_garbage\r_wing_garbage.bloc
36E0, objects\vehicles\banshee\garbage\wing_prong_garbage\wing_prong_garbage.bloc
3742, objects\vehicles\wraith\turrets\mortar\mortar.vehi
3743, objects\vehicles\wraith\turrets\anti_infantry\anti_infantry.vehi
3744, objects\vehicles\wraith\turrets\anti_air\anti_air.vehi
3745, objects\vehicles\wraith\turrets\anti_infantry\anti_infantry_anti_air_wraith.vehi
377B, objects\vehicles\wraith\garbage\mortar_deflector_left\mortar_deflector_left.bloc
377C, objects\vehicles\wraith\garbage\mortar_gun\mortar_gun.bloc
378A, objects\vehicles\wraith\garbage\mortar_door\mortar_door.bloc
3799, objects\vehicles\wraith\turrets\anti_infantry\projectiles\wraith_bolt.proj
379F, objects\vehicles\wraith\garbage\anti_infantry_turret\anti_infantry_turret.bloc
37B0, objects\vehicles\wraith\turrets\anti_air\anti_air_turret.proj
37C6, objects\vehicles\wraith\garbage\anti_air_left_pods\anti_air_left_pods.bloc
37C7, objects\vehicles\wraith\garbage\anti_air_right_pods\anti_air_right_pods.bloc
37D7, objects\vehicles\wraith\garbage\wing_left\wing_left.bloc
37E7, objects\vehicles\wraith\garbage\wing_right\wing_right.bloc
37ED, objects\vehicles\wraith\garbage\hatch\hatch.bloc
37F2, objects\vehicles\wraith\garbage\wing_boost_left\wing_boost_left.bloc
37F7, objects\vehicles\wraith\garbage\wing_boost\wing_boost.bloc
37FC, objects\vehicles\wraith\garbage\rudder_shell\rudder_shell.bloc
3801, objects\vehicles\wraith\garbage\rudder_right\rudder_right.bloc
3806, objects\vehicles\wraith\garbage\mortar_hatch\mortar_hatch.bloc
380B, objects\vehicles\wraith\garbage\gear\gear.bloc
384F, objects\vehicles\ghost\weapon\ghost_bolt.proj
386B, objects\vehicles\ghost\garbage\l_wing_shell\l_wing_shell.bloc
386C, objects\vehicles\ghost\garbage\l_gun\l_gun.bloc
387E, objects\vehicles\ghost\garbage\anti_grav\anti_grav.bloc
3883, objects\vehicles\ghost\garbage\seat\seat.bloc
388B, objects\vehicles\ghost\garbage\hull_shell\hull_shell.bloc
38BF, objects\vehicles\brute_chopper\projectiles\chopper_gun_bullet.proj
38E9, objects\vehicles\brute_chopper\garbage\seat\seat.bloc
38FA, objects\vehicles\brute_chopper\garbage\rotor_r\rotor_r.bloc
3901, objects\vehicles\brute_chopper\garbage\rotor_l\rotor_l.bloc
3907, objects\vehicles\brute_chopper\garbage\bit_b\bit_b.bloc
390D, objects\vehicles\brute_chopper\garbage\bit_l\bit_l.bloc
3912, objects\vehicles\brute_chopper\garbage\bit_r\bit_r.bloc
3917, objects\vehicles\brute_chopper\garbage\blade_l\blade_l.bloc
391C, objects\vehicles\brute_chopper\garbage\blade_r\blade_r.bloc
395C, objects\gear\human\military\fusion_coil\garbage\fusion_coil_garbage01\fc_garbage_01.bloc
395D, objects\gear\human\military\fusion_coil\garbage\fusion_coil_garbage02\fc_garbage02.bloc
397E, objects\gear\human\military\case\garbage\case_bottom\case_bottom.bloc
397F, objects\gear\human\military\case\garbage\case_top\case_top.bloc
3A02, objects\characters\ambient_life\fish_elephantnose_destroyed\fish_elephantnose_destroyed.bloc
3A13, levels\multi\s3d_avalanche\sky\sky.scen
3A14, objects\levels\multi\snowbound\icicle_10_inch\icicle_10_inch.scen
3A15, objects\levels\multi\snowbound\icicle_18_inch\icicle_18_inch.scen
3A17, levels\multi\s3d_avalanche\crack.scen
3A18, objects\weapons\turret\missile_pod\missile_pod.vehi
3A19, objects\levels\multi\s3d_avalanche\avalanche_man_cannon_01\avalanche_man_cannon_01.mach
3A1A, objects\levels\multi\s3d_avalanche\avalanche_man_cannon_02\avalanche_man_cannon_02.mach
3A1B, levels\multi\s3d_avalanche\fx\s3d_avalanche_smoking_impact.efsc
3A1C, levels\multi\s3d_avalanche\fx\water_drip_splash.efsc
3A1D, levels\multi\s3d_reactor\fx\dust.efsc
3A1E, levels\multi\s3d_turf\fx\light_teal.efsc
3A1F, levels\multi\s3d_turf\fx\light_teal_dark.efsc
3A20, levels\multi\s3d_reactor\fx\bug_swarm.efsc
3A21, levels\multi\s3d_avalanche\fx\sand.efsc
3A22, levels\multi\s3d_edge\fx\steam.efsc
3A23, levels\multi\s3d_edge\fx\steam_2.efsc
3A24, levels\multi\s3d_reactor\fx\electric_arcs.efsc
3A25, levels\multi\s3d_edge\fx\steam_3.efsc
3A26, levels\multi\s3d_reactor\fx\s3d_reactor_effect_scenery_5.efsc
3A27, levels\multi\s3d_reactor\fx\steam.efsc
3A28, levels\multi\s3d_turf\fx\amber_sparks.efsc
3A29, levels\multi\s3d_avalanche\fx\bug_swarm.efsc
3A2A, levels\multi\s3d_reactor\fx\s3d_reactor_effect_scenery_6.efsc
3A2B, levels\multi\s3d_avalanche\fx\s3d_avalanche_effect_scenery_16.efsc
3A2C, levels\multi\s3d_avalanche\fx\splash.efsc
3A94, objects\levels\multi\snowbound\airlock_field\airlock_field.bloc
3A9F, objects\levels\s3d_avalanche\s3d_avalanche_crate_0.bloc
3AA0, objects\levels\s3d_avalanche\s3d_avalanche_crate_5.bloc
3AA1, objects\levels\s3d_avalanche\s3d_avalanche_crate_1.bloc
3AA2, objects\levels\s3d_avalanche\s3d_avalanche_crate_2.bloc
3AA3, objects\levels\s3d_avalanche\s3d_avalanche_crate_3.bloc
3AA4, objects\levels\s3d_avalanche\s3d_avalanche_crate_4.bloc
3AA7, objects\characters\ambient_life\bird_quadwing2\bird_quadwing2.crea
3AA8, objects\levels\multi\s3d_avalanche\seagull\seagull.crea
3C73, objects\levels\multi\snowbound\icicle_18_inch\garbage\icicle_18_inch_fallen\icicle_18_inch_fallen.bloc
3C94, objects\levels\dlc\sidewinder\hornet_lite\weapon\hornet_lite.weap
3C95, objects\levels\dlc\sidewinder\hornet_lite\weapon\hornet_lite_bullet.proj
3CA1, objects\vehicles\hornet\garbage\tail\tail_snow.bloc
3CAA, objects\vehicles\hornet\garbage\thrust_bell\thrust_bell_snow.bloc
3CAB, objects\vehicles\hornet\garbage\rotor\rotor_snow.bloc
3CAC, objects\vehicles\hornet\garbage\flap\flap_snow.bloc
3CB3, objects\vehicles\hornet\garbage\jet_r\jet_r_snow.bloc
3CB6, objects\vehicles\hornet\garbage\winch\winch_snow.bloc
3CB9, objects\vehicles\hornet\garbage\cover_r\cover_r_snow.bloc
3CBC, objects\vehicles\hornet\garbage\cover_l\cover_l_snow.bloc
3CBF, objects\vehicles\hornet\garbage\skid_r\skid_r_snow.bloc
3CC2, objects\vehicles\hornet\garbage\gun_l\gun_l_snow.bloc
3CD1, objects\weapons\turret\missile_pod\garbage\garbage_tripod\garbage_tripod.bloc
3DDE, levels\multi\s3d_edge\sky\sky.scen
3DDF, levels\multi\s3d_edge\s3d_edge\amb_edge_waterfall.ssce
3DE0, levels\multi\s3d_edge\fx\s3d_edge_effect_scenery_4.efsc
3DE1, levels\multi\s3d_edge\fx\s3d_edge_effect_scenery_5.efsc
3DE2, levels\multi\s3d_edge\fx\s3d_edge_effect_scenery_6.efsc
3DE3, levels\multi\s3d_edge\fx\s3d_edge_effect_scenery_7.efsc
3DE4, levels\multi\s3d_edge\fx\s3d_edge_effect_scenery_8.efsc
3DE5, levels\multi\s3d_edge\fx\s3d_edge_effect_scenery_9.efsc
3DE6, levels\multi\s3d_edge\fx\s3d_edge_effect_scenery_10.efsc
3E08, objects\levels\s3d_edge\s3d_edge_crate_0.bloc
3E0B, objects\characters\sentinel_constructor\sentinel_constructor.crea
3E0C, objects\characters\sentinel_constructor2\sentinel_constructor2.crea
3F60, objects\characters\sentinel_constructor\garbage\body\body.bloc
3F61, objects\characters\sentinel_constructor\garbage\engine\engine.bloc
3F88, levels\multi\s3d_reactor\sky\sky.scen
3F89, objects\levels\multi\s3d_reactor\frigate\frigate.scen
3F8A, levels\multi\s3d_reactor\reactor_flare.scen
3F8B, levels\multi\s3d_reactor\reactor_flare_blue.scen
3F8C, levels\multi\s3d_reactor\reactor_flare_yellow.scen
3F95, objects\levels\multi\s3d_reactor\rod\rod.mach
3F96, objects\levels\multi\s3d_reactor\reactor_lightnings\reactor_lightnings.mach
3F97, levels\multi\s3d_reactor\s3d_reactor\amb_wind_reactor_hole.ssce
3F98, levels\multi\s3d_reactor\s3d_reactor\water_splash.ssce
3F99, levels\multi\s3d_reactor\s3d_reactor\amb_wind_trees.ssce
3F9A, levels\multi\s3d_avalanche\fx\mancannon\mancannon.efsc
3F9B, levels\multi\s3d_reactor\fx\s3d_reactor_effect_scenery_1.efsc
3F9C, levels\multi\s3d_reactor\fx\s3d_reactor_effect_scenery_2.efsc
3F9D, levels\multi\s3d_turf\fx\s3d_turf_effect_scenery_8.efsc
3F9E, levels\multi\s3d_turf\fx\s3d_turf_effect_scenery_6.efsc
3F9F, levels\multi\s3d_reactor\fx\s3d_reactor_effect_scenery_13.efsc
3FA0, levels\multi\s3d_turf\fx\s3d_turf_effect_scenery_2.efsc
3FA1, levels\multi\s3d_reactor\fx\s3d_reactor_effect_scenery_16.efsc
3FA2, levels\multi\s3d_reactor\fx\s3d_reactor_effect_scenery_17.efsc
3FA3, levels\multi\s3d_reactor\fx\s3d_reactor_effect_scenery_18.efsc
3FA4, levels\multi\s3d_reactor\fx\s3d_reactor_effect_scenery_19.efsc
3FA5, levels\multi\s3d_turf\fx\s3d_turf_effect_scenery_4.efsc
3FA6, levels\multi\s3d_turf\fx\s3d_turf_effect_scenery_10.efsc
3FA7, levels\multi\s3d_reactor\fx\s3d_reactor_effect_scenery_22.efsc
3FA8, levels\multi\s3d_reactor\fx\s3d_reactor_effect_scenery_23.efsc
3FA9, levels\multi\s3d_reactor\fx\s3d_reactor_effect_scenery_24.efsc
3FAA, levels\multi\s3d_reactor\fx\s3d_reactor_effect_scenery_27.efsc
3FAB, levels\multi\s3d_reactor\fx\s3d_reactor_effect_scenery_28.efsc
3FD3, objects\levels\multi\s3d_turf\s3d_turf_turf_crate_large\s3d_turf_turf_crate_large.bloc
3FD4, objects\gear\human\military\barricade_small\barricade_small.bloc
3FD5, objects\gear\human\military\h_ammo_crate_sm\h_ammo_crate_sm.bloc
3FD6, objects\gear\human\military\h_ammo_crate_lg\h_ammo_crate_lg.bloc
3FD9, objects\characters\ambient_life\lod_pelican\lod_pelican.crea
3FDA, objects\characters\ambient_life\lod_hornet\lod_hornet.crea
41E8, levels\multi\s3d_turf\sky\sky.scen
41E9, objects\levels\multi\snowbound\icicle_06_inch\icicle_06_inch.scen
41EA, levels\multi\s3d_turf\radio_tower.scen
41EB, levels\multi\s3d_turf\cart_electric.scen
41EC, levels\multi\s3d_turf\turf_train.scen
41ED, levels\multi\s3d_turf\turf_wagon.scen
41EF, levels\multi\s3d_turf\turf_hornet.scen
41F3, objects\levels\multi\s3d_turf\fabric_01_moving\fabric_01_moving.mach
41F4, objects\levels\multi\s3d_turf\fabric_04_moving\fabric_04_moving.mach
41F5, objects\levels\multi\s3d_turf\fabric_15_moving\fabric_15_moving.mach
41F6, levels\multi\s3d_turf\s3d_turf\water_splash.ssce
41F7, levels\multi\s3d_turf\s3d_turf\water_splash_metal.ssce
41F8, levels\multi\s3d_turf\s3d_turf\energy_barrier.ssce
41F9, levels\multi\s3d_turf\s3d_turf\vent_hum.ssce
41FA, levels\multi\s3d_turf\fx\s3d_turf_effect_scenery_0.efsc
41FB, levels\multi\s3d_turf\fx\s3d_turf_effect_scenery_5.efsc
41FC, levels\multi\s3d_turf\fx\s3d_turf_effect_scenery_light_rain.efsc
41FD, levels\multi\s3d_turf\fx\s3d_turf_effect_scenery_11.efsc
4220, objects\levels\multi\s3d_turf\s3d_turf_crate_heavy_tech\s3d_turf_crate_heavy_tech.bloc
4221, objects\gear\human\industrial\crate_multi\crate_multi_destructible.bloc
4222, objects\gear\human\industrial\pallet\pallet.bloc
4223, objects\levels\multi\s3d_turf\turf_swinging_door\turf_swinging_door.bloc
4224, objects\levels\multi\s3d_turf\s3d_turf_cabinet\s3d_turf_cabinet.bloc
4225, objects\levels\multi\s3d_turf\s3d_turf_dumpster\s3d_turf_dumpster.bloc
4226, objects\levels\multi\s3d_turf\s3d_turf_trash_can\s3d_turf_trash_can.bloc
430C, objects\multi\s3d_turf\turf_cart_electric\garbage\cart_axle_garbage.bloc
430D, objects\multi\s3d_turf\turf_cart_electric\garbage\cart_tire_garbage.bloc
430E, objects\multi\s3d_turf\turf_cart_electric\garbage\cart_seat_garbage.bloc
433A, objects\vehicles\warthog\turrets\chaingun\chaingun_snow.vehi
433B, objects\vehicles\warthog\turrets\gauss\gauss_snow.vehi
4348, objects\vehicles\warthog\turrets\chaingun\garbage\barrel_garbage\barrel_garbage_snow.bloc
434E, objects\vehicles\warthog\turrets\chaingun\garbage\shield_garbage\shield_garbage_snow.bloc
4355, objects\vehicles\warthog\turrets\gauss\garbage\barrel_garbage\barrel_garbage_snow.bloc
435B, objects\vehicles\warthog\turrets\gauss\garbage\display_garbage\display_garbage_snow.bloc
435C, objects\vehicles\warthog\turrets\gauss\garbage\shield_garbage\shield_garbage_snow.bloc
4361, objects\vehicles\warthog\garbage\tire\tire_snow.bloc
4368, objects\vehicles\warthog\garbage\winch\winch_snow.bloc
4369, objects\vehicles\warthog\garbage\hood\hood_snow.bloc
4376, objects\vehicles\warthog\garbage\hubcap\hubcap_snow.bloc
4379, objects\vehicles\warthog\garbage\lf_fender\lf_fender_snow.bloc
437C, objects\vehicles\warthog\garbage\rf_fender\rf_fender_snow.bloc
437F, objects\vehicles\warthog\garbage\lb_fender\lb_fender_snow.bloc
4380, objects\vehicles\warthog\garbage\sailpanel\sailpanel_snow.bloc
4385, objects\vehicles\warthog\garbage\rb_fender\rb_fender_snow.bloc
4388, objects\vehicles\warthog\garbage\bumper\bumper_snow.bloc
43F3, objects\gear\human\industrial\crate_multi_single\crate_multi_single.bloc
4402, objects\gear\human\industrial\crate_multi\garbage\busted_frame1\busted_frame1.bloc
4403, objects\gear\human\industrial\crate_multi\garbage\busted_frame3\busted_frame3.bloc
4441, objects\vehicles\warthog\warthog_gauss.vehi
4442, objects\vehicles\warthog\warthog_troop.vehi
4443, objects\vehicles\warthog\warthog_no_turret.vehi
4444, objects\vehicles\warthog\warthog_snow_gauss.vehi
4445, objects\vehicles\warthog\warthog_snow_troop.vehi
4446, objects\vehicles\warthog\warthog_snow_no_turret.vehi
4447, objects\vehicles\warthog\warthog_wrecked.vehi
4448, objects\vehicles\warthog\warthog_snow_wrecked.vehi
4449, objects\vehicles\wraith\wraith_anti_air.vehi
444A, objects\levels\multi\s3d_turf\s3d_turf_cart_electric\s3d_turf_cart_electric.bloc
444C, objects\gear\human\industrial\crate_multi\crate_multi.bloc
444E, objects\multi\teleporter_sender\teleporter_sender_vehicle.bloc
444F, objects\multi\teleporter_receiver\teleporter_receiver_vehicle.bloc
4450, objects\multi\teleporter_2way\teleporter_2way_vehicle.bloc
4451, objects\multi\teleporter_sender\teleporter_sender_vehicle_only.bloc
4452, objects\multi\teleporter_receiver\teleporter_receiver_vehicle_only.bloc
4453, objects\multi\teleporter_2way\teleporter_2way_vehicle_only.bloc
45DD, levels\multi\cyberdyne\sky\sky.scen
4608, objects\gear\human\residential\office_table\office_table.scen
461D, objects\gear\human\industrial\tarp_box_stack\tarp_box_stack.scen
4628, objects\gear\human\industrial\tarp_crate_large_a\tarp_crate_large_a.scen
4662, objects\gear\human\military\missle_stand\missle_stand.scen
4686, objects\levels\multi\cyberdyne\cyber_monitor_med\cyber_monitor.scen
469F, objects\levels\solo\020_base\monitor_sm\monitor_sm.scen
46A4, objects\levels\solo\020_base\sink\sink.scen
46AD, objects\levels\solo\020_base\armory_shelf\armory_shelf.scen
46BE, objects\levels\multi\cyberdyne\cyber_speaker\cyber_speaker.scen
4703, objects\levels\multi\cyberdyne\security_camera\security_camera.vehi
4710, objects\levels\dlc\chillout\mon_defender_integrated\mon_defender_integrated.weap
476D, objects\levels\multi\cyberdyne\jump_fan\jump_fan.mach
477A, objects\levels\multi\cyberdyne\brute_target\brute_target.mach
4791, objects\levels\multi\cyberdyne\jackal_target\jackal_target.mach
479E, sound\levels\cyberdyne\sound_scenery\africam_birds_verb\africam_birds_verb.ssce
47A1, sound\levels\cyberdyne\sound_scenery\zanzibar_distant_flyby\zanzibar_distant_flyby.ssce
47A4, sound\levels\cyberdyne\sound_scenery\dist_expl2\dist_expl2.ssce
47A7, sound\levels\cyberdyne\sound_scenery\flourescent_light\flourescent_light.ssce
47AA, sound\device_machines\doors_lifts\cyberdine_fan_wind\cyberdine_fan_wind.ssce
47AD, levels\multi\cyberdyne\fx\airlift\airlift.efsc
47B1, objects\gear\human\industrial\box_metal_small\box_metal_small.bloc
47BA, objects\gear\human\military\generator\generator_off.bloc
47BB, objects\gear\human\military\generator_light\generator_flood_no_lights.bloc
47CD, objects\gear\human\industrial\generator_heavy_grill\generator_heavy_grill.bloc
47E0, objects\gear\human\industrial\generator_heavy_grill\garbage\generator_heavy_grill_top_piece\generator_heavy_grill_top_piece.bloc
47E5, objects\gear\human\residential\h_locker_closed_mp\h_locker_closed_mp.bloc
47F2, objects\gear\human\industrial\jersey_barrier\jersey_barrier.bloc
47FD, objects\gear\human\industrial\jersey_barrier\garbage_chunks\jersey_chunk_left_med\jersey_chunk_left_med.bloc
4809, objects\gear\human\industrial\jersey_barrier\garbage_chunks\jersey_chunk_right_med\jersey_chunk_right_med.bloc
480F, objects\gear\human\industrial\jersey_barrier\garbage_chunks\jersey_chunk_front_med\jersey_chunk_front_med.bloc
4815, objects\gear\human\industrial\jersey_barrier\garbage_chunks\jersey_chunk_back_med\jersey_chunk_back_med.bloc
481A, objects\gear\human\industrial\jersey_barrier_short\jersey_barrier_short.bloc
4820, objects\gear\human\industrial\jersey_barrier_short\garbage_chunks\med_c_left\med_c_left.bloc
4825, objects\gear\human\industrial\jersey_barrier_short\garbage_chunks\med_c_left_back\med_c_left_back.bloc
482A, objects\gear\human\industrial\jersey_barrier_short\garbage_chunks\med_c_left_front\med_c_left_front.bloc
482F, objects\gear\human\industrial\jersey_barrier_short\garbage_chunks\med_c_left_top\med_c_left_top.bloc
4835, objects\gear\human\industrial\jersey_barrier_short\garbage_chunks\med_c_right\med_c_right.bloc
483A, objects\gear\human\industrial\jersey_barrier_short\garbage_chunks\med_c_right_back\med_c_right_back.bloc
483F, objects\gear\human\industrial\jersey_barrier_short\garbage_chunks\med_c_right_front\med_c_right_front.bloc
4844, objects\gear\human\industrial\jersey_barrier_short\garbage_chunks\med_c_right_top\med_c_right_top.bloc
4849, objects\gear\human\military\missle_body\missle_body.bloc
484E, objects\gear\human\military\missle_cap\missle_cap.bloc
4853, objects\gear\human\industrial\toolbox_small\toolbox_small.bloc
4865, objects\gear\human\industrial\toolbox_large\toolbox_large.bloc
486D, objects\gear\human\industrial\propane_tank\propane_tank.bloc
48EF, objects\gear\human\residential\telephone_wall_box\telephone_wall_box.bloc
4904, objects\gear\human\residential\telephone_wall_handset\telephone_wall_handset.bloc
490E, objects\gear\human\residential\office_keyboard\office_keyboard.bloc
4918, objects\gear\human\residential\office_monitor\office_monitor.bloc
4923, objects\gear\human\residential\office_file_tall\office_file_tall.bloc
492F, objects\gear\human\residential\office_file_short\office_file_short.bloc
4934, objects\gear\human\residential\office_chair\office_chair.bloc
493D, objects\gear\human\industrial\generator_heavy_kettle\generator_heavy_kettle.bloc
4946, objects\gear\human\industrial\generator_heavy_kettle\garbage\generator_heavy_kettle_keg\generator_heavy_kettle_keg.bloc
494B, objects\gear\human\industrial\generator_heavy_kettle\garbage\generator_heavy_kettle_meter\generator_heavy_kettle_meter.bloc
4950, objects\levels\multi\cyberdyne\cyber_pad\cyber_pad.bloc
4965, objects\vehicles\civilian\forklift\forklift.bloc
4984, objects\vehicles\civilian\forklift\garbage\arms_destro\arms_destro.bloc
4989, objects\vehicles\civilian\forklift\garbage\cage_a\cage_a.bloc
498E, objects\vehicles\civilian\forklift\garbage\engine_door\engine_door.bloc
4993, objects\vehicles\civilian\forklift\garbage\small_wheel\small_wheel.bloc
4998, objects\vehicles\civilian\forklift\garbage\large_wheel\large_wheel.bloc
49A1, objects\characters\ambient_life\bird_cockatoo\bird_cockatoo.crea
49B1, objects\characters\ambient_life\bird_cockatoo_dead\bird_cockatoo_dead.bloc
49BA, objects\characters\ambient_life\bird_guira\bird_guira.crea
49C3, objects\characters\ambient_life\bird_guira_dead\bird_guira_dead.bloc
49CA, objects\characters\ambient_life\bird_hornbill\bird_hornbill.crea
49D3, objects\characters\ambient_life\bird_hornbill_wing_right_dead\bird_hornbill_wing_right_dead.bloc
49D9, objects\characters\ambient_life\bird_hornbill_wing_left_dead\bird_hornbill_wing_left_dead.bloc
49DE, objects\characters\ambient_life\bird_hornbill_dead_body\bird_hornbill_dead_body.bloc
49E4, objects\characters\ambient_life\bird_small_multi\bird_small_multi.crea
49EC, objects\characters\ambient_life\bird_small_multi_dead\bird_small_multi_dead.bloc
49F2, objects\characters\ambient_life\bird_smallyellow\bird_smallyellow.crea
49FB, objects\characters\ambient_life\bird_smallyellow_dead\bird_smallyellow_dead.bloc
4B0F, levels\multi\chill\sky\sky.scen
4B4E, objects\levels\multi\snowbound\icicle_24_inch\icicle_24_inch.scen
4B5E, objects\levels\multi\snowbound\icicle_24_inch\garbage\icicle_24_inch_fallen\icicle_24_inch_fallen.bloc
4B81, sound\levels\chill\chill_heavy_wind\chill_heavy_wind.ssce
4B85, sound\levels\chill\sound_scenery\waterfall_far\waterfall_far.ssce
4B88, sound\levels\chill\sound_scenery\energy_beam\energy_beam.ssce
4B8B, sound\levels\chill\sound_scenery\chill_waterfall_stereo\chill_waterfall_stereo.ssce
4B8F, sound\levels\chill\chill_waterfall_ambient\chill_waterfall_ambient.ssce
4B92, sound\levels\chill\sound_scenery\blue_beams\blue_beams.ssce
4B95, sound\levels\chill\sound_scenery\blue_beams_down\blue_beams_down.ssce
4B98, sound\levels\chill\sound_scenery\blue_beams_down_more\blue_beams_down_more.ssce
4B9B, sound\levels\chill\sound_scenery\waterfall_far_obstructed\waterfall_far_obstructed.ssce
4B9E, sound\levels\chill\sound_scenery\inside_waterfall\inside_waterfall.ssce
4BA1, levels\multi\chill\fx\mancannon\mancannon.efsc
4BA3, objects\gear\human\military\h_ammo_crate_sm\h_ammo_crate_sm.bloc
4BAA, objects\gear\human\military\propane_burner\propane_burner.bloc
4BC8, objects\levels\multi\chill\man_cannon_chill\man_cannon_chill.bloc
4CD6, levels\dlc\bunkerworld\sky\bunkerworld.scen
4D16, objects\gear\human\military\sandbag_wall_turret\sandbag_wall_turret.scen
4D20, levels\dlc\bunkerworld\sky\dish\dish.scen
4D4F, objects\levels\dlc\bunkerworld\sandbag_wall_45corner_bunker\sandbag_wall_45corner_bunker.scen
4D54, levels\dlc\bunkerworld\sky\dish\dish_no_sound.scen
4D7D, objects\levels\dlc\bunkerworld\tunnel_door\tunnel_door.mach
4D8C, objects\levels\dlc\bunkerworld\shutter_doors\shutter_doors.mach
4D9A, objects\levels\dlc\bunkerworld\button\button.mach
4DA6, objects\levels\dlc\bunkerworld\door_sign\door_sign.mach
4DA7, objects\levels\dlc\bunkerworld\shutter_doors_blue_base\shutter_doors_blue_base.mach
4DBA, objects\levels\dlc\bunkerworld\button_control\button_control.ctrl
4DC2, objects\device_machines\controls\invisible_switch\bunkerworld_invisible_switch.ctrl
4DCA, sound\levels\dlc\bunkerworld\sound_scenery\stereo_leaves_details\stereo_leaves_details.ssce
4DCD, sound\levels\dlc\bunkerworld\sound_scenery\computer3\computer3.ssce
4DD0, sound\levels\dlc\bunkerworld\sound_scenery\computer6\computer6.ssce
4DD4, sound\levels\dlc\bunkerworld\sound_scenery\computer_telemetry\computer_telemetry.ssce
4DD7, sound\levels\dlc\bunkerworld\sound_scenery\h_server_loop\h_server_loop.ssce
4DDB, sound\levels\dlc\bunkerworld\sound_scenery\antennae_hum\antennae_hum.ssce
4DDE, levels\dlc\bunkerworld\fx\dish_lights\red_pulse.efsc
4DE2, levels\dlc\bunkerworld\fx\dish_lights\red_blink.efsc
4DE6, levels\dlc\bunkerworld\fx\dish_lights\blue_static.efsc
4DE9, levels\dlc\bunkerworld\fx\dish_lights\white_static.efsc
4DEF, objects\levels\dlc\warehouse\fencebox\fencebox.bloc
4DF8, objects\levels\dlc\bunkerworld\drum_55gal_bunker\drum_55gal_bunker.bloc
4E08, objects\levels\dlc\warehouse\dlc_wh_wire_spool_large\dlc_wh_wire_spool_large.bloc
4E10, objects\levels\dlc\warehouse\dlc_wh_crate_large_open\dlc_wh_crate_large_open.bloc
4E20, objects\levels\dlc\warehouse\bridge\bridge.bloc
4E2B, objects\levels\dlc\shared\large_shield_door\large_shield_door.bloc
4E41, objects\levels\dlc\shared\small_shield_door\small_shield_door.bloc
4E46, objects\levels\dlc\shared\man_cannon_forge\man_cannon_forge.bloc
4E58, objects\levels\dlc\shared\soccer_ball\soccer_ball.vehi
4EE3, objects\gear\human\military\case_ap_turret_open\case_ap_turret_open.bloc
4EF0, objects\gear\human\military\case_ap_turret_lid\case_ap_turret_lid.bloc
4EF5, objects\levels\solo\030_outskirts\foliage\small_bush\small_bush.bloc
4F04, objects\levels\solo\030_outskirts\foliage\bushlow\bushlow.bloc
4F09, objects\levels\solo\030_outskirts\foliage\bushc\bushc.bloc
50A6, levels\multi\zanzibar\sky\sky.scen
50E0, objects\levels\multi\zanzibar\dome_light\dome_light.scen
50E9, objects\gear\human\military\sandbag_wall_90corner\sandbag_wall_90corner.scen
50EE, objects\gear\human\military\sandbag_wall_endcap\sandbag_wall_endcap.scen
50F3, objects\gear\human\military\sandbag_wall\sandbag_wall.scen
50F8, objects\gear\human\military\sandbag_detail1\sandbag_detail1.scen
50FD, objects\gear\human\military\sandbag_detail2\sandbag_detail2.scen
5102, objects\levels\multi\zanzibar\zan_barricade\zan_barricade.mach
510D, objects\levels\multi\zanzibar\big_wheel\big_wheel.mach
511C, objects\levels\multi\zanzibar\house_gate\house_gate.mach
512B, objects\levels\multi\zanzibar\gate_control\house_gate_control.ctrl
5138, sound\device_machines\zanzibar_big_fan\zanzibar_big_fan_center\zanzibar_big_fan_center.ssce
513B, sound\levels\zanzibar\zanzibar_sound_scenery\zanzibar_distant_battle\zanzibar_distant_battle.ssce
513F, sound\levels\zanzibar\zanzibar_sound_scenery\zanzibar_distant_flyby\zanzibar_distant_flyby.ssce
5142, sound\levels\zanzibar\zanzibar_sound_scenery\waves\waves.ssce
5145, sound\levels\zanzibar\zanzibar_sound_scenery\waves2\waves2.ssce
5148, sound\levels\zanzibar\zanzibar_sound_scenery\monopoint_seagulls\monopoint_seagulls.ssce
514B, sound\levels\zanzibar\zanzibar_sound_scenery\earthcity_birds\earthcity_birds.ssce
514E, sound\levels\zanzibar\zanzibar_ocean_loop\ocean_loop_point.ssce
5150, sound\levels\zanzibar\zanzibar_sound_scenery\flourescent_light\flourescent_light.ssce
5153, sound\levels\zanzibar\zanzibar_base_turbine_close\zanzibar_base_turbine_close.ssce
5156, sound\levels\zanzibar\zanzibar_froman_power_point\zanzibar_froman_power_point.ssce
5159, sound\levels\zanzibar\zanzibar_froman_power_point_low\zanzibar_froman_power_point_low.ssce
515C, sound\device_machines\zanzibar_big_fan\zanzibar_big_fan_center_stereo\zanzibar_big_fan_center_stereo.ssce
515F, sound\levels\zanzibar\zanzibar_sound_scenery\water_against_surface\water_against_surface.ssce
5162, sound\levels\zanzibar\zanzibar_sound_scenery\zanzibar_stereo_wind\zanzibar_stereo_wind.ssce
517A, objects\gear\human\industrial\crate_tech_semi_short_closed\crate_tech_semi_short_closed.bloc
5189, objects\levels\dlc\shared\crate_tech_semi_short\crate_tech_semi_short.bloc
518E, objects\gear\human\industrial\garbage_can\garbage_can.bloc
51C7, objects\levels\multi\zanzibar\main_crane\main_crane.bloc
51EA, objects\levels\multi\zanzibar\awning_def\awning_def.bloc
51FD, objects\levels\multi\zanzibar\hinge_light\hinge_light.bloc
5210, objects\levels\multi\zanzibar\battle_shield\battle_shield.bloc
521A, objects\levels\solo\010_jungle\foliage\plant_bush_med_palm\plant_bush_med_palm.bloc
5225, objects\levels\solo\010_jungle\foliage\plant_tree_palm\plant_tree_palm.bloc
522A, objects\levels\multi\zanzibar\foliage\plant_bush_large_palm\plant_bush_large_palm.bloc
5230, objects\characters\ambient_life\seagull\seagull.crea
5239, objects\characters\ambient_life\seagull_dead\seagull_dead.bloc
5301, levels\multi\deadlock\sky\deadlock.scen
5326, objects\gear\human\military\h_ammo_crate_lg\h_ammo_crate_lg.bloc
532B, levels\shared\widgets\deadlock_leaves.scen
535B, objects\levels\multi\deadlock\switch_lever\switch_lever.mach
535F, objects\levels\multi\deadlock\deadlock_gate_v3\deadlock_gate_v3.mach
536A, objects\levels\multi\deadlock\switch_monitor\switch_monitor.ctrl
536B, objects\levels\multi\deadlock\switch_monitor_on\switch_monitor_on.ctrl
536C, sound\levels\deadlock\sound_scenery\baby_kookaburra_close\baby_kookaburra_close.ssce
536F, sound\levels\deadlock\sound_scenery\stereo_cicadas_loop\stereo_cicadas_loop.ssce
5372, sound\levels\deadlock\sound_scenery\drips\drips.ssce
5375, sound\levels\deadlock\sound_scenery\computer_beeps\computer_beeps.ssce
5378, sound\levels\deadlock\sound_scenery\flybys_halo3\flybys_halo3.ssce
537A, sound\levels\deadlock\sound_scenery\lakeside_breeze\lakeside_breeze.ssce
537D, sound\levels\deadlock\sound_scenery\water_light_lapping\water_light_lapping.ssce
5380, objects\vehicles\mauler\mauler.vehi
53A2, objects\vehicles\mauler\anti_infantry\anti_infantry.vehi
53A9, objects\vehicles\mauler\garbage\turret_garbage\turret_garbage.bloc
53AE, objects\vehicles\mauler\anti_infantry\weapon\anti_infantry.weap
53AF, objects\vehicles\mauler\anti_infantry\weapon\mauler_bolt.proj
53B4, objects\vehicles\mauler\garbage\left_skid_garbage\left_skid_garbage.bloc
53BA, objects\vehicles\mauler\garbage\right_skid_garbage\right_skid_garbage.bloc
53C0, objects\vehicles\mauler\garbage\seat_garbage\seat_garbage.bloc
53C6, objects\vehicles\mauler\garbage\hull_garbage\hull_garbage.bloc
53CD, objects\vehicles\mauler\garbage\sprocket_garbage\sprocket_garbage.bloc
53D2, objects\vehicles\mauler\garbage\gear_garbage\gear_garbage.bloc
53D8, objects\vehicles\mauler\garbage\scoop_garbage\scoop_garbage.bloc
53DE, objects\vehicles\mauler\garbage\screen_garbage\screen_garbage.bloc
53E3, objects\vehicles\mauler\garbage\clamp_garbage\clamp_garbage.bloc
541C, objects\gear\human\medical\cabinet\cabinet.bloc
5427, objects\levels\solo\020_base\computer_briefcase\computer_briefcase.bloc
5441, objects\levels\solo\020_base\computer_briefcase_small\computer_briefcase_small.bloc
544A, objects\gear\human\medical\crashcart\crashcart.bloc
5460, objects\gear\human\medical\medical_tray\medical_tray.bloc
546B, objects\gear\human\military\comm_phonebox\comm_phonebox.bloc
5474, objects\gear\human\military\comm_phone\comm_phone.bloc
5495, objects\levels\multi\deadlock\wall_hatch\wall_hatch.bloc
54A2, objects\levels\multi\deadlock\deadlock_chainlinkgate\deadlock_chainlinkgate.bloc
54D1, objects\gear\human\residential\office_stand\office_stand.bloc
54D7, objects\levels\multi\deadlock\deadlock_chainlinkgate_ii\deadlock_chainlinkgate_ii.bloc
54DC, objects\levels\multi\deadlock\pipe_block\pipe_block.bloc
5599, levels\multi\shrine\sky\sky.scen
55E6, objects\levels\multi\shrine\marinebeacon\marinebeacon.scen
55EE, objects\cinematics\human\frigate\frigate\frigate_shrine.scen
55FB, objects\vehicles\phantom\phantom_damaged\garbage\phd_backpiece_lg_top\phd_backpiece_lg_top.scen
5600, objects\vehicles\phantom\phantom_damaged\garbage\phd_backpiece_md\phd_backpiece_md.scen
5605, objects\vehicles\phantom\phantom_damaged\garbage\phd_backpiece_sm\phd_backpiece_sm.scen
560A, objects\vehicles\phantom\phantom_damaged\garbage\phd_bottompiece\phd_bottompiece.scen
5614, objects\vehicles\phantom\phantom_damaged\garbage\phd_flatpiece_r\phd_flatpiece_r.scen
561E, objects\vehicles\phantom\phantom_damaged\garbage\phd_sidepiece_l\phd_sidepiece_l.scen
5623, objects\vehicles\phantom\phantom_damaged\garbage\phd_sidepiece_r\phd_sidepiece_r.scen
5628, objects\levels\multi\shrine\behemoth\behemoth.vehi
5680, objects\levels\multi\shrine\behemoth\behemoth_turret.vehi
5682, objects\levels\multi\shrine\behemoth\weapon\behemoth_chaingun_turret.weap
5683, objects\levels\multi\shrine\behemoth\weapon\behemoth_chaingun_bullet.proj
5690, objects\levels\multi\shrine\behemoth\weapon\behemoth_horn.weap
5699, objects\levels\multi\shrine\shrine_defender\shrine_defender.vehi
569F, objects\levels\multi\shrine\shrine_defender\shrine_defender_minelayer.weap
56A0, objects\levels\multi\shrine\shrine_defender\jumpmine\jumpmine_invisible_minelayer.proj
56A2, objects\levels\multi\shrine\shrine_defender\jumpmine\jumpmine.proj
56CF, sound\device_machines\doors_lifts\construct_main_lift\construct_main_lift.ssce
56D2, sound\levels\shrine\shrine_ship\shrine_ship.ssce
56D5, sound\device_machines\doors_lifts\shrine_main_lift\shrine_main_lift.ssce
56D8, levels\multi\shrine\fx\jump_pad\jump_pad.efsc
56F5, objects\levels\multi\shrine\sand_jump_pad\sand_jump_pad.bloc
5709, objects\gear\human\industrial\crowbar\crowbar.bloc
5728, objects\eldewrito\forge\map_modifier.scen
5729, unused.scen
5A68, objects\equipment\tripmine\tripmine_forge.eqip
5A69, objects\levels\multi\shrine\behemoth\behemoth_forge.vehi
5A83, objects\gear\human\industrial\sawhorse\sawhorse_light.bloc
5A86, objects\multi\generic\mp_cinematic_camera.scen
5A8E, objects\multi\boundaries\kill_volume.bloc
5A8F, objects\multi\boundaries\garbage_collection_volume.bloc
5A91, objects\weapons\melee\energy_blade\unarmed.weap
5AD5, objects\levels\multi\guardian\holy_light_guardian\holy_light_guardian_forge.bloc
";

    }
}
