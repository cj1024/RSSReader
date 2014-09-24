using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using RSSReader.Common;
using RSSReader.Library.Common;

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
            Refresh();
        }

        internal async void Refresh()
        {
            OnLoading = true;
            LoadFailed = false;
            if (Reader == null)
            {
                Reader = await RSSReaderFactory.GetRSSReaderInstance<Library.Common.RSSReader>(_feed);
                LoadFailed = Reader == null;
            }
            else
            {
                LoadFailed = !await RSSReaderFactory.RefreshRSSReader(Reader, _feed);
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

        public MainPageViewModel()
        {
            RSSFeeds.Add(new RSSFeed(new Uri("http://i.kamigami.org/feed", UriKind.Absolute), "诸神发布站"));
            RSSFeeds.Add(new RSSFeed(new Uri("http://www.ithome.com/rss/", UriKind.Absolute), "IT之家"));
        }

    }

}
