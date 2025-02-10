using System;

namespace Epsilon
{
	public class SettingDefinition
    {

        public string CollectionKey { get; private set; }
		public string Key { get; }
        public string DefaultValue { get; }

		private SettingsCollection _collection;
		private SettingsCollection Collection { get { return _collection; } }

		public SettingDefinition(string collectionKey, string key, string defaultValue)
        {
            if (string.IsNullOrWhiteSpace(collectionKey)) { throw new System.ArgumentException(nameof(collectionKey)); }
			CollectionKey = collectionKey;
			Key = key;
            DefaultValue = defaultValue;
			_collection = (SettingsCollection)SettingsService.GetCollection(CollectionKey);
		}

		#region Helpers

		public bool SettingHasValue { get { return Collection.SettingHasValue(Key); } }

		#endregion

		#region String Set / Get

		private void Set(string value) { Collection.Set(Key, value); }

		public string Get() { return Collection.Get(Key, DefaultValue); }

		public string Get(string defaultValue) { return Collection.Get(Key, defaultValue); }

		public bool TryGet(out string value, string defaultValue = null) {
			if (SettingHasValue) { value = Get(); return true; }
			else { value = defaultValue; return false; }
		}

		public bool TryGet(out string value) { return TryGet(out value, DefaultValue); }

		#endregion

		#region Bool Set / Get


		public void SetBool(bool value) { Set(value.ToString()); }
		public bool GetBool(bool defaultValue = false) {
			try { return bool.Parse(Get(defaultValue.ToString())); }
			catch { return defaultValue; }
		}

		public bool GetBool() { return GetBool(bool.Parse(DefaultValue)); }

		public bool TryGetBool(out bool value, bool defaultValue = false) {
			if (SettingHasValue) { value = GetBool(defaultValue); return true; }
			else { value = defaultValue; return false; }
		}

		public bool TryGetBool(out bool value) { return TryGetBool(out value, bool.Parse(DefaultValue)); }

		#endregion

		#region Int Set / Get

		public void SetInt(int value) { Set(value.ToString()); }

		public int GetInt(int defaultValue = 0) {
			try { return int.Parse(Get(defaultValue.ToString())); }
			catch { return defaultValue; }
		}

		public int GetInt() { return GetInt(int.Parse(DefaultValue)); }

		public bool TryGetInt(out int value, int defaultValue = 0) {
			if (SettingHasValue) { value = GetInt(defaultValue); return true; }
			else { value = defaultValue; return false; }
		}

		public bool TryGetInt(out int value) { return TryGetInt(out value, int.Parse(DefaultValue)); }

		#endregion

		#region Double Set / Get

		public void SetDouble(double value) { Set(value.ToString()); }

		public double GetDouble(double defaultValue = 0) {
			try { return double.Parse(Get(defaultValue.ToString())); }
			catch { return defaultValue; }
		}

		public double GetDouble() { return GetDouble(int.Parse(DefaultValue)); }

		public bool TryGetDouble(out double value, double defaultValue = 0) {
			if (SettingHasValue) { value = GetDouble(defaultValue); return true; }
			else { value = defaultValue; return false; }
		}


		#endregion

		#region Enum Set / Get

		public void SetEnum<T>(T value) where T : Enum { Set(Convert.ToInt32(value).ToString()); }

		public T GetEnum<T>() where T : Enum { return GetEnum((T)Enum.ToObject(typeof(T), int.Parse(DefaultValue))); }

		public T GetEnum<T>(T defaultValue = default) where T : Enum {
			try {
				int storedValue = GetInt(Convert.ToInt32(defaultValue));
				return (T)Enum.ToObject(typeof(T), storedValue);
			}
			catch { return defaultValue; }
		}

		public bool TryGetEnum<T>(out T value, T defaultValue = default) where T : Enum {
			if (SettingHasValue) { value = GetEnum(defaultValue); return true; }
			else { value = defaultValue; return false; }
		}

		public bool TryGetEnum<T>(out T value) where T : Enum { T defaultT = default; return TryGetEnum(out value, defaultT); }

		#endregion


	}
}
