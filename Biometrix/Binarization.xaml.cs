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
    /// Logika interakcji dla klasy Binarization.xaml
    /// </summary>
    public partial class Binarization : Window
    {
        public byte[] modifiedPixels;

        WriteableBitmap previewBitmap;
        byte[] pixels;
        int stride;
        int width;
        int height;

        public Binarization(byte[] pixels, int stride, int width, int height, WriteableBitmap modifiedBitmap)
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

        private void UpdatePreviewImage(byte[] pixels)
        {
            BitmapSource bitmap = BitmapImage.Create(width, height, 96, 96, PixelFormats.Bgr32, null, pixels, stride);
            previewBitmap = new WriteableBitmap(bitmap);

            PreviewImage.Source = previewBitmap;

            modifiedPixels = pixels;
        }

        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            Binarize();
        }

        private void Binarize()
        {
            byte threshold = (byte)ThresholdSpinValue.Value;
            byte[] LUT = new byte[256];

            if (UnderThresholdRadioBtn.IsChecked == true)
            {
                for (int i = 0; i < LUT.Length; i++)
                {
                    LUT[i] = i <= threshold ? (byte)0 : (byte)255;
                }
            }
            else if (AboveThresholdRadioBtn.IsChecked == true)
            {
                for (int i = 0; i < LUT.Length; i++)
                {
                    LUT[i] = i >= threshold ? (byte)0 : (byte)255;
                }
            }
            else if (BetweenThresholdRadioBtn.IsChecked == true)
            {
                byte threshold_B = (byte)ThresholdBSpinValue.Value;
                for (int i = 0; i < LUT.Length; i++)
                {
                    LUT[i] = (i <= threshold || i > threshold_B) ? (byte)0 : (byte)255;
                }
            }
            else if (ExceptThresholdRadioBtn.IsChecked == true)
            {
                byte threshold_B = (byte)ThresholdBSpinValue.Value;
                for (int i = 0; i < LUT.Length; i++)
                {
                    LUT[i] = (i >= threshold && i < threshold_B) ? (byte)0 : (byte)255;
                }
            }

            byte[] p = new byte[pixels.Length];
            for (int i = 0; i < p.Length; i += 4)
            {
                p[i] = LUT[pixels[i]];
                p[i + 1] = LUT[pixels[i+1]];
                p[i + 2] = LUT[pixels[i+2]];
                p[i + 3] = pixels[i + 3];
            }

            UpdatePreviewImage(p);
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
    }
}
