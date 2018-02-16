using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AForge.Math;
using System.IO;
using System.Globalization;

namespace ImageReadCS
{
    class Count
    {
        public static int[,] make_mask(int x, int y, float sigma, float th, int w, int h, bool test = false)
        {
            int[,] mask = new int[w, h];
            if (test)
            {
                for (int i = 0; i < w; i++)
                    for (int j = 0; j < h; j++)
                        mask[i, j] = 1;
                return mask;
            }

            for (int i = 0; i < 8; i++)
            {
                float alpha = (float) (th + i * Math.PI / 4);
                try
                {
                    mask[(int)(x + sigma * Math.Cos(alpha)), (int)(y + sigma * Math.Sin(alpha))] = 1;
                }
                catch
                {

                }
            }

            for (int i = 0; i < 16; i++)
            {
                float alpha = (float) (th + i * Math.PI / 8);
                try
                {
                    mask[(int)(x + 2 * sigma * Math.Cos(alpha)), (int)(y + 2 * sigma * Math.Sin(alpha))] = 1;
                }
                catch
                {

                }
            }
            return mask;
        }
        public static float[,] lpc(string name, GrayscaleFloatImage image, float sigma, float th, int x, int y, bool test = false)//новая метрика
        {
            GrayscaleFloatImage image2 = new GrayscaleFloatImage(image.Width, image.Height);

            float sig = sigma, lam = sig;
            float eps = 0.01f;
            int r = (int)(3 * sig);

            Complex[][,] img = new Complex[20][,];
            int l = 0;
            float[,] num = new float[image.Width, image.Height];
            float[,] ans = new float[image.Width, image.Height];
            float[,] denum = new float[image.Width, image.Height];
            int[,] mask = make_mask(x, y, sigma, th, image.Width, image.Height, test);

            //for (sig = 1; sig <=16; sig++)
            //  for (r=3; r<=10; r++)
            //for (lam = 1; lam < 5; lam++)

            {
                for (int q = 0; q < 6; q++)
                {
                    l = 0;
                    int m_l = 5;

                    for (float s = 1; s <= 3; s += 0.5f, l++)
                    {
                        img[l] = Gabor.GaborFiltered(image, mask, (int)(r * s), lam, (float)(q * (Math.PI / 6) + th), sig, 1f, 0f, s);
                    }

                    for (int i = 0; i < image.Width; i++)
                        for (int j = 0; j < image.Height; j++)
                        {
                            float e_x = 0, e_y = 0, a = 0;
                            for (l = 0; l < m_l; l++)
                            {
                                e_x += (float) img[l][i, j].Re;
                                e_y += (float) img[l][i, j].Im;
                                a += (float) img[l][i, j].Magnitude;
                            }
                            num[i, j] += (float) Math.Sqrt(e_x * e_x + e_y * e_y);
                            denum[i, j] += a;
                        }
                }
                for (int i = 0; i < image.Width; i++)
                    for (int j = 0; j < image.Height; j++)
                        ans[i, j] = num[i, j] / (eps + denum[i, j]);

                //    Console.WriteLine("pc {0}",ans[x, y]);
                for (int i = 0; i < image.Width; i++)
                    for (int j = 0; j < image.Height; j++)
                    {
                        image2[i, j] = (float)ans[i, j];
                        image2[i, j] = (float)Math.Round(image2[i, j], 1, MidpointRounding.AwayFromZero);
                    }

                Extra.Norm(image2);
                string pic1 = String.Format("!!ans {0} sigma= {1} lam = {2} rad= {3}", name, sig, lam, r);
                ImageIO.ImageToFile(image2, pic1 + ".bmp");

            }
            return ans;
        }

        public static void CountMetric(string name)
        {
            StreamReader sr = new StreamReader(name + ".key");
            NumberFormatInfo provider = new NumberFormatInfo();
            provider.NumberDecimalSeparator = ".";

            GrayscaleFloatImage img = ImageIO.FileToGrayscaleFloatImage(name + ".bmp");

            float[,] lpc_map;
            StreamWriter file = new StreamWriter("ans_lpc_all_info" + name + ".txt");
            StreamWriter file1 = new StreamWriter("ans_lpc" + name + ".txt");
            StreamWriter file2 = new StreamWriter("ans_lbp" + name + ".txt");
            int dots = 0;
            while (!sr.EndOfStream)
            {
                dots++;
                Console.WriteLine(dots);
                string str = sr.ReadLine();
                string[] numbers = str.Split(' ');
                float x = (float) Convert.ToDouble(numbers[0], provider);
                float y = (float) Convert.ToDouble(numbers[1], provider);
                float sigma = (float) Convert.ToDouble(numbers[2], provider);
                float th = (float) Convert.ToDouble(numbers[3], provider);
                // if (!(x== && y==250)) continue; 
                // if (sigma > 4.5) continue;
                lpc_map = lpc(name, img, sigma, th, (int)Math.Round(x), (int)Math.Round(y));
                file.WriteLine(String.Format("{0} {1} {2} {3} {4}\n", x, y, sigma, th, lpc_map[(int)Math.Round(x), (int)Math.Round(y)]));
                file.Flush();
                file1.WriteLine(String.Format("{0} {1} {2}\n", x, y, lpc_map[(int)Math.Round(x), (int)Math.Round(y)]));
                file1.Flush();
                file2.WriteLine(String.Format("{0} {1} {2} {3}", x, y, sigma, th));
                for (int i = 0; i < 8; i++)
                {
                    try
                    {
                        float alpha = (float) (th + i * Math.PI / 4);
                        file2.WriteLine(String.Format("{0}", lpc_map[(int)((int)Math.Round(x) + sigma * Math.Cos(alpha)), (int)((int)Math.Round(y) + sigma * Math.Sin(alpha))]));
                    }
                    catch
                    {
                        file2.WriteLine("0");
                    }
                }

                for (int i = 0; i < 16; i++)
                {
                    try
                    {
                        float alpha = (float) (th + i * Math.PI / 8);
                        file2.WriteLine(String.Format("{0}", lpc_map[(int)((int)Math.Round(x) + 2 * sigma * Math.Cos(alpha)), (int)((int)Math.Round(y) + 2 * sigma * Math.Sin(alpha))]));
                    }
                    catch
                    {
                        file2.WriteLine("0");
                    }
                }

                file2.WriteLine("\n");
                file2.Flush();
            }
            file.Close();
            file1.Close();
            file2.Close();
            sr.Close();
        }

    }
}
