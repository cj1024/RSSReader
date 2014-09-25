using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Data.Xml.Dom;

namespace NotificationManager.WindowsPhone
{

    /// <summary>
    /// 为WindowsPhone提供IToastNotificationManager接口封装
    /// </summary>
    internal sealed class ToastNotificationManager : IToastNotificationManager
    {

        private const string EXTRANODENAME = "extrainfo";
        private const string TOKENATTRIBUTENAME = "token";

        private global::Windows.UI.Notifications.ToastNotifier ToastNotifier
        {
            get { return global::Windows.UI.Notifications.ToastNotificationManager.CreateToastNotifier(); }
        }

        public void ShowToast(string title, string content,ToastAuidoType auidoType, uint delayInSecond, uint repeat, uint repeatInterval, string token, string id)
        {
            if (repeat > 5)
            {
                throw new ArgumentOutOfRangeException("repeat", repeat, "Should Be Less Than 5");
            }
            if (!string.IsNullOrEmpty(id) && id.Length > 15)
            {
                throw new ArgumentOutOfRangeException("id", id, "Should Be Less Than 15 Characters");
            }
            var toastDOM = new ToastNotificationContent(title, content, auidoType).GetXmlDocument();
            if (!string.IsNullOrEmpty(token))
            {
                var toastNode = toastDOM.SelectSingleNode("/toast");
                if (toastNode != null)
                {
                    var element = toastDOM.CreateElement(EXTRANODENAME);
                    element.SetAttribute(TOKENATTRIBUTENAME, token);
                    toastNode.AppendChild(element);
                }
            }
            try
            {
                if (delayInSecond == 0)
                {
                    var toast = new global::Windows.UI.Notifications.ToastNotification(toastDOM);
                    toast.Activated += Toast_Activated;
                    toast.Dismissed += Toast_Dismissed;
                    toast.Failed += Toast_Failed;
                    ToastNotifier.Show(toast);
                }
                else
                {
                    var dueTime = DateTimeOffset.Now.AddSeconds(delayInSecond);
                    global::Windows.UI.Notifications.ScheduledToastNotification toast = repeat > 0 ? new global::Windows.UI.Notifications.ScheduledToastNotification(toastDOM, dueTime, TimeSpan.FromSeconds(Math.Min(3600, Math.Max(60, repeatInterval))), repeat) : new global::Windows.UI.Notifications.ScheduledToastNotification(toastDOM, dueTime);
                    if (!string.IsNullOrEmpty(id))
                    {
                        toast.Id = id;
                    }
                    ToastNotifier.AddToSchedule(toast);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IReadOnlyList<global::Windows.UI.Notifications.ScheduledToastNotification> GetScheduledToastNotifications()
        {
            return ToastNotifier.GetScheduledToastNotifications();
        }

        public void RemoveScheduledToastNotification(global::Windows.UI.Notifications.ScheduledToastNotification notification)
        {
            if (notification!=null)
            {
                RemoveScheduledToastNotifications(new[] { notification });
            }
        }

        public void RemoveScheduledToastNotifications(IList<global::Windows.UI.Notifications.ScheduledToastNotification> notifications)
        {
            if (notifications != null && notifications.Any())
            {
                foreach (var notification in GetScheduledToastNotifications())
                {
                    if (notifications.Any(n => notification.Id == n.Id))
                    {
                        ToastNotifier.RemoveFromSchedule(notification);
                    }
                }
            }
        }

        private string GetToken(XmlDocument content)
        {
            var toastNode = content.SelectSingleNode("/toast");
            if (toastNode != null)
            {
                var extrainfonode = toastNode.ChildNodes.FirstOrDefault(node => EXTRANODENAME.Equals(node.NodeName, StringComparison.OrdinalIgnoreCase));
                if (extrainfonode != null)
                {
                    var tokenattribute = extrainfonode.Attributes.FirstOrDefault(attribute => TOKENATTRIBUTENAME.Equals(attribute.NodeName, StringComparison.OrdinalIgnoreCase));
                    if (tokenattribute != null)
                    {
                        return tokenattribute.InnerText;
                    }
                }
            }
            return null;
        }

        public string GetToken(global::Windows.UI.Notifications.ToastNotification notification)
        {
            return GetToken(notification.Content);
        }

        public string GetToken(global::Windows.UI.Notifications.ScheduledToastNotification notification)
        {
            return GetToken(notification.Content);
        }

        public event global::Windows.Foundation.TypedEventHandler<global::Windows.UI.Notifications.ToastNotification, global::Windows.UI.Notifications.ToastActivatedEventArgs> ToastActivated;

        public event global::Windows.Foundation.TypedEventHandler<global::Windows.UI.Notifications.ToastNotification, global::Windows.UI.Notifications.ToastDismissedEventArgs> ToastDismissed;

        public event global::Windows.Foundation.TypedEventHandler<global::Windows.UI.Notifications.ToastNotification, global::Windows.UI.Notifications.ToastFailedEventArgs> ToastFailed;

        void Toast_Failed(global::Windows.UI.Notifications.ToastNotification sender, global::Windows.UI.Notifications.ToastFailedEventArgs args)
        {
            if (ToastFailed!=null)
            {
                ToastFailed.Invoke(sender, args);
            }
        }

        void Toast_Dismissed(global::Windows.UI.Notifications.ToastNotification sender, global::Windows.UI.Notifications.ToastDismissedEventArgs args)
        {
            if (ToastDismissed != null)
            {
                ToastDismissed.Invoke(sender, args);
            }
        }

        void Toast_Activated(global::Windows.UI.Notifications.ToastNotification sender, object args)
        {
            if (ToastActivated!=null)
            {
                ToastActivated.Invoke(sender, args as global::Windows.UI.Notifications.ToastActivatedEventArgs);
            }
        }

    }

}
