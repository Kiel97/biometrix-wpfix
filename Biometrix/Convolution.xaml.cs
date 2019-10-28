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
    public enum FilterType
    {
        PREWITT_H, PREWITT_V, SOBEL_H, SOBEL_V, LAPLACE, SHARP, CORNER, CUSTOM
    }

    public partial class Convolution : Window
    {
        public byte[] modifiedPixels;

        WriteableBitmap previewBitmap;
        byte[] pixels;
        int stride;
        int bytesPerPixel;
        int width;
        int height;

        FilterType filterType = FilterType.CUSTOM;

        //http://www.algorytm.org/przetwarzanie-obrazow/filtrowanie-obrazow.html
        //-1 -1 -1
        // 0  0  0
        // 1  1  1
        readonly int[,] PREWITT_HOR_FRAME = {{-1,  0,  1},
                                             {-1,  0,  1},
                                             {-1,  0,  1}};

        // 1  0 -1
        // 1  0 -1
        // 1  0 -1
        readonly int[,] PREWITT_VER_FRAME = {{ 1,  1,  1},
                                             { 0,  0,  0},
                                             {-1, -1, -1}};

        // 1  2  1
        // 0  0  0
        //-1 -2 -1
        readonly int[,] SOBEL_HOR_FRAME = {{ 1,  0, -1},
                                           { 2,  0, -2},
                                           { 1,  0, -1}};

        // 1  0 -1
        // 2  0 -2
        // 1  0 -1
        readonly int[,] SOBEL_VER_FRAME = {{ 1,  2,  1},
                                           { 0,  0,  0},
                                           {-1, -2, -1}};

        // 0 -1  0
        //-1  4 -1
        // 0 -1  0
        readonly int[,] LAPLACE_FRAME = {{ 0, -1,  0},
                                         {-1,  4, -1},
                                         { 0, -1,  0}};

        //-1 -1 -1
        //-1  9 -1
        //-1 -1 -1
        readonly int[,] SHAPR_FRAME =   {{-1, -1, -1},
                                         {-1,  9, -1},
                                         {-1, -1, -1}};

        // 1  1  1
        // 1 -2 -1
        // 1 -1 -1
        readonly int[,] CORNER_FRAME = {{ 1,  1,  1},
                                        { 1, -2, -1},
                                        { 1, -1, -1}};

        public Convolution(byte[] pixels, int stride, int width, int height, WriteableBitmap modifiedBitmap, int bytesPerPixel)
        {
            InitializeComponent();
            ConnectEvents();

            previewBitmap = new WriteableBitmap(modifiedBitmap);

            this.pixels = pixels;
            this.stride = stride;
            this.width = width;
            this.height = height;
            this.bytesPerPixel = bytesPerPixel;

            modifiedPixels = new byte[pixels.Length];

            UpdatePreviewImage(pixels);
        }

        private void ConnectEvents()
        {
            PrewittVerRadio.Checked += FilterTypeRadio_Checked;
            PrewittHorRadio.Checked += FilterTypeRadio_Checked;
            SobelVerRadio.Checked += FilterTypeRadio_Checked;
            SobelHorRadio.Checked += FilterTypeRadio_Checked;
            LaplaceRadio.Checked += FilterTypeRadio_Checked;
            SharpRadio.Checked += FilterTypeRadio_Checked;
            CornerDetectRadio.Checked += FilterTypeRadio_Checked;
            CustomRadio.Checked += FilterTypeRadio_Checked;
        }

        private void UpdatePreviewImage(byte[] pixels)
        {
            BitmapSource bitmap = BitmapImage.Create(width, height, 96, 96, PixelFormats.Bgr32, null, pixels, stride);
            previewBitmap = new WriteableBitmap(bitmap);

            PreviewImage.Source = previewBitmap;

            modifiedPixels = pixels;
        }

        private void ConvoluteWithFilter()
        {
            int windowsize = 3;
            int radius = windowsize / 2;

            int[,] frame = GetFrame();

            byte[] p = new byte[pixels.Length];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int index = i * bytesPerPixel + j * stride;

                    if (IsImageBorder(i, j, height, width, radius))
                    {
                        p[index] = 0;
                        p[index + 1] = 0;
                        p[index + 2] = 0;
                    }
                    else
                    {
                        int[,] neighbours = GetNeighbouringPixelIndexes(i, j, windowsize);

                        for (int colorOffset = 0; colorOffset < 3; colorOffset++)
                        {//0 - niebieski, 1 - zielony, 2 - czerwony, 3 - alfa
                            //byte pixelValue = pixels[neighbours[1, 1] + colorOffset];
                            byte pixelValue = CalculateNewPixelValue(ref neighbours, ref frame, colorOffset);
                            p[index + colorOffset] = pixelValue;
                        }
                    }

                    p[index + 3] = pixels[index + 3];
                }
            }

            UpdatePreviewImage(p);
        }

        private bool IsImageBorder(int i, int j, int height, int width, int radius)
        {
            return i < radius || j < radius || i >= height - radius || j >= width - radius;
        }

        private int[,] GetFrame()
        {
            switch (filterType)
            {   
                case FilterType.PREWITT_H:
                    return PREWITT_HOR_FRAME;
                case FilterType.PREWITT_V:
                    return PREWITT_VER_FRAME;
                case FilterType.SOBEL_H:
                    return SOBEL_HOR_FRAME;
                case FilterType.SOBEL_V:
                    return SOBEL_VER_FRAME;
                case FilterType.LAPLACE:
                    return LAPLACE_FRAME;
                case FilterType.SHARP:
                    return SHAPR_FRAME;
                case FilterType.CORNER:
                    return CORNER_FRAME;
                case FilterType.CUSTOM:
                    int[,] custom_frame = new int[3, 3];
                    int result;
                    custom_frame[0, 0] = int.TryParse(Fm1m1.Text, out result) ? result : 1;
                    custom_frame[0, 1] = int.TryParse(F0m1.Text, out result) ? result : 1;
                    custom_frame[0, 2] = int.TryParse(F1m1.Text, out result) ? result : 1;
                    custom_frame[1, 0] = int.TryParse(Fm10.Text, out result) ? result : 1;
                    custom_frame[1, 1] = int.TryParse(F00.Text, out result) ? result : 1;
                    custom_frame[1, 2] = int.TryParse(F10.Text, out result) ? result : 1;
                    custom_frame[2, 0] = int.TryParse(Fm11.Text, out result) ? result : 1;
                    custom_frame[2, 1] = int.TryParse(F01.Text, out result) ? result : 1;
                    custom_frame[2, 2] = int.TryParse(F11.Text, out result) ? result : 1;
                    return custom_frame;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private int[,] GetNeighbouringPixelIndexes(int x, int y, int windowsize)
        {
            int[,] neighbours = new int[windowsize, windowsize];
            int radius = windowsize / 2;

            for (int i = 0; i < windowsize; i++)
            {
                for (int j = 0; j < windowsize; j++)
                {
                    neighbours[i, j] = (x - radius + i) * bytesPerPixel + (y - radius + j) * stride;
                }
            }

            /*Przykład:
            neighbours[0, 0] = (x - 1) * bytesPerPixel + (y - 1) * stride;
            neighbours[0, 1] = (x - 1) * bytesPerPixel + y * stride;
            neighbours[0, 2] = (x - 1) * bytesPerPixel + (y + 1) * stride;
            neighbours[1, 0] = x * bytesPerPixel + (y - 1) * stride;
            neighbours[1, 1] = x * bytesPerPixel + y * stride;
            neighbours[1, 2] = x * bytesPerPixel + (y + 1) * stride;
            neighbours[2, 0] = (x + 1) * bytesPerPixel + (y - 1) * stride;
            neighbours[2, 1] = (x + 1) * bytesPerPixel + y * stride;
            neighbours[2, 2] = (x + 1) * bytesPerPixel + (y + 1) * stride;*/

            return neighbours;
        }

        private byte CalculateNewPixelValue(ref int[,] neighbours, ref int[,] frame, int offset)
        {
            int pixelSum = CalculateWeightedSumOfPixel(ref neighbours, ref frame, offset);
            int weightSum = CalculateSumOfWeight(ref frame);

            return (byte)Math.Abs(pixelSum/weightSum);
        }

        private int CalculateWeightedSumOfPixel(ref int[,] neighbours, ref int[,] frame, int offset)
        {
            int sum = 0;

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    sum += pixels[neighbours[i,j] + offset] * frame[i,j];

            return sum;
        }

        private int CalculateSumOfWeight(ref int[,] frame)
        {
            int sum = 0;

            for (int i = 0; i < frame.GetLength(0); i++)
                for (int j = 0; j < frame.GetLength(1); j++)
                    sum += frame[i, j];

            if (sum == 0)
                return 1;

            return sum;
        }

        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            ConvoluteWithFilter();
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

        private void FilterTypeRadio_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;

            CustomFrameGrid.IsEnabled = false;

            switch (radioButton.Name)
            {
                case "PrewittVerRadio":
                    filterType = FilterType.PREWITT_V;
                    break;
                case "PrewittHorRadio":
                    filterType = FilterType.PREWITT_H;
                    break;
                case "SobelVerRadio":
                    filterType = FilterType.SOBEL_V;
                    break;
                case "SobelHorRadio":
                    filterType = FilterType.SOBEL_H;
                    break;
                case "LaplaceRadio":
                    filterType = FilterType.LAPLACE;
                    break;
                case "SharpRadio":
                    filterType = FilterType.SHARP;
                    break;
                case "CornerDetectRadio":
                    filterType = FilterType.CORNER;
                    break;
                case "CustomRadio":
                    filterType = FilterType.CUSTOM;
                    CustomFrameGrid.IsEnabled = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Nie uwzględniono radioButtona innego, niż wyżej zdefiniowane.");
            }
        }
    }
}
