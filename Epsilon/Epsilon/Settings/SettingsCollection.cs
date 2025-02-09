using Newtonsoft.Json.Linq;
using System;

namespace Epsilon
{

	/// <summary>
	/// Represents a collection of settings.<br/>
	/// This is a node that contains settings for a specific collection.
	/// </summary>
	public class SettingsCollection : ISettingsCollection
    {

		#region Members

		/// <summary>
		/// The <see cref="JObject"/> that represents the settings collection.<br/>
		/// This is the node that contains the settings for this collection.
		/// </summary>
		public JObject Node => (JObject)_node;
		
		/// <summary>
		/// The <see cref="JToken"/> that represents the settings collection.<br/>
		/// This is the node that contains the settings for this collection.
		/// </summary>
		private JToken _node;

				
		public event EventHandler<SettingChangedEventArgs> SettingChanged;

		#endregion

		#region Constructors

		/// <summary>
		/// Creates a new <see cref="SettingsCollection"/> with the given <paramref name="service"/> and <paramref name="node"/>.
		/// </summary>
		/// <param name="service">The <see cref="SettingsService"/> that this collection belongs to.</param>
		/// <param name="node">The <see cref="JToken"/> that represents the settings collection.</param>
		public SettingsCollection(JToken node) { _node = node; }

		/// <summary>
		/// Returns a <see cref="SettingsCollection"/> associated with the given <paramref name="key"/>.
		/// </summary>
		/// <param name="key">The key of the collection to get.</param>
		/// <returns>A <see cref="SettingsCollection"/> associated with the given <paramref name="key"/>.</returns>
		/// <exception cref="ArgumentException">Thrown if <paramref name="key"/> is <see langword="null"> or empty.</exception>
		public SettingsCollection GetCollection(string key)
        {
			if (string.IsNullOrEmpty(key)) { throw new ArgumentException("Key cannot be null or empty", nameof(key)); }
			JToken node = _node[key] ?? (_node[key] = new JObject());
            return new SettingsCollection(node);
        }

		#endregion

		#region Helpers
		
		public bool SettingHasValue(string key) { return string.IsNullOrEmpty(key) ? false : _node[key] != null; }
		
		public bool SettingHasValue(SettingDefinition settingDefinition) {
			if (settingDefinition == null) { throw new ArgumentNullException($"{nameof(SettingDefinition)} cannot be null", nameof(settingDefinition)); }
			if (string.IsNullOrEmpty(settingDefinition.Key)) { throw new ArgumentException($"Invalid {nameof(SettingDefinition)}. Null or empty 'Key'.", nameof(settingDefinition)); }
			return SettingHasValue(settingDefinition.Key); 
		}

		#endregion

		#region String Set / Get
		
		public void Set(string key, string value)
        {
            if (string.IsNullOrEmpty(key)) { throw new ArgumentException("Key cannot be null or empty", nameof(key)); }
			_node[key] = JToken.FromObject(value, SettingsService.Serializer);
            SettingChanged?.Invoke(this, new SettingChangedEventArgs(this, key));
			SettingsService.NotifySettingChanged(this, key);
        }
		
		public string Get(string key, string defaultValue = null) { 
			if (string.IsNullOrEmpty(key)) { return defaultValue; }
			else { 
				try { return _node[key]?.ToString() ?? defaultValue; }
				catch { return defaultValue; }
			}
		}
		
		public string Get(SettingDefinition settingDefinition) { 
			if (settingDefinition == null) { throw new ArgumentNullException($"{nameof(SettingDefinition)} cannot be null", nameof(settingDefinition)); }
			if (string.IsNullOrEmpty(settingDefinition.Key)) { throw new ArgumentException($"Invalid {nameof(SettingDefinition)}. Null or empty 'Key'.", nameof(settingDefinition)); }
			return Get(settingDefinition.Key, settingDefinition.DefaultValue); 
		}
		
		public bool TryGet(string key, out string value, string defaultValue = null) {
			if (SettingHasValue(key)) { value = Get(key, defaultValue); return true; }
			else { value = defaultValue; return false; }
		}
		
		public bool TryGet(SettingDefinition settingDefinition, out string value) {
			if (settingDefinition == null) { throw new ArgumentNullException($"{nameof(SettingDefinition)} cannot be null", nameof(settingDefinition)); }
			if (string.IsNullOrEmpty(settingDefinition.Key)) { throw new ArgumentException($"Invalid {nameof(SettingDefinition)}. Null or empty 'Key'.", nameof(settingDefinition)); }
			return TryGet(settingDefinition.Key, out value, settingDefinition.DefaultValue); 
		}
		
		#endregion

		#region Bool Set / Get
		
		public void SetBool(string key, bool value) { Set(key, value.ToString()); }
		
		public bool GetBool(string key, bool defaultValue = false) { 
			try { return bool.Parse(Get(key, defaultValue.ToString())); }
			catch { return defaultValue; }
		}
		
		public bool GetBool(SettingDefinition settingDefinition) {
			if (settingDefinition == null) { throw new ArgumentNullException($"{nameof(SettingDefinition)} cannot be null", nameof(settingDefinition)); }
			if (string.IsNullOrEmpty(settingDefinition.Key)) { throw new ArgumentException($"Invalid {nameof(SettingDefinition)}. Null or empty 'Key'.", nameof(settingDefinition)); }
			if (!bool.TryParse(settingDefinition.DefaultValue, out bool defaultValue)) { defaultValue = false; }
			return GetBool(settingDefinition.Key, defaultValue);
		}

		public bool TryGetBool(string key, out bool value, bool defaultValue = false) {
			if (SettingHasValue(key)) { value = GetBool(key, defaultValue); return true; }
			else { value = defaultValue; return false; }
		}
		
		public bool TryGetBool(SettingDefinition settingDefinition, out bool value) {
			if (settingDefinition == null) { throw new ArgumentNullException($"{nameof(SettingDefinition)} cannot be null", nameof(settingDefinition)); }
			if (string.IsNullOrEmpty(settingDefinition.Key)) { throw new ArgumentException($"Invalid {nameof(SettingDefinition)}. Null or empty 'Key'.", nameof(settingDefinition)); }
			if (!bool.TryParse(settingDefinition.DefaultValue, out bool defaultValue)) { defaultValue = false; }
			return TryGetBool(settingDefinition.Key, out value, defaultValue);
		}
		
		#endregion

		#region Int Set / Get
		
		public void SetInt(string key, int value) { Set(key, value.ToString()); }
		
		public int GetInt(string key, int defaultValue = 0) {
			try { return int.Parse(Get(key, defaultValue.ToString())); }
			catch { return defaultValue; }
		}
		
		public int GetInt(SettingDefinition settingDefinition) {
			if (settingDefinition == null) { throw new ArgumentNullException($"{nameof(SettingDefinition)} cannot be null", nameof(settingDefinition)); }
			if (string.IsNullOrEmpty(settingDefinition.Key)) { throw new ArgumentException($"Invalid {nameof(SettingDefinition)}. Null or empty 'Key'.", nameof(settingDefinition)); }
			if (!int.TryParse(settingDefinition.DefaultValue, out int defaultValue)) { defaultValue = 0; }
			return GetInt(settingDefinition.Key, defaultValue);
		}

		public bool TryGetInt(string key, out int value, int defaultValue = 0) {
			if (SettingHasValue(key)) { value = GetInt(key, defaultValue); return true; }
			else { value = defaultValue; return false; }
		}
		
		public bool TryGetInt(SettingDefinition settingDefinition, out int value) {
			if (settingDefinition == null) { throw new ArgumentNullException($"{nameof(SettingDefinition)} cannot be null", nameof(settingDefinition)); }
			if (string.IsNullOrEmpty(settingDefinition.Key)) { throw new ArgumentException($"Invalid {nameof(SettingDefinition)}. Null or empty 'Key'.", nameof(settingDefinition)); }
			if (!int.TryParse(settingDefinition.DefaultValue, out int defaultValue)) { defaultValue = 0; }
			return TryGetInt(settingDefinition.Key, out value, defaultValue);
		}
		
		#endregion

		#region Double Set / Get
		
		public void SetDouble(string key, double value) { Set(key, value.ToString()); }
		
		public double GetDouble(string key, double defaultValue = 0) {
			try { return double.Parse(Get(key, defaultValue.ToString())); }
			catch { return defaultValue; }
		}
		
		public double GetDouble(SettingDefinition settingDefinition) {
			if (settingDefinition == null) { throw new ArgumentNullException($"{nameof(SettingDefinition)} cannot be null", nameof(settingDefinition)); }
			if (string.IsNullOrEmpty(settingDefinition.Key)) { throw new ArgumentException($"Invalid {nameof(SettingDefinition)}. Null or empty 'Key'.", nameof(settingDefinition)); }
			if (!double.TryParse(settingDefinition.DefaultValue, out double defaultValue)) { defaultValue = 0; }
			return GetDouble(settingDefinition.Key, defaultValue);
		}

		public bool TryGetDouble(string key, out double value, double defaultValue = 0) {
			if (SettingHasValue(key)) { value = GetDouble(key, defaultValue); return true; }
			else { value = defaultValue; return false; }
		}
		
		public bool TryGetDouble(SettingDefinition settingDefinition, out double value) {
			if (settingDefinition == null) { throw new ArgumentNullException($"{nameof(SettingDefinition)} cannot be null", nameof(settingDefinition)); }
			if (string.IsNullOrEmpty(settingDefinition.Key)) { throw new ArgumentException($"Invalid {nameof(SettingDefinition)}. Null or empty 'Key'.", nameof(settingDefinition)); }
			if (!double.TryParse(settingDefinition.DefaultValue, out double defaultValue)) { defaultValue = 0; }
			return TryGetDouble(settingDefinition.Key, out value, defaultValue);
		}
		
		#endregion

		#region Enum Set / Get
		
		public void SetEnum<T>(string key, T value) where T : Enum { Set(key, Convert.ToInt32(value).ToString()); }
		
		public T GetEnum<T>(string key, T defaultValue = default) where T : Enum {
			try {
				int storedValue = GetInt(key, Convert.ToInt32(defaultValue));
				return (T)Enum.ToObject(typeof(T), storedValue); 
			}
			catch { return defaultValue; }
		}
		
		public T GetEnum<T>(SettingDefinition settingDefinition) where T : Enum {
			if (settingDefinition == null) { throw new ArgumentNullException($"{nameof(SettingDefinition)} cannot be null", nameof(settingDefinition)); }
			if (string.IsNullOrEmpty(settingDefinition.Key)) { throw new ArgumentException($"Invalid {nameof(SettingDefinition)}. Null or empty 'Key'.", nameof(settingDefinition)); }
			if (!int.TryParse(settingDefinition.DefaultValue, out int defaultValue)) {
				try { defaultValue = Convert.ToInt32(default(T)); }
				catch { defaultValue = 0; }
			}
			try { return GetEnum<T>(settingDefinition.Key, (T)Enum.ToObject(typeof(T), defaultValue)); }
			catch { return default; }
		}

		public bool TryGetEnum<T>(string key, out T value, T defaultValue = default) where T : Enum {
			if (SettingHasValue(key)) { value = GetEnum<T>(key, defaultValue); return true; }
			else { value = defaultValue; return false; }
		}
		
		public bool TryGetEnum<T>(SettingDefinition settingDefinition, out T value) where T : Enum { T defaultT = default;
			if (settingDefinition == null) { throw new ArgumentNullException($"{nameof(SettingDefinition)} cannot be null", nameof(settingDefinition)); }
			if (string.IsNullOrEmpty(settingDefinition.Key)) { throw new ArgumentException($"Invalid {nameof(SettingDefinition)}. Null or empty 'Key'.", nameof(settingDefinition)); }
			if (!int.TryParse(settingDefinition.DefaultValue, out int defaultValue)) {
				try { defaultValue = Convert.ToInt32(defaultT); }
				catch { defaultValue = 0; }
			}
			try { defaultT = (T)Enum.ToObject(typeof(T), defaultValue); } catch { defaultT = default; }
			return TryGetEnum(settingDefinition.Key, out value, defaultT);
		}

		#endregion

	}
}
