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
        public bool isEditing = false;

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
            RefreshData();
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
                if (isEditing && Selected != null)
                {
                    AssignVehicleValues(Selected, value);
                    UnselectItem();
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

        //Price is passed here to avoid another parse. Can this be better?
        private void AssignVehicleValues(Vehicle v, double price)
        {
            v.Year = ValidYears[yearSelector.ItemIndex];
            v.Make = makeTextBox.TextContent;
            v.Model = modelTextBox.TextContent;
            v.Condition = (VehicleConditon)stateSelector.ItemIndex;
            v.Price = price;
        }

        //Kinda Temporary untill statistics tracking is better.
        private void RefreshData()
        {
            ResetFields();
            RefreshDataGrid();
            UpdateStats();
        }

        public void ResetRegistration()
        {
            ResetFields();
            UnselectItem();
        }

        //The below three functions somewhat work for now but need improving.
        private void OnVehicleSelected(object obj, RoutedEventArgs args)
        {
            if (dataGrid.SelectedIndex != -1)
            {
                editBtn.IsEnabled = true;
                removeBtn.IsEnabled = true;
                unselectBtn.IsEnabled = true;
                Selected = (Vehicle)dataGrid.SelectedItem;
            }
            else 
            {
                editBtn.IsEnabled = false;
                removeBtn.IsEnabled = false;
                unselectBtn.IsEnabled = false;
                Selected = null; 
            }
        }

        private void RemoveVehicle()
        {
            if (Selected != null)
            {
                if (VehicleFactory.RemoveVehicle(Selected.ID))
                {
                    UnselectItem();
                    UpdateStats();
                    RefreshDataGrid();
                    DisplaySystemMessage("Vehicle removed successfully");
                }
            }
        }

        private void OnEditBtnPress(object obj, RoutedEventArgs ars)
        {
            if (Selected != null)
            {
                isEditing = true;
                PopulateFields(Selected);
                tabControl.SelectedIndex = 0;
                submitBtn.Content = "Update";
                typeSelector.IsEnabled = false;
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

        public void ResetFields()
        {
            typeSelector.IsEnabled = true;

            typeSelector.ItemIndex = 0;
            yearSelector.ItemIndex = 0;
            stateSelector.ItemIndex = 0;

            makeTextBox.TextContent = string.Empty;
            modelTextBox.TextContent = string.Empty;
            priceTextBox.TextContent = string.Empty;

            submitBtn.Content = "Submit";
        }

        public void UnselectItem()
        {
            Selected = null;
            isEditing = false;
            dataGrid.SelectedIndex = -1;
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


        public void DisplaySystemMessage(string message)
        {
            statusOutput.Content = $"System [{timeLabel.Content}]: " + message;
        }

        private void OnResetBtnPress(object obj, RoutedEventArgs args) => ResetRegistration();
        private void OnRemoveBtnPress(object obj, RoutedEventArgs args) => RemoveVehicle();
        private void OnUnselectBtnPress(object obj, RoutedEventArgs args) => UnselectItem();
        public void RefreshDataGrid() => dataGrid.ItemsSource = VehicleFactory.Vehicles.Values.ToList();
    }
}