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
    /// Logika interakcji dla klasy GrayScale.xaml
    /// </summary>
    public partial class GrayScale : Window
    {
        public byte[] modifiedPixels;

        WriteableBitmap previewBitmap;
        byte[] pixels;
        int stride;
        int width;
        int height;

        public GrayScale(byte[] pixels, int stride, int width, int height, WriteableBitmap modifiedBitmap)
        {
            InitializeComponent();

            this.pixels = pixels;
            this.stride = stride;
            this.width = width;
            this.height = height;

            modifiedPixels = new byte[pixels.Length];

            UpdatePreviewImage(pixels);
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

        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            byte[] newPixels = null;

            if (Function03.IsChecked == true)
            {
                newPixels = CalculateGrayScale03(pixels);
            }
            else if (Function04.IsChecked == true)
            {
                newPixels = CalculateGrayScale04(pixels);
            }

            UpdatePreviewImage(newPixels);
        }

        private byte[] CalculateGrayScale03(byte[] pixels)
        {
            byte[] p = new byte[pixels.Length];
            int newValue;

            for (int i = 0; i < p.Length; i += 4)
            {
                newValue = (pixels[i] + pixels[i + 1] + pixels[i + 2]) / 3;
                p[i] = (byte)newValue;
                p[i + 1] = (byte)newValue;
                p[i + 2] = (byte)newValue;
                p[i + 3] = pixels[i + 3];
            }

            return p;
        }

        private byte[] CalculateGrayScale04(byte[] pixels)
        {
            byte[] p = new byte[pixels.Length];
            int newValue;

            for (int i = 0; i < p.Length; i += 4)
            {
                newValue = (int)((pixels[i] * 1.6) + (pixels[i + 1] * 0.7) + (pixels[i + 2] * 0.7)) / 3;
                p[i] = (byte)newValue;
                p[i + 1] = (byte)newValue;
                p[i + 2] = (byte)newValue;
                p[i + 3] = pixels[i + 3];
            }

            return p;
        }

        private void UpdatePreviewImage(byte[] pixels)
        {
            BitmapSource bitmap = BitmapImage.Create(width, height, 96, 96, PixelFormats.Bgr32, null, pixels, stride);
            previewBitmap = new WriteableBitmap(bitmap);

            PreviewImage.Source = previewBitmap;

            modifiedPixels = pixels;
        }
    }
}
