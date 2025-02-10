namespace Epsilon
{
	public class PluginLoader
	{
		//private List<PluginInfo> _plugins = new List<PluginInfo>();
		//private List<PluginInfo> _loadQueue = new List<PluginInfo>();

		//public IEnumerable<PluginInfo> Plugins => _loadQueue;

		//public void LoadPlugins() {
		//	Assembly currentAssembly = Assembly.GetExecutingAssembly();
		//	foreach (Type type in currentAssembly.GetTypes()) {
		//		if (type.GetCustomAttribute<PluginAttribute>() != null) {
		//			PluginInfo pluginInfo = new PluginInfo()
		//			{
		//				Assembly = currentAssembly,
		//				Type = type
		//			};

		//			_plugins.Add(pluginInfo);
		//		}
		//	}

		//	ResolveDependencies();
		//	BuildLoadQueue();
		//}

		//private void ResolveDependencies() {
		//	foreach (PluginInfo plugin in _plugins) {
		//		foreach (Type dependencyType in plugin.Type.GetCustomAttributes<DependsOnAttribute>().Select(attr => attr.DependencyType)) {
		//			PluginInfo referencedPlugin = _plugins.FirstOrDefault(p => p.Type == dependencyType);
		//			if (referencedPlugin != null)
		//				plugin.Dependencies.Add(referencedPlugin);
		//		}
		//	}
		//}

		//private void BuildLoadQueue() {
		//	foreach (PluginInfo plugin in _plugins)
		//		AddPluginToLoadQueue(plugin);
		//}

		//private void AddPluginToLoadQueue(PluginInfo plugin) {
		//	if (_loadQueue.Contains(plugin))
		//		return;

		//	foreach (PluginInfo dependency in plugin.Dependencies)
		//		AddPluginToLoadQueue(dependency);

		//	_loadQueue.Add(plugin);
		//}
	}

	public class PluginInfo //: IEquatable<PluginInfo>
	{
		//public Assembly Assembly { get; set; }
		//public Type Type { get; set; }
		//public HashSet<PluginInfo> Dependencies { get; set; } = new HashSet<PluginInfo>();

		//public bool Equals(PluginInfo other) {
		//	return other != null && Type == other.Type;
		//}

		//public override bool Equals(object obj) {
		//	return Equals(obj as PluginInfo);
		//}

		//public override int GetHashCode() {
		//	return Type.GetHashCode();
		//}
	}
}
