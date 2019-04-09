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
        private int[] redHistogram;
        private int[] greenHistogram;
        private int[] blueHistogram;
        private int[] averagedHistogram;

        public Histogram(byte[] imagePixels, bool grayScale)
        {
            redHistogram = HistogramCreator.GetHistogramFromByteArray(imagePixels, HistogramCreator.ColorMode.RED);
            greenHistogram = HistogramCreator.GetHistogramFromByteArray(imagePixels, HistogramCreator.ColorMode.GREEN);
            blueHistogram = HistogramCreator.GetHistogramFromByteArray(imagePixels, HistogramCreator.ColorMode.BLUE);
            averagedHistogram = HistogramCreator.GetHistogramFromByteArray(imagePixels, HistogramCreator.ColorMode.GRAYSCALE);

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
    }
}
