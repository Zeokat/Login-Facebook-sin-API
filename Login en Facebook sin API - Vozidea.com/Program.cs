using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Login_en_Facebook_sin_API___Vozidea.com
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
