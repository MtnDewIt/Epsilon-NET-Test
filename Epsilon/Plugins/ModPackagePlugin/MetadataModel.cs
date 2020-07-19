using Stylet;
using TagTool.Cache;

namespace ModPackagePlugin
{
    class MetadataModel : PropertyChangedBase
    {
        private ModPackageMetadata _metadata;

        public MetadataModel(ModPackageMetadata metadata)
        {
            _metadata = metadata;
        }

        public string Name
        {
            get => _metadata.Name;
            set
            {
                _metadata.Name = value;
                NotifyOfPropertyChange();
            }
        }

        public string Author
        {
            get => _metadata.Author;
            set
            {
                _metadata.Author = value;
                NotifyOfPropertyChange();
            }
        }

        public string Description
        {
            get => _metadata.Description;
            set
            {
                _metadata.Description = value;
                NotifyOfPropertyChange();
            }
        }
    }
}
