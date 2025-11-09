using Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Bongs_Vehicle_Viewer_V2
{
    public partial class MainWindow : Window
    {
        public List<int> ValidYears = GetValidYears(2026-50, 2026);

        public MainWindow()
        {
            InitializeComponent();
            yearSelector.SetItemSource(ValidYears);
            yearSelector.ItemIndex = 0;

            stateSelector.SetItemSource(Enum.GetNames(typeof(VehicleConditon)));
            stateSelector.ItemIndex = 0;
        }

        public static List<int> GetValidYears(int start, int end, bool flip = true)
        {
            List<int> years = [];
            for (int i = start; i < end; i++) { years.Add(i); }
            if (flip) { years.Reverse(); }
            return years;
        }
    }
}