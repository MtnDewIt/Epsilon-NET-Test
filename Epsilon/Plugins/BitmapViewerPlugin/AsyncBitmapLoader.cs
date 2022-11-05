using CacheEditor;
using System;
using System.Threading.Tasks;
using TagTool.Bitmaps;
using TagTool.Bitmaps.Utils;
using TagTool.Cache;
using TagTool.Tags.Definitions;
using BitmapDecoder = TagTool.Bitmaps.BitmapDecoder;

namespace BitmapViewerPlugin
{
    public partial class BitmapViewerViewModel
    {
        class AsyncBitmapLoader
        {
            private readonly ICacheFile _cacheFile;
            private readonly Bitmap _bitmapGroup;
            private readonly CachedTag _tag;
            private BaseBitmap _baseBitmap;
            private int _currentBitmapIndex = -1;
            private int _currentLayerIndex = -1;
            private int _currentMipLevel = -1;
            private Task<LoadResult> _currentTask;

            public event Action<LoadResult> BitmapLoadCompleted;
            public event Action<string> BitmapLoadFailed;

            public AsyncBitmapLoader(ICacheFile cacheFile, CachedTag tag, Bitmap bitmapGroup)
            {
                _cacheFile = cacheFile;
                _bitmapGroup = bitmapGroup;
                _tag = tag;
            }

            public Task CurrentTask => _currentTask;

            public async void LoadBitmap(int bitmapIndex, int layerIndex, int mipLevel)
            {
                // kind of shitty but will do for now

                if (bitmapIndex == _currentBitmapIndex &&
                    layerIndex == _currentLayerIndex &&
                    _currentMipLevel == mipLevel)
                    return;

                LoadResult result = null;

                try
                {

                    // wait for the current task to finish
                    if (_currentTask != null)
                        await _currentTask;
                }
                // swallow as we're about to load another
                catch { }

                try
                {
                    _currentTask = LoadAsync(_baseBitmap, bitmapIndex, layerIndex, mipLevel);
                    result = await _currentTask;
                    _baseBitmap = result.BaseBitmap;
                }
                catch (Exception ex)
                {
                    BitmapLoadFailed?.Invoke(ex.Message);
                }

                if (result != null)
                    BitmapLoadCompleted?.Invoke(result);
            }

            private Task<LoadResult> LoadAsync(BaseBitmap baseBitmap, int bitmapIndex, int layerIndex, int mipLevel)
            {
                bool bitmapDirty = _currentBitmapIndex != bitmapIndex;
                bool layerDirty = bitmapDirty || (_currentLayerIndex != layerIndex);
                bool mipLevelDirty = layerDirty || (_currentMipLevel != mipLevel);

                _currentBitmapIndex = bitmapIndex;
                _currentLayerIndex = layerIndex;
                _currentMipLevel = mipLevel;

                return Task.Run(() =>
                {
                    if (bitmapDirty)
                        baseBitmap = BitmapExtractor.ExtractBitmap(_cacheFile.Cache, _bitmapGroup, bitmapIndex, _tag.Name, false);

                    byte[] mipData = GetMipmapData(baseBitmap, layerIndex, mipLevel, out int mipWidth, out int mipHeight);
                    mipData = BitmapDecoder.DecodeBitmap(mipData, baseBitmap.Format, mipWidth, mipHeight);
                    return new LoadResult()
                    {
                        BaseBitmap = baseBitmap,
                        MipWidth = mipWidth,
                        MipHeight = mipHeight,
                        MipData = mipData,
                        BitmapDirty = bitmapDirty,
                        LayerDirty = layerDirty,
                        MipLevelDirty = mipLevelDirty
                    };
                });
            }

            private static byte[] GetMipmapData(BaseBitmap bitmap, int layerIndex, int mipmapIndex, out int width, out int height)
            {
                var tempImage = new Bitmap.Image();
                tempImage.Width = (short)bitmap.Width;
                tempImage.Height = (short)bitmap.Height;
                tempImage.Depth = (sbyte)bitmap.Depth;
                tempImage.Type = bitmap.Type;
                tempImage.Format = bitmap.Format;
                tempImage.Flags = bitmap.Flags;

                width = BitmapUtilsPC.GetMipmapWidth(tempImage, mipmapIndex);
                height = BitmapUtilsPC.GetMipmapHeight(tempImage, mipmapIndex);

                int mipmapSize = 0;
                int mipmapOffset = 0;
                if (bitmap.Type == BitmapType.Texture2D)
                {
                    mipmapOffset = BitmapUtilsPC.GetTextureOffset(tempImage, mipmapIndex);
                    mipmapSize = width * height * BitmapFormatUtils.GetBitsPerPixel(tempImage.Format) / 8;
                }
                else if (bitmap.Type == BitmapType.CubeMap)
                {
                    mipmapOffset = BitmapUtilsPC.GetTextureCubemapOffset(tempImage, 0, 0, layerIndex, mipmapIndex);
                    mipmapSize = width * height * BitmapFormatUtils.GetBitsPerPixel(tempImage.Format) / 8;
                }
                else if (bitmap.Type == BitmapType.Array)
                {
                    mipmapOffset = BitmapUtilsPC.GetTextureArrayOffset(tempImage, 0, 0, layerIndex, mipmapIndex);
                    mipmapSize = width * height * BitmapFormatUtils.GetBitsPerPixel(tempImage.Format) / 8;
                }
                else if (bitmap.Type == BitmapType.Texture3D)
                {
                    mipmapOffset = BitmapUtilsPC.GetTexture3DOffset(tempImage, 0, 0, layerIndex, mipmapIndex);
                    mipmapSize = width * height * BitmapFormatUtils.GetBitsPerPixel(tempImage.Format) / 8;
                }
                else
                {
                    throw new NotSupportedException();
                }

                var mipmapData = new byte[mipmapSize];
                Array.Copy(bitmap.Data, mipmapOffset, mipmapData, 0, mipmapSize);
                return mipmapData;
            }

            public class LoadResult
            {
                public BaseBitmap BaseBitmap;
                public byte[] MipData;
                public int MipWidth;
                public int MipHeight;
                public bool BitmapDirty;
                public bool LayerDirty;
                public bool MipLevelDirty;
            }
        }
    }
}
