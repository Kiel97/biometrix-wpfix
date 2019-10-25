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
        public byte[] modifiedPixels;

        WriteableBitmap previewBitmap;
        byte[] pixels;
        int stride;
        int width;
        int height;

        const string ADD_FORMULA = "f(x) = x + b";
        const string SUB_FORMULA = "f(x) = x - b";
        const string MUL_FORMULA = "f(x) = x * b";
        const string DIV_FORMULA = "f(x) = x / b";
        const string LOG_FORMULA = "f(x) = c * loga x + b";
        const string SQUARE_FORMULA = "f(x) = ax^2 + bx + c";

        public Brightness(byte[] pixels, int stride, int width, int height, WriteableBitmap modifiedBitmap)
        {
            InitializeComponent();
            previewBitmap = new WriteableBitmap(modifiedBitmap);

            LogFunction.Checked += LogFunction_Checked;
            SquareFunction.Checked += SquareFunction_Checked;
            AddFunction.Checked += AddFunction_Checked;
            SubFunction.Checked += SubFunction_Checked;
            MulFunction.Checked += MulFunction_Checked;
            DivFunction.Checked += DivFunction_Checked;

            this.pixels = pixels;
            this.stride = stride;
            this.width = width;
            this.height = height;

            modifiedPixels = new byte[pixels.Length];

            UpdatePreviewImage(pixels);
        }

        private void ASpinValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                if (ASpinValue.Value == 1 && LogFunction.IsChecked == true)
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

        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            byte[] newPixels = null;

            if (LogFunction.IsChecked == true)
            {
                newPixels = CalculateLogImageBrightness(pixels);
            }
            else if (SquareFunction.IsChecked == true)
            {
                newPixels = CalculateSquareImageBrightness(pixels);
            }
            else if (AddFunction.IsChecked == true)
            {
                newPixels = CalculateSimplePixelAddition(pixels, 'a');
            }
            else if (SubFunction.IsChecked == true)
            {
                newPixels = CalculateSimplePixelAddition(pixels, 's');
            }
            else if (MulFunction.IsChecked == true)
            {
                newPixels = CalculateSimplePixelAddition(pixels, 'm');
            }
            else if (DivFunction.IsChecked == true)
            {
                newPixels = CalculateSimplePixelAddition(pixels, 'd');
            }

            UpdatePreviewImage(newPixels);
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

        private void UpdatePreviewImage(byte[] pixels)
        {
            BitmapSource bitmap = BitmapImage.Create(width, height, 96, 96, PixelFormats.Bgr32, null, pixels, stride);
            previewBitmap = new WriteableBitmap(bitmap);

            PreviewImage.Source = previewBitmap;

            modifiedPixels = pixels;
        }

        private byte[] CalculateLogImageBrightness(byte[] pixels)
        {
            double a = (double)ASpinValue.Value;
            double b = (double)BSpinValue.Value;
            double c = (double)CSpinValue.Value;

            byte[] p = new byte[pixels.Length];

            for (int i = 0; i < p.Length; i+=4)
            {
                p[i] = (byte)GetLogFunctionValue(a, b, c, pixels[i]);
                p[i+1] = (byte)GetLogFunctionValue(a, b, c, pixels[i+1]);
                p[i+2] = (byte)GetLogFunctionValue(a, b, c, pixels[i+2]);
                p[i+3] = pixels[i+3];
            }

            return p;
        }

        private byte[] CalculateSquareImageBrightness(byte[] pixels)
        {
            double a = (double)ASpinValue.Value;
            double b = (double)BSpinValue.Value;
            double c = (double)CSpinValue.Value;

            byte[] p = new byte[pixels.Length];

            for (int i = 0; i < p.Length; i+=4)
            {
                p[i] = (byte)GetSquareFunctionValue(a, b, c, pixels[i]);
                p[i+1] = (byte)GetSquareFunctionValue(a, b, c, pixels[i+1]);
                p[i+2] = (byte)GetSquareFunctionValue(a, b, c, pixels[i+2]);
                p[i+3] = pixels[i+3];
            }

            return p;
        }
        
        private byte[] CalculateSimplePixelAddition(byte[] pixels, char type)
        {
            double b = (double)BSpinValue.Value;

            byte[] p = new byte[pixels.Length];
            switch (type)
            {
                case 'a':
                    for (int i = 0; i < p.Length; i += 4)
                    {
                        p[i] = (byte)GetAddFunctionValue(b, pixels[i]);
                        p[i + 1] = (byte)GetAddFunctionValue(b, pixels[i + 1]);
                        p[i + 2] = (byte)GetAddFunctionValue(b, pixels[i + 2]);
                        p[i + 3] = pixels[i + 3];
                    }
                    break;

                case 's':
                    for (int i = 0; i < p.Length; i += 4)
                    {
                        p[i] = (byte)GetSubFunctionValue(b, pixels[i]);
                        p[i + 1] = (byte)GetSubFunctionValue(b, pixels[i + 1]);
                        p[i + 2] = (byte)GetSubFunctionValue(b, pixels[i + 2]);
                        p[i + 3] = pixels[i + 3];
                    }
                    break;

                case 'm':
                    for (int i = 0; i < p.Length; i += 4)
                    {
                        p[i] = (byte)GetMulFunctionValue(b, pixels[i]);
                        p[i + 1] = (byte)GetMulFunctionValue(b, pixels[i + 1]);
                        p[i + 2] = (byte)GetMulFunctionValue(b, pixels[i + 2]);
                        p[i + 3] = pixels[i + 3];
                    }
                    break;
                case 'd':
                    for (int i = 0; i < p.Length; i += 4)
                    {
                        p[i] = (byte)GetDivFunctionValue(b, pixels[i]);
                        p[i + 1] = (byte)GetDivFunctionValue(b, pixels[i + 1]);
                        p[i + 2] = (byte)GetDivFunctionValue(b, pixels[i + 2]);
                        p[i + 3] = pixels[i + 3];
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Unknown simple calculation type.");
            }
            return p;
        }

        private double GetAddFunctionValue(double b, byte x)
        {
            return x + b;
        }
        private double GetSubFunctionValue(double b, byte x)
        {
            return x - b;
        }
        private double GetMulFunctionValue(double b, byte x)
        {
            return x * b;
        }
        private double GetDivFunctionValue(double b, byte x)
        {
            try
            {
                return x / b;
            }
            catch (DivideByZeroException)
            {
                return x;
            }
        }

        private double GetLogFunctionValue(double a, double b, double c, byte x)
        {
            return c * Math.Log(x, a) + b;
        }

        private double GetSquareFunctionValue(double a, double b, double c, byte x)
        {
            return a * Math.Pow(x, 2) + b * x + c;
        }

        private void LogFunction_Checked(object sender, RoutedEventArgs e)
        {
            FunctionFormulaLabel.Content = LOG_FORMULA;
        }

        private void SquareFunction_Checked(object sender, RoutedEventArgs e)
        {
            FunctionFormulaLabel.Content = SQUARE_FORMULA;
        }

        private void AddFunction_Checked(object sender, RoutedEventArgs e)
        {
            FunctionFormulaLabel.Content = ADD_FORMULA;
        }
        private void SubFunction_Checked(object sender, RoutedEventArgs e)
        {
            FunctionFormulaLabel.Content = SUB_FORMULA;
        }
        private void MulFunction_Checked(object sender, RoutedEventArgs e)
        {
            FunctionFormulaLabel.Content = MUL_FORMULA;
        }
        private void DivFunction_Checked(object sender, RoutedEventArgs e)
        {
            FunctionFormulaLabel.Content = DIV_FORMULA;
        }
    }
}
