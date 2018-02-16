using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AForge.Math;
using System.Runtime.InteropServices;

namespace ImageReadCS
{
    class Gabor
    {

       // [DllImport("..\\..\\Release\\lib.dll", CallingConvention = CallingConvention.Cdecl)]
       // public static extern void GaborFilterValue_C(int x, int y, float lam, float th, float psi, float sig, float gam, float s, ref float Re, ref float Im); 
        public static Complex GaborFilterValue(int x, int y, float lam, float th, float psi, float sig, float gam, float s)
        {
            float cos = (float) Math.Cos(th);
            float sin = (float)Math.Sin(th);
            float nx = (float)(x * cos + y * sin);
            float ny = (float)(-x * sin + y * cos);
            float xx = nx / s;
            float yy = ny / s;
            float koef = (float) (Math.Exp(-((xx * xx + gam * gam * yy * yy) / (2.0f * sig * sig))));
            float arg = (float) (2.0f * Math.PI * xx / lam + psi * Math.PI / 180);

            Complex ans;
            ans.Re = koef * Math.Cos(arg);
            ans.Im = koef * Math.Sin(arg);
            return ans;
           /* float Re=0, Im=0;
            GaborFilterValue_C(x, y, lam, th, psi, sig, gam, s, ref Re, ref Im); 
            Complex ans;
            ans.Re = Re;
            ans.Im = Im;
            return ans;*/
        }
        public static Complex[,] GetFilter(int r, float th, float lam, float sig, float gam, float psi, float s)
        {
            r = r * 2 + 1;
            Complex[,] filter = new Complex[r, r];

            for (int y = 0; y < r; ++y)
            {
                for (int x = 0; x < r; ++x)
                {
                    int dy = -(r / 2) + y;
                    int dx = -(r / 2) + x;

                    filter[x, y] = GaborFilterValue(dy, dx, lam, th, psi, sig, gam, s);
                }
            }
            return filter;
        }
        public static Complex Convolution(GrayscaleFloatImage image, Complex[,] filter, int x, int y)
        {
            Complex result = new Complex(0, 0);
            int pix = 0;
            Complex koef = new Complex(0, 0);
            int r = filter.GetLength(0) / 2;

            for (int i = 0; i < filter.GetLength(0); i++)
            {
                for (int j = 0; j < filter.GetLength(1); j++)
                {
                    Complex deltaResult = image[x + i - r, y + j - r] * filter[i, j];
                    result += deltaResult;
                    koef += filter[i, j];
                    pix++;
                }
            }

            Complex filteredValue = result;
            return filteredValue;
        }
        public static Complex[,] GaborFiltered(GrayscaleFloatImage image, int[,] mask, int r, float lam, float th, float sig, float gam, float psi, float s)
        {
            Complex[,] ans = new Complex[image.Width, image.Height];
            Complex[,] filter = GetFilter(r, th, lam, sig, gam, psi, s);
            float sre = 0, sim = 0;
            for (int i = 0; i <= 2 * r; i++)
                for (int j = 0; j <= 2 * r; j++)
                {
                    sre += (float) filter[i, j].Re;
                    sim += (float)filter[i, j].Im;
                }
            //     Console.WriteLine("re"+sre.ToString());
            //   Console.WriteLine("im" + sim.ToString());

            for (int y = 0; y < image.Height; y++)
                for (int x = 0; x < image.Width; x++)
                {
                    if (x < 0 || y < 0 || x >= image.Width || y >= image.Height) continue;
                    if (mask[x, y] == 1)
                        ans[x, y] = Convolution(image, filter, x, y) / (sig * sig);
                    //  ans[x, y] = Convolution(image, filter, x, y);
                }
            return ans;
        }

    }
}
