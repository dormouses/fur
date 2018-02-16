using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AForge.Math;
using System.IO;
using System.Drawing;
using System.Globalization;
using ZedGraph;

namespace ImageReadCS
{
    class Extra
    {
        static float gmax = 255, gmin = 0;

        public static void DrawGpaph(PointPairList us, PointPairList sift, string name)
        {
            GraphPane myPane = new GraphPane();
            myPane.XAxis.Title.Text = "l / (r + l)";
            myPane.YAxis.Title.Text = "r / corresp";

            myPane.XAxis.Scale.Min = 0;
            myPane.XAxis.Scale.Max = 1;
            myPane.YAxis.Scale.Min = 0;
            myPane.YAxis.Scale.Max = 1;


            LineItem myCurve1 = myPane.AddCurve("us", us, Color.Red);
            LineItem myCurve2 = myPane.AddCurve("Sift", sift, Color.Blue);

            myPane.AxisChange();
            Bitmap bmp = myPane.GetImage();
            bmp.Save(name);

        }

        public static void Draw_Comparasion(GrayscaleFloatImage img1, GrayscaleFloatImage img2, List<float> x1, List<float> y1, List<float> x2, List<float> y2, string name)
        {
            int delta = 10;
            Pen redPen = new Pen(Color.Red, 1);
            Pen greenPen = new Pen(Color.Green, 1);
            int h = Math.Max(img1.Height, img2.Height);
            Bitmap map = new Bitmap(img1.Width + delta + img2.Width, h);
            Graphics gr = Graphics.FromImage(map);
            for (int i = 0; i < img1.Width; i++)
                for (int j = 0; j < img1.Height; j++)
                {
                    int col = (int)(img1[i, j]);
                    map.SetPixel(i, j, Color.FromArgb(col, col, col));
                }
            for (int i = 0; i < img2.Width; i++)
                for (int j = 0; j < img2.Height; j++)
                {
                    int col = (int)(img2[i, j]);
                    map.SetPixel(img1.Width + delta + i, j, Color.FromArgb(col, col, col));
                }

            for (int i = 0; i < x1.Count; i++)
                // if (Math.Abs(x1[i]* 0.9 - x2[i]) < 3 && Math.Abs(y1[i]* 0.9 - y2[i]) < 3)
                if (Math.Abs(x2[i] - (256 - y1[i])) < 4 && Math.Abs(y2[i] - x1[i]) < 4)
                    gr.DrawLine(greenPen, (int)x1[i], (int)y1[i], img1.Width + delta + (int)x2[i], (int)y2[i]);
                else
                    gr.DrawLine(redPen, (int)x1[i], (int)y1[i], img1.Width + delta + (int)x2[i], (int)y2[i]);
            map.Save(name);
        }

        public static void Draw_Circles(string name)
        {
            StreamReader sr = new StreamReader(name + ".key");
            NumberFormatInfo provider = new NumberFormatInfo();
            provider.NumberDecimalSeparator = ".";
            Pen redPen = new Pen(Color.Red, 1);
            Bitmap map = new Bitmap(name + ".bmp");
            Graphics gr = Graphics.FromImage(map);
            while (!sr.EndOfStream)
            {
                string str = sr.ReadLine();
                string[] numbers = str.Split(' ');
                int x = (int)Convert.ToDouble(numbers[0], provider);
                int y = (int)Convert.ToDouble(numbers[1], provider);
                float sigma =(float) Convert.ToDouble(numbers[3], provider);
                float th = (float) Convert.ToDouble(numbers[4], provider);
                if (sigma > 6) continue;

                // sigma = 1 * sigma;
                gr.DrawEllipse(redPen, (int)(x - sigma), (int)(y - sigma), (int)(2 * sigma), (int)(2 * sigma));

            }
            map.Save(name + "_cirlcs.bmp");
        }

        public static void Norm(GrayscaleFloatImage data)
        {
            float min, max;
            gmin = 0;
            gmax = 255;
            min = max = data[0, 0];
            for (int i = 0; i < data.Width; i++)
                for (int j = 0; j < data.Height; j++)
                {
                    if (data[i, j] < min) min = data[i, j];
                }

            max = data[0, 0];
            for (int i = 0; i < data.Width; i++)
                for (int j = 0; j < data.Height; j++)
                {
                    //if (data[i, j].Re < min) min = data[i, j].Re;
                    if (data[i, j] > max) max = data[i, j];
                }

            float k = (gmax - gmin) / (max - min);
            for (int i = 0; i < data.Width; i++)
                for (int j = 0; j < data.Height; j++)
                {
                    data[i, j] *= (float)k;
                }

            min = data[0, 0];
            for (int i = 0; i < data.Width; i++)
                for (int j = 0; j < data.Height; j++)
                {
                    if (data[i, j] < min) min = data[i, j];
                }


            float dif = gmin - min;
            for (int i = 0; i < data.Width; i++)
                for (int j = 0; j < data.Height; j++)
                {
                    data[i, j] += (float)dif;
                }

        }
        static void Norm(float[,] data, int w, int h)
        {
            float min, max;
            min = max = data[0, 0];
            for (int i = 0; i < w; i++)
                for (int j = 0; j < h; j++)
                {
                    if (data[i, j] < min) min = data[i, j];
                }

            max = data[0, 0];
            for (int i = 0; i < w; i++)
                for (int j = 0; j < h; j++)
                {
                    //if (data[i, j].Re < min) min = data[i, j].Re;
                    if (data[i, j] > max) max = data[i, j];
                }

            float k = (gmax - gmin) / (max - min);
            for (int i = 0; i < w; i++)
                for (int j = 0; j < h; j++)
                {
                    data[i, j] *= (float)k;
                }

            min = data[0, 0];
            for (int i = 0; i < w; i++)
                for (int j = 0; j < h; j++)
                {
                    if (data[i, j] < min) min = data[i, j];
                }


            float dif = gmin - min;
            for (int i = 0; i < h; i++)
                for (int j = 0; j < w; j++)
                {
                    data[i, j] += (float)dif;
                }

        }
        static Complex[,] shift(Complex[,] data, int a, int b)
        {
            int w = data.GetLength(0);
            int h = data.GetLength(1);
            Complex[,] ans = new Complex[w, h];

            for (int i = 0; i < w; i++)
                for (int j = 0; j < h; j++)
                {
                    ans[(i + a) % w, (j + b) % h] = data[i, j];
                }
            return ans;
        }
        static void ToComplex(Complex[,] data, GrayscaleFloatImage img)
        {
            for (int i = 0; i < img.Width; i++)
                for (int j = 0; j < img.Height; j++)
                    data[i, j] = new Complex(img[i, j], 0);
        }

        static void ToImg(Complex[,] data, GrayscaleFloatImage img)
        {
            img = new GrayscaleFloatImage(data.GetLength(0), data.GetLength(1));
            for (int i = 0; i < img.Width; i++)
                for (int j = 0; j < img.Height; j++)
                    img[i, j] = (float)data[i, j].Re;
        }

        static GrayscaleFloatImage EdgeFunction(GrayscaleFloatImage edges)
        {
            GrayscaleFloatImage[] ans = new GrayscaleFloatImage[2];

            ans[0] = new GrayscaleFloatImage(edges.Width, edges.Height);
            ans[1] = new GrayscaleFloatImage(edges.Width, edges.Height);


            for (int i = 0; i < ans[0].Width; i++)
            {
                for (int j = 0; j <= 1; j++)
                {
                    ans[j][i, 0] = edges[i, 0];
                    ans[j][i, ans[0].Height] = edges[i, ans[0].Height];
                }
            }

            for (int k = 0; k < 100; k++)
                for (int i = 0; i < edges.Width; i++)
                    for (int j = 0; j < edges.Height; j++)
                        ans[(k + 1) % 2][i, j] = 1 / 4 * (ans[k % 2][i + 1, j] + ans[k % 2][i - 1, j] + ans[k % 2][i, j + 1] + ans[k % 2][i, j + 1]);

            return ans[1];
        }


    }
}
