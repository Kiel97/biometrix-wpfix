﻿using System;
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

        private byte[] pixels;
        private HistogramColorMode colorMode;

        public Histogram()
        {
            InitializeComponent();
        }

        public Histogram(byte[] imagePixels)
        {
            InitializeComponent();
            pixels = imagePixels;
        }

        private void ValuesRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            if (AveragedValuesRadioBtn.IsChecked == true)
            {
                colorMode = HistogramColorMode.GRAYSCALE;
            }
            else if (RedValuesRadioBtn.IsChecked == true)
            {
                colorMode = HistogramColorMode.RED;
            }
            else if (GreenValuesRadioBtn.IsChecked == true)
            {
                colorMode = HistogramColorMode.GREEN;
            }
            else if (BlueValuesRadioBtn.IsChecked == true)
            {
                colorMode = HistogramColorMode.BLUE;
            }
        }
    }
}
