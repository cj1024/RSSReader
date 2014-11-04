using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using Windows.Storage;

namespace RSSReader.Common
{

    public sealed class RSSFeed
    {
        public RSSFeed() { }
        public RSSFeed(string link, string title)
        {
            Link = link;
            Title = title;
        }
        public string Link { get; set; }
        public string Title { get; set; }
    }

    public partial class RSSFeedsManager
    {

        private readonly static object Lock = new object();

        private static RSSFeedsManager _instance;

        public static RSSFeedsManager GetInstance()
        {
            if (_instance == null)
            {
                lock (Lock)
                {
                    if (_instance == null)
                    {
                        _instance = new RSSFeedsManager();
                    }
                }
            }
            return _instance;
        }

    }

    public partial class RSSFeedsManager
    {
        private static readonly DataContractJsonSerializer Serializer = new DataContractJsonSerializer(typeof(IList<RSSFeed>));

        private readonly IList<RSSFeed> _feeds = new List<RSSFeed>();

        private const string FeedsKey = "RSSReader.RSSFeedsManager.Feeds.json";
        
        static RSSFeedsManager()
        {

        }

        private RSSFeedsManager()
        {
            Load();
        }

        void Load()
        {
            var file = TaskExtensions.RunSynchronously(() => ApplicationData.Current.LocalFolder.CreateFileAsync(FeedsKey, CreationCollisionOption.OpenIfExists).AsTask());
            using (var fileStream = TaskExtensions.RunSynchronously(file.OpenStreamForReadAsync))
            {
                if (fileStream.Length == 0)
                {
                    return;
                }
                var items = (IList<RSSFeed>) Serializer.ReadObject(fileStream);
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        _feeds.Add(item);
                    }
                }
            }
        }

        void Save()
        {
            var file = TaskExtensions.RunSynchronously(() => ApplicationData.Current.LocalFolder.CreateFileAsync(FeedsKey, CreationCollisionOption.ReplaceExisting).AsTask());
            using (var fileStream = TaskExtensions.RunSynchronously(file.OpenStreamForWriteAsync))
            {
                Serializer.WriteObject(fileStream, _feeds);
            }
        }

        public IReadOnlyList<RSSFeed> Feeds
        {
            get { return (IReadOnlyList<RSSFeed>)_feeds; }
        }

        public bool AddFeed(RSSFeed feed)
        {
            if (_feeds.Any(existedFeed => string.Equals(existedFeed.Title, feed.Title, StringComparison.OrdinalIgnoreCase) && string.Equals(existedFeed.Link, feed.Link, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
            _feeds.Add(feed);
            Save();
            return true;
        }

        public void RemoveFeed(RSSFeed feed)
        {
            var feedToRemove = _feeds.FirstOrDefault(existedFeed => string.Equals(existedFeed.Title, feed.Title, StringComparison.OrdinalIgnoreCase) && string.Equals(existedFeed.Link, feed.Link, StringComparison.OrdinalIgnoreCase));
            if (feedToRemove != null)
            {
                _feeds.Remove(feedToRemove);
                Save();
            }
        }

    }

}
