using EpsilonLib.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TagStructEditor.Common;
using TagTool.Cache;

namespace TagStructEditor.Fields
{
    public class CachedTagField : ValueField
    {
        private readonly Func<CachedTag> _browseTagCallback;

        public CachedTagField(
            ValueFieldInfo info, 
            TagList tagList, 
            Action<CachedTag> openTagCallback, 
            Func<CachedTag> browseTagCallback) : base(info)
        {
            _browseTagCallback = browseTagCallback;

            Groups = tagList.Groups;

            GotoCommand = new DelegateCommand(() => openTagCallback(SelectedInstance.Instance), () => SelectedInstance != null);
            NullCommand = new DelegateCommand(Null, () => SelectedGroup != null);
            BrowseCommand = new DelegateCommand(BrowseTag);
            CopyTagNameCommand = new DelegateCommand(CopyTagName, () => SelectedInstance != null);
            CopyTagIndexCommand = new DelegateCommand(CopyTagIndex, () => SelectedInstance != null);
            PasteTagNameCommand = new DelegateCommand(PasteTagName);
        }

        public IEnumerable<TagGroupItem> Groups { get; set; }
        public TagGroupItem SelectedGroup { get; set; }
        public TagInstanceItem SelectedInstance { get; set; }

        public bool SelectedGroupValid => SelectedGroup != null;
        public bool SelectedInstanceValid => SelectedInstance != null;

        public DelegateCommand GotoCommand { get; }
        public DelegateCommand NullCommand { get; }
        public DelegateCommand BrowseCommand { get; }
        public DelegateCommand CopyTagNameCommand { get; }
        public DelegateCommand CopyTagIndexCommand { get; }
        public DelegateCommand PasteTagNameCommand { get; }

        public override void Accept(IFieldVisitor visitor) => visitor.Visit(this);

        protected override void OnPopulate(object value)
        {
            var instance = (CachedTag)value;
            if (instance != null)
            {
                SelectCachedTag(instance);
            }
            else
            {
                SelectedGroup = null;
                SelectedInstance = null;
            }

            InvalidateCommands();
        }

        private void SelectCachedTag(CachedTag instance)
        {
            SelectedGroup = Groups.FirstOrDefault(item => item.Group == instance.Group);
            SelectedInstance = SelectedGroup?.Instances.FirstOrDefault(item => $"{item.Instance}" == $"{instance}");
        }

        public void OnSelectedInstanceChanged()
        {
            if (SelectedInstance != null || SelectedGroup == null)
                SetActualValue(SelectedInstance?.Instance);

            InvalidateCommands();
        }

        public void OnSelectedGroupChanged()
        {
            InvalidateCommands();
        }

        private void InvalidateCommands()
        {
            GotoCommand.RaiseCanExecuteChanged();
            NullCommand.RaiseCanExecuteChanged();
            BrowseCommand.RaiseCanExecuteChanged();
            CopyTagIndexCommand.RaiseCanExecuteChanged();
            CopyTagNameCommand.RaiseCanExecuteChanged();
            PasteTagNameCommand.RaiseCanExecuteChanged();
        }

        public void Null()
        {
            SelectedGroup = null;
        }

        private void BrowseTag()
        {
            var instance = _browseTagCallback();
            if (instance == null)
                return;

            SelectCachedTag(instance);
        }

        private void CopyTagName()
        {
            ClipboardEx.SetTextSafe($"{SelectedInstance.Instance}");
        }

        private void CopyTagIndex()
        {
            ClipboardEx.SetTextSafe($"0x{SelectedInstance.Instance.Index:X08}");
        }

        private void PasteTagName()
        {
            string[] split = Clipboard.GetText().Split('.');
            if (split.Count() != 2)
                return;
            else
            {
                SelectedGroup = Groups.FirstOrDefault(item => ((TagTool.Cache.Gen3.TagGroupGen3)item.Group).Name == split.Last()) ?? SelectedGroup;
                SelectedInstance = SelectedGroup.Instances.First(item => item.Name == split.First()) ?? SelectedInstance;
            }
        }

        protected override void OnPopulateContextMenu(ContextMenu menu)
        {
            if (menu.Items.Count > 0)
                menu.Items.Add(new Separator());

            menu.Items.Add(new MenuItem() { Header = "Copy Tag Name", Command = CopyTagNameCommand });
            menu.Items.Add(new MenuItem() { Header = "Copy Tag Index", Command = CopyTagIndexCommand });
            menu.Items.Add(new MenuItem() { Header = "Paste Tag Name", Command = PasteTagNameCommand });
            menu.Items.Add(new Separator());
            menu.Items.Add(new MenuItem() { Header = "Open in a new tab", Command = GotoCommand });
            menu.Items.Add(new MenuItem() { Header = "Select a tag", ToolTip = "Select a tag", Command = BrowseCommand });
            menu.Items.Add(new Separator());
            menu.Items.Add(new MenuItem() { Header = "Null", ToolTip = "Null this tag reference", Command = NullCommand,  });
        }
    }
}
