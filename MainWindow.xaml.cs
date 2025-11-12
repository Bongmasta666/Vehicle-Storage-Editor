using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Bongs_Vehicle_Viewer_V2.Resources.CustomControls;
using Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem;
using Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem.Vehicles.abstracts;

namespace Bongs_Vehicle_Viewer_V2
{
    public partial class MainWindow : Window
    {
        //Maybe Move These
        private readonly List<int> ValidYears = VehicleFactory.GetValidYears(2026 - 50, 2026);
        private readonly List<string> classNames = VehicleFactory.GetClassNames();

        public Vehicle? Selected { get; private set; } = null;

        public MainWindow()
        {
            InitializeComponent();

            typeSelector.SetItemSource(classNames);
            yearSelector.SetItemSource(ValidYears);
            stateSelector.SetItemSource(Enum.GetNames(typeof(VehicleConditon)));

            DispatcherTimer timer = new() { Interval = TimeSpan.FromMilliseconds(100) };
            timer.Tick += (obj, args) => { timeLabel.Content = DateTime.Now.ToLongTimeString(); };
            timer.Start();

            VehicleFactory.LoadAllVehicles();
            RefreshDataGrid();
        }

        private void OnSubmitBtnPress(object obj, RoutedEventArgs args)
        {
            //This validation still sucks. W.I.P.
            string log = "";
            log += ValidateTextBox(makeTextBox);
            log += ValidateTextBox(modelTextBox);
            log += ValidateTextBox(priceTextBox);

            double value = GetPriceValue();
            if (value == -1) { log += "Price Must Be A Positive Numeric Value"; }

            if (log == "") 
            {
                if (Selected != null)
                {
                    AssignVehicleValues(Selected, value);
                    RefreshData();
                    tabControl.SelectedIndex = 1;
                    VehicleFactory.SaveVehicleList();
                }
                else  //Vehicle factory is called alot here. This could use some improvement. 
                {
                    Vehicle? v = VehicleFactory.NewVehicle(typeSelector.ItemName);
                    AssignVehicleValues(v, value);

                    if (VehicleFactory.AddVehicle(v)) 
                    {
                        RefreshData();
                        DisplaySystemMessage("Vehicle added successfully");
                    }
                    else { DisplaySystemMessage("Failed To Add Vehicle"); }
                }
            }
        }

        //Kinda Temporary untill statistics tracking is better.
        private void RefreshData()
        {
            ResetFields();
            RefreshDataGrid();
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
            ResetFields();
            UnselectItem();
        }

        public void ResetFields()
        {
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
                Selected = (Vehicle)dataGrid.SelectedItem;
                unselectBtn.IsEnabled = true;
            }
            else { Selected = null; }
        }

        private void OnRemoveBtnPress(object obj, RoutedEventArgs args)
        {
            if (Selected != null) { RemoveVehicle(Selected); }
        }

        private void RemoveVehicle(Vehicle v)
        {
            if (VehicleFactory.RemoveVehicle(v.ID))
            {
                UnselectItem();
                UpdateStats();
                RefreshDataGrid();
                DisplaySystemMessage("Vehicle removed successfully");
            }
        }

        private void OnEditBtnPress(object obj, RoutedEventArgs ars)
        {
            if (Selected != null)
            {
                PopulateFields(Selected);
                submitBtn.Content = "Update";
                tabControl.SelectedIndex = 0;
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

        //These is semi-useless. Just helps reduce repeatition atm.
        private static string ValidateTextBox(LabeledTextBox textBox)
        {
            if (textBox.IsNullOrEmpty(true)) 
            {
                return $"{textBox.TextContent} Cannot Be Blank"; 
            }
            return "";              
        }

        private double GetPriceValue()
        {
            double toReturn = -1;
            if (double.TryParse(priceTextBox.TextContent, out double value))
            {
                if (value >= 0.0) { toReturn = value; }
                else { priceTextBox.HighLight(); }
            }
            else { priceTextBox.HighLight(); }
            return toReturn;
        }

        public void UnselectItem()
        {
            Selected = null; 
            dataGrid.SelectedIndex = -1;
            unselectBtn.IsEnabled = false;
            submitBtn.Content = "Submit";
        }

        public void DisplaySystemMessage(string message)
        {
            statusOutput.Content = $"System [{timeLabel.Content}]: " + message;
        }

        public void OnUnselectBtnPress(object obj, RoutedEventArgs args) => UnselectItem();
        public void RefreshDataGrid() => dataGrid.ItemsSource = VehicleFactory.Vehicles.Values.ToList();
    }
}