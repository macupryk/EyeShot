using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PerformanceStudio
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private MainWindow _mainWindow;

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            string renderer = "Native";
            if (e.Args.Length > 0)
            {
                if (e.Args[0].Contains("DirectX"))
                    renderer = "DirectX";
                else if (e.Args[0].Contains("OpenGL"))
                    renderer = "OpenGL";
            }

            _mainWindow = new MainWindow(renderer);
            _mainWindow.rendererButton.Content = renderer;
            _mainWindow.Show();
        }
    }
}
