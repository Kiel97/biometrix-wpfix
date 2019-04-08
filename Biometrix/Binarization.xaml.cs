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
        public Binarization()
        {
            InitializeComponent();
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
