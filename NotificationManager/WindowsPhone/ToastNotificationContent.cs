using System;
using Windows.Data.Xml.Dom;

namespace NotificationManager.WindowsPhone
{

    internal sealed class ToastNotificationContent : INotificationContent
    {

        internal ToastNotificationContent(string title, string content, ToastAuidoType auidoType)
        {
            _title = title ?? string.Empty;
            _content = content ?? string.Empty;
            _auidoType = auidoType;
        }

        private readonly string _title, _content;

        private readonly ToastAuidoType _auidoType;

        public string GetXmlString()
        {
            return GetXmlDocument().GetXml();
        }

        public XmlDocument GetXmlDocument()
        {
            //WindowsPhone的Toast的样式实际只有这一种
            var toastXml = global::Windows.UI.Notifications.ToastNotificationManager.GetTemplateContent(global::Windows.UI.Notifications.ToastTemplateType.ToastText02);
            var toastTextElements = toastXml.GetElementsByTagName("text");
            if (toastTextElements.Count>0)
            {
                toastTextElements[0].AppendChild(toastXml.CreateTextNode(_title));
            }
            if (toastTextElements.Count>1)
            {
                toastTextElements[1].AppendChild(toastXml.CreateTextNode(_content));
            }
            var toastNode = toastXml.SelectSingleNode("/toast");
            if (toastNode != null)
            {
                var audio = toastXml.CreateElement("audio");
                switch (_auidoType)
                {
                    case ToastAuidoType.Silent:
                        audio.SetAttribute("silent", "true");
                        break;
                    default:
                        audio.SetAttribute("src", string.Format("ms-winsoundevent:Notification.{0}", Enum.GetName(typeof(ToastAuidoType), _auidoType)));
                        break;
                }
                toastNode.AppendChild(audio);
            }
            return toastXml;
        }

    }

}
