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
    public enum FilterType
    {
        PREWITT_H, PREWITT_V, SOBEL_H, SOBEL_V, LAPLACE, CORNER, CUSTOM
    }

    public partial class Convolution : Window
    {
        public byte[] modifiedPixels;

        WriteableBitmap previewBitmap;
        byte[] pixels;
        int stride;
        int bytesPerPixel;
        int width;
        int height;

        FilterType filterType = FilterType.CUSTOM;

        public Convolution(byte[] pixels, int stride, int width, int height, WriteableBitmap modifiedBitmap, int bytesPerPixel)
        {
            InitializeComponent();
            PrewittVerRadio.Checked += FilterTypeRadio_Checked;
            PrewittHorRadio.Checked += FilterTypeRadio_Checked;
            SobelVerRadio.Checked += FilterTypeRadio_Checked;
            SobelHorRadio.Checked += FilterTypeRadio_Checked;
            LaplaceRadio.Checked += FilterTypeRadio_Checked;
            CornerDetectRadio.Checked += FilterTypeRadio_Checked;
            CustomRadio.Checked += FilterTypeRadio_Checked;

            previewBitmap = new WriteableBitmap(modifiedBitmap);

            this.pixels = pixels;
            this.stride = stride;
            this.width = width;
            this.height = height;
            this.bytesPerPixel = bytesPerPixel;

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
            ConvoluteWithFilter();
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

        private void ConvoluteWithFilter()
        {
            byte[] p = new byte[pixels.Length];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int index = i * bytesPerPixel + j * stride;

                    if (i == 0 || j == 0 || i == height - 1 || j == width - 1)
                    {
                        p[index] = 0;
                        p[index + 1] = 0;
                        p[index + 2] = 0;
                    }
                    else
                    {
                        p[index] = pixels[index];
                        p[index + 1] = pixels[index + 1];
                        p[index + 2] = pixels[index + 2];
                    }
                    
                    p[index + 3] = pixels[index + 3];
                }
            }

            UpdatePreviewImage(p);
        }

        private void FilterTypeRadio_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;

            CustomFrameGrid.IsEnabled = false;

            switch (radioButton.Name)
            {
                case "PrewittVerRadio":
                    filterType = FilterType.PREWITT_V;
                    break;
                case "PrewittHorRadio":
                    filterType = FilterType.PREWITT_H;
                    break;
                case "SobelVerRadio":
                    filterType = FilterType.SOBEL_V;
                    break;
                case "SobelHorRadio":
                    filterType = FilterType.SOBEL_H;
                    break;
                case "LaplaceRadio":
                    filterType = FilterType.LAPLACE;
                    break;
                case "CornerDetectRadio":
                    filterType = FilterType.CORNER;
                    break;
                case "CustomRadio":
                    filterType = FilterType.CUSTOM;
                    CustomFrameGrid.IsEnabled = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Nie uwzględniono radioButtona innego, niż wyżej zdefiniowane.");
            }
        }
    }
}
