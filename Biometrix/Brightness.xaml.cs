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
    /// Logika interakcji dla klasy Brightness.xaml
    /// </summary>
    public partial class Brightness : Window
    {
        WriteableBitmap previewBitmap;
        WriteableBitmap beforeBitmap;
        int stride;
        int width;
        int height;

        public Brightness(byte[] pixels, int stride, int width, int height, WriteableBitmap modifiedBitmap)
        {
            InitializeComponent();
            beforeBitmap = modifiedBitmap;
            previewBitmap = new WriteableBitmap(modifiedBitmap);
            previewBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
            PreviewImage.Source = previewBitmap;
        }

        private void ASpinValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                if (ASpinValue.Value == 1)
                {
                    PreviewButton.IsEnabled = false;
                    ConfirmButton.IsEnabled = false;
                }
                else
                {
                    PreviewButton.IsEnabled = true;
                    ConfirmButton.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
