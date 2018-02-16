using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageReadCS
{
    class Point_pc
    {
        public float x;
        public float y;
        public List<float> lbp;
        public Point_pc(float _x, float _y, List<float> _lbp)
        {
            x = _x;
            y = _y;
            lbp = _lbp;
        }
    }
}
