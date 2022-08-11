using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using FaceTrackingWpfSample;
using IDScanNet.Faces.SDK;
using IDScanNet.Faces.SDK.Video;
using Microsoft.WindowsAPICodePack.Dialogs;
using OpenCvSharp;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Point = OpenCvSharp.Point;
using Rect = OpenCvSharp.Rect;

namespace VideoSample
{
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            _videoSourceProcessor?.Dispose();

            await Task.Delay(250);
            _renderMat1?.Dispose();
            _videoSourceProcessor?.Dispose();
            _templatesIndex?.Dispose();
            if (_service is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            EmotionsCheckbox.IsChecked = true;
            AgeCheckbox.IsChecked = true;
        }

        private bool InitializeSdk()
        {
            try
            {
                var settings = new Settings
                {
                    SdkDataDirectoryPath = _dataPath,
                    License = File.ReadAllBytes(_licensePath),
                    RecognitionPreset = RecognitionPreset.Realtime
                };
                _service = new FaceService(settings);

                OnInitialized(this, EventArgs.Empty);

                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Sdk initialization failed. {e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return false;
        }

        private void OnInitialized(object sender, EventArgs eventArgs)
        {
            Dispatcher.Invoke(() =>
            {
                //enable buttons when sdk initialized
                ReadyText.Text = "SDK Ready";
                ButtonsPanel.IsEnabled = true;
                StartVideo.IsEnabled = true;
            });            
        }

        private async void EnrollFileButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.CheckFileExists = true;
            var result = dialog.ShowDialog(this);

            if (result == true)
            {
                try
                {
                    var path = dialog.FileName;
                    var bytes = File.ReadAllBytes(path);

                    var sample = await _service.DetectSingleAsync(bytes);
                    var template = await _service.ComputeTemplateAsync(sample);
                    template.SetCustomData($"{Path.GetFileNameWithoutExtension(path)}");
                    template.Tag = $"Created: {DateTime.Now:MM/dd/yyyy}";
                    
                    if (_templatesIndex == null)
                    {
                        _templatesIndex = await _service.CreateIndexAsync(new List<ITemplate>{template});
                        _videoSourceProcessor.SetTemplatesIndex(_templatesIndex);
                    }
                    else
                    {
                        _templatesIndex.Add(template);
                        await _service.UpdateIndexAsync(_templatesIndex);
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void MatchButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.CheckFileExists = true;
            var result = dialog.ShowDialog(this);

            if (result == true)
            {
                var path = dialog.FileName;
                var bytes = File.ReadAllBytes(path);

                var sample1 = await _service.DetectSingleAsync(bytes);
                var template = await _service.ComputeTemplateAsync(sample1);
                var searchResult = await _service.SearchAsync(template, _templatesIndex);
                
                // match score - can be adjusted to suit different use cases
                if (searchResult.MatchResult.Score >= 0.7f)
                {
                    MessageBox.Show($"Matched with template Id # {searchResult.MatchedTemplate.Guid} {searchResult.ConfidencePercent:F} %.", "Search result");
                }
                else if (searchResult.MatchResult.Score < 0.75f && searchResult.MatchResult.Score <= 0.25f)
                {
                    MessageBox.Show($"Possible match with template Id # {searchResult.MatchedTemplate.Guid} {searchResult.ConfidencePercent:F} %.", "Search result");
                }
                else
                {
                    MessageBox.Show($"No match", "Match result");
                }
            }
        }
        
        private IFaceService _service;
        private IVideoProcessor _videoSourceProcessor;
        private Mat _renderMat1;
        private string _licensePath;
        private string _dataPath;
        private ITemplatesIndex _templatesIndex;
        private CancellationTokenSource _cancellationTokenSource;

        private async void CompareButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "Select first image";
            dialog.CheckFileExists = true;
            var result = dialog.ShowDialog(this);

            byte[] bytes1 = null;
            if (result == true)
            {
                var path = dialog.FileName;
                bytes1 = File.ReadAllBytes(path);
            }


            dialog.Title = "Select second image";
            result = dialog.ShowDialog(this);

            byte[] bytes2 = null;
            if (result == true)
            {
                var path = dialog.FileName;
                bytes2 = File.ReadAllBytes(path);
            }

            var sample1 = await _service.DetectSingleAsync(bytes1);
            var sample2 = await _service.DetectSingleAsync(bytes2);
            var template1 = await _service.ComputeTemplateAsync(sample1);
            var template2 = await _service.ComputeTemplateAsync(sample2);
            var matchResult = await _service.MatchAsync(template1, template2);

            MessageBox.Show($"Match: {matchResult.Score*100f:F} %.");
        }

        private async void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (_templatesIndex != null)
            {
                _templatesIndex.DeleteAll();
                await _service.UpdateIndexAsync(_templatesIndex);
            }
        }

        private async void StartVideo_OnClick(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            StartVideo.IsEnabled = false;

            var isAnalyzeEmotions = EmotionsCheckbox.IsChecked.HasValue && EmotionsCheckbox.IsChecked.Value;
            var isAnalyzeAgeGender = AgeCheckbox.IsChecked.HasValue && AgeCheckbox.IsChecked.Value;

            if (_videoSourceProcessor == null)
            {
                _videoSourceProcessor = await _service.CreateVideoProcessorAsync("processor");
                _videoSourceProcessor.ExceptionReceived += (o, exception) =>
                {
                    //handle possible exceptions here
                };
                _videoSourceProcessor.TrackUpdated += (o, details) =>
                {
                    //track updated
                };
                _videoSourceProcessor.TrackStatusChanged += (o, details) =>
                {
                    //track status changed
                };

                _videoSourceProcessor.FrameProcessed += (o, result) =>
                {
                    // frame processed
                };

                // you can set your own events handler
                // _videoSourceProcessor.SetEventsHandlerAsync(new CustomEventsHandler());
            }
            
            if (_templatesIndex != null)
            {
                _videoSourceProcessor.SetTemplatesIndex(_templatesIndex);
            }
            
            var thread1 = new Thread(async () =>
            {
                VideoCapture capture = default;
                try
                {
                    // first available webcam 
                    capture = new VideoCapture(0);
                    // or path to the video file
                    // capture = new VideoCapture("video.mp4");

                    int sleepTime = (int)Math.Round(1000 / 60f);

                    var current_width = capture.Get(VideoCaptureProperties.FrameWidth);
                    var current_height = capture.Get(VideoCaptureProperties.FrameHeight);

                    capture.Set(VideoCaptureProperties.FrameWidth, 10000);
                    capture.Set(VideoCaptureProperties.FrameHeight, 10000);

                    var max_width = capture.Get(VideoCaptureProperties.FrameWidth);
                    var max_height = capture.Get(VideoCaptureProperties.FrameHeight);

                    capture.Set(VideoCaptureProperties.FrameWidth, 1280);
                    capture.Set(VideoCaptureProperties.FrameHeight, 720);

                    var width = capture.FrameWidth;
                    var height = capture.FrameHeight;

                    var current_width1 = capture.Get(VideoCaptureProperties.FrameWidth);
                    var current_height1 = capture.Get(VideoCaptureProperties.FrameHeight);

                    await _videoSourceProcessor.ResetAsync();
                    await _videoSourceProcessor.SetResolutionAsync(width, height);

                    WriteableBitmap _writeableBitmap = null;
                    Dispatcher.Invoke(() =>
                    {
                        _writeableBitmap = new WriteableBitmap(
                            width,
                            height,
                            96,
                            96,
                            PixelFormats.Bgr24,
                            null);
                        Draws.Source = _writeableBitmap;
                    });

                    _videoSourceProcessor.Settings.IsAnalyzeEmotions = isAnalyzeEmotions;
                    _videoSourceProcessor.Settings.IsAnalyzeAgeGender = isAnalyzeAgeGender;



                    _videoSourceProcessor.Settings.IsAnalyzeRealFace = true;
                    _videoSourceProcessor.Settings.IsProcessTemplates = true;

                    // Frame image buffer
                    //_renderWindow1 = new Window("Handler1");
                    _renderMat1 = new Mat();

                    int bitsPerPixel = 24;
                    var bytesPerPixel = (bitsPerPixel + 7) / 8;
                    var stride = bytesPerPixel * width;
                    var length = height * stride;

                    // When the movie playback reaches end, Mat.data becomes NULL.
                    while (!_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        capture.Read(_renderMat1); // same as cvQueryFrame
                        if (_renderMat1.Empty())
                        {
                            break;
                        }

                        _videoSourceProcessor.AddFrame(_renderMat1.Data);

                        var tracks = await _videoSourceProcessor.GetActiveTracksAsync();
                        foreach (var track in tracks)
                        {
                            var color = track.IsRecognized ? Scalar.GreenYellow :
                                !track.ProcessingDetails.IsDetected ? Scalar.Gray : Scalar.Crimson;

                            var rectangle = track.TrackingDetails.Rectangle;

                            _renderMat1.PutText(
                                $"Tests: A:{(track.ProcessingDetails.IsFineAngles ? "+" : "-")}  E:{(!track.ProcessingDetails.IsMainSnapshotLowQuality ?? false ? "+" : "-")}  D:{(track.ProcessingDetails.IsDetected ? "+" : "-")}",
                                new Point(rectangle.X + 2, rectangle.Y + 2),
                                HersheyFonts.HersheyPlain, 0.8f, Scalar.WhiteSmoke);

                            _renderMat1.Rectangle(
                                new Rect(rectangle.X, rectangle.Y, rectangle.Width,
                                    rectangle.Height),
                                color);
                            _renderMat1.PutText($"Id: {track.Id}",
                                new Point(rectangle.X + rectangle.Width, rectangle.Y + 20),
                                HersheyFonts.HersheyPlain, 0.8f, Scalar.WhiteSmoke);
                            _renderMat1.PutText($"Q: {track.ProcessingDetails.FaceQuality}",
                                new Point(rectangle.X + rectangle.Width, rectangle.Y + 40),
                                HersheyFonts.HersheyPlain, 0.8f, Scalar.WhiteSmoke);

                            if (track.IsRecognized)
                            {
                                _renderMat1.PutText(
                                    $"C:  {track.SearchResult?.MatchedTemplate?.GetCustomData<string>()}  {track.SearchResult?.ConfidencePercent}",
                                    new Point(rectangle.X + rectangle.Width, rectangle.Y + 60),
                                    HersheyFonts.HersheyPlain, 0.8f, Scalar.WhiteSmoke);
                            }

                            _renderMat1.PutText($"L: {track.ProcessingDetails.RealFaceState}",
                                new Point(rectangle.X + rectangle.Width, rectangle.Y + 80),
                                HersheyFonts.HersheyPlain, 0.8f, Scalar.WhiteSmoke);

                            _renderMat1.PutText($"G: {track.ProcessingDetails.Gender}",
                                new Point(rectangle.X + rectangle.Width, rectangle.Y + 100),
                                HersheyFonts.HersheyPlain, 0.8f, Scalar.WhiteSmoke);

                            _renderMat1.PutText($"AG: {track.ProcessingDetails.AgeGender?.AgeInfo?.Age}",
                                new Point(rectangle.X + rectangle.Width, rectangle.Y + 120),
                                HersheyFonts.HersheyPlain, 0.8f, Scalar.WhiteSmoke);

                            _renderMat1.PutText($"EmA: {track.ProcessingDetails.Emotions?.Angry}",
                                new Point(rectangle.X + rectangle.Width, rectangle.Y + 140),
                                HersheyFonts.HersheyPlain, 0.8f, Scalar.WhiteSmoke);
                            _renderMat1.PutText($"EmH: {track.ProcessingDetails.Emotions?.Happy}",
                                new Point(rectangle.X + rectangle.Width, rectangle.Y + 160),
                                HersheyFonts.HersheyPlain, 0.8f, Scalar.WhiteSmoke);
                            _renderMat1.PutText($"EmN: {track.ProcessingDetails.Emotions?.Neutral}",
                                new Point(rectangle.X + rectangle.Width, rectangle.Y + 180),
                                HersheyFonts.HersheyPlain, 0.8f, Scalar.WhiteSmoke);
                            _renderMat1.PutText($"EmS: {track.ProcessingDetails.Emotions?.Surprise}",
                                new Point(rectangle.X + rectangle.Width, rectangle.Y + 200),
                                HersheyFonts.HersheyPlain, 0.8f, Scalar.WhiteSmoke);

                            _renderMat1.PutText(
                                $"Angles: Y: {track.TrackingDetails.Rotation.Yaw}  P: {track.TrackingDetails.Rotation.Pitch}  R: {track.TrackingDetails.Rotation.Roll}",
                                new Point(rectangle.X + rectangle.Width, rectangle.Y + 220),
                                HersheyFonts.HersheyPlain, 0.8f, Scalar.WhiteSmoke);
                            _renderMat1.PutText($"Quality: {track.ProcessingDetails.ImageQuality?.Total}",
                                new Point(rectangle.X, rectangle.Y + rectangle.Height + 10),
                                HersheyFonts.HersheyPlain, 0.8f, Scalar.WhiteSmoke);

//                            if (!track.BestSnapshot.IsEmpty)
//                            {
//                                using var mat = Mat.FromStream(track.BestSnapshot.GetStream(), ImreadModes.Color);
//                                mat.CopyTo(_renderMat1.RowRange(0, mat.Height)
//                                    .ColRange(_renderMat1.Width - mat.Width, _renderMat1.Width));
//                            }

                        }

                        Dispatcher.Invoke(() =>
                        {
                            if (_writeableBitmap != null)
                            {

                                _writeableBitmap.Lock();

                                _writeableBitmap.WritePixels(new Int32Rect(0, 0, width, height), _renderMat1.Data,
                                    length, stride);

                                _writeableBitmap.Unlock();
                            }
                        });

                        Cv2.WaitKey(sleepTime);
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    throw;
                }
                finally
                {
                    capture?.Dispose();
                }
            });
            thread1.IsBackground = true;
            thread1.Start();
        }

        private void StopVideo_OnClick(object sender, RoutedEventArgs e)
        {
            Draws.Source = null;

            _cancellationTokenSource.Cancel();
            StartVideo.IsEnabled = true;
        }

        private void EmotionsCheckbox_OnChecked(object sender, RoutedEventArgs e)
        {
            if (_videoSourceProcessor != null)
            {
                _videoSourceProcessor.Settings.IsAnalyzeEmotions =
                    EmotionsCheckbox.IsChecked.HasValue && EmotionsCheckbox.IsChecked.Value;
            }
        }

        private void AgeCheckbox_OnChecked(object sender, RoutedEventArgs e)
        {
            if (_videoSourceProcessor != null)
            {
                _videoSourceProcessor.Settings.IsAnalyzeAgeGender =
                    AgeCheckbox.IsChecked.HasValue && AgeCheckbox.IsChecked.Value;
            }
        }

        private async void EnrollFacesFiles_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.CheckFileExists = true;            
            var result = dialog.ShowDialog(this);

            if (result == true)
            {
                var dir = Path.GetDirectoryName(dialog.FileName);
                var path = Path.GetFullPath(dir);

                var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                var templateCount = 0;
                await Task.Run(async () =>
                {
                    try
                    {
                        var templates = new List<ITemplate>();

                        foreach (var file in files)
                        {
                            var bytes = File.ReadAllBytes(file);

                            var faceSamples = await _service.DetectAsync(bytes);

                            foreach (var faceSample in faceSamples)
                            {
                                try
                                {
                                    var template = await _service.ComputeTemplateAsync(faceSample);
                                    template.Guid = Guid.NewGuid();
                                    template.SetCustomData($"{Path.GetFileNameWithoutExtension(file)}");
                                    template.Tag = $"Created: {DateTime.Now}";                               
                                    templates.Add(template);
                                    templateCount++;
                                }
                                catch (Exception exception)
                                {
                                    Debug.WriteLine(exception);                                
                                }
                            }
                        }
                        
                        if (_templatesIndex == null)
                        {
                            _templatesIndex = await _service.CreateIndexAsync(templates);
                            _videoSourceProcessor.SetTemplatesIndex(_templatesIndex);
                        }
                        else
                        {
                            _templatesIndex.AddRange(templates);
                            await _service.UpdateIndexAsync(_templatesIndex);
                        }
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });

                MessageBox.Show($"{templateCount} templates created.");

            }
        }

        private void SelectLicFile(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.CheckFileExists = true;
            var result = dialog.ShowDialog(this);

            _licensePath = dialog.FileName;
            LicensePath.Text = _licensePath;
        }

        private void OnInitButton(object sender, RoutedEventArgs e)
        {
            var isInitialized = InitializeSdk();

            InitPanel.IsEnabled = !isInitialized;
        }
    }
}
