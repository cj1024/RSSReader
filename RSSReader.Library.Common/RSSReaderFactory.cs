using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Foundation.Metadata;
using Windows.Storage;

namespace RSSReader.Library.Common
{

    partial class RSSReaderFactory
    {

        private static readonly Windows.Security.Cryptography.Core.HashAlgorithmProvider SHA1 = Windows.Security.Cryptography.Core.HashAlgorithmProvider.OpenAlgorithm(Windows.Security.Cryptography.Core.HashAlgorithmNames.Sha1);

        private static string GetCaptureFileName(Uri source)
        {
            const string pattern = "\\\\|\\/|:|\\?|\\*|\"|<|>|\\|";
            var regex = new Regex(pattern);
            var evaluator = new MatchEvaluator(ConvertToLegalPath);
            var bytes = Encoding.UTF8.GetBytes(source.OriginalString);
            var str = Convert.ToBase64String(SHA1.HashData(WindowsRuntimeBuffer.Create(bytes, 0, bytes.Length, bytes.Length)).ToArray());
            return regex.Replace(str, evaluator);
        }

        static string ConvertToLegalPath(Match m)
        {
            return string.Format("${0}$", (int)m.Value[0]);
        }

    }
    
    public partial class RSSReaderFactory
    {

        const string FILENAME = "Feed.xml";

        private const string TOAST_NO_NETWORK = "RSSReaderFactory.RSSNoNetwork";

        static RSSReaderFactory()
        {
            NotificationManager.ToastNotificationManagerFacade.GetInstanceForCurrentApplication().ToastActivated += RSSReaderFactory_ToastActivated;
        }

        static void RSSReaderFactory_ToastActivated(Windows.UI.Notifications.ToastNotification sender, Windows.UI.Notifications.ToastActivatedEventArgs args)
        {
            if (TOAST_NO_NETWORK.Equals(NotificationManager.ToastNotificationManagerFacade.GetInstanceForCurrentApplication().GetToken(sender), StringComparison.OrdinalIgnoreCase))
            {
                if (App.Library.Common.DeviceInfoHelper.GetCurrentPlatform() == Platform.WindowsPhone)
                {
                    Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-wifi:"));
                }
            }
        }

        private static async Task<XmlDocument> GetRSSDocumentIgnoreCache(Uri rssFeed)
        {
            if (!App.Library.Common.DeviceInfoHelper.IsNetworkAvailable)
            {
                NotificationManager.ToastNotificationManagerFacade.GetInstanceForCurrentApplication().ShowToast("无网络", "请检查网络设置", delayInSecond: 0, token: TOAST_NO_NETWORK);
                return null;
            }
            try
            {
                return await XmlDocument.LoadFromUriAsync(rssFeed);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static async Task<XmlDocument> GetRSSDocument<T>(Uri rssFeed) where T : IRSSReader, new()
        {
            XmlDocument result;
            var folder = await ApplicationData.Current.TemporaryFolder.CreateFolderAsync(GetCaptureFileName(rssFeed), CreationCollisionOption.OpenIfExists);
            var file = (await folder.GetFilesAsync()).FirstOrDefault(f => f.Name.Equals(FILENAME, StringComparison.OrdinalIgnoreCase));
            if (file != null)
            {
                var tempResult = await XmlDocument.LoadFromFileAsync(file);
                var lastReader = new T();
                lastReader.InitializeWithXmlDocument(tempResult);
                if (lastReader.LastUpdateTime.HasValue && lastReader.UpdateTimeSpan.HasValue && lastReader.LastUpdateTime.Value.Add(lastReader.UpdateTimeSpan.Value) < DateTime.Now)
                {
                    result = tempResult;
                }
                else
                {
                    result = await GetRSSDocumentIgnoreCache(rssFeed) ?? tempResult;
                }
                await file.DeleteAsync();
            }
            else
            {
                try
                {
                    result = await GetRSSDocumentIgnoreCache(rssFeed);
                }
                catch (Exception)
                {
                    result = null;
                }
            }
            if (result != null)
            {
                file = await folder.CreateFileAsync(FILENAME);
                await result.SaveToFileAsync(file);
            }
            return result;
        }

        public static async Task<T> GetRSSReaderInstance<T>(Uri rssFeed) where T : IRSSReader, new ()
        {
            var doc = await GetRSSDocument<T>(rssFeed);
            if (doc == null)
            {
                return default(T);
            }
            var reader = Activator.CreateInstance<T>();
            reader.InitializeWithXmlDocument(doc);
            return reader;
        }

        public static async Task<bool> RefreshRSSReader<T>(IRSSReader instance, Uri rssFeed, bool ignoreCache) where  T : IRSSReader, new ()
        {
            var doc = ignoreCache? await GetRSSDocumentIgnoreCache(rssFeed) : await GetRSSDocument<T>(rssFeed);
            if (doc != null)
            {
                instance.InitializeWithXmlDocument(doc);
            }
            return false;
        }

    }

}
