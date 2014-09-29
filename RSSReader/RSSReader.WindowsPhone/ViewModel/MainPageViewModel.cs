using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using RSSReader.Common;
using RSSReader.Library.Common;
using RSSReader.View;

namespace RSSReader.ViewModel
{

    public class RSSFeed : NotificationObject
    {

        private readonly Uri _feed;

        private readonly string _defaultHeader;

        public RSSFeed(Uri feed, string defaultHeader)
        {
            _feed = feed;
            _defaultHeader = defaultHeader;
            ItemClickedCommand = new RelayCommand<ItemClickEventArgs>(ItemClick);
            Refresh();
        }

        void ItemClick(ItemClickEventArgs e)
        {
            var item = e.ClickedItem as RSSItem;
            if (item != null)
            {
                Windows.System.Launcher.LaunchUriAsync(new Uri(item.Link, UriKind.Absolute));
            }
        }

        internal async void Refresh()
        {
            if (OnLoading)
            {
                return;
            }
            OnLoading = true;
            LoadFailed = false;
            if (Reader == null)
            {
                Reader = await RSSReaderFactory.GetRSSReaderInstance<Library.Common.RSSReader>(_feed);
                LoadFailed = Reader == null;
            }
            else
            {
                LoadFailed = !await RSSReaderFactory.RefreshRSSReader<Library.Common.RSSReader>(Reader, _feed, false);
            }
            OnLoading = false;
            RaisePropertyChanged(() => Header);
            RaisePropertyChanged(() => ListItems);
        }

        private bool _onLoading;

        public bool OnLoading
        {
            get { return _onLoading; }
            set
            {
                _onLoading = value;
                RaisePropertyChanged(() => OnLoading);
                if (OnLoadingChanged != null)
                {
                    OnLoadingChanged.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private bool _loadFailed;

        public bool LoadFailed
        {
            get
            {
                return _loadFailed;
            }
            set
            {
                _loadFailed = value;
                RaisePropertyChanged(() => LoadFailed);
            }
        }

        private IRSSReader _reader;

        public IRSSReader Reader
        {
            get
            {
                return _reader;
            }
            private set
            {
                _reader = value;
                RaisePropertyChanged(()=>Reader);
            }
        }

        public string Header
        {
            get { return Reader == null || string.IsNullOrEmpty(Reader.Title) ? _defaultHeader : Reader.Title; }
        }

        public IList<RSSItem> ListItems
        {
            get { return Reader == null ? new List<RSSItem>() : Reader.ItemList.ToList(); }
        }

        public ICommand ItemClickedCommand { get; private set; }

        public event EventHandler OnLoadingChanged;

    }

    public partial class MainPageViewModel : ViewModelBase
    {

        internal override void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        internal override void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

    }

    partial class MainPageViewModel
    {

        private readonly ObservableCollection<RSSFeed> _rssFeeds = new ObservableCollection<RSSFeed>();

        public ObservableCollection<RSSFeed> RSSFeeds { get { return _rssFeeds; } }

        #region Commands

        public ICommand AddCommand { get; private set; }

        public ICommand RefreshCommand { get; private set; }

        public ICommand DeleteCommand { get; private set; }

        #endregion

        #region Properties
        
        private bool _canAdd = true;

        public bool CanAdd
        {
            get
            {
                return _canAdd;
            }
            set
            {
                _canAdd = value;
                RaisePropertyChanged(() => CanAdd);
            }
        }

        public bool CanRefresh
        {
            get
            {
                return RSSFeeds.Count > SelectedIndex && !RSSFeeds[SelectedIndex].OnLoading;
            }
        }

        public bool CanDelete
        {
            get
            {
                return RSSFeeds.Count > SelectedIndex && !RSSFeeds[SelectedIndex].OnLoading;
            }
        }
        
        private int _selectedIndex;

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                _selectedIndex = value;
                RaisePropertyChanged(() => SelectedIndex);
                RefreshCurrentRSSFeed();
            }
        }

        #endregion

        public MainPageViewModel()
        {
            AddCommand = new RelayCommand(AddRSSFeed);
            RefreshCommand = new RelayCommand(RefreshCurrentRSSFeed);
            DeleteCommand = new RelayCommand(DeleteCurrentRSSFeed);
            LoadRSSFeeds();
        }

        internal override void OnNavigatedToPage(NavigationEventArgs e)
        {
            base.OnNavigatedToPage(e);
            RaisePropertyChanged(() => CanRefresh);
        }

        void CurrentItemLoaded()
        {
            RaisePropertyChanged(() => CanRefresh);
            RaisePropertyChanged(() => CanDelete);
        }

        void LoadRSSFeeds()
        {
            if (RSSFeedsManager.GetInstance().Feeds.Count == 0)
            {
                RSSFeedsManager.GetInstance().AddFeed(new Common.RSSFeed("http://www.ithome.com/rss/", "IT之家"));
            }
            foreach (var rssFeed in RSSFeedsManager.GetInstance().Feeds)
            {
                RSSFeeds.Add(new RSSFeed(new Uri(rssFeed.Link, UriKind.Absolute), rssFeed.Title));
            }
            foreach (var rssFeed in RSSFeeds)
            {
                rssFeed.OnLoadingChanged -= RSSFeed_OnLoadingChanged;
                rssFeed.OnLoadingChanged += RSSFeed_OnLoadingChanged;
            }
        }

        void RSSFeed_OnLoadingChanged(object sender, EventArgs e)
        {
            CurrentItemLoaded();
        }

        void AddRSSFeed()
        {
            if (Page.BottomAppBar != null)
            {
                Page.BottomAppBar.Visibility = Visibility.Collapsed;
            }
            var chooser = new AddRSSFeedControl();
            chooser.Closed += (sender, e) =>
                              {
                                  if (!string.IsNullOrEmpty(chooser.Url))
                                  {
                                      var itemToAdd = new Common.RSSFeed(chooser.Url, chooser.Title);
                                      if (RSSFeedsManager.GetInstance().AddFeed(itemToAdd))
                                      {
                                          try
                                          {
                                              RSSFeeds.Add(new RSSFeed(new Uri(itemToAdd.Link, UriKind.Absolute), itemToAdd.Title));
                                          }
                                          catch (Exception)
                                          {
                                              RSSFeedsManager.GetInstance().RemoveFeed(itemToAdd);
                                          }
                                      }
                                  }
                                  if (Page.BottomAppBar != null)
                                  {
                                      Page.BottomAppBar.Visibility = Visibility.Visible;
                                  }
                              };
            chooser.Show(Page);
        }

        void RefreshCurrentRSSFeed()
        {
            if (RSSFeeds.Count > SelectedIndex)
            {
                RSSFeeds[SelectedIndex].Refresh();
            }
            CurrentItemLoaded();
        }

        void DeleteCurrentRSSFeed()
        {
            if (RSSFeeds.Count > SelectedIndex)
            {
                var item = RSSFeeds[SelectedIndex];
                RSSFeeds.Remove(item);
                RSSFeedsManager.GetInstance().RemoveFeed(new Common.RSSFeed(item.Reader.Link, item.Reader.Title));
            }
            CurrentItemLoaded();
        }

    }

}
