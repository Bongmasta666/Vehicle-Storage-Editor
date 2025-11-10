using Bongs_Vehicle_Viewer_V2.Resources.CustomControls;
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
            //This validation still sucks. W.I.P.
            string log = "";
            log += ValidateTextBox(makeTextBox);
            log += ValidateTextBox(modelTextBox);
            log += ValidateTextBox(priceTextBox);

            if (double.TryParse(priceTextBox.TextContent, out double value))
            {
                if (value < 0)
                {
                    log += "System: Price Cannot Be Negative";
                    priceTextBox.HighLight();
                }
            }
            else
            {
                log += "System: Price Must Be Numeric";
                priceTextBox.HighLight();
            }

            if (log == "")
            {
                //Vehicle factory is called alot here. This could use some improvement. 
                Vehicle v = VehicleFactory.NewVehicle(typeSelector.ItemIndex);
                AssignVehicleValues(v);

                if (VehicleFactory.AddVehicle(v))
                {
                    ResetRegistration();
                    dataGrid.ItemsSource = VehicleFactory.GetVehicleList();
                    statusOutput.Content = "System: " + v.ToString() + " added successfully";
                }
                else { statusOutput.Content = "System: Failed To Add Vehicle"; }
            }
        }

        //Right now this assumes the price checkbox has already been confirmed to be a double.
        //Todo: Price will probably be parsed before this. Try reducing the need to parse again.
        //Solution: Passing as argument may be the best solution unfortunatley.
        private void AssignVehicleValues(Vehicle v)
        {
            v.Year = ValidYears[yearSelector.ItemIndex];
            v.Make = makeTextBox.TextContent;
            v.Model = modelTextBox.TextContent;
            v.Price = double.Parse(priceTextBox.TextContent);
            v.Condition = (VehicleConditon)stateSelector.ItemIndex;
        }

        //Maybe make this a function that can be call on the text.
        private static string ValidateTextBox(LabeledTextBox textBox)
        {
            string toReturn = "";
            if (string.IsNullOrEmpty(textBox.TextContent))
            {
                textBox.HighLight();
                toReturn = $"System: {textBox.TextContent} Cannot Be Blank";
            } 
            return toReturn;
        }

        private void OnResetBtnPress(object obj, RoutedEventArgs args) => ResetRegistration();

        public void ResetRegistration()
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