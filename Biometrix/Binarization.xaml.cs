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
                p[i + 1] = LUT[pixels[i + 1]];
                p[i + 2] = LUT[pixels[i + 2]];
                p[i + 3] = pixels[i + 3];
            }

            UpdatePreviewImage(p);
        }

        private void OtsuThresholdCalculate()
        {
            int[] histogram = HistogramCreator.GetHistogramFromByteArray(pixels, HistogramCreator.ColorMode.GRAYSCALE);

            int threshold = 0;
            double minClassVariance = double.MaxValue;

            for (int T = 0; T < 256; T++)
            {
                double classVariance = CalculateClassVariance(ref histogram, T);
                if (classVariance < minClassVariance)
                {
                    threshold = T;
                    minClassVariance = classVariance;
                }
            }

            MessageBox.Show($"Znaleziony optymalny próg metodą Otsu wynosi {threshold}.");

            ThresholdSpinValue.Value = (byte)threshold;
        }

        public double CalculateClassVariance(ref int[] p, int T)
        {
            int weightBackground = CalculateWeightBackground(ref p, T);
            int weightForeground = CalculateWeightForeground(ref p, T);

            double meanBackground = CalculateMeanBackground(ref p, T);
            double meanForeground = CalculateMeanForeground(ref p, T);

            double standardDeviationBackground = CalculateStandardDeviationBackground(ref p, T, weightBackground, meanBackground);
            double standardDeviationForeground = CalculateStandardDeviationForeground(ref p, T, weightForeground, meanForeground);

            double classStandardDeviation = weightForeground * Math.Pow(standardDeviationForeground, 2) + weightBackground * Math.Pow(standardDeviationBackground, 2);

            return classStandardDeviation;
        }

        public int CalculateWeightBackground(ref int[] p, int T)
        {
            int sum = 0;
            for (int i = 0; i < T; i++)
            {
                sum += p[i];
            }
            return sum;
        }

        public int CalculateWeightForeground(ref int[] p, int T)
        {
            int sum = 0;
            for (int i = T; i < p.Length; i++)
            {
                sum += p[i];
            }
            return sum;
        }

        public double CalculateMeanBackground(ref int[]p, int T)
        {
            if (T == 0)
                return 0;

            int sum = CalculateWeightBackground(ref p, T);
            double mean = sum / T;
            return mean;
        }

        public double CalculateMeanForeground(ref int[]p , int T)
        {
            int sum = CalculateWeightForeground(ref p, T);
            double mean = sum / (p.Length - T);
            return mean;
        }

        public double CalculateStandardDeviationBackground(ref int[]p, int T, int weightBackground, double meanBackground)
        {
            double standardDeviation = 0;
            for (int i = 0; i < T; i++)
            {
                standardDeviation += (p[i]*(Math.Pow(i - meanBackground,2))/weightBackground);
            }

            return standardDeviation;
        }

        public double CalculateStandardDeviationForeground(ref int[] p, int T, int weightForeground, double meanForeground)
        {
            double standardDeviation = 0;
            for (int i = T; i < p.Length; i++)
            {
                standardDeviation += (p[i] * (Math.Pow(i - meanForeground, 2)) / weightForeground);
            }

            return standardDeviation;
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

        private void OtsuThresholdBtn_Click(object sender, RoutedEventArgs e)
        {
            OtsuThresholdCalculate();
        }
    }
}
