using System;
using Windows.Data.Xml.Dom;

namespace RSSReader.Library.Common
{

    public interface IRSSReader
    {
        string Title { get; }
        string Link { get; }
        string Description { get; }
        DateTime? LastUpdateTime { get; }
        TimeSpan? UpdateTimeSpan { get; }
        string Image { get; }
        System.Collections.Generic.IEnumerable<RSSItem> ItemList { get; }
        void InitializeWithXmlDocument(XmlDocument document);
    }

}
