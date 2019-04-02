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
    /// Logika interakcji dla klasy Histogram.xaml
    /// </summary>
    public partial class Histogram : Window
    {
        enum HistogramColorMode
        {
            GRAYSCALE, RED, GREEN, BLUE
        }

        private int[] redHistogram;
        private int[] greenHistogram;
        private int[] blueHistogram;
        private int[] averagedHistogram;

        public Histogram(byte[] imagePixels, bool grayScale)
        {
            redHistogram = GetHistogramFromByteArray(imagePixels, HistogramColorMode.RED);
            greenHistogram = GetHistogramFromByteArray(imagePixels, HistogramColorMode.GREEN);
            blueHistogram = GetHistogramFromByteArray(imagePixels, HistogramColorMode.BLUE);
            averagedHistogram = GetHistogramFromByteArray(imagePixels, HistogramColorMode.GRAYSCALE);

            InitializeComponent();

            if (grayScale)
            {
                RedValuesRadioBtn.IsEnabled = false;
                GreenValuesRadioBtn.IsEnabled = false;
                BlueValuesRadioBtn.IsEnabled = false;
            }
        }

        private void ValuesRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            int[] histogram;

            switch (radioButton.Name)
            {
                case "AveragedValuesRadioBtn":
                    histogram = averagedHistogram;
                    break;
                case "RedValuesRadioBtn":
                    histogram = redHistogram;
                    break;
                case "GreenValuesRadioBtn":
                    histogram = greenHistogram;
                    break;
                case "BlueValuesRadioBtn":
                    histogram = blueHistogram;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            HistogramChart.DisplayHistogram(histogram);
        }

        private int[] GetHistogramFromByteArray(byte[] pixels, HistogramColorMode colorMode)
        {
            int[] histogram = new int[256];

            for (int j = 0; j < pixels.Length; j += 4)
            {
                int value;
                switch (colorMode)
                {
                    case HistogramColorMode.RED:
                        value = pixels[j + 2];
                        break;
                    case HistogramColorMode.GREEN:
                        value = pixels[j + 1];
                        break;
                    case HistogramColorMode.BLUE:
                        value = pixels[j];
                        break;
                    case HistogramColorMode.GRAYSCALE:
                        value = (int)(pixels[j + 2] + pixels[j + 1] + pixels[j]) / 3;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                histogram[value] += 1;
            }
            return histogram;
        }
    }
}
