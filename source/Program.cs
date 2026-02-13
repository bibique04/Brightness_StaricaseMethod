using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using GanzfeldController;  // using the files in the GanzfeldController namespace

namespace Program  // container that labels this file as part of the "Program" group
{
    static class Program  // static - the computer runs the program directly
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]   // technical tag to handle the program's memory in a compatible way with the UI elements
        static void Main()  // method
        {
            Application.EnableVisualStyles();  // app modern style
            Application.SetCompatibleTextRenderingDefault(false);  // how to draw the text; false - better performance

            GanzeldController controller = new GanzeldController();  // new instance of GanzeldController type
            Application.Run(controller);  // open the window managed by 'controller' and keep it open until the user closes it
        }
    }
}
