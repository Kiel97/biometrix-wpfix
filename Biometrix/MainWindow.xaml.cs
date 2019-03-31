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
        byte[] pixels;

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
                pixels = new byte[stride * imageHeight];

                originalBitmap.CopyPixels(pixels, stride, 0);

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

                switch (extension)
                {
                    case ".jpeg":
                        JpegBitmapEncoder jpegEncoder = new JpegBitmapEncoder();
                        jpegEncoder.Frames.Add(BitmapFrame.Create((BitmapSource)ModifiedImage.Source));
                        jpegEncoder.Save(saveStream);
                        break;
                    case ".png":
                        PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
                        pngEncoder.Frames.Add(BitmapFrame.Create((BitmapSource)ModifiedImage.Source));
                        pngEncoder.Save(saveStream);
                        break;
                    case ".gif":
                        GifBitmapEncoder gifEncoder = new GifBitmapEncoder();
                        gifEncoder.Frames.Add(BitmapFrame.Create((BitmapSource)ModifiedImage.Source));
                        gifEncoder.Save(saveStream);
                        break;
                    case ".bmp":
                        BmpBitmapEncoder bmpEncoder = new BmpBitmapEncoder();
                        bmpEncoder.Frames.Add(BitmapFrame.Create((BitmapSource)ModifiedImage.Source));
                        bmpEncoder.Save(saveStream);
                        break;
                    case ".tiff":
                        TiffBitmapEncoder tiffEncoder = new TiffBitmapEncoder();
                        tiffEncoder.Frames.Add(BitmapFrame.Create((BitmapSource)ModifiedImage.Source));
                        tiffEncoder.Save(saveStream);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(extension);
                }
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
            Point p = e.GetPosition(OriginalImage);
            int x = (int)p.X;
            int y = (int)p.Y;

            int index = x * bytesPerPixel + y * stride;

            int rValue = pixels[index + 2];
            int gValue = pixels[index + 1];
            int bValue = pixels[index];

            ImageStatusBarItem.Content = $"({x},{y}) - R:{rValue}, G:{gValue}, B:{bValue}";
        }

        private void ModifiedImage_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(ModifiedImage);
            ImageStatusBarItem.Content = $"({(int)p.X},{(int)p.Y})";
        }
    }
}
