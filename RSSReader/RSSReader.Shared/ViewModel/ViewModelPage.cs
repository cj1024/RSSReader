using RSSReader.Common;

namespace RSSReader.ViewModel
{
    public abstract class ViewModelPage : NavigationHelperPage
    {

        protected ViewModelBase ViewModel { get { return DataContext as ViewModelBase; } }

        protected override void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            base.NavigationHelper_SaveState(sender, e);
            if (ViewModel != null)
            {
                ViewModel.NavigationHelper_SaveState(sender, e);
            }
        }

        protected override void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            base.NavigationHelper_LoadState(sender, e);
            if (ViewModel != null)
            {
                ViewModel.NavigationHelper_LoadState(sender, e);
            }
        }

    }
}
