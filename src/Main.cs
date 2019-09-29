using System;
using Gtk;
 
namespace Photobooth
{
    class Program
    {
        static void Main(string[] args)
        {
            Application.Init();

            // Replace the following code with your MainWindow
            // Window window = new Window("Empty Window");
            // window.DeleteEvent += delegate { Application.Quit(); };
            // window.ShowAll();

            // Application.Run();

            Application.Init();
            MainWindow mainWindow = new MainWindow("Photobooth");
            mainWindow.ShowAll();
            Application.Run();
        }
    }
}