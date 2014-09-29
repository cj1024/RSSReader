﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.System.Threading;
using Windows.UI.Xaml.Media.Imaging;
using com.google.zxing;
using com.google.zxing.common;
using com.google.zxing.qrcode;

namespace BarCode.Library
{

    public enum BarcodeDecodeResultType
    {
        /// <summary>
        /// 未知异常
        /// </summary>
        Unknown,
        /// <summary>
        /// 探测失败
        /// </summary>
        NotDetected,
        /// <summary>
        /// 解析失败
        /// </summary>
        Fail,
        /// <summary>
        /// 成功
        /// </summary>
        Success
    }

    public class BarcodeDecodeResult : EventArgs
    {

        private readonly BarcodeDecodeResultType _result;

        public BarcodeDecodeResultType Result { get { return _result; } }

        private readonly string _message;

        public string Message { get { return _message; } }

        public BarcodeDecodeResult(BarcodeDecodeResultType result, string message)
        {
            _result = result;
            _message = message;
        }

    }

    public class BarcodeDecodeTask
    {

        private readonly Stream _imageStream;
        private readonly int _width, _height;
        public Stream ImageStream { get { return _imageStream; } }
        public int Width { get { return _width; } }
        public int Height { get { return _height; } }
        public BarcodeDecodeTask(Stream imageStream,int witdh, int height)
        {
            _imageStream = imageStream;
            _width = witdh;
            _height = height;
        }

        public event EventHandler<BarcodeDecodeResult> Decoded;

        internal void HandleResult(BarcodeDecodeResult result)
        {
            if (Decoded != null)
            {
                Decoded.Invoke(this, result);
            }
        }

    }

    public class BarcodeDecodeManager
    {

        public const int MinPictureWidth = 480;
        public const int MinPictureHeight = 640;

        private const int MAX_THREAD = 2;

        private const int MAX_Task = 10;

        private readonly Queue<BarcodeDecodeTask> _decodeTasksQueue = new Queue<BarcodeDecodeTask>(MAX_Task);

        private readonly Queue<Reader> _availableReader = new Queue<Reader>(MAX_THREAD);

        public BarcodeDecodeManager()
        {
            for (int i = 0; i < MAX_THREAD; i++)
            {
                _availableReader.Enqueue(new QRCodeReader());
            }
        }

        public bool IsFree
        {
            get { return _decodeTasksQueue.Count <= MAX_Task; }
        }

        public void AddTask(BarcodeDecodeTask task)
        {
            _decodeTasksQueue.Enqueue(task);
            TryStartTask();
        }

        void TryStartTask()
        {
            if (_availableReader.Count > 0)
            {
                var reader = _availableReader.Dequeue();
                if (_decodeTasksQueue.Count > 0)
                {
                    var task = _decodeTasksQueue.Dequeue();
                    Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => RealStartTask(reader, task));
                }
                else
                {
                    _availableReader.Enqueue(reader);
                    return;
                }
            }
            else
            {
                return;
            }
            TryStartTask();
        }

        private void RealStartTask(Reader reader, BarcodeDecodeTask task)
        {
            var bitmap = new WriteableBitmap(task.Width, task.Height);
            bitmap.SetSource(task.ImageStream.AsRandomAccessStream());
            ThreadPool.RunAsync(p =>
                                {
                                    var result = BarcodeDecodeResultType.Unknown;
                                    var message = string.Empty;
                                    try
                                    {
                                        var wb = bitmap;
                                        //Code from: btnReadTag_Click in "SLZXingQRSample\SLZXingQRSample\SLZXingSample\MainPage.xaml.vb"
                                        var qrRead = reader; // new com.google.zxing.qrcode.QRCodeReader();
                                        var luminiance = new RGBLuminanceSource(wb.PixelBuffer.ToArray(), wb.PixelWidth, wb.PixelHeight);
                                        var binarizer = new HybridBinarizer(luminiance);
                                        var binBitmap = new BinaryBitmap(binarizer);
                                        var results = qrRead.decode(binBitmap); //NOTE: will throw exception if cannot decode image.
                                        if (results == null)
                                        {
                                            result = BarcodeDecodeResultType.NotDetected;
                                            message = "Error: Cannot detect barcode image. Please make sure scan mode is correct and try again.";
                                        }
                                        else
                                        {
                                            result = BarcodeDecodeResultType.Success;
                                            message = results.Text;
                                        }
                                    }
                                    catch (ReaderException)
                                    {
                                        result = BarcodeDecodeResultType.Fail;
                                        message = "Error: Cannot decode barcode image. Please make sure scan mode is correct and try again.";
                                    }
                                    catch (Exception ex)
                                    {
                                        result = BarcodeDecodeResultType.Unknown;
                                        message = String.Format("Barcode Library Processing Error: {0}\r\n{1}", ex.GetType(), ex.Message);
                                    }
                                    finally
                                    {
                                        task.HandleResult(new BarcodeDecodeResult(result, message));
                                        _availableReader.Enqueue(reader);
                                        TryStartTask();
                                    }
                                });
        }

        public void ClearQueuedTask()
        {
            _decodeTasksQueue.Clear();
        }

    }

}
