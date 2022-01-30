using CacheEditor;
using EpsilonLib.Commands;
using Shared;
using SimpleJSON;
using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ServerJsonEditor
{
    class ServerJsonEditorViewModel : Screen, INotifyPropertyChanged
    {
        private IShell _shell;
        private ICacheFile _cacheFile;
        private string _output;

        public class MapEntry
        {
            public string DisplayName { get; set; }
            public string MapFileName { get; set; }
        }
        public class ModEntry : JSONNode
        {
            public string FileName { get; set; }
            public string DisplayName { get; set; }
            public string Link { get; set; }
        }
        public class TypeEntry : INotifyPropertyChanged
        {
            private string _typeName;
            private string _typeDisplayName;
            private int _duplicateAmount;
            private ModEntry _modPackage;
            private ObservableCollection<ServerCommand> _commands;
            private ObservableCollection<CharacterOverride> _characterOverrides;
            private ObservableCollection<MapEntry> _specificMaps;

            public string TypeName { get => _typeName; set => _typeName = value; }
            public string TypeDisplayName { get => _typeDisplayName; set => _typeDisplayName = value; }
            public int DuplicateAmount { get => _duplicateAmount; set => _duplicateAmount = value; }
            public ModEntry ModPackage { get => _modPackage; set => _modPackage = value; }
            public ObservableCollection<ServerCommand> Commands { get => _commands; set => _commands = value; }
            public ObservableCollection<CharacterOverride> CharacterOverrides { get => _characterOverrides; set => _characterOverrides = value; }
            public ObservableCollection<MapEntry> SpecificMaps { get => _specificMaps; set => _specificMaps = value; }

            public event PropertyChangedEventHandler PropertyChanged;

            private void NotifyPropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public class ServerCommand : INotifyPropertyChanged
        {
            private string _name;
            private string _alias;
            private bool _enabled;
            public event PropertyChangedEventHandler PropertyChanged;

            public string Name { get => _name; set => _name = value; }
            public string Alias { get => _alias; set => _alias = value; }
            public bool Enabled
            {
                get => _enabled;
                set { _enabled = value; NotifyPropertyChanged("Commands"); }
            }

            private void NotifyPropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public class CharacterOverride : INotifyPropertyChanged
        {
            private string _team;
            private string _character;
            public event PropertyChangedEventHandler PropertyChanged;

            public string Team
            {
                get => _team;
                set { _team = value; NotifyPropertyChanged("Commands"); }
            }

            public string Character
            {
                get => _character;
                set { _character = value; NotifyPropertyChanged("Commands"); }
            }

            private void NotifyPropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private Dictionary<string, JSONNode> modsJsonDictionary;
        private JSONArray votingDefaultMapsArray;
        private JSONArray votingTypesArray;
        private ObservableCollection<string> _privateLocalMods;
        private ObservableCollection<string> _privateLocalGametypes;
        private ObservableCollection<ModEntry> _CurrentModList;
        private ObservableCollection<TypeEntry> _CurrentGametypeList;
        private ObservableCollection<ServerCommand> _CurrentEntryCommands;
        private ObservableCollection<CharacterOverride> _CurrentCharacterOverrides;
        private ObservableCollection<MapEntry> _CurrentSpecificMaps;
        private ObservableCollection<MapEntry> VotingDefaultMaps;

        private ObservableCollection<ServerCommand> CommandDefaults = new ObservableCollection<ServerCommand>()
        {
            new ServerCommand{ Name = "Server.SprintEnabled", Alias="Sprint Allowed", Enabled = true },
            new ServerCommand{ Name = "Server.UnlimitedSprint", Alias="Unlimited Sprint", Enabled = false },
            new ServerCommand{ Name = "Server.EmotesEnabled", Alias="Emotes Allowed", Enabled = true },
            new ServerCommand{ Name = "Server.AssassinationEnabled", Alias="Assassinations", Enabled = true },
            new ServerCommand{ Name = "Server.NumberofTeams", Alias="Multi-Team", Enabled = false },
            new ServerCommand{ Name = "Server.BottomlessClipEnabled", Alias="Bottomless Clip", Enabled = false },
            new ServerCommand{ Name = "Server.DualWieldEnabled", Alias="Dual Wielding Allowed", Enabled = true },
            new ServerCommand{ Name = "Server.HitMarkersEnabled", Alias="Hit Markers", Enabled = false },
            new ServerCommand{ Name = "Server.KillCommandEnabled", Alias="Kill Command", Enabled = false }
        };

        public Dictionary<ModEntry, ObservableCollection<TypeEntry>> modGametypeMapping =
            new Dictionary<ModEntry, ObservableCollection<TypeEntry>>();

        public string CacheFilePath => _cacheFile.File.FullName;
        public event PropertyChangedEventHandler PropertyChanged;

        public DelegateCommand SaveCommand { get; }

        public ObservableCollection<string> LocalModList {
            get { return _privateLocalMods; }
            set
            {
                _privateLocalMods = value;
                NotifyPropertyChanged("LocalModList");
            }
        }
        public ObservableCollection<string> LocalMapList { get; set; }
        public ObservableCollection<string> LocalGametypeList
        {
            get { return _privateLocalGametypes; }
            set
            {
                _privateLocalGametypes = value;
                NotifyPropertyChanged("CurrentModList");
            }
        }
        public ObservableCollection<ModEntry> CurrentModList
        {
            get { return _CurrentModList; }
            set
            {
                _CurrentModList = value;
                NotifyPropertyChanged("CurrentModList");
            }
        }
        public ObservableCollection<TypeEntry> CurrentGametypeList
        {
            get { return _CurrentGametypeList; }
            set
            {
                _CurrentGametypeList = value;
                NotifyPropertyChanged("CurrentGametypeList");
            }
        }
        public ObservableCollection<ServerCommand> CurrentEntryCommands
        {
            get { return _CurrentEntryCommands; }
            set
            {
                _CurrentEntryCommands = value;
                NotifyPropertyChanged("CurrentEntryCommands");
            }
        }
        public ObservableCollection<MapEntry> CurrentSpecificMaps
        {
            get { return _CurrentSpecificMaps; }
            set
            {
                _CurrentSpecificMaps = value;
                NotifyPropertyChanged("CurrentSpecificMaps");
            }
        }


        public ServerJsonEditorViewModel(IShell shell, ICacheFile cacheFile)
        {
            _shell = shell;
            _cacheFile = cacheFile;
            DisplayName = "Server Voting Editor";

            GetServerCollections();
            GetPathCollections();
            
            SaveCommand = new DelegateCommand(Save, () => LocalGametypeList.Count() > 0);
        }

            // Initial Methods

        private ObservableCollection<string> AddDirectoryNames(DirectoryInfo directory)
        {
            ObservableCollection<string> itemList = new ObservableCollection<string>();
            int countTrim = 1;
            string criterion = "";

            switch (directory.Name)
            {
                case "maps":
                    criterion = "sandbox.map";
                    break;
                case "variants":
                    criterion = "variant.";
                    break;
                case "downloads":
                    criterion = ".pak";
                    break;
            }

            foreach (var file in directory.GetFiles(".", SearchOption.AllDirectories).Where(s => s.ToString().Contains(criterion)))
            {
                string[] dirSplit = file.Directory.ToString().Split('\\');
                string name = dirSplit[dirSplit.Count() - countTrim];

                if (directory.Name == "downloads")
                    name = file.Name.Split('.')[0];

                itemList.Add(name);
            }

            return itemList;
        }
        public object ParseJsonInitial(string path)
        {
            string contents;
            try
            {
                contents = File.ReadAllText(path);
                //StreamReader sr = new StreamReader(path);
                //contents = sr.ReadToEnd();
                //sr.Close();
            }
            catch (Exception ex)
            {
                if (ex is FileNotFoundException || ex is DirectoryNotFoundException)
                    MessageBox.Show($"JSON at \"{path}\" could not be found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    MessageBox.Show($"JSON at \"{path}\" could not be parsed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            };

            var json = JSON.Parse(contents);

            return json;
        }
        private void GetServerCollections()
        {
            var serverDirectory = new DirectoryInfo(Path.Combine(_cacheFile.File.Directory.Parent.FullName, @"mods\server"));
            if (serverDirectory.Exists)
            {
                var modtest = ((JSONClass)ParseJsonInitial(serverDirectory + "\\mods.json"))["mods"];
                modsJsonDictionary = ((JSONClass)modtest).ToDictionary();
                votingDefaultMapsArray = ((JSONClass)ParseJsonInitial(serverDirectory + "\\voting.json"))["Maps"].AsArray;
                votingTypesArray = ((JSONClass)ParseJsonInitial(serverDirectory + "\\voting.json"))["Types"].AsArray;
            }
            else
                MessageBox.Show($"The directory \"{serverDirectory}\" could not be found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            modGametypeMapping = CreateModEntryCollection(modsJsonDictionary, votingTypesArray);
            VotingDefaultMaps = CreateMapEntryCollection(votingDefaultMapsArray);

            //votingMapList.CollectionChanged += Files_CollectionChanged;
        }
        private void GetPathCollections()
        {
            string fullPath = _cacheFile.File.Directory.FullName;

            // Gather Available Map Names

            var mapDirectory = new DirectoryInfo(Path.Combine(fullPath, "..\\mods\\maps"));
            if (mapDirectory.Exists)
                LocalMapList = AddDirectoryNames(mapDirectory);

            // Gather Available Variant Names

            var variantDirectory = new DirectoryInfo(Path.Combine(fullPath, "..\\mods\\variants"));
            if (variantDirectory.Exists)
                LocalGametypeList = AddDirectoryNames(variantDirectory);

            if (!variantDirectory.Exists || LocalGametypeList.Count() == 0)
                MessageBox.Show($"Couldn't find any game variants saved to your \"mods\\variants\" folder. Download or create some gametypes!",
                    "No Gametypes Found", MessageBoxButton.OK, MessageBoxImage.Error);

            // Gather Available Pak Names

            var pakDirectory = new DirectoryInfo(Path.Combine(fullPath, "..\\mods\\downloads"));
            if (pakDirectory.Exists)
                LocalModList = AddDirectoryNames(pakDirectory);
            if (!pakDirectory.Exists || LocalModList.Count() == 0)
                MessageBox.Show($"Couldn't find any mod packages in your \"mods\\downloads\" folder. Download some mods!",
                    "No Mod Packages Found", MessageBoxButton.OK, MessageBoxImage.Error);

            CurrentModList = new ObservableCollection<ModEntry>(modGametypeMapping.Keys.ToList());
            NotifyPropertyChanged("CurrentModList");
            UpdateAvailablePaks(null);
        }
        private ObservableCollection<MapEntry> CreateMapEntryCollection(JSONArray mapNodes)
        {
            ObservableCollection<MapEntry> mapCollection = new ObservableCollection<MapEntry>();

            foreach (JSONNode mapElem in mapNodes)
            {
                mapCollection.Add(new MapEntry
                {
                    DisplayName = mapElem["displayName"],
                    MapFileName = mapElem["mapName"],
                });
            }
            return mapCollection;
        }
        private Dictionary<ModEntry, ObservableCollection<TypeEntry>> CreateModEntryCollection(Dictionary<string, JSONNode> modNodes, JSONArray typeNodes)
        {
            Dictionary<ModEntry, ObservableCollection<TypeEntry>> modTypeDictionary = new Dictionary<ModEntry, ObservableCollection<TypeEntry>>();

            // create mod collection    

            foreach (KeyValuePair<string, JSONNode> N in modNodes)
            {
                modTypeDictionary.Add(new ModEntry
                {
                    FileName = N.Key.ToString().Replace("\"", ""),
                    DisplayName = N.Key.ToString().Replace("\"", ""),
                    Link = N.Value["package_url"].ToString().Replace("\"", "")
                },
                new ObservableCollection<TypeEntry>());
            }

            // map each voting entry to a mod in the collection

            foreach (JSONNode typeElem in typeNodes)
            {
                int dupeAmount = -1;
                int.TryParse(typeElem["duplicateAmount"].Value.ToString(), out dupeAmount);

                for (int i = 0; i < modTypeDictionary.Count; i++)
                {
                    ModEntry mod = null;
                    string modKeyName = (modTypeDictionary.ElementAt(i).Key).FileName;
                    string entryModName = typeElem["modPack"].ToString().Replace("\"", "");

                    if (entryModName == modKeyName)
                    {
                        mod = modTypeDictionary.ElementAt(i).Key;

                        if (mod.DisplayName != typeElem["modDisplayName"].ToString())
                            modTypeDictionary.ElementAt(i).Key.DisplayName = typeElem["modDisplayName"].ToString().Replace("\"", "");

                        modTypeDictionary.ElementAt(i).Value.Add(new TypeEntry
                        {
                            TypeName = typeElem["typeName"].ToString().Replace("\"", ""),
                            TypeDisplayName = typeElem["displayName"].ToString().Replace("\"", ""),
                            DuplicateAmount = (dupeAmount == 0) ? 1 : dupeAmount,
                            ModPackage = mod,
                            Commands = GetCommands(typeElem["commands"]),
                            SpecificMaps = CreateMapEntryCollection(typeElem["SpecificMaps"].AsArray),
                            CharacterOverrides = GetCharacterOverrides(typeElem["characterOverrides"].AsArray)
                        });
                    }
                }
            }

            return modTypeDictionary;
        }

        public ObservableCollection<ServerCommand> GetCommands(JSONNode node)
        {
            string[] cmdList = node.ToString().Trim(new char[] { '{', '}', '[', ']' }).Split(',');
            cmdList = cmdList.Select(x => x.Replace("\"", "").Trim()).ToArray();

            var commandList = CommandDefaults;

            if (cmdList.Count() > 0)
            {
                foreach (string cmd in cmdList)
                {
                    string[] pair = cmd.Split(' ');
                    bool enabled = false;

                    if (pair[1] != "0")
                        enabled = true;

                    foreach (ServerCommand command in commandList)
                        if (command.Name == pair[0])
                            command.Enabled = enabled;
                }
            }
            return commandList;
        }

        public ObservableCollection<CharacterOverride> GetCharacterOverrides(JSONArray overrideNodes)
        {
            ObservableCollection<CharacterOverride> overrides = new ObservableCollection<CharacterOverride>() { };

            if (overrideNodes != null)
			{
                foreach (JSONNode node in overrideNodes)
                {
                    var test = node.ToString();

                    overrides.Add(new CharacterOverride
                    {
                        Team = node.ToString(),
                        Character = node[node.ToString()]
                    });
                }
            }

            return overrides;
        }

        // Modification Methods

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public void UpdateAvailablePaks(string pakFileName)
        {
            if (pakFileName == null)
            {
                foreach (ModEntry key in modGametypeMapping.Keys)
                {
                    if (LocalModList.IndexOf(key.FileName) != -1)
                    {
                        int x = LocalModList.IndexOf(key.FileName);
                        LocalModList.RemoveAt(x);
                    }
                }
            }
            else
            {
                //int i = LocalModList.IndexOf(pakFileName);
                if (LocalModList.Contains(pakFileName))
                    LocalModList.Remove(pakFileName);
                else
                    LocalModList.Add(pakFileName);

                //if (i == -1)
                //    LocalModList.Add(pakFileName);
                //else
                //    LocalModList.RemoveAt(i);

                NotifyPropertyChanged("LocalModList");
            }
        }

            // Addition/Removal Methods

        public void AddMod(string pakFileName)
        {
            ModEntry newEntry = new ModEntry()
            {
                FileName = pakFileName,
                DisplayName = pakFileName,
                Link = ""
            };

            modGametypeMapping.Add(newEntry, new ObservableCollection<TypeEntry>());
            CurrentModList.Add(newEntry);

            NotifyPropertyChanged("CurrentModList");
            UpdateAvailablePaks(pakFileName);
        }
        public void RemoveMod(ModEntry modToRemove)
        {
            modGametypeMapping.Remove(modToRemove);
            CurrentModList.Remove(modToRemove);

            NotifyPropertyChanged("CurrentModList");
            UpdateAvailablePaks(modToRemove.FileName);
        }

        public void AddGametype(string gameVariantName)
        {
            TypeEntry newEntry = new TypeEntry()
            {
                TypeName = gameVariantName,
                DuplicateAmount = 1,
                Commands = new ObservableCollection<ServerCommand> { }
            };

            //gametypeMapping.Add(newEntry, new ObservableCollection<TypeEntry>());
            CurrentGametypeList.Add(newEntry);
            NotifyPropertyChanged("CurrentGametypeList");
        }
        public void RemoveGametype(TypeEntry typeToRemove)
        {
            CurrentGametypeList.Remove(typeToRemove);
            NotifyPropertyChanged("CurrentGametypeList");
        }

        public void AddMap(string mapNameToAdd)
		{
            MapEntry newEntry = new MapEntry()
            {
                MapFileName = mapNameToAdd,
                DisplayName = mapNameToAdd
            };

            CurrentSpecificMaps.Add(newEntry);
            NotifyPropertyChanged("CurrentSpecificMaps");
		}

            // Misc

        private void Files_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SaveCommand.RaiseCanExecuteChanged();
        }
        private async Task CreateBackupAsync(string outputPath)
        {
            var zipArchive = new ZipArchive(File.Create(outputPath), ZipArchiveMode.Create);
            var zipEntries = new HashSet<string>();

            foreach (var filePath in LocalMapList)
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
        private void WriteLog(string output)
        {
            Application.Current.Dispatcher.Invoke(() => Output += $"{output}\n");
        }
        public string Output
        {
            get => _output;
            set => SetAndNotify(ref _output, value);
        }

        public void ReloadAll()
		{
            GetServerCollections();
            GetPathCollections();
        }

        internal async void Save()
        {

        }
    }
}
