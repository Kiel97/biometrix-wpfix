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
    /// Logika interakcji dla klasy Median.xaml
    /// </summary>
    public partial class Median : Window
    {
        public byte[] modifiedPixels;

        WriteableBitmap previewBitmap;
        byte[] pixels;
        int stride;
        int bytesPerPixel;
        int width;
        int height;

        public Median(byte[] pixels, int stride, int width, int height, WriteableBitmap modifiedBitmap, int bytesPerPixel)
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

        private void MedianFilter()
        {
            int windowsize = GetWindowSize();
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

                        for (int colorOffset = 0; colorOffset < 3; colorOffset++)
                        {//0 - niebieski, 1 - zielony, 2 - czerwony, 3 - alfa
                            byte pixelValue = GetMedianPixel(ref neighbours, colorOffset);
                            p[index + colorOffset] = pixelValue;
                        }
                    }

                    p[index + 3] = pixels[index + 3];
                }
            }

            UpdatePreviewImage(p);
        }

        private int GetWindowSize()
        {
            if (Frame3by3RadioBtn.IsChecked == true)
                return 3;
            else if (Frame5by5RadioBtn.IsChecked == true)
                return 5;
            else
                throw new NotImplementedException("Nie zaimplementowano tego rozmiaru ramki. Dostępne rozmiary ramek to 3 i 5!");
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

        private byte GetMedianPixel(ref int[,] neighbours, int offset)
        {
            List<int> neighbouringPixelValues = new List<int>(neighbours.Length);
            for (int i = 0; i < neighbours.GetLength(0); i++)
            {
                for (int j = 0; j < neighbours.GetLength(1); j++)
                {
                    neighbouringPixelValues.Add(pixels[neighbours[i, j] + offset]);
                }
            }

            neighbouringPixelValues.Sort();
            return (byte)Math.Abs(neighbouringPixelValues[neighbouringPixelValues.Count/2]);
        }

        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            MedianFilter();
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
