using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Biometrix
{
    /// <summary>
    /// Logika interakcji dla klasy Brightness.xaml
    /// </summary>
    public partial class Brightness : Window
    {
        public byte[] modifiedPixels;

        WriteableBitmap previewBitmap;
        byte[] pixels;
        int stride;
        int width;
        int height;

        const string LOG_FORMULA = "f(x) = c * loga x + b";
        const string SQUARE_FORMULA = "f(x) = ax^2 + bx + c";

        public Brightness(byte[] pixels, int stride, int width, int height, WriteableBitmap modifiedBitmap)
        {
            InitializeComponent();
            previewBitmap = new WriteableBitmap(modifiedBitmap);

            this.pixels = pixels;
            this.stride = stride;
            this.width = width;
            this.height = height;

            modifiedPixels = new byte[pixels.Length];

            UpdatePreviewImage(pixels);
        }

        private void ASpinValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                if (ASpinValue.Value == 1 && LogFunction.IsChecked == true)
                {
                    PreviewButton.IsEnabled = false;
                    ConfirmButton.IsEnabled = false;
                }
                else
                {
                    PreviewButton.IsEnabled = true;
                    ConfirmButton.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            byte[] newPixels = null;

            if (LogFunction.IsChecked == true)
            {
                newPixels = CalculateLogImageBrightness(pixels);
            }
            else if (SquareFunction.IsChecked == true)
            {
                newPixels = CalculateSquareImageBrightness(pixels);
            }

            UpdatePreviewImage(newPixels);
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void UpdatePreviewImage(byte[] pixels)
        {
            BitmapSource bitmap = BitmapImage.Create(width, height, 96, 96, PixelFormats.Bgr32, null, pixels, stride);
            previewBitmap = new WriteableBitmap(bitmap);

            PreviewImage.Source = previewBitmap;

            modifiedPixels = pixels;
        }

        private byte[] CalculateLogImageBrightness(byte[] pixels)
        {
            double a = (double)ASpinValue.Value;
            double b = (double)BSpinValue.Value;
            double c = (double)CSpinValue.Value;

            byte[] p = new byte[pixels.Length];

            for (int i = 0; i < p.Length; i+=4)
            {
                p[i] = (byte)GetLogFunctionValue(a, b, c, pixels[i]);
                p[i+1] = (byte)GetLogFunctionValue(a, b, c, pixels[i+1]);
                p[i+2] = (byte)GetLogFunctionValue(a, b, c, pixels[i+2]);
                p[i+3] = pixels[i+3];
            }

            return p;
        }

        private byte[] CalculateSquareImageBrightness(byte[] pixels)
        {
            double a = (double)ASpinValue.Value;
            double b = (double)BSpinValue.Value;
            double c = (double)CSpinValue.Value;

            byte[] p = new byte[pixels.Length];

            for (int i = 0; i < p.Length; i+=4)
            {
                p[i] = (byte)GetSquareFunctionValue(a, b, c, pixels[i]);
                p[i+1] = (byte)GetSquareFunctionValue(a, b, c, pixels[i+1]);
                p[i+2] = (byte)GetSquareFunctionValue(a, b, c, pixels[i+2]);
                p[i+3] = pixels[i+3];
            }

            return p;
        }

        private double GetLogFunctionValue(double a, double b, double c, byte x)
        {
            return c * Math.Log(x, a) + b;
        }

        private double GetSquareFunctionValue(double a, double b, double c, byte x)
        {
            return a * Math.Pow(x, 2) + b * x + c;
        }

        private void LogFunction_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                FunctionFormulaLabel.Content = LOG_FORMULA;
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void SquareFunction_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                FunctionFormulaLabel.Content = SQUARE_FORMULA;
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
