using Epsilon.Components.TagExplorer.Commands;
using Epsilon.Components.TagTree;
using Epsilon.TagEditing.Messages;
using Epsilon.ViewModels;
using Epsilon.Commands;
using Epsilon.Dialogs;
using Epsilon.Logging;
using Epsilon.Shell.TreeModels;
using Microsoft.Win32;
using Shared;
using Stylet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TagTool.Cache;

namespace Epsilon
{
	class CacheEditorViewModel
        : Conductor<IScreen>.Collection.OneActive, 
        ICacheEditor,
        ICommandHandler<OpenTagCommand>,
        ICommandHandler<RenameTagCommand>,
        ICommandHandler<DuplicateTagCommand>,
        ICommandHandler<ExtractTagCommand>,
        ICommandHandler<ImportTagCommand>,
        ICommandHandler<DeleteTagCommand>
    {
        private ICacheEditingService _cacheEditingService;
        private IShell _shell;
		private ICacheFile _cacheFile;
        public static int _counter;

        public int Counter => _counter;

        public CacheEditorViewModel(
            IShell shell,
            ICacheEditingService cacheEditingService,
            ICacheFile cacheFile)
        {
            _counter++;
            _shell = shell;
            CacheFile = cacheFile;
            _cacheEditingService = cacheEditingService;
            _cacheFile = cacheFile;
            cacheFile.Reloaded += CacheFile_Reloaded;

            DisplayName = _cacheFile.File.Name;
            TagTree = new TagTreeViewModel(_cacheEditingService, _cacheFile, new TagExtract(_shell));
            TagTree.ContextMenu = Components.TagExplorer.MenuDefinitions.ContextMenu;
            TagTree.NodeActivated += TagTree_ItemAttemptOpen;
            CloseCommand = new DelegateCommand(Close);
        }

        public ICacheFile CacheFile { get; }
        public IDictionary<string, object> PluginStorage { get; } = new Dictionary<string, object>();
        public IObservableCollection<IScreen> Documents => Items;
        public IObservableCollection<ICacheEditorTool> Tools { get; } = new BindableCollection<ICacheEditorTool>();
        public ICommand CloseCommand { get; }
        public TagTreeViewModel TagTree { get; set; }
        public event EventHandler CurrentTagChanged;
        public CachedTag CurrentTag => (ActiveItem as TagEditorViewModel)?.Tag;

        public void CopyCacheInfoToClipboard() {
            Epsilon.Shell.ClipboardEx.SetTextSafe(new CacheInfo(_cacheFile).ToString());
        }

		public object _activeLayoutItem;

        public object ActiveLayoutItem
        {
            get => _activeLayoutItem;
            set
            {
                if (ReferenceEquals(_activeLayoutItem, value)) {
					return;
				}

				_activeLayoutItem = value;

                if (!(value is ICacheEditorTool)) {
					ActivateItem((IScreen)value);
				}

				if (value is TagEditorViewModel) {
					CurrentTagChanged?.Invoke(this, EventArgs.Empty);
				}

				if (value is TagEditorViewModel) {
					Logger.ActiveTag = (value as TagEditorViewModel).Tag;
				}

				NotifyOfPropertyChange();
            }
        }

        public override void ActivateItem(IScreen item)
        {
            if (item != null && item.ScreenState == ScreenState.Closed) {
				return;
			}

			ActiveLayoutItem = item;

            base.ActivateItem(item);
        }

        private void TagTree_ItemAttemptOpen(object sender, TreeNodeEventArgs e)
        {
            if (e.Node.Tag is CachedTag instance) {
				OpenTag(instance);
			}
		}

        private void OpenTag(CachedTag instance)
        {
            using (IProgressReporter progress = _shell.CreateProgressScope())
            {
                progress.Report($"Deserializing Tag '{instance}'...");
				
                Task<object> futureDefinitionData = Task.Run(() =>
                {
                    using (Stream stream = CacheFile.Cache.OpenCacheRead()) {
						return CacheFile.Cache.Deserialize(stream, instance);
					}
				});
				
				ActiveItem = new TagEditorViewModel(
                    new TagEditorContext(this, _shell, null, instance, futureDefinitionData), 
                    _cacheEditingService
                );

            }
            Logger.LogCommand($"{instance.Name}.{instance.Group}", null, Logger.CommandEvent.CommandType.none, $"edittag {instance.Name}.{instance.Group}");
        }

        public List<string> GetOpenTagNames()
        {
            List<string> names = new List<string>();
            foreach(IScreen item in Documents) {
				names.Add(item.ToString());
			}

			return names;
        }

        protected override void OnInitialActivate()
        {
            base.OnInitialActivate();
            CreateAndShowTools();
        }

        private void CreateAndShowTools()
        {
            foreach (ICacheEditorToolProvider toolFactory in _cacheEditingService.Tools)
            {
                if (!toolFactory.ValidForEditor(this)) {
					continue;
				}

				ICacheEditorTool tool = toolFactory.CreateTool(this);
                Tools.Add(tool);
                if (tool.IsVisible) {
					ShowTool(tool, false);
				}
			}
        }

        private void CacheFile_Reloaded(object sender, System.EventArgs e)
        {
            Documents.Clear();
            TagTree.Refresh();
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            _cacheEditingService.ActiveEditor = this;
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
            _cacheEditingService.ActiveEditor = null;
        }

        protected override void OnClose()
        {
            base.OnClose();
            TagTree.NodeActivated -= TagTree_ItemAttemptOpen;
            TagTree?.Dispose();
            TagTree = null;
            _shell = null;
            _cacheFile.Cache = null;
            _cacheFile = null;
            _cacheEditingService?.Dispose();
            _cacheEditingService = null;
            PluginStorage?.Clear();
            CloseAllPanes();
        }

        private void CloseAllPanes()
        {
            foreach (ICacheEditorTool tool in Tools) {
				tool.Close();
			}

			Tools.Clear();

            foreach (IScreen document in Documents) {
				document.Close();
			}

			Documents.Clear();
        }

        public void Close()
        {
            RequestClose();
        }

        private void ShowTool(ICacheEditorTool tool, bool activate = false)
        {
            if (!Tools.Contains(tool)) {
				Tools.Add(tool);
			}

			tool.Show(true, activate);
        }

        public ICacheEditorTool GetTool(string name)
        {
            return Tools.FirstOrDefault(x => x.Name == name);
        }

        #region ICacheEditor Members

        TreeModel ICacheEditor.TagTree => TagTree;

        CachedTag ICacheEditor.RunBrowseTagDialog()
        {
			BrowseTagDialogViewModel vm = new BrowseTagDialogViewModel(_cacheEditingService, _cacheFile);
            if (_shell.ShowDialog(vm) == true)
            {
                return vm.TagTree.SelectedNode.Tag as CachedTag;
            }

            return null;
        }

        void ICacheEditor.Reload()
        {
            TagTree.Refresh();
            Documents.Clear();
        }


        #endregion

        #region Command Handlers

        void ICommandHandler<OpenTagCommand>.UpdateCommand(Command command)
        {
            command.IsVisible = IsActive && TagTree?.SelectedNode?.Tag is CachedTag;
        }

        void ICommandHandler<OpenTagCommand>.ExecuteCommand(Command command)
        {
            if (TagTree.SelectedNode?.Tag is CachedTag instance) {
				OpenTag(instance);
			}
		}

        void ICacheEditor.OpenTag(CachedTag tag)
        {
            OpenTag(tag);
        }


        void ICommandHandler<RenameTagCommand>.UpdateCommand(Command command)
        {
            command.IsVisible = _cacheFile.CanRenameTag && TagTree.SelectedNode?.Tag is CachedTag;
        }

        void ICommandHandler<RenameTagCommand>.ExecuteCommand(Command command)
        {
            if (TagTree.SelectedNode?.Tag is CachedTag tag)
            {
				NameTagDialogViewModel vm = new NameTagDialogViewModel()
                {
                    DisplayName = "Rename Tag",
                    Message = "Enter a new name for this tag:",
                    InputText = tag.Name
                };

                if (_shell.ShowDialog(vm) == true)
                {
                    if (!BaseCacheModifyCheck(_cacheFile.Cache)) {
						return;
					}

					Logger.LogCommand(null, null, Logger.CommandEvent.CommandType.none, $"nametag {tag.Name}.{tag.Group} {vm.InputText}");
                    _cacheFile.RenameTag(tag, vm.InputText);
                    TagTree.Refresh(true);
                }
            }
        }

        void ICommandHandler<ExtractTagCommand>.UpdateCommand(Command command)
        {
            command.IsVisible = _cacheFile.CanExtractTag && TagTree.SelectedNode?.Tag is CachedTag;
        }

        void ICommandHandler<ExtractTagCommand>.ExecuteCommand(Command command)
        {
            if (TagTree.SelectedNode?.Tag is CachedTag tag)
            {
				SaveFileDialog ofd = new SaveFileDialog();
                ofd.FileName = $"{Path.GetFileName(tag.Name)}.{tag.Group}";
                if(ofd.ShowDialog() == true)
                {
                    _cacheFile.ExtractTag(tag, ofd.FileName);

					AlertDialogViewModel success = new AlertDialogViewModel
                    {
                        AlertType = Alert.Success,
                        Message = "Tag extracted successfully."
                    };
                    _shell.ShowDialog(success);
                    Logger.LogCommand(null, null, Logger.CommandEvent.CommandType.none, $"extracttag {tag.Name}.{tag.Group} {ofd.FileName}");
                }
            }
        }

        void ICommandHandler<ImportTagCommand>.UpdateCommand(Command command)
        {
            command.IsVisible = _cacheFile.CanImportTag && TagTree.SelectedNode?.Tag is CachedTag;
        }

        void ICommandHandler<ImportTagCommand>.ExecuteCommand(Command command)
        {
            if (TagTree.SelectedNode?.Tag is CachedTag tag)
            {
				OpenFileDialog ofd = new OpenFileDialog();
                ofd.FileName = $"{Path.GetFileName(tag.Name)}.{tag.Group}";
                if (ofd.ShowDialog() == true)
                {
                    _cacheFile.ImportTag(tag, ofd.FileName);

					AlertDialogViewModel success = new AlertDialogViewModel
                    {
                        AlertType = Alert.Success,
                        Message = "Tag imported successfully."
                    };
                    _shell.ShowDialog(success);
                    Logger.LogCommand(null, null, Logger.CommandEvent.CommandType.none, $"importtag {tag.Name}.{tag.Group} {ofd.FileName}");
                }
            }
        }

        void ICommandHandler<DuplicateTagCommand>.UpdateCommand(Command command)
        {
            command.IsVisible = _cacheFile.CanDuplicateTag && TagTree.SelectedNode?.Tag is CachedTag;
        }

        void ICommandHandler<DuplicateTagCommand>.ExecuteCommand(Command command)
        {
            if (TagTree.SelectedNode?.Tag is CachedTag tag)
            {
				NameTagDialogViewModel vm = new NameTagDialogViewModel();
                vm.DisplayName = "Duplicate Tag";
                vm.Message = "Enter a name for the duplicate:";
                vm.InputText = tag.Name;

                if (_shell.ShowDialog(vm) == true)
                {
                    if (!BaseCacheModifyCheck(_cacheFile.Cache)) {
						return;
					}

					_cacheFile.DuplicateTag(tag, vm.InputText);
                    TagTree.Refresh(true);
                    Logger.LogCommand(null, null, Logger.CommandEvent.CommandType.none, $"duplicatetag {tag.Name}.{tag.Group} {vm.InputText}");
                }
            }
        }

        void ICommandHandler<DeleteTagCommand>.UpdateCommand(Command command)
        {
            command.IsVisible = _cacheFile.CanDeleteTag && TagTree.SelectedNode?.Tag is CachedTag;
        }

        void ICommandHandler<DeleteTagCommand>.ExecuteCommand(Command command)
        {
            if (TagTree.SelectedNode?.Tag is CachedTag tag)
            {
                if (!BaseCacheModifyCheck(_cacheFile.Cache)) {
					return;
				}

				AlertDialogViewModel warning = new AlertDialogViewModel
                {
                    AlertType = Alert.Warning,
                    Message = $"Are you sure you want to delete '{Path.GetFileName(tag.Name)}.{tag.Group}'?"
                };

                if (_shell.ShowDialog(warning) == true)
                {
                    Logger.LogCommand(null, null, Logger.CommandEvent.CommandType.none, $"deletetag {tag.Name}.{tag.Group}");
                    _cacheFile.DeleteTag(tag);
                    TagTree.Refresh(true);
                }         
            }
        }

        public void ReloadCurrentTag()
        {
            if(ActiveItem is ITagEditorPluginClient tagEditor)
            {
                using (Stream stream = CacheFile.Cache.OpenCacheRead())
                {
					object data = CacheFile.Cache.Deserialize(stream, CurrentTag);
                    tagEditor.PostMessage(this, new DefinitionDataChangedEvent(data));
                }
            }
        }

        public bool BaseCacheModifyCheck(GameCache cache)
        {
            bool warningsEnabled = _cacheEditingService.Settings.GetBool(Settings.BaseCacheWarningsSetting);

            if (!warningsEnabled) {
				return true;
			}

			if (cache is GameCacheHaloOnlineBase && !(_cacheFile.Cache is GameCacheModPackage))
            {
				AlertDialogViewModel alert = new AlertDialogViewModel
                {
                    AlertType = Alert.Warning,
                    Message = "This action will modify your base cache. Are you sure you want to proceed?"
                };

                if (_shell.ShowDialog(alert) == true) {
					return true;
				}
				else {
					return false;
				}
			}
            else {
				return true;
			}
		}


        public MessageBox asdfj;

        #endregion
    }
}
