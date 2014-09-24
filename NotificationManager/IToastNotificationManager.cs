using System.Collections.Generic;

namespace NotificationManager
{

    /// <summary>
    /// Toast弹出时的声音
    /// </summary>
    public enum ToastAuidoType
    {
        Default,
        IM,
        Mail,
        Reminder,
        SMS,
        Silent
    }

    /// <summary>
    /// 提供程在前台和后台触发ToastNotification的接口
    /// </summary>
    public interface IToastNotificationManager
    {
        /// <summary>
        /// 显示一个ToastNotification
        /// </summary>
        /// <param name="title">粗体的标题</param>
        /// <param name="content">细体的内容</param>
        /// <param name="auidoType">Toast弹出时的声音</param>
        /// <param name="delayInSecond">延迟秒数，需大于0</param>
        /// <param name="repeat">如果用户忽略，则重复的次数，1-5次之间，0表示不重复</param>
        /// <param name="repeatInterval">每次重复的间隔秒数</param>
        /// <param name="token">ToastNotification的标识，长度在15位以内</param>
        void ShowToast(string title, string content, ToastAuidoType auidoType = ToastAuidoType.Default,uint delayInSecond = 1, uint repeat = 0, uint repeatInterval = 60, string token = null);

        /// <summary>
        /// 获取所有仍在发送队列中的ScheduledToastNotification
        /// </summary>
        /// <returns>所有仍在发送队列中的ScheduledToastNotification列表</returns>
        IReadOnlyList<global::Windows.UI.Notifications.ScheduledToastNotification> GetScheduledToastNotifications();

        /// <summary>
        /// 清除仍在发送队列中的ScheduledToastNotification
        /// </summary>
        /// <param name="notification">要清除的ScheduledToastNotification</param>
        void RemoveScheduledToastNotification(global::Windows.UI.Notifications.ScheduledToastNotification notification);

        /// <summary>
        /// 清除仍在发送队列中的ScheduledToastNotification
        /// </summary>
        /// <param name="notifications">要清除的ScheduledToastNotification列表</param>
        void RemoveScheduledToastNotifications(IList<global::Windows.UI.Notifications.ScheduledToastNotification> notifications);

        event global::Windows.Foundation.TypedEventHandler<global::Windows.UI.Notifications.ToastNotification, global::Windows.UI.Notifications.ToastActivatedEventArgs> ToastActivated;
        
        event global::Windows.Foundation.TypedEventHandler<global::Windows.UI.Notifications.ToastNotification, global::Windows.UI.Notifications.ToastDismissedEventArgs> ToastDismissed;
        
        event global::Windows.Foundation.TypedEventHandler<global::Windows.UI.Notifications.ToastNotification, global::Windows.UI.Notifications.ToastFailedEventArgs> ToastFailed;

    }
}
