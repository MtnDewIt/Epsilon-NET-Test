using System.ComponentModel;
using TagStructEditor.Fields;

namespace DefinitionEditor
{
    public class BlockOutlineItem : INotifyPropertyChanged
    {
        public IField EditorField { get; set; }
        public string Header { get; set; }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public BlockOutlineItem(string header, IField field)
        {
            EditorField = field;
            Header = header;
        }

        public BlockOutlineItem(BlockField field)
        {
            EditorField = new BlockEditorViewModel(field);
            field.Block.CollectionChanged += (sender, e) => UpdateHeader(field);
            UpdateHeader(field);
        }

        private void UpdateHeader(BlockField field)
        {
            Header = $"{field.Name} ({field.Block.Count})";
        }
    }
}
