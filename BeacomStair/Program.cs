using System;
using System.Windows;
using Tekla.Structures.Model;

namespace BeacomStair
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            try
            {
                Model model = new Model();

                if (!model.GetConnectionStatus())
                {
                    MessageBox.Show(
                        "Open Tekla Structures 2023 and a model first, then run BeacomStair again.",
                        "Beacom Stair",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                Application application = new Application
                {
                    ShutdownMode = ShutdownMode.OnMainWindowClose
                };

                application.Run(new MainWindow(model));
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "BeacomStair stopped.\r\n\r\n" + ex.Message,
                    "Beacom Stair",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
