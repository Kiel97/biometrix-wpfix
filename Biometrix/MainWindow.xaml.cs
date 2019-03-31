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
using Microsoft.Win32;

namespace Biometrix
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CloseAppMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Czy chcesz zamknąć pracę z aplikacją?", "Zakończ", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
                Application.Current.Shutdown();
        }

        private void OpenImageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            String filter = "Wszystkie wspierane (*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.tif;*.tiff)|*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.tif;*.tiff|" +
                            "Pliki JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg" + 
                            "|Pliki PNG (*.png)|*.png" + 
                            "|Pliki GIF (*.gif)|*.gif" + 
                            "|Pliki BMP (*.bmp)|*.bmp" + 
                            "|Pliki TIFF (*.tif;*.tiff)|*.tif;*.tiff|" +
                            "Wszystkie pliki (*.*)|*.*";
            dialog.Filter = filter;
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            bool result = (bool)dialog.ShowDialog();
            if (result)
            {
                OriginalImage.Source = new BitmapImage(new Uri(dialog.FileName));
                ModifiedImage.Source = new WriteableBitmap(new BitmapImage(new Uri(dialog.FileName)));

                int imageWidth = (int)OriginalImage.Source.Width;
                int imageHeight = (int)OriginalImage.Source.Height;

                Title = $"Biometrix - {dialog.FileName} ({imageWidth}x{imageHeight})";
            }
        }

        private void SpinValueRGB_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {//FIXME: Wywala 3 NullReferenceExceptions, bo ładuje się szybciej, niż spinery - nie wiem jak to naprawić
                ColorLabel.Background = new SolidColorBrush(Color.FromRgb((byte)SpinValueR.Value, (byte)SpinValueG.Value, (byte)SpinValueB.Value));
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void ZoomOriginalSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                ScaleTransform st = (ScaleTransform)OriginalImage.LayoutTransform;
                st.ScaleX = ZoomOriginalSlider.Value;
                st.ScaleY = ZoomOriginalSlider.Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            ZoomOriginalLabel.Content = $"{ZoomOriginalSlider.Value}00%";
        }

        private void ZoomModifiedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ZoomModifiedLabel.Content = $"{ZoomModifiedSlider.Value}00%";
        }
    }
}
