﻿using CacheEditor;
using CacheEditor.TagEditing;
using CacheEditor.TagEditing.Messages;
using CacheEditor.ViewModels;
using EpsilonLib.Commands;
using EpsilonLib.Menus;
using EpsilonLib.Shell;
using EpsilonLib.Utils;
using Shared;
using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using TagStructEditor.Fields;
using TagStructEditor.Helpers;
using TagTool.Cache;
using TagTool.Common;
using TagTool.Commands.Unicode;
using TagTool.Commands.Common;
using TagTool.Tags.Definitions;
using System.IO;
using TagTool.IO;
using TagTool.Serialization;
using TagTool.Tags;
using TagTool.Cache.HaloOnline;
using EpsilonLib.Logging;
using EpsilonLib.Dialogs;
using System.Windows.Data;
using CacheEditor.RTE;
using CacheEditor.RTE.UI;
using System.Windows.Threading;
using System.Windows.Input;

namespace DefinitionEditor
{
    public class DefinitionEditorViewModel : TagEditorPluginBase
    {
        private IShell _shell;
        private ICacheEditor _cacheEditor;
        private ICacheFile _cacheFile;
        private CachedTag _instance;
        private object _definitionData;
        private IDisposable _rteRefreshTimer;
        private IFieldsValueChangeSink _changeSink;

        public DefinitionEditorViewModel(      
            IShell shell,
            IRteService rteService,
            ICacheEditor cacheEditor,
            ICacheFile cacheFile,
            CachedTag instance,
            object definitionData,
            StructField structField,
            IFieldsValueChangeSink changeSink,
            TagStructEditor.Configuration config)
        {
            _shell = shell;
            _definitionData = definitionData;
            _cacheEditor = cacheEditor;
            _cacheFile = cacheFile;
            _changeSink = changeSink;
            _changeSink.ValueChanged += Field_ValueChanged;
            RteService = rteService;

            _instance = instance;
            StructField = structField;
            DisplayField = StructField;

            FieldOffsetsVisible = config.DisplayFieldOffsets;
            FieldTypesVisible = config.DisplayFieldTypes;

            ExpandAllCommand = new DelegateCommand(ExpandAll);
            CollapseAllCommand = new DelegateCommand(CollapseAll);
            PokeCommand = new DelegateCommand(new Action(PokeChanges), () => RteTargetList.Any());
            SaveCommand = new DelegateCommand(SaveChanges, () => _cacheFile.CanSerializeTags);
            ReloadCommand = new DelegateCommand(() => _cacheEditor.ReloadCurrentTag());

            SearchResults.CurrentIndexChanged += SearchResults_CurrentIndexChanged;

            RteTargetList = new TargetListModel(rteService.GetTargetList(cacheFile));
            RteHasTargets = RteTargetList.Any();

            RuntimeTagData = new byte[0];
        }

        public SharedPreferences Preferences { get; } = SharedPreferences.Instance;

        private void PopulateCopyMenu(EMenu menu, IField field)
        {
            
        }

        internal bool PopulateContextMenu(EMenu menu, IField field)
        {
            if (field == null)
                return false;

            if (field is ValueField vf)
            {
                bool blockOrStruct = (field is InlineStructField) || (field is BlockField);
                bool blockOrStructOrData = (field is BlockField) || (field is InlineStructField) || (field is DataField);

                if (!blockOrStruct)
                {
                    menu.Submenu("Field").Group("Copy")
                        .Add(text: "Copy SetField Command",
                                tooltip: "Copies the value of this field",
                                command: new DelegateCommand(() => CopySetFieldCommand(vf)));
                }

                menu.Submenu("Field")
                    .Group("Copy")
                        .Add(text: "Copy Name",
                                tooltip: "Copies the name of this field",
                                command: new DelegateCommand(() => CopyFieldName(vf)))
                        .Add(text: "Copy Path",
                                tooltip: "Copies the path of this field",
                                command: new DelegateCommand(() => CopyFieldPath(vf)));

                if (!blockOrStruct)
                {
                    menu.Submenu("Field").Group("Copy")
                        .Add(text: "Copy Path + Value",
                                tooltip: "Copies the path and value of this field",
                                command: new DelegateCommand(() => CopyFieldPathWithValue(vf)))
                        .Add(text: "Copy Value",
                                tooltip: "Copies the value of this field",
                                command: new DelegateCommand(() => CopyFieldValue(vf)));
                }

                menu.Submenu("Field").Group("Copy")
                        .Add(text: "Copy Offset",
                                tooltip: "Copies the offset of this field",
                                command: new DelegateCommand(() => CopyFieldOffset(vf)));

                switch (field)
                {
                    case StringIdField stringid:
                        {
                            menu.Group("StringId").Add(text: "Edit Unicode String",
                                tooltip: "Open a dialog to edit this StringID's string, if it exists.",
                                command: new DelegateCommand(() => EditUnicString(stringid)));
                            break;
                        }
                }

                if (!blockOrStructOrData) 
                {
                    menu.Submenu("Field")
                        .Add(text: "Poke Field",
                                tooltip: "Pokes the current field to game memory",
                                command: new DelegateCommand(() => PokeField(vf)));
                }

                menu.Submenu("Field")
                    .Group("Copy")
                        .Add(text: "Copy Memory Address", 
                                tooltip: "Copies the memory address of this field",
                                command: new DelegateCommand(() => CopyFieldMemoryAddress(vf), () => RteHasTargets && SelectedRteTargetItem != null));
            }
            

            field.PopulateContextMenu(menu);

            return true;
        }

        private void CopyFieldOffset(ValueField field)
        {
            ClipboardEx.SetTextSafe($"{((ValueField)field).FieldOffset:X}");
        }

        private void CopyFieldName(ValueField field)
        {
            ClipboardEx.SetTextSafe(((ValueField)field).FieldInfo.ActualName);
        }

        private void CopyFieldPath(ValueField field)
        {
            ClipboardEx.SetTextSafe(FieldHelper.GetFieldPath(field));
        }

        private void CopyFieldValue(ValueField field)
        {
            ClipboardEx.SetTextSafe(FieldHelper.GetFieldValueForSetField(_cacheFile.Cache.StringTable, field));
        }

        private void CopyFieldPathWithValue(ValueField field)
        {
            var value = FieldHelper.GetFieldValueForSetField(_cacheFile.Cache.StringTable, field);
            ClipboardEx.SetTextSafe($"{FieldHelper.GetFieldPath(field)} {value}");
        }

        private void CopySetFieldCommand(ValueField field)
        {
            var value = FieldHelper.GetFieldValueForSetField(_cacheFile.Cache.StringTable, field);
            ClipboardEx.SetTextSafe($"SetField {FieldHelper.GetFieldPath(field)} {value}\n");
        }

        private void CopyFieldMemoryAddress(IField field)
        {
            uint fieldMemoryAddress = new RTEFieldHelper(SelectedRteTargetItem.Target, _cacheFile, _definitionData.GetType(), _instance).GetFieldMemoryAddress(field);

            if (fieldMemoryAddress == 0x0)
            {
                AlertDialogViewModel alertDialogViewModel = new AlertDialogViewModel
                {
                    AlertType = Alert.Error,
                    DisplayName = "Failed to Copy Address",
                    Message = "Tag not loaded."
                };
                _shell.ShowDialog(alertDialogViewModel);
            }
            else
                ClipboardEx.SetTextSafe(string.Format("{0:X8}", fieldMemoryAddress, 10));
        }

        private void PokeField(ValueField field)
        {
            if (SelectedRteTargetItem == null)
            {
                AlertDialogViewModel alertDialogViewModel = new AlertDialogViewModel
                {
                    AlertType = Alert.Error,
                    DisplayName = "Failed to Poke",
                    Message = "Not attached to game instance!"
                };
                _shell.ShowDialog(alertDialogViewModel);
            }
            else
            {
                uint fieldMemoryAddress = new RTEFieldHelper(SelectedRteTargetItem.Target, _cacheFile, _definitionData.GetType(), _instance).GetFieldMemoryAddress(field);

                if (fieldMemoryAddress == 0x0)
                {
                    AlertDialogViewModel alertDialogViewModel = new AlertDialogViewModel
                    {
                        AlertType = Alert.Error,
                        DisplayName = "Failed to Poke",
                        Message = "Tag not loaded!"
                    };
                    _shell.ShowDialog(alertDialogViewModel);
                }
                else
                {
                    GameCache cache = _cacheEditor.CacheFile.Cache;

                    Type fieldType = field.FieldType;

                    object fieldValue = field.FieldInfo.ValueGetter.Invoke(field.Owner);

                    if (fieldType == typeof(CachedTag) && cache is GameCacheModPackage)
                    {
                        CachedTag tagRef = (CachedTag)fieldValue;

                        if (cache.TagCache.TryGetCachedTag(tagRef.Index, out var cachedTag) && !((CachedTagHaloOnline)cachedTag).IsEmpty())
                        {
                            int index = cache.TagCache.NonNull().ToList().Count(x => x.Index < tagRef.Index);
                            tagRef.Index = 0xFFFE - index;
                            fieldValue = tagRef;
                        }
                    }

                    MemoryStream memoryStream = new MemoryStream();
                    DataSerializationContext serializationContext = new DataSerializationContext(new EndianWriter(memoryStream, cache.Endianness));
                    IDataBlock block = serializationContext.CreateBlock();

                    _cacheEditor.CacheFile.Cache.Serializer.SerializeValue(serializationContext, memoryStream, block, fieldValue, new TagFieldAttribute(), field.FieldInfo.FieldType);

                    byte[] array = block.Stream.ToArray();

                    using (ProcessMemoryStream stream = SelectedRteTargetItem.Target.Provider.CreateStream(SelectedRteTargetItem.Target))
                    {
                        stream.Seek(fieldMemoryAddress, SeekOrigin.Begin);
                        stream.Write(array, 0, array.Length);
                        stream.Flush();
                    }
                }
            }
        }

        public StructField StructField { get; set; }
        public IField DisplayField { get; set; }
        
        public bool ShowFieldTypesFlag { get; set; }

        public string SearchQuery { get; set; }
        public ValueField SearchResultField { get; set; }

        private List<BlockOutlineItem> _blockOutline;
        public int BlockOutlineIndex { get; set; } = -1;
        public bool BlockOutlineVisible { get; set; }

        public bool FieldOffsetsVisible { get; set; }
        public bool FieldTypesVisible { get; set; }

        public NavigableSearchResults SearchResults { get; } = new NavigableSearchResults();

        public IRteService RteService { get; }

        public TargetListModel RteTargetList { get; }

        public TargetListItem SelectedRteTargetItem { get; set; }

        public bool RteHasTargets { get; set; }
        public byte[] RuntimeTagData;

        public DelegateCommand ExpandAllCommand { get; }
        public DelegateCommand CollapseAllCommand { get; }
        public DelegateCommand SaveCommand { get; }
        public DelegateCommand PokeCommand { get; }
        public DelegateCommand ReloadCommand { get; }

        public List<BlockOutlineItem> BlockOutline
        {
            get
            {
                if (_blockOutline == null && BlockOutlineVisible)
                    CreateBlockOutline();

                return _blockOutline;
            }
            set
            {
                _blockOutline = value;
            }
        }

        private void CreateBlockOutline()
        {
            var blockOutLine = new List<BlockOutlineItem>();
            blockOutLine.Add(new BlockOutlineItem("[Definition]", StructField));
            blockOutLine.AddRange(StructField.Fields.OfType<BlockField>()
                .Select(field => new BlockOutlineItem(field)));
            _blockOutline = blockOutLine;
        }

        public void OnBlockOutlineVisibleChanged()
        {
            DisplayField = StructField;
        }

        public void OnFieldTypesVisibleChanged()
        {
            Application.Current.Resources["FieldTypeVisibility"] = FieldTypesVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public void OnFieldOffsetsVisibleChanged()
        {
            Application.Current.Resources["FieldOffsetVisibility"] = FieldOffsetsVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public void OnDisplayFieldChanged()
        {
            PerformSearch();
        }

        public void OnSearchQueryChanged()
        {
            PerformSearch();
        }

        private void PerformSearch()
        {
            if(string.IsNullOrEmpty(SearchQuery) || DisplayField == null)
            {
                SearchResults.Clear();
                return;
            }

            SearchResults.Results =
               new ObservableCollection<SearchResultItem>(
                   FieldSearch.Search(DisplayField, SearchQuery)
                    .Select(field => new SearchResultItem(field)));
        }

        private void Field_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (Preferences.AutoPokeEnabled && PokeCommand.CanExecute(null))
                PokeCommand.Execute(null);
            if (e.Field is ValueField field && !(field is BlockField))
            {
                var value = FieldHelper.GetFieldValueForSetField(_cacheFile.Cache.StringTable, field);
                Logger.LogCommand($"{_instance.Name}.{_instance.Group}", FieldHelper.GetFieldPath(field), Logger.CommandEvent.CommandType.SetField,
                    $"SetField {FieldHelper.GetFieldPath(field)} {value}");
            }
        }

        private void ExpandAll()
        {
            FieldExpander.Expand(DisplayField, FieldExpander.ExpandTarget.All, FieldExpander.ExpandMode.Expand);
        }

        private void CollapseAll()
        {
            FieldExpander.Expand(DisplayField, FieldExpander.ExpandTarget.All, FieldExpander.ExpandMode.Collapse);
        }

        public void OnBlockOutlineIndexChanged()
        {
            if (BlockOutlineIndex == -1)
                DisplayField = StructField;
            else
                DisplayField = BlockOutline[BlockOutlineIndex].EditorField;
        }

        private void SearchResults_CurrentIndexChanged(object sender, EventArgs e)
        {
            // clear the previously highlighted result
            if (SearchResultField != null)
            {
                SearchResultField.IsHighlighted = false;
                SearchResultField = null;
            }

            // the current index will be -1 when the results are cleared
            if (SearchResults.CurrentIndex >= 0 && SearchResults.CurrentIndex < SearchResults.Results.Count)
            {
                var currentResult = SearchResults.Results[SearchResults.CurrentIndex] as SearchResultItem;
                currentResult.Field.IsHighlighted = true;

                SearchResultField = currentResult.Field;
            }
        }

        private void RefreshRteTargets()
        {
            RteTargetList.Refresh();
            RteHasTargets = RteTargetList.Any();
            if (SelectedRteTargetItem == null || !(RteTargetList).Contains(SelectedRteTargetItem))
                SelectedRteTargetItem = RteTargetList.FirstOrDefault();
            PokeCommand.RaiseCanExecuteChanged();
        }

        private async void SaveChanges()
        {
            if (!_cacheFile.CanSerializeTags)
                throw new NotSupportedException();

            try
            {
                using (var progress = _shell.CreateProgressScope())
                {
                    progress.Report($"Serializing tag '{_instance}'...");
                    await _cacheFile.SerializeTagAsync(_instance, _definitionData);
                }
                _shell.StatusBar.ShowStatusText("Saved Changes");
                Logger.LogCommand($"{_instance.Name}.{_instance.Group}", null, Logger.CommandEvent.CommandType.None, "SaveTagChanges");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
               
                var alert = new AlertDialogViewModel
                {
                    AlertType = Alert.Error,
                    Message = $"An exception was thrown while attempting to save tag changes.",
                    SubMessage = ex.Message
                };

                _shell.ShowDialog(alert);
            }
        }

        private void PokeChanges()
        {
            if (SelectedRteTargetItem == null)
                return;

            try
            {
                IRteTarget target = SelectedRteTargetItem.Target;
                target.Provider.PokeTag(target, _cacheFile.Cache, _instance, _definitionData, ref RuntimeTagData);
                _shell.StatusBar.ShowStatusText("Poked Changes");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                _shell.ShowDialog(new AlertDialogViewModel()
                {
                    AlertType = Alert.Error,
                    Message = "An exception was thrown while attempting to poke tag changes.",
                    SubMessage = ex.Message
                });
            }
        }

        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            if (_rteRefreshTimer == null)
                _rteRefreshTimer = DispatcherEx.CreateTimer(TimeSpan.FromSeconds(5.0), new Action(RefreshRteTargets));
            RefreshRteTargets();
        }

        protected override void OnClose()
        {
            Logger.LogCommand($"{_instance.Name}.{_instance.Group}", null, Logger.CommandEvent.CommandType.None, "Exit");
            base.OnClose();
            DisplayField?.Dispose();
            DisplayField = null;
            StructField?.Dispose();
            StructField = null;
            _cacheFile = null;
            _cacheEditor = null;
            _instance = null;
            _changeSink.ValueChanged -= Field_ValueChanged;

            if (_rteRefreshTimer == null)
                return;

            _rteRefreshTimer.Dispose();
            _rteRefreshTimer = null;
        }

        protected override void OnMessage(object sender, object message)
        {
            if (message is DefinitionDataChangedEvent e)
            {
                _definitionData = e.NewData;
                StructField.Populate(null, e.NewData);
            }
        }

        public class SearchResultItem
        {
            public ValueField Field { get; set; }
            public string FieldName => Field.Name;

            public SearchResultItem(ValueField field)
            {
                Field = field;
            }
        }

        public class SharedPreferences : PropertyChangedBase
        {
            private bool _autopokeEnabled = false;

            public static SharedPreferences Instance = new SharedPreferences();

            public bool AutoPokeEnabled
            {
                get => _autopokeEnabled;
                set => SetAndNotify(ref _autopokeEnabled, value);
            }
        }

        public bool BaseCacheModifyCheck(GameCache cache)
        {
            if (cache is GameCacheHaloOnlineBase && !(_cacheFile.Cache is GameCacheModPackage))
            {
                var alert = new AlertDialogViewModel
                {
                    AlertType = Alert.Warning,
                    Message = "This action will modify your base cache. Are you sure you want to proceed?"
                };

                if (_shell.ShowDialog(alert) == true)
                    return true;
                else
                    return false;
            }
            else
                return true;
        }

        // field-specific methods and commands

        private void EditUnicString(StringIdField stringid)
        {
            StringId id = _cacheFile.Cache.StringTable.GetStringId(stringid.Value);
            stringid.UnicText = _cacheFile.Cache.StringTable.GetString(id);

            using (var stream = _cacheFile.Cache.OpenCacheReadWrite())
            {
                LocalizedString locstr = null;
                var unic = new MultilingualUnicodeStringList();

                foreach (var unicTag in _cacheFile.Cache.TagCache.FindAllInGroup("unic"))
                {
                    unic = _cacheFile.Cache.Deserialize<MultilingualUnicodeStringList>(stream, unicTag);
                    locstr = unic.Strings.SingleOrDefault(x => x.StringID == id);

                    if (locstr != null)
                    {
                        var tempstring = unic.GetString(locstr, GameLanguage.English);
                        tempstring = LocalizedStringPrinter.EncodeNonAsciiCharacters(unic.GetString(locstr, GameLanguage.English));
                        stringid.UnicText = tempstring;

                        var dialog = new NameTagDialogViewModel()
                        {
                            DisplayName = stringid.Value,
                            Message = "Enter a new string for this StringID.",
                            SubMessage = unicTag.ToString(),
                            InputText = stringid.UnicText
                        };

                        if (_shell.ShowDialog(dialog) == true)
                        {
                            if (!BaseCacheModifyCheck(_cacheFile.Cache))
                                return;

                            var setstringcmd = new SetStringCommand(_cacheFile.Cache, unicTag, unic);
                            bool success = (bool)setstringcmd.Execute(new List<string>() { "english", stringid.Value, dialog.InputText });

                            if (success)
                            {
                                _cacheFile.Cache.Serialize(stream, unicTag, unic);
                                _cacheFile.Cache.SaveStrings();
                                stringid.UnicText = dialog.InputText;
                            }
                        }

                        continue;
                    }
                }
            }
        }


    }
}
