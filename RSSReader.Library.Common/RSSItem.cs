using System;

namespace RSSReader.Library.Common
{

    public class RSSItem
    {
        public string Title { get; internal set; }
        public string Link { get; internal set; }
        public string Description { get; internal set; }
        public DateTime? PublishTime { get; internal set; }
        public string MediaType { get; internal set; }
        public string MediaUrl { get; internal set; }
        public long MediaLength { get; internal set; }
        public string PreviewUrl
        {
            get
            {
                if (!string.IsNullOrEmpty(MediaType) && MediaType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                {
                    return MediaUrl;
                }
                return null;
            }
        }
    }

}
