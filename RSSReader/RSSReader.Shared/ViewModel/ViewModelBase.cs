using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using RSSReader.Common;

namespace RSSReader.ViewModel
{
    public abstract class ViewModelBase : NotificationObject
    {

        #region 页面导航

        internal Page Page { get; set; }

        internal virtual void OnNavigatedToPage(NavigationEventArgs e) { }
        internal virtual void OnNavigatingFromPage(NavigatingCancelEventArgs e) { }
        internal virtual void OnNavigatedFromPage(NavigationEventArgs e) { }

        #endregion 页面导航

        #region 墓碑化处理

        /// <summary>
        /// 墓碑化处理
        /// </summary>
        internal abstract void NavigationHelper_SaveState(object sender, SaveStateEventArgs e);
        /// <summary>
        /// 反墓碑化处理
        /// </summary>
        internal abstract void NavigationHelper_LoadState(object sender, LoadStateEventArgs e);

        #endregion 墓碑化处理

    }

}
