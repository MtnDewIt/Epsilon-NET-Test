using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.Composition;
using System.IO;

namespace EpsilonLib.Settings
{
    [Export(typeof(ISettingsService))]
    public class SettingsService : ISettingsService
    {
        
        private static SettingsService _instance;
        public static SettingsService Instance => _instance ?? ( _instance = new SettingsService() );
        public event EventHandler<SettingChangedEventArgs> SettingChanged;

		private const string FilePath = "settings.json";
        private const string SettingsVersionKey = "SettingsVersion";
		private static SettingsCollection _rootCollection;

        public SettingsService() { }

        static SettingsService()
        {
			Serializer.Formatting = Formatting.Indented;
			Serializer.Converters.Add(new StringEnumConverter());
			_rootCollection = new SettingsCollection(new JObject());
            Load(FilePath);
        }

		public readonly static JsonSerializer Serializer = JsonSerializer.CreateDefault();

        public static ISettingsCollection GetCollection(params string[] keys) {
            if (keys == null || keys.Length == 0 || string.IsNullOrEmpty(keys[0])) { return null; }
			return _rootCollection.GetCollection(keys[0]);
		}
		public ISettingsCollection GetCollection(string key)
        {
            return _rootCollection.GetCollection(key);
        }

        private static void Load(string filePath)
        {
            if (!File.Exists(filePath))
                return;

			using (JsonReader reader = new JsonTextReader(File.OpenText(filePath)))
            {
                reader.Read();
                _rootCollection = new SettingsCollection(JObject.ReadFrom(reader));
            }

			// 12/05/24 Breaking change to how settings are stored
			// If the Version key is not present we need to wipe the existing settings
			if (_rootCollection.Node[SettingsVersionKey] == null) {
				_rootCollection.Node.RemoveAll();               // Clear all existing data
				_rootCollection.Node[SettingsVersionKey] = 2;   // Record the update to V2
				Save(filePath);                                 // Save the changes
			}

		}

        private static void Save(string filePath)
        {
            using (JsonWriter writer = new JsonTextWriter(File.CreateText(filePath)))
            {
                writer.Formatting = Formatting.Indented;
                _rootCollection.Node.WriteTo(writer);
            }
               
        }

        internal static void NotifySettingChanged(SettingsCollection collection, string key)
        {
            Save(FilePath);
            Instance.SettingChanged?.Invoke(collection, new SettingChangedEventArgs(collection, key));
        }
    }
}
