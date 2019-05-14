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
    /// Logika interakcji dla klasy Kuwahar.xaml
    /// </summary>
    public partial class Kuwahar : Window
    {
        public byte[] modifiedPixels;

        WriteableBitmap previewBitmap;
        byte[] pixels;
        int stride;
        int bytesPerPixel;
        int width;
        int height;

        public Kuwahar(byte[] pixels, int stride, int width, int height, WriteableBitmap modifiedBitmap, int bytesPerPixel)
        {
            InitializeComponent();

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

        private void KuwaharFilter()
        {
            int windowsize = 5;
            int radius = windowsize / 2;

            int subwindowsize = 3;
            int subradius = subwindowsize / 2;

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

                        int[,] neighboursTopLeft = GetSubNeighbouringPixelIndexes(ref neighbours, 0, 0, subwindowsize);
                        int[,] neighboursTopRight = GetSubNeighbouringPixelIndexes(ref neighbours, 2, 0, subwindowsize);
                        int[,] neighboursBottomLeft = GetSubNeighbouringPixelIndexes(ref neighbours, 0, 2, subwindowsize);
                        int[,] neighboursBottomRight = GetSubNeighbouringPixelIndexes(ref neighbours, 2, 2, subwindowsize);

                        p[index] = CalculateKuwahar(ref neighboursTopLeft, ref neighboursTopRight, ref neighboursBottomLeft, ref neighboursBottomRight, 0);
                        p[index + 1] = CalculateKuwahar(ref neighboursTopLeft, ref neighboursTopRight, ref neighboursBottomLeft, ref neighboursBottomRight, 1);
                        p[index + 2] = CalculateKuwahar(ref neighboursTopLeft, ref neighboursTopRight, ref neighboursBottomLeft, ref neighboursBottomRight, 2);
                    }

                    p[index + 3] = pixels[index + 3];
                }
            }

            UpdatePreviewImage(p);
        }

        private byte CalculateKuwahar(ref int[,] TL, ref int[,] TR, ref int[,] BL, ref int[,] BR, int offset)
        {
            int meanTopLeft = CalculateMeanValue(ref TL, offset);
            int meanTopRight = CalculateMeanValue(ref TR, offset);
            int meanBottomLeft = CalculateMeanValue(ref BL, offset);
            int meanBottomRight = CalculateMeanValue(ref BR, offset);

            double varianceTopLeft = CalculateVarianceValue(ref TL, meanTopLeft, offset);
            double varianceTopRight = CalculateVarianceValue(ref TR, meanTopRight, offset);
            double varianceBottomLeft = CalculateVarianceValue(ref BL, meanBottomLeft, offset);
            double varianceBottomRight = CalculateVarianceValue(ref BR, meanBottomRight, offset);

            // Tutaj powinno być wybieranie średniej wartości dla regionu o najmniejszej wariancji
            return (byte)Math.Abs(0);
        }

        private int CalculateMeanValue(ref int[,] neighbours, int offset)
        {
            int sum = 0;

            for (int i = 0; i < neighbours.GetLength(0); i++)
            {
                for (int j = 0; j < neighbours.GetLength(1); j++)
                {
                    sum += pixels[neighbours[i, j] + offset];
                }
            }

            return sum / neighbours.Length;
        }

        private double CalculateVarianceValue(ref int[,] neighbours, int mean, int offset)
        {
            double sum = 0f;

            for (int i = 0; i < neighbours.GetLength(0); i++)
            {
                for (int j = 0; j < neighbours.GetLength(1); j++)
                {
                    sum += Math.Pow(mean - pixels[neighbours[i, j] + offset], 2);
                }
            }

            return sum / neighbours.Length;
        }

        private int[,] GetSubNeighbouringPixelIndexes(ref int[,] neighbours, int startX, int startY, int windowsize)
        {
            int[,] subneighbours = new int[windowsize, windowsize];

            for (int i = 0; i < windowsize; i++)
            {
                for (int j = 0; j < windowsize; j++)
                {
                    subneighbours[i, j] = neighbours[startX + i, startY + j];
                }
            }

            return subneighbours;
        }

        private bool IsImageBorder(int i, int j, int height, int width, int radius)
        {
            return i < radius || j < radius || i >= height - radius || j >= width - radius;
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

            return neighbours;
        }

        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            KuwaharFilter();
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
