using System;

namespace Biometrix
{
    public static class HistogramCreator
    {
        public enum ColorMode
        {
            GRAYSCALE, RED, GREEN, BLUE
        }

        public static int[] GetHistogramFromByteArray(byte[] pixels, ColorMode colorMode)
        {
            int[] histogram = new int[256];

            for (int j = 0; j < pixels.Length; j += 4)
            {
                int value;
                switch (colorMode)
                {
                    case ColorMode.RED:
                        value = pixels[j + 2];
                        break;
                    case ColorMode.GREEN:
                        value = pixels[j + 1];
                        break;
                    case ColorMode.BLUE:
                        value = pixels[j];
                        break;
                    case ColorMode.GRAYSCALE:
                        value = (int)(pixels[j + 2] + pixels[j + 1] + pixels[j]) / 3;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                histogram[value] += 1;
            }
            return histogram;
        }
    }
}
