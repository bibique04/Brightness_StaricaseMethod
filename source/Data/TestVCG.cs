using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Data
{
     static public class TestVCG
     {
        

        static int Sgn(int x)
        {
            if(x < 0) { return -1; }
            else return 1;
        }
    }
    class PostReceptoralMechanisms
    {
        public double MmL;
        public double S;
        public double LMS;
        public double ipRGC;
    }
    class Receptors
    {
        public double L;
        public double M;
        public double S;
        public double ipRGC;
    }
    class RGB2
    {
        public double Red;
        public double Green;
        public double Blue;
        public double Orange;
    }
    class RECORD
    {
        public uint first;
        public uint second;
        public uint third;
    }
}
