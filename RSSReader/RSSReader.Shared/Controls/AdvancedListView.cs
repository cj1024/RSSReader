using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace RSSReader.Controls
{
    public class AdvancedListView : ListView
    {

        public AdvancedListView()
        {
            ItemClick += AdvancedListView_ItemClick;
        }

        void AdvancedListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (ItemClickedCommand != null)
            {
                ItemClickedCommand.Execute(e);
            }
        }

        public static readonly DependencyProperty ItemClickedCommandProperty = DependencyProperty.Register(
            "ItemClickedCommand", typeof (ICommand), typeof (AdvancedListView), new PropertyMetadata(default(ICommand)));

        public ICommand ItemClickedCommand
        {
            get { return (ICommand) GetValue(ItemClickedCommandProperty); }
            set { SetValue(ItemClickedCommandProperty, value); }
        }

    }
}
