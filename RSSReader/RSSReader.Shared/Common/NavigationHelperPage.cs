using Windows.UI.Xaml.Navigation;
using RSSReader.View;

namespace RSSReader.Common
{

    public abstract class NavigationHelperPage : PageBase
    {
        protected NavigationHelper NavigationHelper { get; private set; }

        protected NavigationHelperPage()
        {
            NavigationHelper = new NavigationHelper(this);
            NavigationHelper.LoadState += NavigationHelper_LoadState;
            NavigationHelper.SaveState += NavigationHelper_SaveState;
        }

        protected virtual void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        protected virtual void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }
        /// <summary>
        /// 在此页将要在 Frame 中显示时进行调用。
        /// </summary>
        /// <param name="e">描述如何访问此页的事件数据。
        /// 此参数通常用于配置页。</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            NavigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            NavigationHelper.OnNavigatedFrom(e);
        }

    }

}
