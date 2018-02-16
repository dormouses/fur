using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using ZedGraph;

namespace ImageReadCS
{
    class Compare
    {
        static int count = 0; 
        static float pix_th = 2;
        private const double PixTh = 2;
        private const double ThM = 0.08;

       // [DllImport("..\\..\\Release\\lib.dll", CallingConvention=CallingConvention.Cdecl)]
       // public static extern float difference([MarshalAs(UnmanagedType.LPArray, SizeConst = 24)] float[] a, [MarshalAs(UnmanagedType.LPArray, SizeConst = 24)] float[] b, int n);

        public static float Diff(List<float> l1, List<float> l2)
        {
            //var a = l1.ToArray();
            //var b = l2.ToArray();
            // return difference(a, b, l1.Count);
            float ans = 0;
            for (int i = 0; i < l1.Count; i++)
            {
                //  ans += Math.Abs(l1[i] - l2[i]);
                ans += (l1[i] - l2[i]) * (l1[i] - l2[i]);
            }
            return ans;
        }

        static List<float> current;

        public static void ReadPoint(StreamReader file, List<my_Point> points)
        {
            var provider = new NumberFormatInfo { NumberDecimalSeparator = "." };
            while (!file.EndOfStream)
            {
                var str = file.ReadLine();
                if (String.Compare(str, "") == 0) continue;
                var numbers = str.Split(' ');
                var x = float.Parse(numbers[0], provider);
                var y = float.Parse(numbers[1], provider);
                points.Add(new my_Point(x, y));
            }
        }
        public static int CountCorrespondenses(string name1, string name2)
        {
            var file1 = new StreamReader(name1 + ".key");
            var file2 = new StreamReader(name2 + ".key");

            var points1 = new List<my_Point>();
            var points2 = new List<my_Point>();

            ReadPoint(file1, points1);
            ReadPoint(file2, points2);

            int ans = 0;
            foreach (var p in points1)
            {
                var cx1 = p.X;
                var cy1 = p.Y;
                foreach (var q in points2)
                {
                    var cx2 = q.X;
                    var cy2 = q.Y;
                    if (Math.Abs(cx2 - (256 - cy1)) < PixTh && Math.Abs(cy2 - cx1) < PixTh)
                    {
                        ans++;
                    }
                }
            }
            return ans;
        }

        public static void ReadLPC(StreamReader file, IList<Point_pc> PointDescript, int quant = -1)
        {
            var provider = new NumberFormatInfo { NumberDecimalSeparator = "," };

            int k = 0;
            while (!file.EndOfStream)
            {
                var str = file.ReadLine();
                if (String.Compare(str, "") == 0) continue;

                var numbers = str.Split(' ');
                var x = float.Parse(numbers[0], provider);
                var y = float.Parse(numbers[1], provider);

                PointDescript.Add(new Point_pc(x, y, new List<float>()));

                for (int i = 0; i < 8 + 16; i++)
                {
                    str = file.ReadLine();
                    var descriptor = float.Parse(str, provider);
                    if (quant != -1)
                    {
                        descriptor = (float)Math.Round(descriptor, quant, MidpointRounding.AwayFromZero);
                    }
                    PointDescript[k].lbp.Add(descriptor);
                }
                k++;
            }
        }

        public static void ReadSIFT(StreamReader file, IList<Point_pc> PointDescript)
        {
            var provider = new NumberFormatInfo { NumberDecimalSeparator = "." };

            int k = 0;
            while (!file.EndOfStream)
            {
                var str = file.ReadLine();
                if (String.Compare(str, "") == 0) continue;

                var numbers = str.Split(' ');
                var x = float.Parse(numbers[0], provider);
                var y = float.Parse(numbers[1], provider);

                PointDescript.Add(new Point_pc(x, y, new List<float>()));

                for (int i = 4; i < numbers.Length; i++)
                {
                    var descriptor = Int32.Parse(numbers[i], provider);
                    PointDescript[k].lbp.Add(descriptor);
                }
                k++;
            }
        }

        public static void Compare_metrik(string name1, string name2, int type, int corresp, float th, PointPairList list, int quant = -1)
        {
            StreamReader file1, file2;
            if (type == 0)
            {
                file1 = new StreamReader("ans_lbp" + name1 + ".txt");
                file2 = new StreamReader("ans_lbp" + name2 + ".txt");
            }
            else
            {
                file1 = new StreamReader(name1 + ".key");
                file2 = new StreamReader(name2 + ".key");
            }
            var lbp1 = new List<Point_pc>();
            var lbp2 = new List<Point_pc>();

            if (type == 0)
            {
                ReadLPC(file1, lbp1, quant);
                ReadLPC(file2, lbp2, quant);
            }
            else 
            {
                ReadSIFT(file1, lbp1);
                ReadSIFT(file2, lbp2);
            }

            List<float> xp1 = new List<float>();
            List<float> xp2 = new List<float>();
            List<float> yp1 = new List<float>();
            List<float> yp2 = new List<float>();

            int r = 0;
            int l = 0;
            for (int i = 0; i < lbp1.Count; i++)
            {
                float cx1 = lbp1[i].x;
                float cy1 = lbp1[i].y;

                lbp2.Sort((x, y) =>
                {
                    double dx = Diff(x.lbp, lbp1[i].lbp);
                    double dy = Diff(y.lbp, lbp1[i].lbp);
                    if (dx > dy) return -1;
                    if (dy > dx) return 1;
                    return 0;
                });


                for (int j = lbp2.Count - 1, n = 1; j > 0; j--, n++)
                {
                    float cx2 = lbp2[j].x;
                    float cy2 = lbp2[j].y;
                    float mmm = Diff(lbp2[j].lbp, lbp1[i].lbp);
                  /*  double th;
                    if (type == 1)
                        th = 20000;
                    else
                        th = 0.5;*/
                    Console.WriteLine(mmm.ToString()+' '+th.ToString());
                    if (mmm < th)
                    {
                       // Console.WriteLine("sdf");
                        if (Math.Abs(cx2 - (256 - cy1)) < PixTh && Math.Abs(cy2 - cx1) < PixTh)
                        {
                            r++;
                            xp1.Add(cx1);
                            yp1.Add(cy1);
                            xp2.Add(cx2);
                            yp2.Add(cy2);
                        }
                        else
                        {
                            l++;
                        }
                    }
                    break;
                }
            }


            list.Add((float)l / (float)(r + l), (float)r / corresp);

            var img1 = ImageIO.FileToGrayscaleFloatImage(name1 + ".bmp");
            var img2 = ImageIO.FileToGrayscaleFloatImage(name2 + ".bmp");
            string metr = "";
            if (type == 0)
                metr = "_us_";
            else
                metr = "_sift_";
            count++;
            if (count % 10 == 0)
                Extra.Draw_Comparasion(img1, img2, xp1, yp1, xp2, yp2, "_+lbp" + name1 + "_and_" + name2 + metr + "th"+ th.ToString() +".bmp");
        }
    }
}
