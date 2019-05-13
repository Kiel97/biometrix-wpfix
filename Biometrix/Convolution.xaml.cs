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

        private void ConvoluteWithFilter()
        {
            int windowsize = 3;
            int radius = windowsize / 2;

            byte[] p = new byte[pixels.Length];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int index = i * bytesPerPixel + j * stride;

                    if (IsImageBorder(i, j, height, width, radius))
                    {
                        p[index] = 0;
                        p[index + 1] = 0;
                        p[index + 2] = 0;
                    }
                    else
                    {
                        int[,] neighbours = GetNeighbouringPixelIndexes(i, j, windowsize);
                        p[index] = pixels[neighbours[1,1]];
                        p[index + 1] = pixels[neighbours[1,1] + 1];
                        p[index + 2] = pixels[neighbours[1,1] + 2];
                    }

                    p[index + 3] = pixels[index + 3];
                }
            }

            UpdatePreviewImage(p);
        }

        private int[,] GetNeighbouringPixelIndexes(int x, int y, int windowsize)
        {
            int[,] neighbours = new int[windowsize, windowsize];
            int radius = windowsize / 2;

            for (int i = 0; i < windowsize; i++)
            {
                for (int j = 0; j < windowsize; j++)
                {
                    neighbours[i, j] = (x - radius + i) * bytesPerPixel + (y - radius + j) * stride;
                }
            }

            /*Przykład:
            neighbours[0, 0] = (x - 1) * bytesPerPixel + (y - 1) * stride;
            neighbours[0, 1] = (x - 1) * bytesPerPixel + y * stride;
            neighbours[0, 2] = (x - 1) * bytesPerPixel + (y + 1) * stride;
            neighbours[1, 0] = x * bytesPerPixel + (y - 1) * stride;
            neighbours[1, 1] = x * bytesPerPixel + y * stride;
            neighbours[1, 2] = x * bytesPerPixel + (y + 1) * stride;
            neighbours[2, 0] = (x + 1) * bytesPerPixel + (y - 1) * stride;
            neighbours[2, 1] = (x + 1) * bytesPerPixel + y * stride;
            neighbours[2, 2] = (x + 1) * bytesPerPixel + (y + 1) * stride;*/

            return neighbours;
        }

        private bool IsImageBorder(int i, int j, int height, int width, int radius)
        {
            return i < radius || j < radius || i >= height - radius || j >= width - radius;
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
