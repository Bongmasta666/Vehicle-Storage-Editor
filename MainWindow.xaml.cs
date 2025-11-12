using Bongs_Vehicle_Viewer_V2.Resources.CustomControls;
using Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem;
using Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem.Vehicles.abstracts;
using Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem.Vehicles.motorized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Bongs_Vehicle_Viewer_V2
{
    public partial class MainWindow : Window
    {
        //Maybe Move These
        private readonly List<int> ValidYears = VehicleFactory.GetValidYears(2026 - 50, 2026);
        private readonly List<string> classNames = VehicleFactory.GetClassNames();

        private Vehicle? selected = null;
        
        public MainWindow()
        {
            InitializeComponent();

            typeSelector.SetItemSource(classNames);
            yearSelector.SetItemSource(ValidYears);
            stateSelector.SetItemSource(Enum.GetNames(typeof(VehicleConditon)));

            DispatcherTimer timer = new() { Interval = TimeSpan.FromMilliseconds(100) };
            timer.Tick += (obj, args) => { timeLabel.Content = DateTime.Now.ToLongTimeString(); };
            timer.Start();

            LoadAllVehicles();
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
                    log += "Price Cannot Be Negative";
                    priceTextBox.HighLight();
                }
            } 
            else
            {
                log += "Price Must Be Numeric";
                priceTextBox.HighLight();
            }

            if (log == "")
            {
                //Vehicle factory is called alot here. This could use some improvement. 
                Vehicle v = VehicleFactory.NewVehicle(typeSelector.ItemName);
                AssignVehicleValues(v, value);

                if (VehicleFactory.AddVehicle(v)) { OnVehicleAdded(); }
                else { DisplaySystemMessage("Failed To Add Vehicle"); }
            }
        }

        //Register this to an event and/or put this function somewhere else
        private void OnVehicleAdded()
        {
            ResetRegistration();
            dataGrid.ItemsSource = VehicleFactory.GetVehicleList();
            DisplaySystemMessage("Vehicle added successfully");
            UpdateStats();
        }

        //Price is passed here to avoid another parse. Can this be better?
        private void AssignVehicleValues(Vehicle v, double price)
        {
            v.Year = ValidYears[yearSelector.ItemIndex];
            v.Make = makeTextBox.TextContent;
            v.Model = modelTextBox.TextContent;
            v.Condition = (VehicleConditon)stateSelector.ItemIndex;
            v.Price = price;
        }

        private void OnResetBtnPress(object obj, RoutedEventArgs args) => ResetRegistration();

        public void ResetRegistration()
        {
            selected = null; //Maybe Not Here, But for now.

            typeSelector.ItemIndex = 0;
            yearSelector.ItemIndex = 0;
            stateSelector.ItemIndex = 0;

            makeTextBox.TextContent = string.Empty;
            modelTextBox.TextContent = string.Empty;
            priceTextBox.TextContent = string.Empty;
        }

        private void OnVehicleSelected(object obj, SelectionChangedEventArgs args)
        {
            if (obj != null) 
            { 
                selected = (Vehicle)dataGrid.SelectedItem; 
            }
            else { selected = null; }
        }

        private void OnRemoveBtnPress(object obj, RoutedEventArgs args)
        {
            if (selected != null) { RemoveVehicle(selected); }
        }

        private void RemoveVehicle(Vehicle v)
        {
            if (VehicleFactory.RemoveVehicle(v.ID))
            {
                UpdateStats();
                dataGrid.ItemsSource = VehicleFactory.GetVehicleList();
                DisplaySystemMessage("Vehicle removed successfully");
            }
        }

        private void OnEditBtnPress(object obj, RoutedEventArgs ars)
        {
            if (selected != null) 
            {
                tabControl.SelectedIndex = 0;
                PopulateFields(selected);
            }       
        }

        public void PopulateFields(Vehicle v)
        {
            typeSelector.ItemIndex = classNames.IndexOf(v.Class);
            yearSelector.ItemIndex = ValidYears.IndexOf(v.Year);
            makeTextBox.TextContent = v.Make;
            modelTextBox.TextContent = v.Model;
            priceTextBox.TextContent = v.Price.ToString();
            stateSelector.ItemIndex = (int)v.Condition;
        }

        private void UpdateStats()
        {
            VehicleFactory.UpdateStats();
            totalTracker.Content = $"Total Vehicles: {VehicleFactory.VehicleCount}";
            priceTracker.Content = $"Total Price: {VehicleFactory.TotalPrice:C}";
            motorizedTracker.Content = $"Motorized Vehicles: {VehicleFactory.MotorizedVehicles}";
            aerialTracker.Content = $"Aerial Vehicles: {VehicleFactory.AerialVehicles}";
            aquaticTracker.Content = $"Aquatic Vehicles: {VehicleFactory.AquaticVehicles}";
        }

        public void OnSaveBtnPress(object obj, RoutedEventArgs args) => SaveVehicleList();

        private static string ValidateTextBox(LabeledTextBox textBox)
        {
            if (textBox.IsNullOrEmpty(true))
            {
                return $"{textBox.TextContent} Cannot Be Blank";
            }
            return "";
        }

        public void DisplaySystemMessage(string message)
        {
            statusOutput.Content = $"System [{timeLabel.Content}]: " + message;
        }



        //Quick Rough Loading. Needs Abstraction and Adding vehicle this way increments the ID sequence.
        //Sequence value can either be saved or it's possible to order by ID and get the highest value.
        public void LoadAllVehicles()
        {
            var contents = File.ReadAllText("vehicles.txt");
            List<JsonElement> vehicles = JsonSerializer.Deserialize<List<JsonElement>>(contents);
            foreach (var item in vehicles)
            {
                if (item.TryGetProperty("Class", out JsonElement prop))
                {
                    if (VehicleFactory.TypeDictonary.TryGetValue(prop.ToString(), out Type t))
                    {
                        Vehicle v = (Vehicle)JsonSerializer.Deserialize(item, t);
                        VehicleFactory.AddVehicle(v);
                    }          
                }              
            }
            dataGrid.ItemsSource = VehicleFactory.GetVehicleList();
        }

        public static void SaveVehicleList()
        {
            var json = JsonSerializer.Serialize(VehicleFactory.GetVehicleList());
            File.WriteAllText("vehicles.txt", json);
        }
    }
}