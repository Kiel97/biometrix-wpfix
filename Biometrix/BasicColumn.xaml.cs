using System;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;

namespace Biometrix
{
    /// <summary>
    /// Logika interakcji dla klasy BasicColumn.xaml
    /// </summary>
    public partial class BasicColumn : UserControl
    {
        public BasicColumn()
        {
            InitializeComponent();

            SeriesCollection = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "2015",
                    Values = new ChartValues<double>()
                }
            };

            //adding series will update and animate the chart automatically
            //SeriesCollection.Add(new ColumnSeries
            //{
            //    Title = "2016",
            //    Values = new ChartValues<double> { 11, 56, 42 }
            //});

            //also adding values updates and animates the chart automatically
            SeriesCollection[0].Values.Add(10d);
            SeriesCollection[0].Values.Add(50d);
            SeriesCollection[0].Values.Add(39d);
            SeriesCollection[0].Values.Add(50d);
            SeriesCollection[0].Values.Add(129.49d);
            SeriesCollection[0].Values.Add(15d);
            SeriesCollection[0].Values.Add(15d);

            Labels = new string[256];
            for (int i = 0; i < 256; i++)
            {
                Labels[i] = i.ToString();
            }
            Formatter = value => value.ToString("N");

            DataContext = this;
        }

        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> Formatter { get; set; }
    }
}
