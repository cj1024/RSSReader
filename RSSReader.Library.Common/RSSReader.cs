using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Data.Xml.Dom;

namespace RSSReader.Library.Common
{

    public class RSSReader : IRSSReader
    {

        #region IRSSReader Properties

        public string Title { get; private set; }
        public string Link { get; private set; }
        public string Description { get; private set; }
        public DateTime? LastUpdateTime { get; private set; }
        public TimeSpan? UpdateTimeSpan { get; private set; }
        public string Image { get; private set; }
        public IList<RSSItem> ItemList { get; private set; }

        #endregion IRSSReader Properties

        private readonly string[] DateTimeFormatter =
        {
            "ddd, d MMM yyyy HH:mm:ss zzzzz", "ddd, dd MMM yyyy HH:mm:ss zzzzz",
            "R"
        };

        protected virtual DateTime? ChangeDateTimeType(string value)
        {
            try
            {
                return System.Xml.XmlConvert.ToDateTimeOffset(value, DateTimeFormatter).DateTime;
            }
            catch (FormatException)
            {
                try
                {
                    return System.Xml.XmlConvert.ToDateTimeOffset(value).DateTime;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        protected virtual TimeSpan? ChangeTimeSpanType(string period, string frequency)
        {
            int iFrequency;
            if (int.TryParse(frequency, out iFrequency) && iFrequency > 0)
            {
                switch (period.ToLower())
                {
                    case "hourly":
                        return TimeSpan.FromHours(iFrequency);
                    case "daily":
                        return TimeSpan.FromDays(iFrequency);
                    case "weekly":
                        return TimeSpan.FromDays(iFrequency * 7);
                    case "monthly":
                        return TimeSpan.FromDays(iFrequency * 30);
                    case "yearly":
                        return TimeSpan.FromDays(iFrequency * 365);
                }
            }
            return null;
        }

        private static T ChangeType<T>(string value)
        {
            try
            {
                return (T) Convert.ChangeType(value, typeof (T));
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        private static T GetInnerTextForTagName<T>(IXmlNode xNode, string tag, T defaultValue = default(T))
        {
            var element = xNode == null ? null : xNode.ChildNodes.FirstOrDefault(node => node.NodeName.Equals(tag, StringComparison.OrdinalIgnoreCase));
            return element == null ? defaultValue : ChangeType<T>(element.InnerText);
        }

        private static T GetInnerAttribute<T>(IXmlNode xNode, string tag, string attribute, T defaultValue = default(T))
        {
            var element = xNode == null ? null : xNode.ChildNodes.FirstOrDefault(node => node.NodeName.Equals(tag, StringComparison.OrdinalIgnoreCase));
            var attributeNode = element == null ? null : element.Attributes.GetNamedItem(attribute);
            return attributeNode == null ? defaultValue : ChangeType<T>(attributeNode.InnerText);
        }

        public void InitializeWithXmlDocument(XmlDocument document)
        {
            if (document == null)
            {
                ItemList = new List<RSSItem>();
                return;
            }
            Title = GetInnerTextForTagName<string>(document, "title");
            Link = GetInnerTextForTagName<string>(document, "link");
            Description = GetInnerTextForTagName<string>(document, "description");
            Image = GetInnerTextForTagName<string>(document, "image");
            LastUpdateTime = ChangeDateTimeType(GetInnerTextForTagName<string>(document, "lastBuildDate")) ?? ChangeDateTimeType(GetInnerTextForTagName<string>(document, "pubDate"));
            UpdateTimeSpan = ChangeTimeSpanType(GetInnerTextForTagName<string>(document, "sy:updatePeriod"), GetInnerTextForTagName<string>(document, "sy:updateFrequency"));
            ItemList = (from item in document.GetElementsByTagName("item")
                select new RSSItem
                       {
                           Title = GetInnerTextForTagName<string>(item, "title"),
                           Link = GetInnerTextForTagName<string>(item, "link"),
                           Description = GetInnerTextForTagName<string>(item, "description"),
                           PublishTime = ChangeDateTimeType(GetInnerTextForTagName<string>(item, "pubDate")),
                           MediaType = GetInnerAttribute<string>(item, "enclosure", "type"),
                           MediaUrl = GetInnerAttribute<string>(item, "enclosure", "url"),
                           MediaLength = GetInnerAttribute<long>(item, "enclosure", "length")
                       }).ToList();
        }

    }

}
