namespace OfisEkranG�r�nt�s�
{
    using System;
    using System.Windows.Forms;

    namespace OfisEkranG�r�nt�s�
    {
        static class Program
        {
            [STAThread]
            static void Main()
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
        }
    }
}