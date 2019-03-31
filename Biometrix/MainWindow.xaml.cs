using System;
using System.IO;
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
        private WriteableBitmap originalBitmap;
        private WriteableBitmap modifiedBitmap;
        private int stride;
        private int bytesPerPixel;
        byte[] originalPixels;
        byte[] modifiedPixels;

        bool isDrawing = false;

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
                originalBitmap = new WriteableBitmap(new BitmapImage(new Uri(dialog.FileName)));
                modifiedBitmap = new WriteableBitmap(new BitmapImage(new Uri(dialog.FileName)));

                OriginalImage.Source = originalBitmap;
                ModifiedImage.Source = modifiedBitmap;

                int imageWidth = (int)OriginalImage.Source.Width;
                int imageHeight = (int)OriginalImage.Source.Height;

                Title = $"Biometrix - {dialog.FileName} ({imageWidth}x{imageHeight})";

                bytesPerPixel = originalBitmap.Format.BitsPerPixel / 8;
                stride = imageWidth * bytesPerPixel;
                originalPixels = new byte[stride * imageHeight];
                modifiedPixels = new byte[stride * imageHeight];

                originalBitmap.CopyPixels(originalPixels, stride, 0);
                modifiedBitmap.CopyPixels(modifiedPixels, stride, 0);

                SaveImageMenuItem.IsEnabled = true;
            }
        }


        private void SaveImageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            String filter = "Plik JPEG (*.jpeg)|*.jpeg" +
                            "|Plik PNG (*.png)|*.png" +
                            "|Plik GIF (*.gif)|*.gif" +
                            "|Plik BMP (*.bmp)|*.bmp" +
                            "|Plik TIFF (*.tiff)|*.tiff";
            dialog.Filter = filter;
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            bool result = (bool)dialog.ShowDialog();
            if (result)
            {
                string extension = System.IO.Path.GetExtension(dialog.FileName);
                FileStream saveStream = new FileStream(dialog.FileName, FileMode.Create);

                BitmapEncoder encoder = null;

                switch (extension)
                {
                    case ".jpeg":
                        encoder = new JpegBitmapEncoder();
                        break;
                    case ".png":
                        encoder = new PngBitmapEncoder();
                        break;
                    case ".gif":
                        encoder = new GifBitmapEncoder();
                        break;
                    case ".bmp":
                        encoder = new BmpBitmapEncoder();
                        break;
                    case ".tiff":
                        encoder = new TiffBitmapEncoder();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(extension);
                }
                encoder.Frames.Add(BitmapFrame.Create((BitmapSource)ModifiedImage.Source));
                encoder.Save(saveStream);
                saveStream.Close();

                ImageStatusBarItem.Content = $"Pomyślnie zapisano modyfikacje do pliku {dialog.FileName}";
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
            try
            {
                ScaleTransform st = (ScaleTransform)ModifiedImage.LayoutTransform;
                st.ScaleX = ZoomModifiedSlider.Value;
                st.ScaleY = ZoomModifiedSlider.Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            ZoomModifiedLabel.Content = $"{ZoomModifiedSlider.Value}00%";
        }

        private void OriginalImage_MouseMove(object sender, MouseEventArgs e)
        {
            //FIXME: x i y nie mogą być większe/równe wymiarom obrazka
            Point p = e.GetPosition(OriginalImage);
            int x = (int)p.X;
            int y = (int)p.Y;

            int index = x * bytesPerPixel + y * stride;

            int rValue = originalPixels[index + 2];
            int gValue = originalPixels[index + 1];
            int bValue = originalPixels[index];

            ImageStatusBarItem.Content = $"Oryginał: ({x},{y}) - R:{rValue}, G:{gValue}, B:{bValue}";
        }

        private void ModifiedImage_MouseMove(object sender, MouseEventArgs e)
        {
            //FIXME: x i y nie mogą być większe/równe wymiarom obrazka
            Point p = e.GetPosition(ModifiedImage);
            int x = (int)p.X;
            int y = (int)p.Y;

            int index = x * bytesPerPixel + y * stride;

            int rValue = modifiedPixels[index + 2];
            int gValue = modifiedPixels[index + 1];
            int bValue = modifiedPixels[index];

            ImageStatusBarItem.Content = $"Modyfikacja: ({x},{y}) - R:{rValue}, G:{gValue}, B:{bValue}";

            if (IsDrawingCheckBox.IsChecked == true && isDrawing)
            {
                modifiedPixels[index + 2] = (byte)SpinValueR.Value;
                modifiedPixels[index + 1] = (byte)SpinValueG.Value;
                modifiedPixels[index]     = (byte)SpinValueB.Value;

                modifiedBitmap.WritePixels(new Int32Rect(0, 0, (int)modifiedBitmap.Width, (int)modifiedBitmap.Height), modifiedPixels, stride, 0);
            }
        }

        private void ModifiedImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDrawing = true;
        }

        private void ModifiedImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDrawing = false;
        }

        private void OriginalImage_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(OriginalImage);
            int x = (int)p.X;
            int y = (int)p.Y;

            int index = x * bytesPerPixel + y * stride;
            SpinValueR.Value = originalPixels[index + 2];
            SpinValueG.Value = originalPixels[index + 1];
            SpinValueB.Value = originalPixels[index];
        }

        private void ModifiedImage_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(ModifiedImage);
            int x = (int)p.X;
            int y = (int)p.Y;

            int index = x * bytesPerPixel + y * stride;
            SpinValueR.Value = originalPixels[index + 2];
            SpinValueG.Value = originalPixels[index + 1];
            SpinValueB.Value = originalPixels[index];
        }
    }
}
