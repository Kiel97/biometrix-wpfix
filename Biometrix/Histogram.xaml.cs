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

        private HistogramColorMode colorMode;
        private byte[] pixels;
        private int[] histogram;

        public Histogram()
        {
            InitializeComponent();
        }

        public Histogram(byte[] imagePixels)
        {
            pixels = imagePixels;
            InitializeComponent();
            HistogramChart.DisplayHistogram(histogram);
        }

        private void ValuesRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;

            switch (radioButton.Name)
            {
                case "AveragedValuesRadioBtn":
                    colorMode = HistogramColorMode.GRAYSCALE;
                    break;
                case "RedValuesRadioBtn":
                    colorMode = HistogramColorMode.RED;
                    break;
                case "GreenValuesRadioBtn":
                    colorMode = HistogramColorMode.GREEN;
                    break;
                case "BlueValuesRadioBtn":
                    colorMode = HistogramColorMode.BLUE;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            histogram = getHistogramFromByteArray(pixels);

            try
            {
                HistogramChart.DisplayHistogram(histogram);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private int[] getHistogramFromByteArray(byte[] pixels)
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
