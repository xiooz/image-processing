using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Windows;

namespace ImageProcessingWinForm
{
    public class Deskew
    {
        // Representation of a line in the image
        private class HoughLine
        {
            // Count of points in the line.
            public int Count;
            // Index in matrix.
            public int Index;
            // The line is represented as all x, y that solve y*cos(alpha)-x*sin(alpha)=d 
            public double Alpha;
        }

        Bitmap _internalBmp;

        // The range of angles to search for lines
        const double ALPHA_START = -20;
        const double ALPHA_STEP = 0.2;
        const int STEPS = 40 * 5;
        const double STEP = 1;

        // Precalculation of sin and cos
        double[] _sinA;
        double[] _cosA;

        // Range of d
        double _min;
        int _count;

        // Count of points that fit in a line
        int[] _hMatrix;

        public Bitmap DeskewImage(Bitmap image)
        {
            _internalBmp = image;

            return RotateImage(_internalBmp, GetSkewAngle());

            //return RotateImage(image, GetSkewAngle());
        }

        // Calculate the skew angle of the image 
        private double GetSkewAngle()
        {
            // Hough transformation
            Calc();

            // Top 20 of the detected lines in the image
            HoughLine[] hl = GetTop(20);
            
            // Average angle of the lines
            double sum = 0;
            int count = 0; 
            for (int i = 0; i < 19; i++)
            {
                sum += hl[i].Alpha;
                count += 1;
            }
            return sum / count;
        }

        // Calculate the Count lines in the image with most points
        private HoughLine[] GetTop(int count)
        {
            HoughLine[] hl = new HoughLine[count];

            for (int i = 0; i < count; i++)
            {
                hl[i] = new HoughLine();
            }

            for (int i = 0; i < _hMatrix.Length - 1; i++)
            {
                if (_hMatrix[i] > hl[count - 1].Count)
                {
                    hl[count - 1].Count = _hMatrix[i];
                    hl[count - 1].Index = i;
                    int j = count - 1;

                    while (j > 0 && hl[j].Count > hl[j - 1].Count)
                    {
                        HoughLine tmp = hl[j];
                        hl[j] = hl[j - 1];
                        hl[j - 1] = tmp;
                        j -= 1;
                    }
                }
            }

            for (int i = 0; i < count; i++)
            {
                int dIndex = hl[i].Index / STEPS;
                int alphaIndex = hl[i].Index - dIndex * STEPS;
                hl[i].Alpha = GetAlpha(alphaIndex);
                //hl[i].D = dIndex + _min;
            }

            return hl;
        }

        // Hough transformation
        private void Calc()
        {
            int hMin = _internalBmp.Height / 4;
            int hMax = _internalBmp.Height * 3 / 4;
   
            Init();

            for (int y = hMin; y <= hMax; y++)
            {
                for (int x = 1; x <= _internalBmp.Width - 2; x++)
                {
                    // Only lower edges are considered
                    if (IsBlack(x, y))
                    {
                        if (!IsBlack(x, y + 1))
                        {
                            Calc(x, y);
                        }
                    }
                }
            }
        }

        // Calculate all lines through the point (x,y)
        private void Calc(int x, int y)
        {
            int alpha;

            for (alpha = 0; alpha <= STEPS - 1; alpha++)
            {
                double d = y * _cosA[alpha] - x * _sinA[alpha];
                int calculatedIndex = (int)CalcDIndex(d);
                int index = calculatedIndex * STEPS + alpha;

                try
                {
                    _hMatrix[index] += 1;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                }
            }
        }

        private double CalcDIndex(double d)
        {
            return Convert.ToInt32(d - _min);
        }

        private bool IsBlack(int x, int y)
        {
            Color c = _internalBmp.GetPixel(x, y);
            double luminance = (c.R * 0.299) + (c.G * 0.587) + (c.B * 0.114);
            return luminance < 140;
        }

        private void Init()
        {
            // Precalculation of sin and cos
            _sinA = new double[STEPS];
            _cosA = new double[STEPS];

            for (int i = 0; i < STEPS; i++)
            {
                double angle = GetAlpha(i) * Math.PI / 180.0;
                _sinA[i] = Math.Sin(angle);
                _cosA[i] = Math.Cos(angle);
            }

            // Range of d
            _min = -_internalBmp.Width;
            _count = (int)(2 * (_internalBmp.Width + _internalBmp.Height) / STEP);
            _hMatrix = new int[_count * STEPS];
        }

        private static double GetAlpha(int index)
        {
            return ALPHA_START + index * ALPHA_STEP;
        }

        private static Bitmap RotateImage(Bitmap bmp, double angle)
        {
            Graphics g;
            Bitmap tmp = new Bitmap(bmp.Width, bmp.Height);
            tmp.SetResolution(bmp.HorizontalResolution, bmp.VerticalResolution);
            g = Graphics.FromImage(tmp);

            try
            {
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                g.FillRectangle(Brushes.White, 0, 0, bmp.Width, bmp.Height);
                g.TranslateTransform(0, 0);
                g.RotateTransform(-((float)angle));
                g.DrawImage(bmp, 0, 0);
            }
            finally
            {
                g.Dispose();
            }
            return tmp;
        }

    }
}
