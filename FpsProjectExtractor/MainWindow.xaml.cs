using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
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


namespace FpsProjectExtractor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly string OutDir = @"C:\Users\fletcher\projects\FpsProjectExtractor\out";
        private static readonly string InDir = @"C:\Users\fletcher\projects\FpsProjectExtractor\in";
        private static readonly int x = 151;
        private static readonly int y = 61;
        private static readonly int w = 178;
        private static readonly int h = 29;

        private DateTime? StartTime = null;
        private string[] InputFiles;

        public MainWindow()
        {
            InitializeComponent();
            log.Info($"Starting a new session.");
            
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ReadInDir();
        }

        private void ReadInDir()
        {
            InputFiles = Directory.GetFiles(System.IO.Path.Join(InDir, "demo"), "out*");
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
            System.Drawing.Rectangle roi = new System.Drawing.Rectangle(x, y, w, h);
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

        private string CreateLine(string filename, string result)
        {
            return $"{filename},{result}";
        }

        private string Parse(string result)
        {
            // Define a regular expression for repeated words.
            Regex rx = new Regex(@"\b\d+\b",
              RegexOptions.Compiled | RegexOptions.IgnoreCase);

            // Find matches.
            MatchCollection matches = rx.Matches(result);
            if (matches.Count == 0)
            {
                return "ERROR";
            }
            else
            {
                string parsed = "";
                foreach(Match match in matches)
                {
                    parsed += match.Value + ",";
                }
                return parsed;
            }
        }

        private void WriteResults(List<string> fileNames, List<string> results)
        {
            List<string> lines = new List<string>(fileNames.Count);
            for(int i = 0; i < results.Count; i++)
            {
                string parsed = Parse(results[i]);
                lines.Add(CreateLine(fileNames[i], parsed));
            }
            System.IO.File.WriteAllLines(System.IO.Path.Join(OutDir, "out.csv"), lines);
        }

        private string Process(string file)
        {
            byte[] imgData = Preprocess(file);
            return Tess(imgData);
        }

        private void DoSubset(IProgress<ProgressUpdate> progress, int offset = 0, int? limit = null)
        {
            progress.Report(new ProgressUpdate("Getting files", 0, 0));
            string[] files = InputFiles;
            int count = limit ?? (files.Length - offset);
            int end = offset + count;

            
            List<string> fileNames = new List<string>(count);
            List<string> ocrResults = new List<string>(count);

            int progressCount = 0;
            for (int i = offset; i < end; i++)
            {
                fileNames.Add(files[i]);
                ocrResults.Add(Process(files[i]));
                progressCount += 1;
                progress.Report(new ProgressUpdate(files[i], progressCount, count));
            }

            progress.Report(new ProgressUpdate("Writing to CSV", progressCount, count));
            WriteResults(fileNames, ocrResults);
            progress.Report(new ProgressUpdate("Finished", progressCount, count));
        }

        public void DoIt(object sender, RoutedEventArgs eventAargs)
        {
            RunBtn.IsEnabled = false;
            Log("Execution start");
            StartTime = DateTime.UtcNow;
            Progress<ProgressUpdate> progress = new Progress<ProgressUpdate>(UpdateProgress);
            Task.Factory.StartNew(() => DoSubset(progress, 0, 10));
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
