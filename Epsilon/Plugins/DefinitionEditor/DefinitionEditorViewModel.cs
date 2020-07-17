using CacheEditor;
using CacheEditor.RTE;
using CacheEditor.RTE.UI;
using EpsilonLib.Commands;
using EpsilonLib.Settings;
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

namespace DefinitionEditor
{
    public class DefinitionEditorViewModel : Screen, ITagEditorPlugin
    {
        private IShell _shell;
        private ICacheFile _cacheFile;
        private CachedTag _instance;
        private object _definitionData;
        private IDisposable _rteRefreshTimer;
        private IFieldsValueChangeSink _changeSink;

        public DefinitionEditorViewModel(      
            IShell shell,
            IRteService rteService,
            ICacheFile cacheFile,
            CachedTag instance,
            object definitionData,
            StructField structField,
            IFieldsValueChangeSink changeSink)
        {
            _shell = shell;
            _definitionData = definitionData;
            _cacheFile = cacheFile;
            _changeSink = changeSink;
            _changeSink.ValueChanged += Field_ValueChanged;
            

            RteService = rteService;
            _instance = instance;
            StructField = structField;
            DisplayField = StructField;

    
            ExpandAllCommand = new DelegateCommand(ExpandAll);
            CollapseAllCommand = new DelegateCommand(CollapseAll);
            PokeCommand = new DelegateCommand(PokeChanges, () => RteTargetList.Any());
            SaveCommand = new DelegateCommand(SaveChanges, () => _cacheFile.CanSerializeTags);

            SearchResults.CurrentIndexChanged += SearchResults_CurrentIndexChanged;

            RteTargetList = new TargetListModel(rteService.GetTargetList(cacheFile));

            Preferences.PropertyChanged += Preferences_PropertyChanged;
        }

        public SharedPreferences Preferences { get; } = SharedPreferences.Instance;

        public StructField StructField { get; set; }
        public IField DisplayField { get; set; }
        
        public bool ShowFieldTypesFlag { get; set; }

        public string SearchQuery { get; set; }
        public ValueField SearchResultField { get; set; }

        private List<BlockOutlineItem> _blockOutline;
        public int BlockOutlineIndex { get; set; } = -1;
        public bool BlockOutlineVisible
        {
            get => Preferences.BlockOutlineToggled;
            set 
            {
                Preferences.BlockOutlineToggled = value;
                NotifyOfPropertyChange();
            }
        }

        public NavigableSearchResults SearchResults { get; } = new NavigableSearchResults();

        public IRteService RteService { get; }
        public TargetListModel RteTargetList { get; }
        public TargetListItem SelectedRteTargetItem { get; set; }

        public DelegateCommand ExpandAllCommand { get; }
        public DelegateCommand CollapseAllCommand { get; }
        public DelegateCommand SaveCommand { get; }
        public DelegateCommand PokeCommand { get; }

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

            if (SelectedRteTargetItem == null || !RteTargetList.Contains(SelectedRteTargetItem))
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
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show($"An exception was thrown while attempting to save tag changes.\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PokeChanges()
        {
            if (SelectedRteTargetItem == null)
                return;

            try
            {
                var target = SelectedRteTargetItem.Target;
                target.Provider.PokeTag(target, _cacheFile.Cache, _instance, _definitionData);
                _shell.StatusBar.ShowStatusText("Poked Changes");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show($"An exception was thrown while attempting to poke tag changes.\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();

            if (_rteRefreshTimer == null)
                _rteRefreshTimer = DispatcherEx.CreateTimer(TimeSpan.FromSeconds(5), RefreshRteTargets);

            RefreshRteTargets();
        }

        protected override void OnClose()
        {
            base.OnClose();
            DisplayField = null;
            StructField = null;
            _cacheFile = null;
            _cacheFile = null;

            Preferences.PropertyChanged -= Preferences_PropertyChanged;
            _changeSink.ValueChanged -= Field_ValueChanged;

            if (_rteRefreshTimer != null)
            {
                _rteRefreshTimer.Dispose();
                _rteRefreshTimer = null;
            }
        }

        private void Preferences_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SharedPreferences.BlockOutlineToggled))
                NotifyOfPropertyChange(nameof(BlockOutlineVisible));
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
            private bool _blockOutlineToggled = false;

            public static SharedPreferences Instance = new SharedPreferences();

            public bool AutoPokeEnabled
            {
                get => _autopokeEnabled;
                set => SetAndNotify(ref _autopokeEnabled, value);
            }

            public bool BlockOutlineToggled
            {
                get => _blockOutlineToggled;
                set => SetAndNotify(ref _blockOutlineToggled, value);
            }
        }
    }
}
