using RSSReader.Common;

namespace RSSReader.ViewModel
{
    public abstract class ViewModelBase : NotificationObject
    {
        /// <summary>
        /// 墓碑化处理
        /// </summary>
        internal abstract void NavigationHelper_SaveState(object sender, SaveStateEventArgs e);
        /// <summary>
        /// 反墓碑化处理
        /// </summary>
        internal abstract void NavigationHelper_LoadState(object sender, LoadStateEventArgs e);
    }

}
