using System;
using System.Windows.Forms;

namespace App.UI
{
    // Program pertenece a App.UI, no a App.UI.Forms
    // App.UI.Forms es solo para los formularios (FrmLogin, FrmMDI, etc.)
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Ruta completa del namespace porque no usamos 'using' para Forms
            Application.Run(new App.UI.Forms.FrmLogin());
        }
    }
}