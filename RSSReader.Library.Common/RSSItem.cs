using System;

namespace RSSReader.Library.Common
{

    public class RSSItem
    {
        public string Title { get; internal set; }
        public string Link { get; internal set; }
        public string Description { get; internal set; }
        public DateTime? PublishTime { get; internal set; }
    }

}
