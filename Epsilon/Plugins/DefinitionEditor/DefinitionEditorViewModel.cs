#undef DEBUG
using CacheEditor;
using CacheEditor.RTE;
using CacheEditor.RTE.UI;
using CacheEditor.TagEditing.Messages;
using CacheEditor.ViewModels;
using DefinitionEditor.RTE;
using EpsilonLib.Dialogs;
using EpsilonLib.Logging;
using EpsilonLib.Menus;
using EpsilonLib.Shell;
using EpsilonLib.Utils;
using Shared;
using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using TagStructEditor;
using TagStructEditor.Fields;
using TagStructEditor.Helpers;
using TagTool.Cache;
using TagTool.Cache.HaloOnline;
using TagTool.Commands.Common;
using TagTool.Commands.Unicode;
using TagTool.Common;
using TagTool.IO;
using TagTool.Serialization;
using TagTool.Tags;
using TagTool.Tags.Definitions;

namespace DefinitionEditor
{
	public class DefinitionEditorViewModel : TagEditorPlugin, ITagEditorPlugin
	{

		public class SearchResultItem
		{
			public ValueField Field { get; set; }

			public string FieldName => Field.Name;

			public SearchResultItem(ValueField field) {
				Field = field;
			}
		}

		public class SharedPreferences : PropertyChangedBase
		{
			private bool _autopokeEnabled = false;

			public static SharedPreferences Instance = new SharedPreferences();

			public bool AutoPokeEnabled {
				get {
					return _autopokeEnabled;
				}
				set {
					if (_autopokeEnabled != value) {
						SetAndNotify(ref _autopokeEnabled, value, "AutoPokeEnabled");
						NotifyOfPropertyChange("AutoPokeEnabled");
					}
				}
			}
		}

		private ICacheFile _cacheFile;
		private object _definitionData;
		private IFieldsValueChangeSink _changeSink;

		private IDisposable _rteRefreshTimer;

		private List<BlockOutlineItem> _blockOutline;

		public byte[] RuntimeTagData;

		public SharedPreferences Preferences { get; } = SharedPreferences.Instance;

		private TargetListItem  _SelectedRteTargetItem;
		private bool  _RteHasTargets;

		private StructField  _StructField;
		public StructField StructField {
			get { return _StructField; }
			set {
				if (!object.Equals(_StructField, value)) {
					_StructField = value;
					OnPropertyChanged("BlockOutline");
					OnPropertyChanged("StructField");
				}
			}
		}

		private IField  _DisplayField;
		public IField DisplayField {
			get {
				return _DisplayField;
			}
			set {
				if (!object.Equals(_DisplayField, value)) {
				_DisplayField = value;
					OnDisplayFieldChanged();
					OnPropertyChanged("DisplayField");
				}
			}
		}

		private bool  _ShowFieldTypesFlag;
		public bool ShowFieldTypesFlag {
			get {
				return _ShowFieldTypesFlag;
			}
			set {
				if (_ShowFieldTypesFlag != value) {
				_ShowFieldTypesFlag = value;
					OnPropertyChanged("ShowFieldTypesFlag");
				}
			}
		}

		private string  _SearchQuery;
		public string SearchQuery {
			get {
				return _SearchQuery;
			}
			set {
				if (!string.Equals(_SearchQuery, value, StringComparison.Ordinal)) {
				_SearchQuery = value;
					OnSearchQueryChanged();
					OnPropertyChanged("SearchQuery");
				}
			}
		}

		private ValueField  _SearchResultField;
		public ValueField SearchResultField {
			get {
				return _SearchResultField;
			}
			set {
				if (!object.Equals(_SearchResultField, value)) {
				_SearchResultField = value;
					OnPropertyChanged("SearchResultField");
				}
			}
		}

		private int  _BlockOutlineIndex = -1;
		public int BlockOutlineIndex {
			get {
				return _BlockOutlineIndex;
			}
			set {
				if (_BlockOutlineIndex != value) {
				_BlockOutlineIndex = value;
					OnBlockOutlineIndexChanged();
					OnPropertyChanged("BlockOutlineIndex");
				}
			}
		}

		private bool  _BlockOutlineVisible;
		public bool BlockOutlineVisible {
			get {
				return _BlockOutlineVisible;
			}
			set {
				if (_BlockOutlineVisible != value) {
				_BlockOutlineVisible = value;
					OnPropertyChanged("BlockOutline");
					OnBlockOutlineVisibleChanged();
					OnPropertyChanged("BlockOutlineVisible");
				}
			}
		}

		private bool  _FieldOffsetsVisible;
		public bool FieldOffsetsVisible {
			get {
				return _FieldOffsetsVisible;
			}
			set {
				if (_FieldOffsetsVisible != value) {
				_FieldOffsetsVisible = value;
					OnFieldOffsetsVisibleChanged();
					OnPropertyChanged("FieldOffsetsVisible");
				}
			}
		}

		private bool  _FieldTypesVisible;
		public bool FieldTypesVisible {
			get {
				return _FieldTypesVisible;
			}
			set {
				if (_FieldTypesVisible != value) {
				_FieldTypesVisible = value;
					OnFieldTypesVisibleChanged();
					OnPropertyChanged("FieldTypesVisible");
				}
			}
		}

		public NavigableSearchResults SearchResults { get; } = new NavigableSearchResults();

		public IRteService RteService { get; }

		public TargetListModel RteTargetList { get; }
		
		public TargetListItem SelectedRteTargetItem {
			get {
				return _SelectedRteTargetItem;
			}
			set {
				if (!object.Equals(_SelectedRteTargetItem, value)) {
				_SelectedRteTargetItem = value;
					OnPropertyChanged("SelectedRteTargetItem");
				}
			}
		}

		public bool RteHasTargets {
			get {
				return _RteHasTargets;
			}
			set {
				if (_RteHasTargets != value) {
				_RteHasTargets = value;
					OnPropertyChanged("RteHasTargets");
				}
			}
		}

		public EpsilonLib.Commands.DelegateCommand ExpandAllCommand { get; }

		public EpsilonLib.Commands.DelegateCommand CollapseAllCommand { get; }

		public EpsilonLib.Commands.DelegateCommand SaveCommand { get; }

		public EpsilonLib.Commands.DelegateCommand PokeCommand { get; }

		public EpsilonLib.Commands.DelegateCommand ReloadCommand { get; }

		public List<BlockOutlineItem> BlockOutline {
			get {
				if (_blockOutline == null && BlockOutlineVisible) {
					CreateBlockOutline();
				}
				return _blockOutline;
			}
			set {
				if (!object.Equals(BlockOutline, value)) {
					_blockOutline = value;
					OnPropertyChanged("BlockOutline");
				}
			}
		}

		

		[ImportingConstructor]
		public DefinitionEditorViewModel(TagEditorContext context, params object[] args) : base(context) {
			TagEditorContext = context;
			// args[0] IRteService	_rteService

			// validate that we have args[0] and it's the correct type
			if (args.Length < 1 || !( args[0] is IRteService )) {
				throw new ArgumentException("Invalid arguments passed to DefinitionEditorViewModel.");
			}

			ValueChangedSink valueChangeSink = new ValueChangedSink();
			Configuration config = new TagStructEditor.Configuration()
			{
				OpenTag = context.CacheEditor.OpenTag,
				BrowseTag = context.CacheEditor.RunBrowseTagDialog,
				ValueChanged = valueChangeSink.Invoke
			};
			TagStructEditor.Settings.Load(config);

			PerCacheDefinitionEditorContext ctx = context.GetDefinitionEditorContext();
			FieldFactory factory = new FieldFactory(ctx.Cache, ctx.TagList, config);
			StructField field = context.CreateField(factory);

			_shell = context.Shell;
			_definitionData = context.DefinitionData;
			_cacheEditor = context.CacheEditor;
			_cacheFile = context.CacheEditor.CacheFile;
			_changeSink = valueChangeSink;
			_changeSink.ValueChanged += Field_ValueChanged;
			RteService = args[0] as IRteService;

			_instance = context.Instance;

			StructField = field;
			DisplayField = StructField;

			FieldOffsetsVisible = config.DisplayFieldOffsets;
			FieldTypesVisible = config.DisplayFieldTypes;

			ExpandAllCommand = new EpsilonLib.Commands.DelegateCommand(ExpandAll);
			CollapseAllCommand = new EpsilonLib.Commands.DelegateCommand(CollapseAll);

			PokeCommand = new EpsilonLib.Commands.DelegateCommand(PokeChanges, () => RteTargetList.Any());
			SaveCommand = new EpsilonLib.Commands.DelegateCommand(SaveChanges, () => _cacheFile.CanSerializeTags);
			ReloadCommand = new EpsilonLib.Commands.DelegateCommand(_cacheEditor.ReloadCurrentTag);

			SearchResults.CurrentIndexChanged += SearchResults_CurrentIndexChanged;

			RteTargetList = new TargetListModel(RteService.GetTargetList(context.CacheEditor.CacheFile));
			RteHasTargets = RteTargetList.Any();

			RuntimeTagData = new byte[0];

		}

		internal bool PopulateContextMenu(Node menu, IField field) {
			if (field == null) { return false; }

			if (field is ValueField vf) {
				bool blockOrStruct = field is InlineStructField || field is BlockField;

				if (field is ValueField value && value.IsNot<BlockField, DataField, InlineStructField>()) {

					_ = menu.Submenu(EpsilonLib.Menus.Item.k_FieldEditor)

						.Add("Poke Field", new EpsilonLib.Commands.DelegateCommand(delegate { PokeField(value); }),
							 "Pokes the current field to game memory")

						.AddSeparator();

				}

				IField field2 = field;
				IField field3 = field2;
				if (field3 is StringIdField stringIdField) {

					_ = menu.Submenu(EpsilonLib.Menus.Item.k_FieldEditor)

						.Add("Edit Unicode String", new EpsilonLib.Commands.DelegateCommand(delegate { EditUnicString(stringIdField); }),
							 "Open a dialog to edit this StringID's string, if it exists.")

						.AddSeparator();

				}

				if (!blockOrStruct) {

					_ = menu.Submenu(EpsilonLib.Menus.Item.k_FieldEditor)

							.Add("Copy SetField Command", new EpsilonLib.Commands.DelegateCommand(delegate { CopySetFieldCommand(vf); }),
								 "Copies the value of this field");

				}
				
				_ = menu.Submenu(EpsilonLib.Menus.Item.k_FieldEditor)

						.Add("Copy Name", new EpsilonLib.Commands.DelegateCommand(delegate { CopyFieldName(vf); }),
							 "Copies the name of this field")
						
						.Add("Copy Path", new EpsilonLib.Commands.DelegateCommand(delegate { CopyFieldPath(vf); }),
							 "Copies the path of this field");

				if (!blockOrStruct) {

					_ = menu
						.Submenu(EpsilonLib.Menus.Item.k_FieldEditor)

							.Add("Copy Value", new EpsilonLib.Commands.DelegateCommand(delegate { CopyFieldValue(vf); }),
								 "Copies the value of this field")

							.Add("Copy Path + Value", new EpsilonLib.Commands.DelegateCommand(delegate { CopyFieldPathWithValue(vf); }),
								 "Copies the path and value of this field");

				}

				_ = menu.Submenu(EpsilonLib.Menus.Item.k_FieldEditor)
					
					.Add("Copy Offset", new EpsilonLib.Commands.DelegateCommand(delegate { CopyFieldOffset(vf); }),
						 "Copies the offset of this field");

			}

			_ = menu.Submenu(EpsilonLib.Menus.Item.k_FieldEditor)

				.Add("Copy Memory Address", new EpsilonLib.Commands.DelegateCommand(delegate { CopyFieldMemoryAddress(field); }, RteHasValidTargets), 
					 "Copies the memory address of this field");

			field.PopulateContextMenu(menu);

			return true;

		}

		private bool RteHasValidTargets() {
			return RteHasTargets && SelectedRteTargetItem != null;
		}

		private void CopyFieldOffset(ValueField field) {
			ClipboardEx.SetTextSafe($"{( (ValueField)field ).FieldOffset:X}");
		}

		private void CopyFieldName(ValueField field) {
			ClipboardEx.SetTextSafe(( (ValueField)field ).FieldInfo.ActualName);
		}

		private void CopyFieldPath(ValueField field) {
			ClipboardEx.SetTextSafe(FieldHelper.GetFieldPath(field));
		}

		private void CopyFieldValue(ValueField field) {
			ClipboardEx.SetTextSafe(FieldHelper.GetFieldValueForSetField(_cacheFile.Cache.StringTable, field));
		}

		private void CopyFieldPathWithValue(ValueField field) {
			string value = FieldHelper.GetFieldValueForSetField(_cacheFile.Cache.StringTable, field);
			ClipboardEx.SetTextSafe($"{FieldHelper.GetFieldPath(field)} {value}");
		}

		private void CopySetFieldCommand(ValueField field) {
			string value = FieldHelper.GetFieldValueForSetField(_cacheFile.Cache.StringTable, field);
			ClipboardEx.SetTextSafe($"setfield {FieldHelper.GetFieldPath(field)} {value}\n");
		}

		private void CopyFieldMemoryAddress(IField field) {
			RTEFieldHelper helper = new RTEFieldHelper(SelectedRteTargetItem.Target, _cacheFile, _definitionData.GetType(), _instance);
			uint address = helper.GetFieldMemoryAddress(field);
			if (address == 0) {
				AlertDialogViewModel alert = new AlertDialogViewModel
			{
					AlertType = Alert.Error,
					DisplayName = "Failed to Copy Address",
					Message = "Tag not loaded."
				};
				_shell.ShowDialog(alert);
			}
			else {
				ClipboardEx.SetTextSafe($"{address:X8}");
			}
		}

		private void PokeField(ValueField field) {
			if (SelectedRteTargetItem == null) {
				AlertDialogViewModel alert = new AlertDialogViewModel
			{
					AlertType = Alert.Error,
					DisplayName = "Failed to Poke",
					Message = "Not attached to game instance!"
				};
				_shell.ShowDialog(alert);
				return;
			}
			RTEFieldHelper helper = new RTEFieldHelper(SelectedRteTargetItem.Target, _cacheFile, _definitionData.GetType(), _instance);
			uint address = helper.GetFieldMemoryAddress(field);
			if (address == 0) {
				AlertDialogViewModel alert2 = new AlertDialogViewModel
			{
					AlertType = Alert.Error,
					DisplayName = "Failed to Poke",
					Message = "Tag not loaded!"
				};
				_shell.ShowDialog(alert2);
				return;
			}
			GameCache editorCache = _cacheEditor.CacheFile.Cache;
			Type valueType = field.FieldType;
			object fieldValue = field.FieldInfo.ValueGetter(field.Owner);
			if (valueType == typeof(CachedTag) && editorCache is GameCacheModPackage) {
				CachedTag tagRef = (CachedTag)fieldValue;
				if (editorCache.TagCache.TryGetCachedTag(tagRef.Index, out CachedTag taginstance) && !( (CachedTagHaloOnline)taginstance ).IsEmpty()) {
					List<CachedTag> modpaktagindices = editorCache.TagCache.NonNull().ToList();
					int paktagcount = modpaktagindices.Count((CachedTag x) => x.Index < tagRef.Index);
					tagRef.Index = 65534 - paktagcount;
					fieldValue = tagRef;
				}
			}
			MemoryStream stream = new MemoryStream();
			EndianWriter writer = new EndianWriter(stream, editorCache.Endianness);
			DataSerializationContext dataContext = new DataSerializationContext(writer, CacheAddressType.Memory, useAlignment: false);
			IDataBlock block = dataContext.CreateBlock();
			_cacheEditor.CacheFile.Cache.Serializer.SerializeValue(dataContext, stream, block, fieldValue, new TagFieldAttribute(), field.FieldInfo.FieldType);
			byte[] outData = block.Stream.ToArray();
			using (ProcessMemoryStream processStream = SelectedRteTargetItem.Target.Provider.CreateStream(SelectedRteTargetItem.Target)) {
				processStream.Seek(address, SeekOrigin.Begin);
				processStream.Write(outData, 0, outData.Length);
				processStream.Flush();
			}
		}

		private void CreateBlockOutline() {
			List<BlockOutlineItem> blockOutLine = new List<BlockOutlineItem>();
			blockOutLine.Add(new BlockOutlineItem("[Definition]", StructField));
			blockOutLine.AddRange(
				from field in StructField.Fields.OfType<BlockField>() 
				select new BlockOutlineItem(field)
			);
			_blockOutline = blockOutLine;
		}

		public void OnBlockOutlineVisibleChanged() {
			DisplayField = StructField;
		}

		public void OnFieldTypesVisibleChanged() {
			Application.Current.Resources["FieldTypeVisibility"] = ( ( !FieldTypesVisible ) ? Visibility.Collapsed : Visibility.Visible );
		}

		public void OnFieldOffsetsVisibleChanged() {
			Application.Current.Resources["FieldOffsetVisibility"] = ( ( !FieldOffsetsVisible ) ? Visibility.Collapsed : Visibility.Visible );
		}

		public void OnDisplayFieldChanged() {
			PerformSearch();
		}

		public void OnSearchQueryChanged() {
			PerformSearch();
		}

		private void PerformSearch() {
			if (string.IsNullOrEmpty(SearchQuery) || DisplayField == null) {
				SearchResults.Clear();
				return;
			}

			SearchResults.Results = new ObservableCollection<SearchResultItem>(
				from field in FieldSearch.Search(DisplayField, SearchQuery)
				select new SearchResultItem(field)
			);
		}

		private void Field_ValueChanged(object sender, ValueChangedEventArgs e) {
			if (Preferences.AutoPokeEnabled && PokeCommand.CanExecute(null)) {
				PokeCommand.Execute(null);
			}

			if (e?.Field is ValueField field && !( field is BlockField )) {
				string value = FieldHelper.GetFieldValueForSetField(_cacheFile.Cache.StringTable, field);
				Logger.LogCommand(
					$"{_instance.Name}.{_instance.Group}",
					FieldHelper.GetFieldPath(field), 
					Logger.CommandEvent.CommandType.setfield,
					$"setfield {FieldHelper.GetFieldPath(field)} {value}"
				);
			}
		}

		private void ExpandAll() {
			FieldExpander.Expand(DisplayField, FieldExpander.ExpandTarget.All, FieldExpander.ExpandMode.Expand);
		}

		private void CollapseAll() {
			FieldExpander.Expand(DisplayField, FieldExpander.ExpandTarget.All, FieldExpander.ExpandMode.Collapse);
		}

		public void OnBlockOutlineIndexChanged() {
			if (BlockOutlineIndex == -1) {
				DisplayField = StructField;
			}
			else {
				DisplayField = BlockOutline[BlockOutlineIndex].EditorField;
			}
		}

		private void SearchResults_CurrentIndexChanged(object sender, EventArgs e) {
			// clear the previously highlighted result
			if (SearchResultField != null) {
				SearchResultField.IsHighlighted = false;
				SearchResultField = null;
			}

			// the current index will be -1 when the results are cleared
			if (SearchResults.CurrentIndex >= 0 && SearchResults.CurrentIndex < SearchResults.Results.Count) {
				SearchResultItem currentResult = SearchResults.Results[SearchResults.CurrentIndex] as SearchResultItem;
				currentResult.Field.IsHighlighted = true;
				SearchResultField = currentResult.Field;
			}
		}

		private void RefreshRteTargets() {
			RteTargetList.Refresh();
			RteHasTargets = RteTargetList.Any();
			if (SelectedRteTargetItem == null || !RteTargetList.Contains(SelectedRteTargetItem)) {
				SelectedRteTargetItem = RteTargetList.FirstOrDefault();
			}
			PokeCommand.RaiseCanExecuteChanged();
		}

		private async void SaveChanges() {

			if (!_cacheFile.CanSerializeTags) { throw new NotSupportedException(); }

			try {
				using (IProgressReporter progress = _shell.CreateProgressScope()) {
					progress.Report($"Serializing tag '{_instance}'...");
					await _cacheFile.SerializeTagAsync(_instance, _definitionData);
				}
				_shell.StatusBar.ShowStatusText("Saved Changes");
				Logger.LogCommand(
					$"{_instance.Name}.{_instance.Group}", 
					null, 
					Logger.CommandEvent.CommandType.none, 
					"savetagchanges"
				);
			}

			catch (Exception ex) {
				Debug.WriteLine(ex);

				AlertDialogViewModel alert = new AlertDialogViewModel {
					AlertType = Alert.Error,
					Message = "An exception was thrown while attempting to save tag changes.",
					SubMessage = ex.Message
				};
				_shell.ShowDialog(alert);
			}

		}

		private void PokeChanges() {
			if (SelectedRteTargetItem == null) { return; }
			try {
				IRteTarget target = SelectedRteTargetItem.Target;
				target.Provider.PokeTag(target, _cacheFile.Cache, _instance, _definitionData, ref RuntimeTagData);
				_shell.StatusBar.ShowStatusText("Poked Changes");
			}
			catch (Exception ex) {
				Debug.WriteLine(ex);
				AlertDialogViewModel alert = new AlertDialogViewModel {
					AlertType = Alert.Error,
					Message = "An exception was thrown while attempting to poke tag changes.",
					SubMessage = ex.Message
				};
				_shell.ShowDialog(alert);
			}
		}

		protected override void OnViewLoaded() {
			base.OnViewLoaded();
			if (_rteRefreshTimer == null) {
				_rteRefreshTimer = DispatcherEx.CreateTimer(TimeSpan.FromSeconds(5.0), RefreshRteTargets);
			}
			RefreshRteTargets();
		}

		protected override void OnClose() {
			Logger.LogCommand($"{_instance.Name}.{_instance.Group}", null, Logger.CommandEvent.CommandType.none, "exit");
			base.OnClose();
			DisplayField?.Dispose();
			DisplayField = null;
			StructField?.Dispose();
			StructField = null;
			_cacheFile = null;
			_cacheEditor = null;
			_instance = null;
			_changeSink.ValueChanged -= Field_ValueChanged;
			if (_rteRefreshTimer != null) {
				_rteRefreshTimer.Dispose();
				_rteRefreshTimer = null;
			}
		}

		public override void OnMessage(object sender, object message) {
			if (message is DefinitionDataChangedEvent e) {
				if (e.NewData != null) {
					_definitionData = e.NewData;
					StructField.Populate(null, e.NewData);
				}
				if (e.DefinitionEditorSaveRequested) {
					if (SaveCommand.CanExecute(null)) {
							SaveCommand.Execute(null); 
					}
				}
				if (e.DefinitionEditorPokeRequested) {
					if (PokeCommand.CanExecute(null)) {
						PokeCommand.Execute(null);
					}
				}
			}
		}

		public bool BaseCacheModifyCheck(GameCache cache) {
			if (cache is GameCacheHaloOnlineBase && !( _cacheFile.Cache is GameCacheModPackage )) {
				AlertDialogViewModel alert = new AlertDialogViewModel
				{
					AlertType = Alert.Warning,
					Message = "This action will modify your base cache. Are you sure you want to proceed?"
				};
				return _shell.ShowDialog(alert) == true;
			}
			else {
				return true;
			}
		}

		// field-specific methods and commands

		private void EditUnicString(StringIdField stringid) {
			StringId id = _cacheFile.Cache.StringTable.GetStringId(stringid.Value);
			stringid.UnicText = _cacheFile.Cache.StringTable.GetString(id);

			using (Stream stream = _cacheFile.Cache.OpenCacheReadWrite()) {
				LocalizedString locstr = null;
				MultilingualUnicodeStringList unic = new MultilingualUnicodeStringList();

				foreach (CachedTag unicTag in _cacheFile.Cache.TagCache.FindAllInGroup("unic")) {
					unic = _cacheFile.Cache.Deserialize<MultilingualUnicodeStringList>(stream, unicTag);
					locstr = unic.Strings.SingleOrDefault(x => x.StringID == id);

					if (locstr == null) { continue; }

					string tempstring = unic.GetString(locstr, GameLanguage.English);
					tempstring = LocalizedStringPrinter.EncodeNonAsciiCharacters(unic.GetString(locstr, GameLanguage.English));
					stringid.UnicText = tempstring;

					NameTagDialogViewModel dialog = new NameTagDialogViewModel() {
						DisplayName = stringid.Value,
						Message = "Enter a new string for this StringID.",
						SubMessage = unicTag.ToString(),
						InputText = stringid.UnicText
					};

					if (_shell.ShowDialog(dialog) == true) {
						
						if (!BaseCacheModifyCheck(_cacheFile.Cache)) { break; }

						SetStringCommand setstringcmd = new SetStringCommand(_cacheFile.Cache, unicTag, unic);
						if((bool)setstringcmd.Execute(new List<string>() { "english", stringid.Value, dialog.InputText })) {
							_cacheFile.Cache.Serialize(stream, unicTag, unic);
							_cacheFile.Cache.SaveStrings();
							stringid.UnicText = dialog.InputText;
						}
					}

					continue;
				}
			}
		}

		private readonly IRteService _rteService;

		public new string DisplayName => "Definition";

		public new int SortOrder => -1;

		public override bool ValidForTag(ICacheFile cache, CachedTag tag) {
			return true;
		}

	}
}
