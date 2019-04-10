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
    /// Logika interakcji dla klasy NiblackBinarization.xaml
    /// </summary>
    public partial class NiblackBinarization : Window
    {
        public byte[] modifiedPixels;

        WriteableBitmap previewBitmap;
        byte[] pixels;
        int stride;
        int bytesPerPixel;
        int width;
        int height;

        public NiblackBinarization(byte[] pixels, int stride, int width, int height, WriteableBitmap modifiedBitmap, int bytesPerPixel)
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

            WindowSizeSpinValue.Maximum = Math.Min(width, height);
        }

        private void UpdatePreviewImage(byte[] pixels)
        {
            BitmapSource bitmap = BitmapImage.Create(width, height, 96, 96, PixelFormats.Bgr32, null, pixels, stride);
            previewBitmap = new WriteableBitmap(bitmap);

            PreviewImage.Source = previewBitmap;

            modifiedPixels = pixels;
        }

        private void BinarizeNiblack()
        {
            int windowSize = (int)WindowSizeSpinValue.Value;
            double k = (double)KSpinValue.Value;

            byte[] p = new byte[pixels.Length];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    byte t = CalculateLocalThresholdNiblack(i,j,windowSize,k);

                    int index = i * bytesPerPixel + j * stride;
                    p[index] = t;
                    p[index + 1] = t;
                    p[index + 2] = t;
                    p[index + 3] = pixels[index + 3];
                }
            }

            UpdatePreviewImage(p);
        }

        private byte CalculateLocalThresholdNiblack(int x, int y, int windowSize, double k)
        {
            List<int> neighbouringIndexesList = GetNeighbouringIndexesList(x, y, windowSize);
            List<byte> neighbouringGrayValues = GetColorValuesFromNeighbouringIndexes(neighbouringIndexesList);

            double meanBlockValue = GetMeanOfBlockValue(neighbouringGrayValues);
            double standardDeviationBlockValue = GetStandardDeviationOfBlockValue(neighbouringGrayValues, meanBlockValue);

            return (byte)(meanBlockValue + k * standardDeviationBlockValue);
        }

        private List<int> GetNeighbouringIndexesList(int x, int y, int windowSize)
        {
            List<int> neighbouringIndexes = new List<int>();
            int d = windowSize/2;
            int minX, minY, maxX, maxY;

            minX = x - d;
            minY = y - d;
            maxX = x + d;
            maxY = y + d;

            for (int i = minX; i <= maxX; i++)
            {
                for (int j = minY; j <= maxY; j++)
                {
                    if (i < 0 || j < 0 || i >= height || j >= width)
                        continue;
                    neighbouringIndexes.Add(i * bytesPerPixel + j * stride);
                }
            }
            return neighbouringIndexes;
        }

        private List<byte> GetColorValuesFromNeighbouringIndexes(List<int> neighbouringIndexes)
        {
            List<byte> colorsList = new List<byte>();

            foreach (int index in neighbouringIndexes)
            {
                colorsList.Add(pixels[index]);
            }

            return colorsList;
        }

        private double GetMeanOfBlockValue(List<byte> neighbouringColors)
        {
            int sum = 0;
            foreach (byte value in neighbouringColors)
            {
                sum += value;
            }
            return sum / neighbouringColors.Count;
        }

        private double GetStandardDeviationOfBlockValue(List<byte> neighbouringColors, double meanValue)
        {
            double sum = 0;
            foreach (byte value in neighbouringColors)
            {
                sum += Math.Pow(value - meanValue,2);
            }
            return Math.Sqrt(sum/(neighbouringColors.Count-1));
        }

        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            BinarizeNiblack();
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
