using Windows.Data.Xml.Dom;

namespace NotificationManager
{
    
    internal interface INotificationContent
    {

        string GetXmlString();

        XmlDocument GetXmlDocument();

    }

}
