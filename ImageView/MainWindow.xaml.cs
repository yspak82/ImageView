using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Linq;
using NaturalSort.Extension;
using openCV = OpenCvSharp;
using OpenCvSharp.WpfExtensions;

namespace ImageView
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        
        //BitmapImage bitmapImage;
        byte[] buffer;
        int bufferStride;
        float scale = 100;
        Point rightDownPt = new Point(0, 0);
        Point leftDownPt = new Point(0, 0);
        int bit;
        string[] fileList;
        int fileIndex = 0;
        openCV.Mat image;
        public MainWindow()
        {

            InitializeComponent();
            this.Loaded += MainWindow_Loaded;

        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            imageAnalysis.MouseWheel += ImageAnalysis_MouseWheel;
            imageAnalysis.MouseUp += ImageAnalysis_MouseUp;
            imageAnalysis.MouseMove += ImageAnalysis_MouseMove;
            imageAnalysis.MouseDown += ImageAnalysis_MouseDown;
            imageAnalysis.SizeChanged += ImageAnalysis_SizeChanged;
            scrollView.ScrollChanged += ScrollView_ScrollChanged;


        }
        private void LoadImage(string filePath)
        {
            string[] extensions = new[] { ".jpg", ".jpeg", ".png",".bmp",".tif" };
            DirectoryInfo dInfo = new DirectoryInfo(Path.GetDirectoryName(filePath));
            FileInfo[] files = dInfo.GetFiles().Where(f => extensions.Contains(f.Extension.ToLower())).ToArray();            
            //this.fileLidst = Directory.GetFiles(Path.GetDirectoryName(filePath), "*.jpg|*.jpeg|*.png|*.bmp|*.tif|*.gif");
            this.fileList = files.Select(g => g.FullName).OrderBy(x=>x, StringComparison.OrdinalIgnoreCase.WithNaturalSort()).ToArray();
            

            this.fileIndex = Array.IndexOf(fileList, filePath);

            DisplayImage(filePath);
        }
        private void DisplayImage(string filePath)
        {
            this.Title = $"ImageView - {filePath}";
            image = openCV.Cv2.ImRead(filePath, openCV.ImreadModes.Unchanged);
            var angle = int.Parse(Angle.Content.ToString());
            RotateImage(angle);
            //var bitmap = BitmapSourceConverter.ToBitmapSource(image);

            ////var bitmapImage = new BitmapImage(new Uri(filePath));
            //WriteableBitmap wb = new WriteableBitmap(bitmap);
            //this.buffer = new byte[wb.BackBufferStride * wb.PixelHeight];
            //this.bufferStride = wb.BackBufferStride;
            //wb.CopyPixels(buffer, this.bufferStride, 0);
            //this.bit = this.bufferStride / wb.PixelWidth * 8;


            ////this.bufferStride = (int)image.Step();
            ////this.buffer = new byte[image.Rows * bufferStride];

            //////wb.CopyPixels(buffer, this.bufferStride, 0);
            //////image.GetArray(out this.buffer);
            ////this.buffer = image.ToBytes(".jpg", new int[] { 0 });

            ////this.bit = this.bufferStride / image.Width * 8;

            //SizeString.Content = string.Format("{0} x {1} {2} bit", this.image.Width, this.image.Height, this.bit);

            //imageAnalysis.Source = bitmap;
            //scrollView.Focus();
        }
        private void RotateImage(int angle)
        {
            if (angle == 90 )
                openCV.Cv2.Rotate(image, image, openCV.RotateFlags.Rotate90Clockwise);
            else if (angle == 180)
                openCV.Cv2.Rotate(image, image, openCV.RotateFlags.Rotate180);
            else if (angle == 270)
                openCV.Cv2.Rotate(image, image, openCV.RotateFlags.Rotate90Counterclockwise);


            var bitmap = BitmapSourceConverter.ToBitmapSource(image);

            //var bitmapImage = new BitmapImage(new Uri(filePath));
            WriteableBitmap wb = new WriteableBitmap(bitmap);
            this.buffer = new byte[wb.BackBufferStride * wb.PixelHeight];
            this.bufferStride = wb.BackBufferStride;
            wb.CopyPixels(buffer, this.bufferStride, 0);
            this.bit = this.bufferStride / wb.PixelWidth * 8;


            //this.bufferStride = (int)image.Step();
            //this.buffer = new byte[image.Rows * bufferStride];

            ////wb.CopyPixels(buffer, this.bufferStride, 0);
            ////image.GetArray(out this.buffer);
            //this.buffer = image.ToBytes(".jpg", new int[] { 0 });

            //this.bit = this.bufferStride / image.Width * 8;

            SizeString.Content = string.Format("{0} x {1} {2} bit", this.image.Width, this.image.Height, this.bit);

            imageAnalysis.Source = bitmap;
            scrollView.Focus();
        }
        private void ScrollView_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            DrawCross(this.rightDownPt.X, this.rightDownPt.Y, this.scale);
        }
        private void ImageAnalysis_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            canvasAnalysis.Width = (double)imageAnalysis.ActualWidth;
            canvasAnalysis.Height = (double)imageAnalysis.ActualHeight;

            DrawCross(this.rightDownPt.X, this.rightDownPt.Y, this.scale);
        }

        private void ImageAnalysis_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var img = (IInputElement)sender as Image;
            var pt = e.GetPosition((IInputElement)sender);


            if (e.RightButton == MouseButtonState.Pressed)
            {
                this.rightDownPt = ConvertPoint(img.ActualWidth, img.ActualHeight, pt.X, pt.Y, this.image.Width, image.Height);
                DrawCross(this.rightDownPt.X, this.rightDownPt.Y, this.scale);

            }
        }

        private void ImageAnalysis_MouseMove(object sender, MouseEventArgs e)
        {


            var img = (IInputElement)sender as Image;
            var pt = e.GetPosition((IInputElement)sender);


            var realPt = ConvertPoint(img.ActualWidth, img.ActualHeight, pt.X, pt.Y, this.image.Width, image.Height);
            var x = realPt.X;
            var y = realPt.Y;

            var step = this.bit / 8;

            string pixelValue = string.Format("(X:{0}, Y:{1}) ", x, y);
            while (step > 0)
            {                
                var val = buffer[(int)y * bufferStride + (int)(this.bit / 8 * x) + --step];
                switch (step)
                {
                    case 3:                        
                        pixelValue += string.Format("A:{0} ", val);
                        break;
                    case 2:                        
                        pixelValue += string.Format("R:{0} ", val);
                        break;
                    case 1:                        
                        pixelValue += string.Format("G:{0} ", val);
                        break;
                    case 0:
                        pixelValue += this.bit == 8 ?"Val:" : "B:";
                        pixelValue +=  string.Format("{0} ", val);
                        break;
                    default:
                        break;
                }                
            }


            PtSting.Content = pixelValue;

        }

        private void ImageAnalysis_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void ImageAnalysis_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            this.scale = ChangeScale(this.scale, e.Delta);

            UpdateScale();
        }

        private void UpdateScale()
        {
            ZoomString.Content = string.Format("{0}%", this.scale);

            DrawCross(this.rightDownPt.X, this.rightDownPt.Y, this.scale);

            var delta = this.scale / 100;
            var oldW = imageAnalysis.Width.ToString() == double.NaN.ToString() ? this.image.Width : imageAnalysis.Width;
            imageAnalysis.Width = this.image.Width * delta;
            var newW = imageAnalysis.Width;
            var ratio = newW / oldW;



            var w = this.scrollView.HorizontalOffset * ratio + (this.scrollView.ViewportWidth * ratio - this.scrollView.ViewportWidth) / 2;
            var h = this.scrollView.VerticalOffset * ratio + (this.scrollView.ViewportHeight * ratio - this.scrollView.ViewportHeight) / 2;

            this.scrollView.ScrollToHorizontalOffset(w);
            this.scrollView.ScrollToVerticalOffset(h);
        }
        public string HelpString { 
            get 
            {
                return "앞으로: X \n" +
                    "뒤로: Z \n" +
                    "확대/축소: +,-,마우스휠 \n" +
                    "이동: 화살표, 마우스 좌클릭+이동" +
                    "회전: R" +
                    "";
            }  
        }


        private void imageAnalysis_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Add)
            {
                this.scale = ChangeScale(this.scale, 1);
                UpdateScale();
            }
            else if (e.Key == Key.Subtract)
            {
                this.scale = ChangeScale(this.scale, -1);
                UpdateScale();
            }
            else if (e.Key == Key.Z)
            {
                if (this.fileIndex == 0)
                    this.fileIndex = this.fileList.Length;
                var file = fileList[--this.fileIndex];
                DisplayImage(file);
            }
            else if (e.Key == Key.X)
            {
                if (this.fileIndex == this.fileList.Length - 1)
                    this.fileIndex = 0;
                var file = fileList[++this.fileIndex];
                DisplayImage(file);
            }
            else if (e.Key == Key.R)
            {
                var angle = int.Parse(Angle.Content.ToString()) +90;
                angle = angle == 360 ? 0 : angle;
                Angle.Content = angle.ToString();
                RotateImage(90);
            }

                ZoomString.Content = string.Format("{0}%", this.scale);

            var delta = this.scale / 100;

            DrawCross(this.rightDownPt.X, this.rightDownPt.Y, this.scale);

            imageAnalysis.Width = this.image.Width * delta;
        }

        private float ChangeScale(float scale, int delta, int min = 10, int max = 1000)
        {
            if (delta > 0)
            {
                if (scale < max)
                    scale = scale >= 100 ? scale + 100 : scale * 2;
            }
            else 
            {
                if (scale > min)                    
                    scale = scale > 100 ? scale - 100 : scale / 2;
            }
            return scale;
        }
        private void DrawCross(double x, double y, double ratio)
        {
            var delta = ratio / 100;

            vcross.X1 = (x + 0.5) * delta; ;
            vcross.X2 = (x + 0.5) * delta;

            vcross.Y1 = (y +0.5)* delta - 10;
            vcross.Y2 = (y + 0.5) * delta + 10 ;

            hcross.X1 = (x + 0.5) * delta -10;
            hcross.X2 = (x + 0.5) * delta + 10;

            hcross.Y1 = (y + 0.5) * delta; ;
            hcross.Y2 = (y + 0.5) * delta; ;


            var t = imageAnalysis.TransformToAncestor(scrollView);
            Point currentPoint = t.Transform(new Point(vcross.X1, hcross.Y1));

            vcross1.X2 = vcross1.X1 = currentPoint.X;
            hcross1.Y2 = hcross1.Y1 = currentPoint.Y;


        }
        private Point ConvertPoint(double srcWidth, double srcHeight, double srcX, double srcY, double targetWidth, double targetHeight) 
        {
            var x1 = targetWidth * srcX / srcWidth;
            var y1 = targetHeight * srcY / srcHeight;

            var x = Math.Truncate(x1);
            var y = Math.Truncate(y1);

            return new Point(x,y);
        }

        System.Windows.Point ScrollMousePoint1 = new System.Windows.Point();
        double HorizontalOff1 = 1; double VerticalOff1 = 1;
       

        private void scrollView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            //scrollView.ScrollToHorizontalOffset(scrollView.HorizontalOffset + e.Delta);
            //scrollView.ScrollToVerticalOffset(scrollView.VerticalOffset + e.Delta);
        }

        private void scrollView_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            scrollView.ReleaseMouseCapture();
        }

        private void scrollView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (scrollView.IsMouseCaptured)
            {
                scrollView.ScrollToHorizontalOffset(HorizontalOff1 + (ScrollMousePoint1.X - e.GetPosition(scrollView).X));
                scrollView.ScrollToVerticalOffset(VerticalOff1 + (ScrollMousePoint1.Y - e.GetPosition(scrollView).Y));
            }
        }

        private void scrollView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            
            ScrollMousePoint1 = e.GetPosition(scrollView);
            if (ScrollMousePoint1.X > scrollView.ViewportWidth || ScrollMousePoint1.Y > scrollView.ViewportHeight)
                return;
            HorizontalOff1 = scrollView.HorizontalOffset;
            VerticalOff1 = scrollView.VerticalOffset;
            scrollView.CaptureMouse();
        }

        private void Grid_PreviewDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if(files.Length > 0)
                    LoadImage(files[0]);
            }
        }
    }
}
