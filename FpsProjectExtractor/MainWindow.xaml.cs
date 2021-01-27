using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tesseract;
using Emgu.CV;
using Emgu.CV.Util;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Text.RegularExpressions;



namespace FpsProjectExtractor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly string OutDir = @"E:\fps-project\out";
        private static readonly string InDir = @"E:\fps-project\in";
        private string InFolder = @"extract";
        private static readonly string DefaultImage = @"out_0000001.jpg";

        private DateTime? StartTime = null;
        private string[] InputFiles;

        private Point TopLeft = new Point(0, 0);
        private Point BottomRight = new Point(0, 0);
        private int RoiX = 0;
        private int RoiY = 0;
        private int RoiW = 0;
        private int RoiH = 0;

        public MainWindow()
        {
            InitializeComponent();
            log.Info($"Starting a new session.");
            
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }

        private void SelectNewFolder()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = InDir;
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {

                InFolder = System.IO.Path.GetFileName(dialog.FileName);
            }

            ReadInDir();
            ResetROI();
            SetDefaultImages();
        }

        private void SetDefaultImages()
        {
            string path = System.IO.Path.Join(InDir, InFolder, DefaultImage);
            try
            {
                BitmapImage bi3 = new BitmapImage();
                bi3.BeginInit();
                bi3.UriSource = new Uri(path, UriKind.Absolute);
                bi3.EndInit();
                WholeImage.Source = bi3;
                ZoomImage.Source = bi3;
            } catch(Exception e)
            {
                MessageBox.Show($"Image path invalid: {path}");
            }

        }

        private void ReadInDir()
        {
            InputFiles = Directory.GetFiles(System.IO.Path.Join(InDir, InFolder), "out*");
            this.FileCountLabel.Content = $"Frames: {InputFiles.Length}";
        }

        public void Log(string msg)
        {
            log.Info(msg);
        }

        //private string[] InputFiles()
        //{
        //    string[] files = 
        //    return files;
        //}

        private Mat Crop(Mat inMat)
        {
            Mat outMat = new Mat();
            return outMat;
        }

        
        private Mat BlackWhite(Mat inMat)
        {
            Mat outMat = new Mat();
            CvInvoke.Threshold(inMat, outMat, 200, 255, Emgu.CV.CvEnum.ThresholdType.Binary);
            return outMat;
        }


        private byte[] Preprocess(string fullPath)
        {
            Mat img = CvInvoke.Imread(fullPath, Emgu.CV.CvEnum.ImreadModes.Grayscale);
            System.Drawing.Rectangle roi = new System.Drawing.Rectangle(RoiX, RoiY, RoiW, RoiH);
            Mat croppedImg = new Mat(img, roi);
            Mat outImg = new Mat();
            CvInvoke.BitwiseNot(croppedImg, outImg);
            //Mat outImg = BlackWhite(postInverted);
            
            VectorOfByte tiff = new Emgu.CV.Util.VectorOfByte();
            CvInvoke.Imencode("*.tiff", outImg, tiff);
            return tiff.ToArray();
            //string text = Tess(tiff.ToArray());
            //Log(text);
            //outImg.Save(System.IO.Path.Join(OutDir, "7" + file));
        }

        public string Tess(byte[] data) //string filePath)
        {
            try
            {
                using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
                {
                    using (var img = Pix.LoadFromMemory(data)) //Pix.LoadFromFile(filePath))
                    {
                        using (var page = engine.Process(img))
                        {
                            return page.GetText();

                            //using (var iter = page.GetIterator())
                            //    {
                            //        iter.Begin();

                            //        do
                            //        {
                            //            do
                            //            {
                            //                do
                            //                {
                            //                    do
                            //                    {
                            //                        if (iter.IsAtBeginningOf(PageIteratorLevel.Block))
                            //                        {
                            //                            Console.WriteLine("<BLOCK>");
                            //                        }

                            //                        Console.Write(iter.GetText(PageIteratorLevel.Word));
                            //                        Console.Write(" ");

                            //                        if (iter.IsAtFinalOf(PageIteratorLevel.TextLine, PageIteratorLevel.Word))
                            //                        {
                            //                            Console.WriteLine();
                            //                        }
                            //                    } while (iter.Next(PageIteratorLevel.TextLine, PageIteratorLevel.Word));

                            //                    if (iter.IsAtFinalOf(PageIteratorLevel.Para, PageIteratorLevel.TextLine))
                            //                    {
                            //                        Console.WriteLine();
                            //                    }
                            //                } while (iter.Next(PageIteratorLevel.Para, PageIteratorLevel.TextLine));
                            //            } while (iter.Next(PageIteratorLevel.Block, PageIteratorLevel.Para));
                            //        } while (iter.Next(PageIteratorLevel.Block));
                            //    }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
                Log("Unexpected Error: " + e.Message);
                Log("Details: ");
                Log(e.ToString());
                return null;
            }
        }



        private string Process(string file)
        {
            byte[] imgData = Preprocess(file);
            string rawTess = Tess(imgData);
            return RemoveCRLF(rawTess);
        }

        private string RemoveCRLF(string multiline)
        {
            string pattern = @"\\n|\\r";
            string replacement = "_";
            return Regex.Replace(multiline.Trim(), pattern, replacement);
        }

        private void DoSubset(IProgress<ProgressUpdate> progress, int offset = 0, int? limit = null)
        {
            progress.Report(new ProgressUpdate("Getting files", 0, 0));
            string[] files = InputFiles;
            int count = limit ?? (files.Length - offset);
            int end = offset + count;

            
            List<string> fileNames = new List<string>(count);
            List<string> ocrResults = new List<string>(count);

            List<Record> records = new List<Record>(count);

            int progressCount = 0;
            for (int i = offset; i < end; i++)
            {
                Record record = new Record();
                record.Path = files[i];
                record.Text = Process(files[i]);
                records.Add(record);
                //fileNames.Add(files[i]);
                //ocrResults.Add(Process(files[i]));
                progressCount += 1;
                progress.Report(new ProgressUpdate(files[i], progressCount, count));
            }

            progress.Report(new ProgressUpdate("Writing to CSV", progressCount, count));
            DataWriter writer = new DataWriter();
            writer.Write(records, System.IO.Path.Join(OutDir, $"{InFolder}.csv"));
            //WriteResults(fileNames, ocrResults);
            progress.Report(new ProgressUpdate("Finished", progressCount, count));
        }

        public void DoIt(object sender, RoutedEventArgs eventAargs)
        {
            RunBtn.IsEnabled = false;
            Log("Execution start");
            StartTime = DateTime.UtcNow;
            Progress<ProgressUpdate> progress = new Progress<ProgressUpdate>(UpdateProgress);
            int start = 0;
            int? limit = null;
            try
            {
                start = Int32.Parse(FromBox.Text);
            }
            catch(Exception e)
            {
                FromBox.Text = $"{start}";
            }
            try
            {
                limit = Int32.Parse(LimitBox.Text);
            }
            catch(Exception e)
            {
                LimitBox.Text = "";

            }
            Task.Factory.StartNew(() => DoSubset(progress, start, limit));
            UpdateElapsedTime();
            RunBtn.IsEnabled = true;
            Log("Execution finished");
        }

        private void UpdateElapsedTime()
        {
            if (StartTime!= null)
            {
                TimeSpan elapsed = DateTime.UtcNow.Subtract(StartTime.Value);
                TimeLabel.Content = "Elapsed time: " + elapsed.ToString("c");
            }            
        }

        private void UpdateProgress(ProgressUpdate update)
        {
            MainProgressBar.Maximum = update.Count;
            MainProgressBar.Value = update.Current;
            CurrentFileLabel.Content = update.File;
            ProgressLabel.Content = $"{update.Current} / {update.Count}";
            UpdateElapsedTime();
        }

        private void Test(object sender, RoutedEventArgs e)
        {
            
        }

        private void Analyze(object sender, RoutedEventArgs e)
        {
            Analyzer analyzer = new Analyzer(OutDir);
            AnalyzeStatusLabel.Content = analyzer.Analyze();
        }

        private void ResetROI()
        {
            RoiX = 0;
            RoiY = 0;
            RoiW = 0;
            RoiH = 0;
        }

        private void WholeImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(WholeImage);
            
            double xOffsetPercent = p.X / WholeImage.ActualWidth;
            double yOffsetPercent = p.Y / WholeImage.ActualHeight;
            double xOffset = - xOffsetPercent * (ZoomImage.ActualWidth * 2.0);
            double yOffset = - yOffsetPercent * (ZoomImage.ActualHeight * 2.0);
            Canvas.SetLeft(ZoomImage, xOffset);
            Canvas.SetTop(ZoomImage, yOffset);
            ZoomImageCanvas.Width = Double.NaN;
            ZoomImageCanvas.Height = Double.NaN;
        }

        private void ZoomImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TopLeft = e.GetPosition(ZoomImage);
        }

        private void ZoomImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            BottomRight = e.GetPosition(ZoomImage);
            BoundsLabel.Content = $"{(int)TopLeft.X},{(int)TopLeft.Y} -> {(int)BottomRight.X},{(int)BottomRight.Y}";
            Canvas.SetLeft(ZoomImage, -TopLeft.X * 2);
            Canvas.SetTop(ZoomImage, -TopLeft.Y * 2);
            ZoomImageCanvas.Width = (BottomRight.X - TopLeft.X) * 2;
            ZoomImageCanvas.Height = (BottomRight.Y - TopLeft.Y) * 2;
            ResetSaveBtn();            
        }

        private void ResetSaveBtn()
        {
            SaveBtn.IsEnabled = true;
            SaveBtn.Visibility = Visibility.Visible;
            SaveBtn.Content = "Save ROI";
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            // TODO: write the ROI to file
            SaveBtn.IsEnabled = false;
            SaveBtn.Content = "Saved";
            RoiX = (int)TopLeft.X;
            RoiY = (int)TopLeft.Y;
            RoiW = (int)(BottomRight.X - TopLeft.X);
            RoiH = (int)(BottomRight.Y - TopLeft.Y);
        }

        private void OpenBtn_Click(object sender, RoutedEventArgs e)
        {
            SelectNewFolder();
        }
    }

    class ProgressUpdate
    {
        public int Count = 0;
        public int Current = 0;
        public string File = "";

        public ProgressUpdate(string file, int current, int count)
        {
            Count = count;
            Current = current;
            File = file;
        }
    }
    
}
