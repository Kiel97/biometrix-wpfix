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
        private BitmapImage loadedBitmap;
        private WriteableBitmap originalBitmap;
        private WriteableBitmap modifiedBitmap;
        private int stride;
        private int bytesPerPixel;
        byte[] originalPixels;
        byte[] modifiedPixels;

        bool isDrawing = false;
        bool grayScale = false;

        public MainWindow()
        {
            InitializeComponent();

            SpinValueR.ValueChanged += SpinValueRGB_ValueChanged;
            SpinValueG.ValueChanged += SpinValueRGB_ValueChanged;
            SpinValueB.ValueChanged += SpinValueRGB_ValueChanged;

            ZoomOriginalSlider.ValueChanged += ZoomOriginalSlider_ValueChanged;
            ZoomModifiedSlider.ValueChanged += ZoomModifiedSlider_ValueChanged;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Czy chcesz zakończyć pracę z aplikacją?", "Zakończ", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
                e.Cancel = true;
        }

        private void CloseAppMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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
                loadedBitmap = new BitmapImage(new Uri(dialog.FileName));
                originalBitmap = new WriteableBitmap(new FormatConvertedBitmap(loadedBitmap, PixelFormats.Bgra32, null, 0));
                modifiedBitmap = new WriteableBitmap(new FormatConvertedBitmap(loadedBitmap, PixelFormats.Bgra32, null, 0));

                grayScale = IsInGrayScaleMode(modifiedBitmap);

                OriginalImage.Source = originalBitmap;
                ModifiedImage.Source = modifiedBitmap;

                int imageWidth = (int)OriginalImage.Source.Width;
                int imageHeight = (int)OriginalImage.Source.Height;

                Title = $"Biometrix - {dialog.FileName} ({imageWidth}x{imageHeight})";

                bytesPerPixel = modifiedBitmap.Format.BitsPerPixel / 8;
                stride = imageWidth * bytesPerPixel;
                originalPixels = new byte[stride * imageHeight];
                modifiedPixels = new byte[stride * imageHeight];

                originalBitmap.CopyPixels(originalPixels, stride, 0);
                modifiedBitmap.CopyPixels(modifiedPixels, stride, 0);

                SaveImageMenuItem.IsEnabled = true;
                ColorSectionMenuItem.IsEnabled = true;
                FiltersMenuItem.IsEnabled = true;
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

                BitmapSource saveSource = (BitmapSource)ModifiedImage.Source;

                if (grayScale)
                    encoder.Frames.Add(BitmapFrame.Create(new FormatConvertedBitmap(saveSource, PixelFormats.Gray32Float, null, 0)));
                else
                    encoder.Frames.Add(BitmapFrame.Create(saveSource));

                encoder.Save(saveStream);
                saveStream.Close();

                ImageStatusBarItem.Content = $"Pomyślnie zapisano modyfikacje do pliku {dialog.FileName}";
            }
        }


        private void SpinValueRGB_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ColorLabel.Background = new SolidColorBrush(Color.FromRgb((byte)SpinValueR.Value, (byte)SpinValueG.Value, (byte)SpinValueB.Value));
        }

        private void ZoomOriginalSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ScaleImage(OriginalImage, ZoomOriginalSlider, ZoomOriginalLabel);
        }

        private void ZoomModifiedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ScaleImage(ModifiedImage, ZoomModifiedSlider, ZoomModifiedLabel);
        }

        private void ScaleImage(Image image, Slider zoomSlider, Label zoomLabel)
        {
            ScaleTransform st = (ScaleTransform)image.LayoutTransform;
            st.ScaleX = zoomSlider.Value;
            st.ScaleY = zoomSlider.Value;
            zoomLabel.Content = $"{zoomSlider.Value}00%";
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

            if (IsInGrayScaleMode((BitmapSource)OriginalImage.Source))
            {
                int avgValue = (rValue + gValue + bValue) / 3;
                rValue = avgValue;
                gValue = avgValue;
                bValue = avgValue;
            }

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

            if (IsInGrayScaleMode((BitmapSource)ModifiedImage.Source))
            {
                int avgValue = (rValue + gValue + bValue) / 3;
                rValue = avgValue;
                gValue = avgValue;
                bValue = avgValue;
            }

            ImageStatusBarItem.Content = $"Modyfikacja: ({x},{y}) - R:{rValue}, G:{gValue}, B:{bValue}";

            if (IsDrawingCheckBox.IsChecked == true && isDrawing)
            {
                modifiedPixels[index + 2] = (byte)SpinValueR.Value;
                modifiedPixels[index + 1] = (byte)SpinValueG.Value;
                modifiedPixels[index]     = (byte)SpinValueB.Value;

                modifiedBitmap.WritePixels(new Int32Rect(0, 0, (int)modifiedBitmap.Width, (int)modifiedBitmap.Height), modifiedPixels, stride, 0);

                if (grayScale && (((byte)SpinValueR.Value != (byte)SpinValueB.Value) || ((byte)SpinValueG.Value != (byte)SpinValueB.Value)))
                    grayScale = false;
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
            GetRGBvaluesAndSetThemToPaintSpinners(p, originalPixels, IsInGrayScaleMode((BitmapSource)OriginalImage.Source));
        }

        private void ModifiedImage_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(ModifiedImage);
            GetRGBvaluesAndSetThemToPaintSpinners(p, modifiedPixels, IsInGrayScaleMode((BitmapSource)ModifiedImage.Source));
        }

        private void GetRGBvaluesAndSetThemToPaintSpinners(Point p, byte[] pixels, bool isGrayScale)
        {
            int x = (int)p.X;
            int y = (int)p.Y;

            int index = x * bytesPerPixel + y * stride;

            byte rValue, gValue, bValue;
            rValue = pixels[index + 2];
            gValue = pixels[index + 1];
            bValue = pixels[index];

            if (isGrayScale)
            {
                byte avgValue = (byte)((rValue + gValue + bValue) / 3);
                rValue = avgValue;
                gValue = avgValue;
                bValue = avgValue;
            }

            SpinValueR.Value = rValue;
            SpinValueG.Value = gValue;
            SpinValueB.Value = bValue;
        }

        private bool IsInGrayScaleMode(BitmapSource bitmapSource)
        {
            return bitmapSource.Format == PixelFormats.Gray16 || 
                   bitmapSource.Format == PixelFormats.Gray2 ||
                   bitmapSource.Format == PixelFormats.Gray32Float ||
                   bitmapSource.Format == PixelFormats.Gray4 ||
                   bitmapSource.Format == PixelFormats.Gray8 ||
                   bitmapSource.Format == PixelFormats.BlackWhite;
        }

        private void ShowHistogramMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Histogram histogram = new Histogram(modifiedPixels, grayScale);
            histogram.ShowDialog();
        }

        private void ChangeBrightnessMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Brightness brightness = new Brightness(modifiedPixels, stride,(int)ModifiedImage.ActualWidth, (int)ModifiedImage.ActualHeight, modifiedBitmap);
            if (brightness.ShowDialog() == true)
            {
                modifiedPixels = brightness.modifiedPixels;

                UpdateModifiedImageSource();
            }
        }

        private void StretchHistogramMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Czy chcesz rozciągnąć histogram dla podanego obrazka?", "Rozciągnięcie histogramu", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                StretchHistogram();
            }
        }

        private void StretchHistogram()
        {
            int[] redHistogram = HistogramCreator.GetHistogramFromByteArray(modifiedPixels, HistogramCreator.ColorMode.RED);
            int[] greenHistogram = HistogramCreator.GetHistogramFromByteArray(modifiedPixels, HistogramCreator.ColorMode.GREEN);
            int[] blueHistogram = HistogramCreator.GetHistogramFromByteArray(modifiedPixels, HistogramCreator.ColorMode.BLUE);

            int minRed = HistogramCreator.GetIndexOfMinValue(redHistogram);
            int minGreen = HistogramCreator.GetIndexOfMinValue(greenHistogram);
            int minBlue = HistogramCreator.GetIndexOfMinValue(blueHistogram);
            int maxRed = HistogramCreator.GetIndexOfMaxValue(redHistogram);
            int maxGreen = HistogramCreator.GetIndexOfMaxValue(greenHistogram);
            int maxBlue = HistogramCreator.GetIndexOfMaxValue(blueHistogram);

            byte[] LUTred = new byte[256];
            byte[] LUTgreen = new byte[256];
            byte[] LUTblue = new byte[256];

            for (int i = 0; i < 256; i++)
            {
                LUTred[i] = (byte)(255f / (maxRed - minRed) * (i-minRed));
                LUTgreen[i] = (byte)(255f / (maxGreen - minGreen) * (i-minGreen));
                LUTblue[i] = (byte)(255f / (maxBlue - minBlue) * (i-minBlue));
            }

            for (int i = 0; i < modifiedPixels.Length; i += 4)
            {
                modifiedPixels[i + 2] = LUTred[modifiedPixels[i + 2]];
                modifiedPixels[i + 1] = LUTgreen[modifiedPixels[i + 1]];
                modifiedPixels[i] = LUTblue[modifiedPixels[i]];
            }

            UpdateModifiedImageSource();
        }
        private void EqualizeHistogramMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Czy chcesz wyrównać histogram dla podanego obrazka?", "Wyrównanie histogramu", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                EqualizeHistogram();
            }
        }

        private void EqualizeHistogram()
        {
            int[] redHistogram = HistogramCreator.GetHistogramFromByteArray(modifiedPixels, HistogramCreator.ColorMode.RED);
            int[] greenHistogram = HistogramCreator.GetHistogramFromByteArray(modifiedPixels, HistogramCreator.ColorMode.GREEN);
            int[] blueHistogram = HistogramCreator.GetHistogramFromByteArray(modifiedPixels, HistogramCreator.ColorMode.BLUE);

            int pixelsAmount = redHistogram.Sum();

            double redSum = 0;
            double greenSum = 0;
            double blueSum = 0;

            int minRed = HistogramCreator.GetIndexOfMinValue(redHistogram);
            int minGreen = HistogramCreator.GetIndexOfMinValue(greenHistogram);
            int minBlue = HistogramCreator.GetIndexOfMinValue(blueHistogram);

            double[] distributionRedHistogram = new double[256];
            double[] distributionGreenHistogram = new double[256];
            double[] distributionBlueHistogram = new double[256];

            for (int i = 0; i < 256; i++)
            {
                redSum += (double)redHistogram[i] / (double)pixelsAmount;
                distributionRedHistogram[i] = redSum;

                greenSum += (double)greenHistogram[i] / (double)pixelsAmount;
                distributionGreenHistogram[i] = greenSum;

                blueSum += (double)blueHistogram[i] / (double)pixelsAmount;
                distributionBlueHistogram[i] = blueSum;
            }

            return;
        }

        private void ConvertToGrayMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Czy chcesz przekonwertować obraz do skali szarości?", "Konwersja do skali szarości", MessageBoxButton.OKCancel, MessageBoxImage.Information);
            if (result == MessageBoxResult.OK)
                ConvertImageToGrayscale();
        }

        private void ConvertImageToGrayscale()
        {
            if (grayScale)
                return;

            modifiedBitmap = new WriteableBitmap(new FormatConvertedBitmap(modifiedBitmap, PixelFormats.Gray32Float, null, 0));
            modifiedBitmap = new WriteableBitmap(new FormatConvertedBitmap(modifiedBitmap, PixelFormats.Bgr32, null, 0));
            modifiedBitmap.CopyPixels(modifiedPixels, stride, 0);

            ModifiedImage.Source = modifiedBitmap;

            grayScale = true;
        }

        private void ChangeBinarizationMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!grayScale)
            {
                MessageBoxResult result = MessageBox.Show("Proces binaryzacji można przeprowadzić tylko na obrazach czarnobiałych. Czy chcesz przekonwertować bitmapę do skali szarości?", "Wymaga czarnobiała bitmapa", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if(result == MessageBoxResult.No)
                    return;

                ConvertImageToGrayscale();
            }

            Binarization binarization = new Binarization(modifiedPixels, stride, (int)ModifiedImage.ActualWidth, (int)ModifiedImage.ActualHeight, modifiedBitmap);
            if (binarization.ShowDialog() == true)
            {
                modifiedPixels = binarization.modifiedPixels;

                UpdateModifiedImageSource();
            }
        }

        private void NiblackBinarizationMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!grayScale)
            {
                MessageBoxResult result = MessageBox.Show("Proces binaryzacji można przeprowadzić tylko na obrazach czarnobiałych. Czy chcesz przekonwertować bitmapę do skali szarości?", "Wymaga czarnobiała bitmapa", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result == MessageBoxResult.No)
                    return;

                ConvertImageToGrayscale();
            }

            NiblackBinarization niblackbinarization = new NiblackBinarization(modifiedPixels, stride, (int)ModifiedImage.ActualWidth, (int)ModifiedImage.ActualHeight, modifiedBitmap, bytesPerPixel);
            if (niblackbinarization.ShowDialog() == true)
            {
                modifiedPixels = niblackbinarization.modifiedPixels;

                UpdateModifiedImageSource();
            }
        }

        private void UpdateModifiedImageSource()
        {
            BitmapSource bitmap = BitmapImage.Create((int)ModifiedImage.ActualWidth, (int)ModifiedImage.ActualHeight, 96, 96, PixelFormats.Bgr32, null, modifiedPixels, stride);
            modifiedBitmap = new WriteableBitmap(bitmap);

            ModifiedImage.Source = modifiedBitmap;
        }

        private void ConvolutionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Convolution convolution = new Convolution(modifiedPixels, stride, (int)ModifiedImage.ActualWidth, (int)ModifiedImage.ActualHeight, modifiedBitmap, bytesPerPixel);
            if (convolution.ShowDialog() == true)
            {
                modifiedPixels = convolution.modifiedPixels;

                UpdateModifiedImageSource();
            }
        }

        private void KuwaharMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Kuwahar kuwahar = new Kuwahar(modifiedPixels, stride, (int)ModifiedImage.ActualWidth, (int)ModifiedImage.ActualHeight, modifiedBitmap, bytesPerPixel);
            if (kuwahar.ShowDialog() == true)
            {
                modifiedPixels = kuwahar.modifiedPixels;

                UpdateModifiedImageSource();
            }
        }

        private void MedianMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Median median = new Median(modifiedPixels, stride, (int)ModifiedImage.ActualWidth, (int)ModifiedImage.ActualHeight, modifiedBitmap, bytesPerPixel);
            if (median.ShowDialog() == true)
            {
                modifiedPixels = median.modifiedPixels;

                UpdateModifiedImageSource();
            }
        }

        private void ConvertToGrayScaleMenuItem_Click(object sender, RoutedEventArgs e)
        {
            GrayScale grayScale = new GrayScale(modifiedPixels, stride, (int)ModifiedImage.ActualWidth, (int)ModifiedImage.ActualHeight, modifiedBitmap);
            if(grayScale.ShowDialog() == true)
            {
                modifiedPixels = grayScale.modifiedPixels;

                UpdateModifiedImageSource();
            }
        }
    }
}
