using System;

namespace Epsilon
{

	/// <summary>
	/// Represents a collection of read-only settings.<br/>
	/// This interface provides methods to get the value of settings.
	/// </summary>
	public interface IReadOnlySettingsCollection
    {

		/// <summary>
		/// Event raised when a setting is changed.<br/>
		/// The <see cref="SettingChangedEventArgs"/> will contain the <see cref="SettingsCollection"/> that the setting belongs to, and the <see cref="SettingDefinition.Key"/> of the setting that was changed.
		/// </summary>
		event EventHandler<SettingChangedEventArgs> SettingChanged;

		/// <summary>
		/// Returns <see langword="true"/> if the setting has a value, <see langword="false"/> otherwise.<br/>
		/// Returns <see langword="false"/> if <paramref name="key"/> is <see langword="null"/> or empty.
		/// </summary>
		/// <param name="key">The key of the setting to check.</param>
		/// <returns><see langword="true"/> if the setting exists and has a value, <see langword="false"/> otherwise.</returns>
		bool SettingHasValue(string key);

		/// <summary>
		/// Returns <see langword="true"/> if the setting exists and has a value, <see langword="false"/> otherwise.<br/>
		/// Returns <see langword="false"/> if the <paramref name="settingDefinition"/> <see cref="SettingDefinition.Key"/> value is <see langword="null"/> or empty.
		/// </summary>
		/// <param name="settingDefinition">The setting to check.</param>
		/// <returns><see langword="true"/> if the setting has a value, <see langword="false"/> otherwise.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="settingDefinition"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if <see cref="SettingDefinition.Key"/> is <see langword="null"/> or empty.</exception>
		bool SettingHasValue(SettingDefinition settingDefinition);


		/// <summary>
		/// Returns the value of the setting associated with the given <paramref name="key"/>.<br/>
		/// If the setting does not exist, or <paramref name="key"/> is <see langword="null"/> or empty, <paramref name="defaultValue"/> will be returned.<br/>
		/// </summary>
		/// <param name="key">The key of the setting to get.</param>
		/// <param name="defaultValue">The value to return if the setting does not exist.</param>
		/// <returns>The value of the setting associated with the given <paramref name="key"/>.</returns>
		string Get(string key, string defaultValue = null);
		/// <summary>
		/// Returns the value of the setting associated with the given <paramref name="settingDefinition"/>.<br/>
		/// If the setting does not exist, <see cref="SettingDefinition.DefaultValue"/> will be returned.<br/>
		/// If the <paramref name="settingDefinition"/> <see cref="SettingDefinition.Key"/> value is <see langword="null"/> or empty, <see cref="SettingDefinition.DefaultValue"/> will be returned.
		/// </summary>
		/// <param name="settingDefinition">The setting to get.</param>
		/// <returns>The value of the setting associated with the given <paramref name="settingDefinition"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="settingDefinition"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if <see cref="SettingDefinition.Key"/> is <see langword="null"/> or empty.</exception>
		string Get(SettingDefinition settingDefinition);
		/// <summary>
		/// Returns <see langword="true"/> if the setting's value was retrieved, <see langword="false"/> otherwise.<br/>
		/// If the setting does not exist, <paramref name="defaultValue"/> will be returned.<br/>
		/// If <paramref name="key"/> is <see langword="null"/> or empty, <paramref name="defaultValue"/> will be returned.
		/// </summary>
		/// <param name="key">The key of the setting to get.</param>
		/// <param name="value">The value of the setting associated with the given <paramref name="key"/>.</param>
		/// <param name="defaultValue">The value to return if the setting does not exist.</param>
		/// <returns> <see langword="true"/> if the setting's value was retrieved, <see langword="false"/> otherwise.</returns>
		bool TryGet(string key, out string value, string defaultValue = null);
		/// <summary>
		/// Returns <see langword="true"/> if the setting's value was retrieved, <see langword="false"/> otherwise.<br/>
		/// If the setting does not exist, <see cref="SettingDefinition.DefaultValue"/> will be returned.<br/>
		/// If the <paramref name="settingDefinition"/> <see cref="SettingDefinition.Key"/> value is <see langword="null"/> or empty, <see cref="SettingDefinition.DefaultValue"/> will be returned.
		/// </summary>
		/// <param name="settingDefinition">The setting to get.</param>
		/// <param name="value">The value of the setting associated with the given <paramref name="settingDefinition"/>.</param>
		/// <returns> <see langword="true"/> if the setting's value was retrieved, <see langword="false"/> otherwise.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="settingDefinition"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if <see cref="SettingDefinition.Key"/> is <see langword="null"/> or empty.</exception>
		bool TryGet(SettingDefinition settingDefinition, out string value);


		/// <summary>
		/// Returns the <see langword="bool"/> value of the setting associated with the given <paramref name="key"/>.<br/>
		/// If the setting does not exist, or <paramref name="key"/> is <see langword="null"/> or empty, <paramref name="defaultValue"/> will be returned.
		/// </summary>
		/// <param name="key">The key of the setting to get.</param>
		/// <param name="defaultValue">The value to return if the setting does not exist.</param>
		/// <returns>The value of the setting associated with the given <paramref name="key"/>.</returns>
		bool GetBool(string key, bool defaultValue = false);
		/// <summary>
		/// Returns the <see langword="bool"/> value of the setting associated with the given <paramref name="settingDefinition"/>.<br/>
		/// If the setting does not exist, <see cref="SettingDefinition.DefaultValue"/> will be returned.<br/>
		/// If the <paramref name="settingDefinition"/> <see cref="SettingDefinition.Key"/> value is <see langword="null"/> or empty, <see cref="SettingDefinition.DefaultValue"/> will be returned.
		/// </summary>
		/// <param name="settingDefinition">The setting to get.</param>
		/// <returns>The value of the setting associated with the given <paramref name="settingDefinition"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="settingDefinition"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if <see cref="SettingDefinition.Key"/> is <see langword="null"/> or empty.</exception>
		bool GetBool(SettingDefinition settingDefinition);
		/// <summary>
		/// Returns <see langword="true"/> if the setting's value was retrieved, <see langword="false"/> otherwise.<br/>
		/// If the setting does not exist, or if <paramref name="key"/> is <see langword="null"/> or empty, <paramref name="defaultValue"/> will be returned.
		/// </summary>
		/// <param name="key">The key of the setting to get.</param>
		/// <param name="value">The value of the setting associated with the given <paramref name="key"/>.</param>
		/// <param name="defaultValue">The value to return if the setting does not exist.</param>
		/// <returns> <see langword="true"/> if the setting's value was retrieved, <see langword="false"/> otherwise.</returns>
		bool TryGetBool(string key, out bool value, bool defaultValue = false);
		/// <summary>
		/// Returns <see langword="true"/> if the setting's value was retrieved, <see langword="false"/> otherwise.<br/>
		/// If the setting does not exist, <see cref="SettingDefinition.DefaultValue"/> will be returned.<br/>
		/// If the <paramref name="settingDefinition"/> <see cref="SettingDefinition.Key"/> value is <see langword="null"/> or empty, <see cref="SettingDefinition.DefaultValue"/> will be returned.
		/// </summary>
		/// <param name="settingDefinition">The setting to get.</param>
		/// <param name="value">The value of the setting associated with the given <paramref name="settingDefinition"/>.</param>
		/// <returns> <see langword="true"/> if the setting's value was retrieved, <see langword="false"/> otherwise.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="settingDefinition"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if <see cref="SettingDefinition.Key"/> is <see langword="null"/> or empty.</exception>
		bool TryGetBool(SettingDefinition settingDefinition, out bool value);


		/// <summary>
		/// Returns the <see langword="int"/> value of the setting associated with the given <paramref name="key"/>.<br/>
		/// If the setting does not exist, or <paramref name="key"/> is <see langword="null"/> or empty, <paramref name="defaultValue"/> will be returned.
		/// </summary>
		/// <param name="key">The key of the setting to get.</param>
		/// <param name="defaultValue">The value to return if the setting does not exist, 0 by default.</param>
		/// <returns>The value of the setting associated with the given <paramref name="key"/>.</returns>
		int GetInt(string key, int defaultValue = 0);
		/// <summary>
		/// Returns the <see langword="int"/> value of the setting associated with the given <paramref name="settingDefinition"/>.<br/>
		/// If the setting does not exist, <see cref="SettingDefinition.DefaultValue"/> will be returned.<br/>
		/// If the <paramref name="settingDefinition"/> <see cref="SettingDefinition.Key"/> value is <see langword="null"/> or empty, <see cref="SettingDefinition.DefaultValue"/> will be returned.
		/// </summary>
		/// <param name="settingDefinition">The setting to get.</param>
		/// <returns>The value of the setting associated with the given <paramref name="settingDefinition"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="settingDefinition"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if <see cref="SettingDefinition.Key"/> is <see langword="null"/> or empty.</exception>
		int GetInt(SettingDefinition settingDefinition);
		/// <summary>
		/// Returns <see langword="true"/> if the setting's value was retrieved, <see langword="false"/> otherwise.<br/>
		/// If the setting does not exist, or if <paramref name="key"/> is <see langword="null"/> or empty, <paramref name="defaultValue"/> will be returned.
		/// </summary>
		/// <param name="key">The key of the setting to get.</param>
		/// <param name="value">The value of the setting associated with the given <paramref name="key"/>.</param>
		/// <param name="defaultValue">The value to return if the setting does not exist.</param>
		/// <returns> <see langword="true"/> if the setting's value was retrieved, <see langword="false"/> otherwise.</returns>
		bool TryGetInt(string key, out int value, int defaultValue = 0);
		/// <summary>
		/// Returns <see langword="true"/> if the setting's value was retrieved, <see langword="false"/> otherwise.<br/>
		/// If the setting does not exist, <see cref="SettingDefinition.DefaultValue"/> will be returned.<br/>
		/// If the <paramref name="settingDefinition"/> <see cref="SettingDefinition.Key"/> value is <see langword="null"/> or empty, <see cref="SettingDefinition.DefaultValue"/> will be returned.
		/// </summary>
		/// <param name="settingDefinition">The setting to get.</param>
		/// <param name="value">The value of the setting associated with the given <paramref name="settingDefinition"/>.</param>
		/// <returns> <see langword="true"/> if the setting's value was retrieved, <see langword="false"/> otherwise.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="settingDefinition"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if <see cref="SettingDefinition.Key"/> is <see langword="null"/> or empty.</exception>
		bool TryGetInt(SettingDefinition settingDefinition, out int value);


		/// <summary>
		/// Returns the <see langword="double"/> value of the setting associated with the given <paramref name="key"/>.<br/>
		/// If the setting does not exist, or <paramref name="key"/> is <see langword="null"/> or empty, <paramref name="defaultValue"/> will be returned.
		/// </summary>
		/// <param name="key">The key of the setting to get.</param>
		/// <param name="defaultValue">The value to return if the setting does not exist, 0 by default.</param>
		/// <returns>The value of the setting associated with the given <paramref name="key"/>.</returns>
		double GetDouble(string key, double defaultValue = 0);
		/// <summary>
		/// Returns the <see langword="double"/> value of the setting associated with the given <paramref name="settingDefinition"/>.<br/>
		/// If the setting does not exist, <see cref="SettingDefinition.DefaultValue"/> will be returned.<br/>
		/// If the <paramref name="settingDefinition"/> <see cref="SettingDefinition.Key"/> value is <see langword="null"/> or empty, <see cref="SettingDefinition.DefaultValue"/> will be returned.
		/// </summary>
		/// <param name="settingDefinition">The setting to get.</param>
		/// <returns>The value of the setting associated with the given <paramref name="settingDefinition"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="settingDefinition"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if <see cref="SettingDefinition.Key"/> is <see langword="null"/> or empty.</exception>
		double GetDouble(SettingDefinition settingDefinition);
		/// <summary>
		/// Returns <see langword="true"/> if the setting's value was retrieved, <see langword="false"/> otherwise.<br/>
		/// If the setting does not exist, or if <paramref name="key"/> is <see langword="null"/> or empty, <paramref name="defaultValue"/> will be returned.
		/// </summary>
		/// <param name="key">The key of the setting to get.</param>
		/// <param name="value">The value of the setting associated with the given <paramref name="key"/>.</param>
		/// <param name="defaultValue">The value to return if the setting does not exist.</param>
		/// <returns> <see langword="true"/> if the setting's value was retrieved, <see langword="false"/> otherwise.</returns>
		bool TryGetDouble(string key, out double value, double defaultValue = 0);
		/// <summary>
		/// Returns <see langword="true"/> if the setting's value was retrieved, <see langword="false"/> otherwise.<br/>
		/// If the setting does not exist, <see cref="SettingDefinition.DefaultValue"/> will be returned.<br/>
		/// If the <paramref name="settingDefinition"/> <see cref="SettingDefinition.Key"/> value is <see langword="null"/> or empty, <see cref="SettingDefinition.DefaultValue"/> will be returned.
		/// </summary>
		/// <param name="settingDefinition">The setting to get.</param>
		/// <param name="value">The value of the setting associated with the given <paramref name="settingDefinition"/>.</param>
		/// <returns> <see langword="true"/> if the setting's value was retrieved, <see langword="false"/> otherwise.</returns>
		///	<exception cref="ArgumentNullException">Thrown if <paramref name="settingDefinition"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if <see cref="SettingDefinition.Key"/> is <see langword="null"/> or empty.</exception>
		bool TryGetDouble(SettingDefinition settingDefinition, out double value);

		/// <summary>
		/// Returns the <see cref="Enum"/> value of the setting associated with the given <paramref name="key"/>.<br/>
		/// If the setting does not exist, or <paramref name="key"/> is <see langword="null"/> or empty, <paramref name="defaultValue"/> will be returned.
		/// </summary>
		/// <typeparam name="T">The type of the <see cref="Enum"/> to get.</typeparam>
		/// <param name="key">The key of the setting to get.</param>
		/// <param name="defaultValue">The value to return if the setting does not exist.</param>
		/// <returns>The value of the setting associated with the given <paramref name="key"/>.</returns>
		T GetEnum<T>(string key, T defaultValue = default) where T : Enum;
		/// <summary>
		/// Returns the <see cref="Enum"/> value of the setting associated with the given <paramref name="settingDefinition"/>.<br/>
		/// If the setting does not exist, <see cref="SettingDefinition.DefaultValue"/> will be returned.<br/>
		/// If the <paramref name="settingDefinition"/> <see cref="SettingDefinition.Key"/> value is <see langword="null"/> or empty, <see cref="SettingDefinition.DefaultValue"/> will be returned.
		/// </summary>
		/// <typeparam name="T">The type of the <see cref="Enum"/> to get.</typeparam>
		/// <param name="settingDefinition">The setting to get.</param>
		/// <returns>The value of the setting associated with the given <paramref name="settingDefinition"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="settingDefinition"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if <see cref="SettingDefinition.Key"/> is <see langword="null"/> or empty.</exception>
		T GetEnum<T>(SettingDefinition settingDefinition) where T : Enum;
		/// <summary>
		/// Returns <see langword="true"/> if the setting's value was retrieved, <see langword="false"/> otherwise.<br/>
		/// If the setting does not exist, or if <paramref name="key"/> is <see langword="null"/> or empty, <paramref name="defaultValue"/> will be returned.
		/// </summary>
		/// <typeparam name="T">The type of the <see cref="Enum"/> to get.</typeparam>
		/// <param name="key">The key of the setting to get.</param>
		/// <param name="value">The value of the setting associated with the given <paramref name="key"/>.</param>
		/// <param name="defaultValue">The value to return if the setting does not exist.</param>
		/// <returns> <see langword="true"/> if the setting's value was retrieved, <see langword="false"/> otherwise.</returns>
		bool TryGetEnum<T>(string key, out T value, T defaultValue = default) where T : Enum;
		/// <summary>
		/// Returns <see langword="true"/> if the setting's value was retrieved, <see langword="false"/> otherwise.<br/>
		/// If the setting does not exist, <see cref="SettingDefinition.DefaultValue"/> will be returned.<br/>
		/// If the <paramref name="settingDefinition"/> <see cref="SettingDefinition.Key"/> value is <see langword="null"/> or empty, <see cref="SettingDefinition.DefaultValue"/> will be returned.
		/// </summary>
		/// <typeparam name="T">The type of the <see cref="Enum"/> to get.</typeparam>
		/// <param name="settingDefinition">The setting to get.</param>
		/// <param name="value">The value of the setting associated with the given <paramref name="settingDefinition"/>.</param>
		/// <returns> <see langword="true"/> if the setting's value was retrieved, <see langword="false"/> otherwise.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="settingDefinition"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if <see cref="SettingDefinition.Key"/> is <see langword="null"/> or empty.</exception>
		bool TryGetEnum<T>(SettingDefinition settingDefinition, out T value) where T : Enum;

	}

	/// <summary>
	/// Represents a collection of settings that can be read and written to.<br/>
	/// This interface extends <see cref="IReadOnlySettingsCollection"/> and adds methods to set the value of settings.
	/// </summary>
	public interface ISettingsCollection : IReadOnlySettingsCollection
    {
		/// <summary>
		/// Sets the value of the setting associated with the given <paramref name="key"/> to the given <paramref name="value"/>.<br/>
		/// If the setting does not exist, it will be created. If the setting already has a value, it will be overwritten.<br/>
		/// This method will notify subscribers of the <see cref="SettingChanged"/> event.
		/// </summary>
		/// <param name="key">The key of the setting to set.</param>
		/// <param name="value">The value to set the setting to.</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="key"/> is <see langword="null"/> or empty.</exception>
		void Set(string key, string value);
		/// <summary>
		/// Sets the <see langword="bool"/> value of the setting associated with the given <paramref name="key"/> to the given <paramref name="value"/>.<br/>
		/// If the setting does not exist, it will be created. If the setting already has a value, it will be overwritten.<br/>
		/// This method will notify subscribers of the <see cref="SettingChanged"/> event.
		/// </summary> 
		/// <param name="key">The key of the setting to set.</param>
		/// <param name="value">The value to set the setting to.</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="key"/> is <see langword="null"/> or empty.</exception>
		void SetBool(string key, bool value);
		/// <summary>
		/// Sets the <see langword="int"/> value of the setting associated with the given <paramref name="key"/> to the given <paramref name="value"/>.<br/>
		/// If the setting does not exist, it will be created. If the setting already has a value, it will be overwritten.<br/>
		/// This method will notify subscribers of the <see cref="SettingChanged"/> event.
		/// </summary>
		/// <param name="key">The key of the setting to set.</param>
		/// <param name="value">The value to set the setting to.</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="key"/> is <see langword="null"/> or empty.</exception>
		void SetInt(string key, int value);
		/// <summary>
		/// Sets the <see langword="double"/> value of the setting associated with the given <paramref name="key"/> to the given <paramref name="value"/>.<br/>
		/// If the setting does not exist, it will be created. If the setting already has a value, it will be overwritten.<br/>
		/// This method will notify subscribers of the <see cref="SettingChanged"/> event.
		/// </summary>
		/// <param name="key">The key of the setting to set.</param>
		/// <param name="value">The value to set the setting to.</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="key"/> is <see langword="null"/> or empty.</exception>
		void SetDouble(string key, double value);
		/// <summary>
		/// Sets the <see cref="Enum"/> value of the setting associated with the given <paramref name="key"/> to the given <paramref name="value"/>.<br/>
		/// If the setting does not exist, it will be created. If the setting already has a value, it will be overwritten.<br/>
		/// This method will notify subscribers of the <see cref="SettingChanged"/> event.
		/// </summary>
		/// <typeparam name="T">The type of the <see cref="Enum"/> to set.</typeparam>
		/// <param name="key">The key of the setting to set.</param>
		/// <param name="value">The value to set the setting to.</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="key"/> is <see langword="null"/> or empty.</exception>
		void SetEnum<T>(string key, T value) where T : Enum;
	}

}
