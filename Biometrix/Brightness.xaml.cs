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
        WriteableBitmap previewBitmap;
        WriteableBitmap beforeBitmap;
        byte[] pixels;
        int stride;
        int width;
        int height;

        public Brightness(byte[] pixels, int stride, int width, int height, WriteableBitmap modifiedBitmap)
        {
            InitializeComponent();
            beforeBitmap = modifiedBitmap;
            previewBitmap = new WriteableBitmap(modifiedBitmap);

            this.pixels = pixels;
            this.stride = stride;
            this.width = width;
            this.height = height;

            PreviewImage.Source = previewBitmap;
            UpdatePreviewImage(pixels);
        }

        private void ASpinValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                if (ASpinValue.Value == 1)
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
            byte[] newPixels = CalculateLogImageBrightness(pixels);

            UpdatePreviewImage(newPixels);
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void UpdatePreviewImage(byte[] pixels)
        {
            previewBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
        }

        private byte[] CalculateLogImageBrightness(byte[] pixels)
        {
            byte[] p = new byte[pixels.Length];

            for (int i = 0; i < p.Length; i+=4)
            {
                p[i] = GetLogFunctionValue((double)ASpinValue.Value, (double)BSpinValue.Value, (double)CSpinValue.Value, pixels[i]);
                p[i+1] = GetLogFunctionValue((double)ASpinValue.Value, (double)BSpinValue.Value, (double)CSpinValue.Value, pixels[i+1]);
                p[i+2] = GetLogFunctionValue((double)ASpinValue.Value, (double)BSpinValue.Value, (double)CSpinValue.Value, pixels[i+2]);
                p[i+3] = pixels[i+3];
            }

            return p;
        }

        private byte GetLogFunctionValue(double a, double b, double c, byte x)
        {
            return (byte)(x + 255);
        }
    }
}
