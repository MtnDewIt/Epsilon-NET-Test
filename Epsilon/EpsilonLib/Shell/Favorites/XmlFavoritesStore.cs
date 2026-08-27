using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using TagTool.BlamFile;

namespace Shared
{
    [Export(typeof(IFavoritesStore))]
    public class XmlFavoritesStore : IFavoritesStore
    {
        private readonly string _filePath;
        public bool Writing { get; private set; } = false;

        public XmlFavoritesStore(string filePath)
        {
            _filePath = filePath;
        }

        public async Task<IDictionary<FavoritesCacheRecord, List<TagRecord>>> FetchRecords()
        {
            Dictionary<FavoritesCacheRecord, List<TagRecord>> favorites = [];

            if (!File.Exists(_filePath) || new FileInfo(_filePath).Length == 0)
                return favorites;

            return await Task.Run(() =>
            {
                using (var reader = XmlReader.Create(File.OpenText(_filePath)))
                {
                    while (reader.Read())
                    {
                        if (reader.Name == nameof(FavoritesCacheRecord))
                        {
                            string cacheName = reader.GetAttribute(nameof(FavoritesCacheRecord.CacheFileName));
                            //var lastUseTime = reader.GetAttribute("LastUseTime");
                            List<TagRecord> cacheFavorites = [];

                            while (reader.Read() && reader.NodeType != XmlNodeType.EndElement)
                            {
                                if (reader.Name == nameof(TagRecord))
                                {
                                    var tagName = reader.GetAttribute(nameof(TagRecord.TagName));
                                    cacheFavorites.Add(new(tagName));
                                }
                            }

                            if (cacheFavorites.Count > 0)
                                favorites.Add(new(cacheName), cacheFavorites);
                        }
                    }
                }

                return favorites;
            });
        }

        public Task StoreRecords(IDictionary<FavoritesCacheRecord, List<TagRecord>> cacheFavorites)
        {
            Writing = true;
            return Task.Run(() =>
            {
                XmlWriterSettings settings = new XmlWriterSettings
                {
                    Indent = true,
                    WriteEndDocumentOnClose = false,
                    //CheckCharacters = false
                };

                using var fileStream = new FileStream(_filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                using var bufferedStream = new BufferedStream(fileStream, 65536);

                using (var writer = XmlWriter.Create(bufferedStream, settings))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("Favorites");
                    foreach (var cacheKvp in cacheFavorites)
                    {
                        if (cacheKvp.Value is null || cacheKvp.Value.Count == 0)
                            continue;

                        writer.WriteStartElement(nameof(FavoritesCacheRecord));
                        writer.WriteAttributeString(nameof(FavoritesCacheRecord.CacheFileName), cacheKvp.Key.CacheFileName);
                        foreach (var record in cacheKvp.Value)
                        {
                            writer.WriteStartElement(nameof(TagRecord));
                            writer.WriteAttributeString(nameof(TagRecord.TagName), record.TagName);
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
                Writing = false;
            });
        }
    }
}
