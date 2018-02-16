using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using AForge.Math;
using System.Threading.Tasks;
using System.Globalization;
using MathNet.Numerics.LinearAlgebra;
using ZedGraph;
using System.Runtime.InteropServices; 

namespace ImageReadCS
{
	class Program
    {
        static void Main()
        {
            PointPairList us = new PointPairList(), sift = new PointPairList();
            GrayscaleFloatImage img = ImageIO.FileToGrayscaleFloatImage( "lena_min.bmp");
            var a = Count.lpc("lena", img, 2, 0, 0, 0, true);
            /*
            Console.WriteLine("начало");
         //   Count.CountMetric("lena_min");
         //   Count.CountMetric("lena_min_rot");
            int corresp = Compare.CountCorrespondenses("lena_min", "lena_min_rot");

            for (float th = 0.0f; th < 10; th += 0.1f) 
                Compare.Compare_metrik("lena_min", "lena_min_rot", 0, corresp, th, us, 1);
            
            for (float th = 0; th < 1000; th += 100) 
                Compare.Compare_metrik("lena_min", "lena_min_rot", 1, corresp, th, sift);

            Extra.DrawGpaph(us, sift, "ans_quant.bmp");*/
        }
    }
}