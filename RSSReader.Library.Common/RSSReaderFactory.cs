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

    public class RSSReaderFactoryRefreshResult
    {
        public bool IsSuccessful { get; set; }
        public bool IsCache { get; set; }
    }

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

        private class RSSReaderXmlDocument
        {
            internal XmlDocument Document { get; set; }
            internal bool IsCache { get; set; }
        }

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

        private static async Task<RSSReaderXmlDocument> GetRSSDocumentIgnoreCache(Uri rssFeed)
        {
            if (!App.Library.Common.DeviceInfoHelper.IsNetworkAvailable)
            {
                NotificationManager.ToastNotificationManagerFacade.GetInstanceForCurrentApplication().ShowToast("无网络", "请检查网络设置", delayInSecond: 0, token: TOAST_NO_NETWORK);
                return null;
            }
            try
            {
                return new RSSReaderXmlDocument {Document = await XmlDocument.LoadFromUriAsync(rssFeed), IsCache = true};
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static async Task<RSSReaderXmlDocument> GetRSSDocument<T>(Uri rssFeed) where T : IRSSReader, new()
        {
            XmlDocument result;
            bool isCache;
            var folder = await ApplicationData.Current.TemporaryFolder.CreateFolderAsync(GetCaptureFileName(rssFeed), CreationCollisionOption.OpenIfExists);
            var file = (await folder.GetFilesAsync()).FirstOrDefault(f => f.Name.Equals(FILENAME, StringComparison.OrdinalIgnoreCase));
            if (file != null)
            {
                var tempResult = await XmlDocument.LoadFromFileAsync(file);
                var lastReader = new T();
                lastReader.InitializeWithXmlDocument(tempResult);
                if (lastReader.LastUpdateTime.HasValue && lastReader.UpdateTimeSpan.HasValue)
                {
                    if (lastReader.LastUpdateTime.Value.Add(lastReader.UpdateTimeSpan.Value).ToLocalTime() > DateTime.Now)
                    {
                        result = tempResult;
                        isCache = true;
                    }
                    else
                    {
                        var realResult = await GetRSSDocumentIgnoreCache(rssFeed);
                        if (realResult != null)
                        {
                            result = realResult.Document;
                            isCache = false;
                            await file.DeleteAsync();
                            file = null;
                        }
                        else
                        {
                            result = tempResult;
                            isCache = true;
                        }
                    }
                }
                else if (file.DateCreated.AddMinutes(15).LocalDateTime > DateTime.Now)
                {
                    result = tempResult;
                    isCache = true;
                }
                else
                {
                    var realResult = await GetRSSDocumentIgnoreCache(rssFeed);
                    if (realResult != null)
                    {
                        result = realResult.Document;
                        isCache = false;
                        await file.DeleteAsync();
                        file = null;
                    }
                    else
                    {
                        result = tempResult;
                        isCache = true;
                    }
                }
            }
            else
            {
                try
                {
                    result = (await GetRSSDocumentIgnoreCache(rssFeed)).Document;
                }
                catch (Exception)
                {
                    result = null;
                }
                isCache = false;
            }
            if (file == null && result != null)
            {
                file = await folder.CreateFileAsync(FILENAME);
                await result.SaveToFileAsync(file);
            }
            return new RSSReaderXmlDocument {Document = result, IsCache = isCache};
        }

        public static async Task<T> GetRSSReaderInstance<T>(Uri rssFeed) where T : IRSSReader, new ()
        {
            var doc = await GetRSSDocument<T>(rssFeed);
            if (doc == null)
            {
                return default(T);
            }
            var reader = Activator.CreateInstance<T>();
            reader.InitializeWithXmlDocument(doc.Document);
            return reader;
        }

        public static async Task<RSSReaderFactoryRefreshResult> RefreshRSSReader<T>(IRSSReader instance, Uri rssFeed, bool ignoreCache) where T : IRSSReader, new()
        {
            var doc = ignoreCache ? await GetRSSDocumentIgnoreCache(rssFeed) : await GetRSSDocument<T>(rssFeed);
            if (doc != null)
            {
                instance.InitializeWithXmlDocument(doc.Document);
            }
            return new RSSReaderFactoryRefreshResult {IsSuccessful = doc != null, IsCache = doc != null && doc.IsCache};
        }

    }

}
