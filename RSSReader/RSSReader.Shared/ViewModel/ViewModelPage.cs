using Windows.UI.Xaml.Navigation;
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (ViewModel != null)
            {
                ViewModel.Page = this;
                ViewModel.OnNavigatedToPage(e);
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            if (ViewModel != null)
            {
                ViewModel.OnNavigatingFromPage(e);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (ViewModel != null)
            {
                ViewModel.OnNavigatedFromPage(e);
            }
        }

    }
}
