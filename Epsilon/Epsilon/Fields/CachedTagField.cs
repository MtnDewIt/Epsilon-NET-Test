using Epsilon.Menus;
using Epsilon.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Epsilon.Common;
using TagTool.Cache;
using Epsilon.Commands;

namespace Epsilon.Fields
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

            Groups = (info.Attribute?.ValidTags == null) 
                ? tagList.Groups 
                : GetValidGroups(info, tagList);

            GotoCommand = new DelegateCommand(() => openTagCallback(SelectedInstance.Instance), () => SelectedInstance != null);
            NullCommand = new DelegateCommand(Null, () => SelectedGroup != null);
            BrowseCommand = new DelegateCommand(BrowseTag);
            CopyTagNameCommand = new DelegateCommand(CopyTagName, () => SelectedInstance != null);
            CopyTagIndexCommand = new DelegateCommand(CopyTagIndex, () => SelectedInstance != null);
            PasteTagNameCommand = new DelegateCommand(PasteTagName);
        }

        private const string k_obje = "obje";
		private const string k_devi = "devi";
		private const string k_unit = "unit";
		private const string k_item = "item";
		private const string k_rm   = "rm  ";
		private const string k_argd = "argd";
		private const string k_armr = "armr";
		private const string k_bipd = "bipd";
		private const string k_bloc = "bloc";
		private const string k_cobj = "cobj";
		private const string k_crea = "crea";
		private const string k_ctrl = "ctrl";
		private const string k_efsc = "efsc";
		private const string k_eqip = "eqip";
		private const string k_gint = "gint";
		private const string k_mach = "mach";
		private const string k_proj = "proj";
		private const string k_scen = "scen";
		private const string k_ssce = "ssce";
		private const string k_term = "term";
		private const string k_vehi = "vehi";
		private const string k_weap = "weap";
		private const string k_rmbk = "rmbk";
		private const string k_rmcs = "rmcs";
		private const string k_rmct = "rmct";
		private const string k_rmd  = "rmd ";
		private const string k_rmfl = "rmfl";
		private const string k_rmgl = "rmgl";
		private const string k_rmhg = "rmhg";
		private const string k_rmsh = "rmsh";
		private const string k_rmss = "rmss";
		private const string k_rmtr = "rmtr";
		private const string k_rmw  = "rmw ";
		private const string k_rmzo = "rmzo";

		private readonly string[] k_objeStrings = new string[18] { k_argd, k_armr, k_bipd, k_bloc, k_cobj, k_crea, k_ctrl, k_efsc, k_eqip, k_gint, k_mach, k_proj, k_scen, k_ssce, k_term, k_unit, k_vehi, k_weap };
        private readonly string[] k_rmStrings = new string[12] { k_rmbk, k_rmcs, k_rmct, k_rmd, k_rmfl, k_rmgl, k_rmhg, k_rmsh, k_rmss, k_rmtr, k_rmw, k_rmzo };
		private readonly string[] k_deviStrings = new string[4] { k_argd, k_ctrl, k_mach, k_term };
		private readonly string[] k_unitStrings = new string[3] { k_bipd, k_gint, k_vehi };
        private readonly string[] k_itemTags = new string[2] { k_eqip, k_weap };

		private IEnumerable<TagGroupItem> GetValidGroups(ValueFieldInfo info, TagList tagList )
        {
            List<string> validTags = info.Attribute.ValidTags.ToList();
            if      (validTags.Contains(k_obje))    { _ = validTags.Remove(k_obje); validTags.AddRange(k_objeStrings); }
            else if (validTags.Contains(k_rm))      { _ = validTags.Remove(k_rm);   validTags.AddRange(k_rmStrings);   }
			else if (validTags.Contains(k_devi))    { _ = validTags.Remove(k_devi); validTags.AddRange(k_deviStrings); }
			else if (validTags.Contains(k_unit))    { _ = validTags.Remove(k_unit); validTags.AddRange(k_unitStrings); }
			else if (validTags.Contains(k_item))    { _ = validTags.Remove(k_item); validTags.AddRange(k_itemTags);    }
			return tagList.Groups.Where(x => validTags.Contains(x.TagAscii));
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

            var group = ValidateTagGroup(instance.Group.ToString());
            if (group != null)
            {
                SelectCachedTag(instance);
            }
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
                try
                {
                    var group = ValidateTagGroup(split.Last());
                    if (group != null)
                    {
                        SelectedGroup = group;
                        SelectedInstance = SelectedGroup.Instances.First(item => item.Name == split.First()) ?? SelectedInstance;
                    }
                }
                catch (Exception ex)
                {
                    if (ex is Exception)
                        MessageBox.Show($"{Clipboard.GetText()} is not a valid tag name.", "Tag Not Found");
                    else
                        throw;
                }
            }
        }

        public TagGroupItem ValidateTagGroup(string input)
        {
            // check long form
            input = input.Trim();
            TagGroupItem group = Groups.FirstOrDefault(item =>
                ((TagTool.Cache.Gen3.TagGroupGen3)item.Group).Name == input);

            // check abbreviations
            if (group == null)
            {
                if (input.Length == 3)
                    input += " ";

                group = Groups.FirstOrDefault(item => item.TagAscii == input);
            }

            // alert if not found
            if (group == null)
            {
                var validgroups = string.Join(", ", 
                    Groups.Select(g => 
                    ((TagTool.Cache.Gen3.TagGroupGen3)g.Group).Name + $" ({g.TagAscii})"));

                if (Groups.Count() > 20)
                    validgroups = "unspecified";

                _ = MessageBox.Show($"\'{input}\' is not a valid group for this tag reference."
                    + $"\n\nValid tag groups: {validgroups}"
                    , "Invalid Tag Group");
            }

            return group;
        }

		protected override void OnPopulateContextMenu(Node menu)
        {
#pragma warning disable IDE0058 // Expression value is never used
			menu.Submenu(Epsilon.Menus.Item.k_TagEditor)
                .Add("Open in New Tab", command: GotoCommand)
				.AddSeparator()
				.Add(Epsilon.Menus.Item.k_TagFinder, tooltip: "Open a Tag browser to search for a Tag", command: BrowseCommand)
                .AddSeparator()
                .Add("Copy Tag Name", command: CopyTagNameCommand)
                .Add("Copy Tag Index", command: CopyTagIndexCommand)
                .Add("Paste Tag Name", command: PasteTagNameCommand)
                .AddSeparator()
                .Add(Epsilon.Menus.Item.k_SetToNull, tooltip: "Set this Tag reference to a NULL ⌀ value", command: NullCommand);

#pragma warning restore IDE0058 // Expression value is never used
        }

        public override void Dispose()
        {
            base.Dispose();
            //foreach (var group in Groups)
            //    group.Instances.Clear();
            Groups = null;
            SelectedGroup = null;
            SelectedInstance = null;
        }
    }
}
