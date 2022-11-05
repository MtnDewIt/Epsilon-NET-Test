using CacheEditor;
using CacheEditor.TagEditing;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using TagTool.Bitmaps;
using TagTool.Cache;
using TagTool.Tags.Definitions;

namespace BitmapViewerPlugin
{

    public partial class BitmapViewerViewModel : TagEditorPluginBase
    {
        public enum BitmapLoadState
        {
            Success,
            Loading,
            Failed
        }

        private AsyncBitmapLoader _bitmapLoader;

        private int _mipmapLevel;
        private ObservableCollection<string> _bitmaps;
        private ObservableCollection<string> _layers;
        private ObservableCollection<string> _mipmapLevels;
        private int _bitmapIndex;
        private int _layerIndex;
        private string _format;
        private string _dimensions;
        private BitmapSource _bitmapSource;
        private BitmapLoadState _loadState = BitmapLoadState.Loading;

        public string Format
        {
            get { return _format; }
            set
            {
                _format = value;
                NotifyOfPropertyChange();
            }
        }

        public string Dimensions
        {
            get { return _dimensions; }
            set
            {
                _dimensions = value;
                NotifyOfPropertyChange();
            }
        }

        public ObservableCollection<string> Bitmaps
        {
            get { return _bitmaps; }
            set
            {
                _bitmaps = value;
                NotifyOfPropertyChange();
            }
        }

        public ObservableCollection<string> Layers
        {
            get { return _layers; }
            set
            {
                _layers = value;
                NotifyOfPropertyChange();
            }
        }

        public ObservableCollection<string> MipLevels
        {
            get { return _mipmapLevels; }
            set
            {
                _mipmapLevels = value;
                NotifyOfPropertyChange();
            }
        }

        public int BitmapIndex
        {
            get { return _bitmapIndex; }
            set
            {
                _bitmapIndex = value;
                NotifyOfPropertyChange();
                UpdateBitmapDisplay();
            }
        }

        public int LayerIndex
        {
            get { return _layerIndex; }
            set
            {
                _layerIndex = value;
                NotifyOfPropertyChange();
                UpdateBitmapDisplay();
            }
        }

        public int MipLevel
        {
            get { return _mipmapLevel; }
            set
            {
                _mipmapLevel = value;
                NotifyOfPropertyChange();
                UpdateBitmapDisplay();
            }
        }

        public BitmapSource BitmapSource
        {
            get { return _bitmapSource; }
            set
            {
                _bitmapSource = value;
                NotifyOfPropertyChange();
            }
        }

        public BitmapLoadState LoadState
        {
            get { return _loadState; }
            set
            {
                _loadState = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => IsLoaded);
                NotifyOfPropertyChange(() => LoadingIndicatorVisibility);
            }
        }

        public bool IsLoaded
            => LoadState == BitmapLoadState.Success;

        public Visibility LoadingIndicatorVisibility
            => LoadState == BitmapLoadState.Loading ? Visibility.Visible : Visibility.Collapsed;

        public Task InitializeAsync(ICacheFile cacheFile, CachedTag tag, Bitmap bitmapGroup)
        {
            var newBitmaps = new ObservableCollection<string>();
            for (int i = 0; i < bitmapGroup.Images.Count; i++)
                newBitmaps.Add($"Bitmap: {i}");

            Bitmaps = newBitmaps;
            BitmapIndex = 0;

            _bitmapLoader = new AsyncBitmapLoader(cacheFile, tag, bitmapGroup);
            _bitmapLoader.BitmapLoadCompleted += _bitmapLoader_BitmapLoaded;
            _bitmapLoader.BitmapLoadFailed += _bitmapLoader_BitmapLoadFailed;
            _bitmapLoader.LoadBitmap(0, 0, 0);

            return Task.CompletedTask;
        }

        private void _bitmapLoader_BitmapLoadFailed(string message)
        {
            LoadState = BitmapLoadState.Failed;
        }

        private void _bitmapLoader_BitmapLoaded(AsyncBitmapLoader.LoadResult result)
        {
            int mipmapCount = result.BaseBitmap.MipMapCount;
            int layerCount = result.BaseBitmap.Type == BitmapType.CubeMap ? 6 : Math.Max(1, result.BaseBitmap.Depth);

            if (result.BitmapDirty)
            {
                var newLayers = new ObservableCollection<string>();
                for (int i = 0; i < layerCount; i++)
                    newLayers.Add($"Layer: {i}");
                Layers = newLayers;
                LayerIndex = 0;
            }

            if (result.LayerDirty)
            {
                var newMipLevels = new ObservableCollection<string>();
                for (int i = 0; i < mipmapCount; i++)
                    newMipLevels.Add($"Level: {i}");
                MipLevels = newMipLevels;
                MipLevel = 0;
            }

            BitmapSource = new RawBitmapSource(result.MipData, result.MipWidth);
            Dimensions = $"{result.MipWidth}x{result.MipHeight}";
            Format = result.BaseBitmap.Format.ToString();
            LoadState = BitmapLoadState.Success;
        }


        private void UpdateBitmapDisplay()
        {
            if (_bitmapLoader != null && BitmapIndex >= 0 && LayerIndex >= 0 && MipLevel >= 0)
            {
                LoadState = BitmapLoadState.Loading;
                _bitmapLoader.LoadBitmap(BitmapIndex, LayerIndex, MipLevel);
            }
        }
    }
}
