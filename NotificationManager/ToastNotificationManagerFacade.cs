namespace NotificationManager
{

    /// <summary>
    /// 外部程序集调用的接口
    /// </summary>
    public class ToastNotificationManagerFacade
    {

        private static IToastNotificationManager _instance;

        public static IToastNotificationManager GetInstanceForCurrentApplication()
        {
            if (_instance == null)
            {
                if (App.Library.Common.DeviceInfoHelper.GetCurrentPlatform() == global::Windows.Foundation.Metadata.Platform.WindowsPhone)
                {
                    _instance = new WindowsPhone.ToastNotificationManager();
                }
                else
                {
                    _instance = new Windows.ToastNotificationManager();
                }
            }
            return _instance;
        }

    }

}
