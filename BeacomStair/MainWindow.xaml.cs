using System;
using System.Globalization;
using System.Windows;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;

namespace BeacomStair
{
    public partial class MainWindow : Window
    {
        private readonly Model _model;
        private bool _isBuilding;

        public MainWindow(Model model)
        {
            InitializeComponent();

            _model = model ?? throw new ArgumentNullException(nameof(model));

            LoadOptions(StairOptions.CreateDefault());
        }

        private void ResetDefaults_Click(object sender, RoutedEventArgs e)
        {
            LoadOptions(StairOptions.CreateDefault());
            StatusTextBlock.Text = "Defaults restored.";
        }

        private void CreateStair_Click(object sender, RoutedEventArgs e)
        {
            if (_isBuilding)
                return;

            StairOptions options;

            try
            {
                options = ReadOptions();
                StairSettings.ApplyOptions(options);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Beacom Stair",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            _isBuilding = true;
            StatusTextBlock.Text = "Pick the wall point in Tekla...";
            IsEnabled = false;

            try
            {
                Hide();

                Picker picker = new Picker();

                Point startPoint = picker.PickPoint(
                    "BeacomStair: pick the GROUND point at the WALL, below the top landing.");

                Point directionPoint = picker.PickPoint(
                    "BeacomStair: pick outward from the wall in the direction of the stair FOOT.");

                StairBuilder builder = new StairBuilder(_model);

                StairBuildResult result =
                    builder.Build(startPoint, directionPoint);

                _model.CommitChanges();

                MessageBox.Show(
                    "Beacom stair inserted.\r\n\r\n" +
                    "Width: " + StairSettings.TreadWidth.ToString("0", CultureInfo.CurrentCulture) + " mm\r\n" +
                    "Stringer: " + StairSettings.StringerProfile + "\r\n" +
                    "Handrail: " + StairSettings.RailProfile + "\r\n" +
                    "15 rises @ " + result.Rise.ToString("0.00", CultureInfo.CurrentCulture) + " mm\r\n" +
                    "Objects inserted: " + result.InsertedObjectCount,
                    "Beacom Stair",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                StatusTextBlock.Text = "Stair created. Ready for another.";
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = "Ready.";

                MessageBox.Show(
                    "BeacomStair stopped.\r\n\r\n" + ex.Message,
                    "Beacom Stair",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                Show();
                Activate();
                IsEnabled = true;
                _isBuilding = false;
            }
        }

        private StairOptions ReadOptions()
        {
            double width;

            if (!double.TryParse(
                    WidthTextBox.Text,
                    NumberStyles.Float,
                    CultureInfo.CurrentCulture,
                    out width) &&
                !double.TryParse(
                    WidthTextBox.Text,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out width))
            {
                throw new InvalidOperationException(
                    "Enter a valid stair width in millimetres.");
            }

            return new StairOptions
            {
                Width = width,
                StringerProfile = RequireText(
                    StringerProfileTextBox.Text,
                    "stringer PFC profile"),
                TreadProfile = RequireText(
                    TreadProfileTextBox.Text,
                    "tread plate profile"),
                TreadSidePlateProfile = RequireText(
                    TreadEndPlateProfileTextBox.Text,
                    "tread end-plate profile"),
                RailProfile = RequireText(
                    RailProfileTextBox.Text,
                    "handrail CHS profile"),
                PlatformSupportAngleProfile = RequireText(
                    SupportAngleProfileTextBox.Text,
                    "landing support RSA profile")
            };
        }

        private void LoadOptions(StairOptions options)
        {
            WidthTextBox.Text =
                options.Width.ToString("0", CultureInfo.CurrentCulture);

            StringerProfileTextBox.Text =
                options.StringerProfile;

            TreadProfileTextBox.Text =
                options.TreadProfile;

            TreadEndPlateProfileTextBox.Text =
                options.TreadSidePlateProfile;

            RailProfileTextBox.Text =
                options.RailProfile;

            SupportAngleProfileTextBox.Text =
                options.PlatformSupportAngleProfile;
        }

        private static string RequireText(
            string value,
            string description)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException(
                    "Enter a " + description + ".");
            }

            return value.Trim();
        }
    }
}
