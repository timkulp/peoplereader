using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Media.Capture;
using Windows.Storage;
using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Emotion.Contract;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.Media.MediaProperties;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace peoplereader
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        DispatcherTimer timer = new DispatcherTimer();
        MediaCapture mediaCapture;

        public MainPage()
        {
            this.InitializeComponent();
            mediaCapture = new MediaCapture();
            mediaCapture.Failed += MediaCapture_Failed;

            timer.Interval = new TimeSpan(0, 0, 10);
            timer.Tick += Timer_Tick;
        }

        private void MediaCapture_Failed(MediaCapture sender, MediaCaptureFailedEventArgs errorEventArgs)
        {
            throw new NotImplementedException();
        }

        private async void Timer_Tick(object sender, object e)
        {
            timer.Stop();
            await mediaCapture.InitializeAsync();
            var lowLagCapture = await mediaCapture.PrepareLowLagPhotoCaptureAsync(ImageEncodingProperties.CreateJpeg());
            var capturedPhoto = await lowLagCapture.CaptureAsync();

            EmotionServiceClient emotionServiceClient = new EmotionServiceClient("638268f88e154b588fabdf134ee3e63e");
            //SoftwareBitmap softwareBitmap = capturedPhoto.Frame.SoftwareBitmap;
            //SoftwareBitmap softwareBitmapBGR8 = SoftwareBitmap.Convert(softwareBitmap,
            //    BitmapPixelFormat.Bgra8,
            //    BitmapAlphaMode.Premultiplied);

            //SoftwareBitmapSource bitmapSource = new SoftwareBitmapSource();
            //await bitmapSource.SetBitmapAsync(softwareBitmapBGR8);

            //imgCaptured.Source = bitmapSource;

            Emotion[] emotionResult = await emotionServiceClient.RecognizeAsync(capturedPhoto.Frame.AsStreamForRead());
            processEmotion(emotionResult);

            await lowLagCapture.FinishAsync();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            // Start capturing images every 10 seconds and processing the emotion of the image
            timer.Start();

        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            // Kill the 10 second loop
            timer.Stop();
        }

        private void processEmotion(Emotion[] emotions)
        {
            // draw rectangles on the image
            foreach(var emotion in emotions)
            {
                Border b = new Border();
                b.Height = emotion.FaceRectangle.Height;
                b.Width = emotion.FaceRectangle.Width;
                b.SetValue(Canvas.TopProperty, emotion.FaceRectangle.Top);
                 b.SetValue(Canvas.LeftProperty, emotion.FaceRectangle.Left);
                var feelings = emotion.Scores.ToRankedList();
                var emotionName = feelings.FirstOrDefault().Key.ToLower();
                switch (emotionName)
                {
                    case "happiness":
                        b.Style = (Style)this.Resources["happyStyle"];
                        break;
                    case "sadness":
                        b.Style = (Style)this.Resources["sadStyle"];
                        break;
                    case "anger":
                        b.Style = (Style)this.Resources["angerStyle"];
                        break;
                    case "fear":
                        b.Style = (Style)this.Resources["fearStyle"];
                        break;
                    case "disgust":
                        b.Style = (Style)this.Resources["disgustStyle"];
                        break;
                    default:
                        b.Style = (Style)this.Resources["neutralStyle"];
                        break;
                }


            }
        }

    }
}
