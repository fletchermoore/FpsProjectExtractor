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
        private static readonly int x = 1049;
        private static readonly int y = 32;
        private static readonly int w = 139;
        private static readonly int h = 31;
        private static readonly string testImagePath = @"bigtest.jpg";

        public MainWindow()
        {
            InitializeComponent();
            log.Info($"Starting a new session.");
        }

        public void Log(string msg)
        {
            log.Info(msg);
        }

        private Mat Crop(Mat inMat)
        {
            Mat outMat = new Mat();
            return outMat;
        }

        
        private Mat BlackWhite(Mat inMat)
        {
            Mat outMat = new Mat();
            CvInvoke.Threshold(inMat, outMat, 0, 100, Emgu.CV.CvEnum.ThresholdType.Binary);
            return outMat;
        }


        public void Preprocess(string file)
        {
            string fullPath = System.IO.Path.Join(InDir, file);
            Mat img = CvInvoke.Imread(fullPath, Emgu.CV.CvEnum.ImreadModes.Grayscale);
            System.Drawing.Rectangle roi = new System.Drawing.Rectangle(x, y, w, h);
            Mat croppedImg = new Mat(img, roi);
            Mat postInverted = new Mat();
            CvInvoke.BitwiseNot(croppedImg, postInverted);
            Mat outImg = BlackWhite(postInverted);
            
            VectorOfByte tiff = new Emgu.CV.Util.VectorOfByte();
            CvInvoke.Imencode("*.tiff", outImg, tiff);
            //string text = Tess(tiff.ToArray());
            //Log(text);
            outImg.Save(System.IO.Path.Join(OutDir, "6" + file));
        }

        public string? Tess(byte[] data) //string filePath)
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

        public void DoIt(object sender, RoutedEventArgs eventAargs)
        {
            RunBtn.IsEnabled = false;
            
            Preprocess(testImagePath);
            //Tess(testImagePath);
            RunBtn.IsEnabled = true;
        }
    }
    
}
