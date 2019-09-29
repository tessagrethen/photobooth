using System;
using Gtk;
 
namespace Photobooth
{
    class Program
    {
        static void Main(string[] args)
        {
            Application.Init();
            MainWindow mainWindow = new MainWindow("Photobooth");
            mainWindow.ShowAll();
            Application.Run();
        }
    }
}