using System;
using System.IO;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.UI.Xaml;

namespace BarCode.Library
{

    public class BarCodeCameraContentReadyEventArgs : EventArgs
    {
        public Stream ImageStream { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        internal BarCodeCameraContentReadyEventArgs(Stream imageStream,int width, int height)
        {
            ImageStream = imageStream;
            Width = width;
            Height = height;
        }
    }

    public class BarCodeCamera : DependencyObject, IDisposable
    {

        private readonly MediaCapture _camera;

        private readonly DispatcherTimer _timer;

        private const string StartedState = "Started";
        private const string StoppedState = "Stopped";

        private string _state = StoppedState;

        public BarCodeCamera(MediaCapture camera)
        {
            if (camera == null)
            {
                throw new ArgumentNullException("camera");
            }
            _camera = camera;
            _timer = new DispatcherTimer();
            _timer.Tick += TryCapture;
        }

        private void TryCapture(object sender, object e)
        {
            Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,async () =>
                                                                                                                                {
                                                                                                                                    try
                                                                                                                                    {
                                                                                                                                        if (_camera != null && StartedState.Equals(_state))
                                                                                                                                        {
                                                                                                                                            var pic = await (await _camera.PrepareLowLagPhotoCaptureAsync(ImageEncodingProperties.CreateJpeg())).CaptureAsync();
                                                                                                                                            if (GetBuffImage != null)
                                                                                                                                            {
                                                                                                                                                GetBuffImage(_camera, new BarCodeCameraContentReadyEventArgs(pic.Thumbnail.AsStream(), (int) pic.Thumbnail.Width, (int) pic.Thumbnail.Height));
                                                                                                                                            }
                                                                                                                                        }
                                                                                                                                    }
                                                                                                                                    catch (Exception)
                                                                                                                                    {
                                                                                                                                        throw;
                                                                                                                                    }
                                                                                                                                });
        }

        public void Dispose()
        {
            _timer.Stop();
            if (_camera != null)
            {
                _camera.Dispose();
            }
        }

        public async void Start(TimeSpan interval)
        {
            await _camera.StartPreviewAsync();
            _timer.Stop();
            _timer.Interval = interval;
            //_timer.Start();
            _state = StartedState;
        }

        public async void Stop()
        {
            await _camera.StopPreviewAsync();
            _timer.Stop();
            _state = StoppedState;
        }

        public event EventHandler<BarCodeCameraContentReadyEventArgs> GetBuffImage;

    }

}
