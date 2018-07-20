using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace UART_Demo
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            //LoadResourceDll.RegistDLL();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
