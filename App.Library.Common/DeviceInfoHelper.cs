namespace App.Library.Common
{
    public class DeviceInfoHelper
    {

        private static Windows.Foundation.Metadata.Platform? _platform;

        public static Windows.Foundation.Metadata.Platform GetCurrentPlatform()
        {
            if (_platform == null)
            {
                _platform = Windows.ApplicationModel.Package.Current.InstalledLocation.Path.Contains("\\Data\\SharedData\\PhoneTools") ? Windows.Foundation.Metadata.Platform.WindowsPhone : Windows.Foundation.Metadata.Platform.Windows;
            }
            return _platform.Value;
        }

        public static bool IsNetworkAvailable
        {
            get { return System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();  }
        }

    }
}
