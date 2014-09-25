using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Notifications;

namespace NotificationManager.Windows
{
    internal class ToastNotificationManager : IToastNotificationManager
    {
        public void ShowToast(string title, string content,ToastAuidoType auidoType, uint delayInSecond, uint repeat, uint repeatInterval, string token, string id)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<ScheduledToastNotification> GetScheduledToastNotifications()
        {
            throw new NotImplementedException();
        }

        public void RemoveScheduledToastNotification(ScheduledToastNotification notification)
        {
            throw new NotImplementedException();
        }

        public void RemoveScheduledToastNotifications(IList<ScheduledToastNotification> notifications)
        {
            throw new NotImplementedException();
        }

        public string GetToken(ToastNotification notification)
        {
            throw new NotImplementedException();
        }

        public string GetToken(ScheduledToastNotification notification)
        {
            throw new NotImplementedException();
        }

        public event TypedEventHandler<ToastNotification, ToastActivatedEventArgs> ToastActivated;

        public event TypedEventHandler<ToastNotification, ToastDismissedEventArgs> ToastDismissed;

        public event TypedEventHandler<ToastNotification, ToastFailedEventArgs> ToastFailed;
    }
}
