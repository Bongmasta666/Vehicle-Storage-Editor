using Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem;
using Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem.Vehicles.abstracts;
using System.CodeDom;
using System.Diagnostics;
using System.Reflection;
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
        public List<int> ValidYears = GetValidYears(2026 - 50, 2026);

        public MainWindow()
        {
            InitializeComponent();

            List<string> names = [];
            foreach (Type t in VehicleFactory.VehicleTypes) { names.Add(t.Name); }

            typeSelector.SetItemSource(names);
            yearSelector.SetItemSource(ValidYears);
            stateSelector.SetItemSource(Enum.GetNames(typeof(VehicleConditon)));
        }

        private void OnSubmitBtnPress(object obj, RoutedEventArgs args)
        {
            //Rough and Quick Validation
            if (makeTextBox.TextContent == "")
            {
                statusOutput.Content = "System: Make Cannot Be Blank"; return;
            }

            if (modelTextBox.TextContent == "")
            {
                statusOutput.Content = "System: Model Cannot Be Blank"; return;
            }

            double value;
            if (priceTextBox.TextContent != "")
            {
                if (double.TryParse(priceTextBox.TextContent, out value))
                {
                    if (value < 0) { statusOutput.Content = "System: Price Cannot Be Negative"; return; }
                }
                else { statusOutput.Content = "System: Price Must Be Numeric"; return; }
            }
            else { statusOutput.Content = "System: Price Cannot Be Blank"; return; }

            //Creating Vehicle
            Vehicle v = VehicleFactory.NewVehicle(typeSelector.ItemIndex);
            v.Year = ValidYears[yearSelector.ItemIndex];
            v.Make = makeTextBox.TextContent;
            v.Model = modelTextBox.TextContent;
            v.Price = value;
            v.Condition = (VehicleConditon)stateSelector.ItemIndex;

            //Adding Vehicle
            if (VehicleFactory.AddVehicle(v)) 
            {
                statusOutput.Content = "System: " + v.ToString() + " added successfully";
            }
            else { statusOutput.Content = "System: Failed To Add Vehicle"; }  
        }

        private void OnResetBtnPress(object obj, RoutedEventArgs args)
        {
            typeSelector.ItemIndex = 0;
            yearSelector.ItemIndex = 0;
            stateSelector.ItemIndex = 0;

            makeTextBox.TextContent = string.Empty;
            modelTextBox.TextContent = string.Empty;
            priceTextBox.TextContent = string.Empty;
        }


        //Should Probably Put On Vehicle Factory or Something
        public static List<int> GetValidYears(int start, int end, bool flip = true)
        {
            List<int> years = [];
            for (int i = start; i < end; i++) { years.Add(i); }
            if (flip) { years.Reverse(); }
            return years;
        }
    }
}