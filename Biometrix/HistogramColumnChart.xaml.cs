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
using System.Windows.Navigation;
using System.Windows.Shapes;
using LiveCharts;
using LiveCharts.Wpf;

namespace Biometrix
{
    /// <summary>
    /// Logika interakcji dla klasy HistogramColumnChart.xaml
    /// </summary>
    public partial class HistogramColumnChart : UserControl
    {
        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> Formatter { get; set; }

        public HistogramColumnChart()
        {
            InitializeComponent();

            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Tytuł histogramu",
                    LineSmoothness = 0,
                    Values = new ChartValues<int>()
                }
            };

            Labels = new string[256];
            for (int i = 0; i < Labels.Length; i++)
            {
                Labels[i] = i.ToString();
            }

            Formatter = value => value.ToString("N");
            DataContext = this;
        }

        public void DisplayHistogram(int[] histogram)
        {
            SeriesCollection[0].Values.Clear();
            for (int i = 0; i < histogram.Length; i++)
            {
                SeriesCollection[0].Values.Add(histogram[i]);
            }
        }
    }
}
