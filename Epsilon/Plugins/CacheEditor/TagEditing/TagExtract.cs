using Shared;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows.Forms;
using TagTool.Bitmaps;
using TagTool.Cache;
using TagTool.IO;
using TagTool.Tags.Definitions;

namespace CacheEditor.TagEditing
{
    [Export]
    class TagExtract
    {
        private IShell _shell;

        public TagExtract(IShell shell)
        {
            _shell = shell;
        }

        public void ExtractBitmap(GameCache cache, CachedTag tag)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                var result = dialog.ShowDialog();
                if (result != DialogResult.OK)
                    return;

                using (var stream = cache.OpenCacheRead())
                {
                    var bitmap = cache.Deserialize<Bitmap>(stream, tag);
                    ExractBitmap(cache, tag, bitmap, dialog.SelectedPath);
                }
            }
        }

        private void ExractBitmap(GameCache cache, CachedTag tag, Bitmap bitmap, string directory = "bitmaps")
        {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var ddsOutDir = directory;
            string name;
            if (tag.Name != null)
            {
                var split = tag.Name.Split('\\');
                name = split[split.Length - 1];
            }
            else
                name = tag.Index.ToString("X8");
            if (bitmap.Images.Count > 1)
            {
                ddsOutDir = Path.Combine(directory, name);
                Directory.CreateDirectory(ddsOutDir);
            }

            for (var i = 0; i < bitmap.Images.Count; i++)
            {
                var bitmapName = (bitmap.Images.Count > 1) ? i.ToString() : name;
                bitmapName += ".dds";
                var outPath = Path.Combine(ddsOutDir, bitmapName);

                var ddsFile = BitmapExtractor.ExtractBitmap(cache, bitmap, i);

                if (ddsFile == null)
                    throw new Exception("Invalid bitmap data");

                using (var fileStream = File.Open(outPath, FileMode.Create, FileAccess.Write))
                using (var writer = new EndianWriter(fileStream, EndianFormat.LittleEndian))
                {
                    ddsFile.Write(writer);
                }
            }

            _shell.StatusBar.ShowStatusText("Bitmap Extracted");
        }
    }
}
