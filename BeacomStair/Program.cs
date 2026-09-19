using System;
using System.Windows.Forms;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;

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
                        "BeacomStair",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                Picker picker = new Picker();

                Point startPoint = picker.PickPoint(
                    "BeacomStair: pick the CENTRE of the top platform back edge.");

                Point directionPoint = picker.PickPoint(
                    "BeacomStair: pick any point in the direction the stair should run toward the FOOT.");

                StairBuilder builder = new StairBuilder(model);
                StairBuildResult result = builder.Build(startPoint, directionPoint);

                model.CommitChanges();

                MessageBox.Show(
                    "Beacom stair inserted.\r\n\r\n" +
                    "15 rises @ " + result.Rise.ToString("0.00") + " mm\r\n" +
                    "14 treads @ " + StairSettings.Going.ToString("0") + " mm\r\n" +
                    "Flight run: " + StairSettings.FlightRun.ToString("0") + " mm\r\n" +
                    "Top platform: " + StairSettings.PlatformLength.ToString("0") + " mm\r\n" +
                    "Overall: " + StairSettings.OverallLength.ToString("0") + " mm\r\n" +
                    "Objects inserted: " + result.InsertedObjectCount,
                    "BeacomStair",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "BeacomStair stopped.\r\n\r\n" + ex.Message,
                    "BeacomStair",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
