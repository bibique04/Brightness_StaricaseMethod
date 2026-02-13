using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Data
{
    static class IntensityVoltage

    // coefficients for a 4th-order polynomial equation

    {
        public static double[,] Int2Volt = new double[,]
         {
             { 0.03608 ,  0.02336 ,  0.10248 , 0.83765}, //R
             {-0.2689  ,  0.5585  , -0.3679  , 1.0793 }, //G
             { 0.0008  , -0.0136  ,  0.0485  , 0.9666 }, //B
             { 0.0287  , -0.0107  ,  0.0673  , 0.9144 } //Y
         };
    }
}
