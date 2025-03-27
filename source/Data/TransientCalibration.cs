using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Data
{
    class TransientCalibration
    {
         public double[,] TransCal = new double[,]
         {
             {0,0}, //RedOn.Off
             {0,0}, //GreenOn.Off
             {0,0}, //BLueOn.Off
             {0,0} //YellowOn.Off
         } ; 
    }
}
