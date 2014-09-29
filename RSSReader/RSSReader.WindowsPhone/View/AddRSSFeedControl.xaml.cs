using System;
using Windows.Media.Capture;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using BarCode.Library;


// “用户控件”项模板在 http://go.microsoft.com/fwlink/?LinkId=234236 上提供
namespace RSSReader.View
{


    public sealed partial class AddRSSFeedControl
    {

        private readonly BarcodeDecodeManager _decodeManager = new BarcodeDecodeManager();
        private readonly Flyout Dialoug = new Flyout {Placement = FlyoutPlacementMode.Full};

        public AddRSSFeedControl()
        {
            InitializeComponent();
            Dialoug.Content = this;
            Dialoug.Closed += Dialoug_Closed;
            Loaded += AddRSSFeedControl_Loaded;
            Unloaded += AddRSSFeedControl_Unloaded;
        }

        void AddRSSFeedControl_Loaded(object sender, RoutedEventArgs e)
        {
            //if (CameraPreview.Visibility == Visibility.Visible)
            //{
            //    StartCammera();
            //}
        }

        void AddRSSFeedControl_Unloaded(object sender, RoutedEventArgs e)
        {
            //StopCammera();
        }

        private void UseBarCode_OnClick(object sender, RoutedEventArgs e)
        {
            //UseBarCode.Visibility = Visibility.Collapsed;
            //CameraPreview.Visibility = Visibility.Visible;
            StartCammera();
        }

        private BarCodeCamera _camera;

        private async void StartCammera()
        {
            //try
            //{
            //    var capture = new MediaCapture();
            //    await capture.InitializeAsync();
            //    CameraPreview.Source = capture;
            //    _camera = new BarCodeCamera(capture);
            //    _camera.GetBuffImage += Camera_GetBuffImage;
            //    StartCapture();
            //}
            //catch (Exception)
            //{
            //    (new MessageDialog(string.Format("开启摄像头失败"))).ShowAsync();
            //    UseBarCode.Visibility = Visibility.Visible;
            //    CameraPreview.Visibility = Visibility.Collapsed;
            //    StopCammera();
            //}
        }

        private void Camera_GetBuffImage(object sender, BarCodeCameraContentReadyEventArgs e)
        {
            var task = new BarcodeDecodeTask(e.ImageStream, e.Width, e.Height);
            task.Decoded += Task_Decoded;
            _decodeManager.AddTask(task);
        }

        private void StopCammera()
        {
            if (_camera != null)
            {
                _camera.Dispose();
            }
            StopCapture();
            _camera = null;
        }

        private void StartCapture()
        {
            if (_camera != null)
            {
               _camera.Start(TimeSpan.FromMilliseconds(100));
            }
        }

        private void StopCapture()
        {
            if (_camera != null)
            {
                _camera.Stop();
            }
            _decodeManager.ClearQueuedTask();
        }

        private void Task_Decoded(object sender, BarcodeDecodeResult e)
        {
            //if (e.Result == BarcodeDecodeResultType.Success)
            //{
            //    Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            //                                                       {
            //                                                           UseBarCode.Visibility = Visibility.Visible;
            //                                                           CameraPreview.Visibility = Visibility.Collapsed;
            //                                                           UrlTextBox.Text = e.Message;
            //                                                           StopCapture();
            //                                                       });
            //    ((BarcodeDecodeTask)sender).ImageStream.Dispose();
            //}
        }

        private async void Confirm_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Url))
            {
                await (new MessageDialog(string.Format("请填写{0}", UrlTextBox.Header))).ShowAsync();
            }
            Dialoug.Hide();
        }

        public void Show(FrameworkElement element)
        {
            Dialoug.ShowAt(element);
        }

        public string Title { get { return TitleTextBox.Text; } }

        public string Url { get { return UrlTextBox.Text; } }

        void Dialoug_Closed(object sender, object e)
        {
            if (Closed != null)
            {
                Closed.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler Closed;

    }
}
