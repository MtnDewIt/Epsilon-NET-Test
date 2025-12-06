using EpsilonLib.Commands;
using EpsilonLib.Logging;
using TagTool.Cache;
using TagTool.Tags.Definitions;

namespace RenderMethodEditorPlugin.ShaderParameters
{
    class SamplerShaderParameter : GenericShaderParameter
    {
        private readonly RenderMethodEditorViewModel _editor;

        public SamplerShaderParameter(RenderMethodEditorViewModel editor, RenderMethod.RenderMethodPostprocessBlock property, string name, string desc, int templateIndex) : base(property, name, desc, templateIndex)
        {
            _editor = editor;

            BrowseCommand = new DelegateCommand(BrowseTag);
            NullCommand = new DelegateCommand(NullTag, canExecute: () => SelectedBitmap != null);
            GotoCommand = new DelegateCommand(GotoTag, canExecute: () => SelectedBitmap != null);
        }

        public string Value
        {
            get => Property.TextureConstants[TemplateIndex].Bitmap?.Name ?? "<null>";
        }

        public DelegateCommand BrowseCommand { get; }
        public DelegateCommand NullCommand { get; }
        public DelegateCommand GotoCommand { get; }

        private CachedTag SelectedBitmap => Property.TextureConstants[TemplateIndex].Bitmap;

        private void GotoTag()
        {
            _editor.TagEditor.CacheEditor.OpenTag(Property.TextureConstants[TemplateIndex].Bitmap);
        }

        private void NullTag()
        {
            SetBitmap(null);
        }

        private void BrowseTag()
        {
            CachedTag selectedTag = _editor.TagEditor.CacheEditor.RunBrowseTagDialog(new CacheEditor.BrowseTagOptions(["bitm"]));
            if (selectedTag != null)
            {
                SetBitmap(selectedTag); 
            }
        }

        public override void Refresh()
        {
            NotifyOfPropertyChange(nameof(Value));
        }

        private void SetBitmap(CachedTag tag)
        {
            Property.TextureConstants[TemplateIndex].Bitmap = tag;
            NotifyOfPropertyChange(nameof(Value));
            NotifyValueChanged();
            NullCommand.RaiseCanExecuteChanged();
            GotoCommand.RaiseCanExecuteChanged();

            Logger.LogCommand($"{Logger.ActiveTag.Name}.{Logger.ActiveTag.Group}", Name,
                   Logger.CommandEvent.CommandType.setfield,
                   $"setfield ShaderProperties[0].TextureConstants[{TemplateIndex}] {tag?.ToString() ?? "null"}");
        }
    }
}
