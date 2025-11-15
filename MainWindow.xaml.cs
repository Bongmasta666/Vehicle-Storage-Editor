using System.Windows;
using System.Reflection;
using System.Collections;
using System.Windows.Controls;
using System.Windows.Threading;
using Bongs_Vehicle_Viewer_V2.Resources;
using Bongs_Vehicle_Viewer_V2.Resources.CustomControls;
using Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem;
using Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem.Vehicles.abstracts;
using System.Diagnostics;

namespace Bongs_Vehicle_Viewer_V2
{
    public partial class MainWindow : Window
    {
        public string SavePath { get; private set; } = "vehicle_nsdata.json";
        public VehicleStorage Storage { get; private set; }
        public Vehicle? Selected { get; private set; } = null;
        public bool IsEditing { get; private set; } = false;

        public Dictionary<string, LabeledControl> VehicleFields { get; private set; } = [];
        public Dictionary<string, LabeledControl> ExtraFields { get; private set; } = [];

        private readonly List<int> ValidYears = VehicleFactory.GetValidYears(2026 - 50, 2026);
        private readonly List<string> classNames = VehicleFactory.GetClassNames();

        public MainWindow()
        {
            InitializeComponent();

            VehicleFields.Add("Make", makeTextBox);
            VehicleFields.Add("Model", modelTextBox);
            VehicleFields.Add("Price", priceTextBox);
            VehicleFields.Add("Condition", stateSelector);

            typeSelector.SetItemSource(classNames);
            yearSelector.SetItemSource(ValidYears);
            stateSelector.SetItemSource(Enum.GetNames(typeof(VehicleConditon)));

            DispatcherTimer timer = new() { Interval = TimeSpan.FromMilliseconds(100) };
            timer.Tick += (obj, args) => { timeLabel.Content = DateTime.Now.ToLongTimeString(); };
            timer.Start();

            StorageData data = (StorageData)MyFriendJson.GetThisPlease<StorageData>(SavePath);
            Storage = new VehicleStorage(data.Name);

            //Testing New Message handling
            Storage.VehicleAdded += (obj, args) => DisplaySystemMessage("Vehicle Added Successfully.");
            Storage.VehicleRemoved += (obj, args) => DisplaySystemMessage("Vehicle Removed Successfully.");

            Storage.LoadFromData(data);
            RefreshData();
        }

        //Still W.I.P. .. Are things still jank?
        private static BindingFlags PropertyFlags => BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;
        private void RebuildProperties()
        {
            ExtraFields = [];
            testGrid.Children.Clear();
            Type type = VehicleFactory.TypeDictonary[typeSelector.ItemName];
            PropertyInfo[] subprops = type.BaseType.GetProperties(PropertyFlags);
            foreach (var item in subprops)
            {
                LabeledControl? p;
                Type t = item.PropertyType;

                if (item.Name == "ID") { continue; }
                else if (t.IsEnum) { p = BuildSelector(item.Name, Enum.GetValues(t)); }
                else { p = new LabeledTextBox() { LabelContent = item.Name }; }

                ExtraFields.Add(item.Name, p);

                RowDefinition r = new() { Height = GridLength.Auto };
                testGrid.RowDefinitions.Add(r);
                Grid.SetRow(p, testGrid.Children.Count);
                testGrid.Children.Add(p);
            }
        }

        private void OnSubmitBtnPress(object obj, RoutedEventArgs args)
        {
            string log = "";
            log += ValidateRequiredFields(VehicleFields);
            log += ValidateRequiredFields(ExtraFields);

            if (log == "")
            {
                Vehicle? v = VehicleFactory.NewVehicle(typeSelector.ItemName);
                v.Year = ValidYears[yearSelector.ItemIndex];

                AssignVehicleValues(v, VehicleFields);
                AssignVehicleValues(v, ExtraFields);

                if (IsEditing && Selected != null)
                {
                    v.ID = Selected.ID;
                    if (Storage.TryEditVehicle(v))
                    {
                        UnselectItem(); // This kinda sucks, grid focus is a pain tho.
                        DisplaySystemMessage("Vehicle was edited succesfully");
                        tabControl.SelectedIndex = 1;
                    }
                    else { DisplaySystemMessage("FAILED TO EDIT OLD VEHICLE"); return; }
                }
                else //MAYBE I SHOULD USE EVENTS! CAUSE ALL THIS UP AND DOWN SUCKS!
                {
                    v.ID = VehicleFactory.GetVehicleUID();
      
                    if (!Storage.TryAddVehicle(v))
                    {
                        DisplaySystemMessage("FAILED TO ADD NEW VEHICLE"); return;

                    }

                }

                RefreshData();
                SaveToJson();
            }
        }

        //Will probably need some work but should work for now.
        private static void AssignVehicleValues(Vehicle v, Dictionary<string, LabeledControl> fieldDict)
        {
            foreach (var item in fieldDict)
            {
                PropertyInfo prop = v.GetType().GetProperty(item.Key);
                Type type = prop.PropertyType;
                if (item.Value is LabeledSelector lselect)
                {
                    prop.SetValue(v, lselect.ItemIndex);
                }

                else if (item.Value is LabeledTextBox ltbox) 
                {
                    if (type == typeof(int) || type == typeof(double))
                    {
                        //Kinda rough because we parse in validating ..  it works for now tho
                        if (double.TryParse(ltbox.TextContent, out double value)) { prop.SetValue(v, value); }
                    }
                    else { prop.SetValue(v, ltbox.TextContent); }
                }           
            }
        }

        //Kinda Temporary untill statistics tracking is better. .. Now more stinky and smelly than ever!!
        private void RefreshData()
        {
            yearSelector.ItemIndex = 0;
            submitBtn.Content = "Submit";
            ResetFieldValues(VehicleFields);
            ResetFieldValues(ExtraFields);
            RefreshDataGrid();
            UpdateStats();
        }

        public void ResetRegistration()
        {
            yearSelector.ItemIndex = 0;
            submitBtn.Content = "Submit";
            ResetFieldValues(VehicleFields);
            ResetFieldValues(ExtraFields);
            UnselectItem();
        }

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
            if (Selected != null && Storage.TryRemoveVehicle(Selected.ID))
            {
                UnselectItem();
                UpdateStats();
                RefreshDataGrid();


                SaveToJson();
            }
        }

        private void OnEditBtnPress(object obj, RoutedEventArgs ars)
        {
            if (Selected != null)
            {
                IsEditing = true;
                PopulateAllFields(Selected);
                tabControl.SelectedIndex = 0;
                submitBtn.Content = "Update";
            }
        }

        private void PopulateAllFields(Vehicle vehicle)
        {
            typeSelector.ItemIndex = classNames.IndexOf(vehicle.Class);
            yearSelector.ItemIndex = ValidYears.IndexOf(vehicle.Year);
            PopulateFromDict(vehicle, VehicleFields);
            PopulateFromDict(vehicle, ExtraFields);
        }

        public void UnselectItem()
        {
            Selected = null;
            IsEditing = false;
            dataGrid.SelectedIndex = -1;
        }
        
        private void UpdateStats()
        {
            totalTracker.Content = $"Total Vehicles: {Storage.Vehicles.Count}";
            priceTracker.Content = $"Total Price: {Storage.TotalValue:C}";
            motorizedTracker.Content = $"Motorized Vehicles: {Storage.MotorizedVehicles}";
            aerialTracker.Content = $"Aerial Vehicles: {Storage.AerialVehicles}";
            aquaticTracker.Content = $"Aquatic Vehicles: {Storage.AquaticVehicles}";
        }

        public void DisplaySystemMessage(string message)
        {
            statusOutput.Content = $"System [{timeLabel.Content}]: " + message;
        }

        private void SaveToJson()
        {
            StorageData data = new("Test Storage", [.. Storage.Vehicles.Values]);
            MyFriendJson.SaveThisPlease(data, SavePath);
        }

        public static void PopulateFromDict(Vehicle v, Dictionary<string, LabeledControl> fieldDict)
        {
            foreach (var item in fieldDict)
            {
                //Some Null Checking Here Would Be Good..
                var value = v.GetType().GetProperty(item.Key).GetValue(v);
                if (item.Value is LabeledSelector ls) { ls.ItemIndex = (int)value; }
                else if (item.Value is LabeledTextBox lt) { lt.TextContent = value.ToString(); }
            }
        }

        //Numeric validation is done in ValidateTextBox for now but we need to get the numeric value.
        private static string ValidateRequiredFields(Dictionary<string, LabeledControl> fieldDict)
        {
            string log = "";
            foreach (var item in fieldDict)
            {
                if (item.Value is LabeledTextBox lt)
                {
                    if (!ValidateTextBox(lt)) { log += $"{item.Key} Is Empty Or Invalid\n"; }
                }
            }
            return log;
        }

        private static void ResetFieldValues(Dictionary<string, LabeledControl> fieldDict)
        {
            foreach (var item in fieldDict.Values)
            {
                if (item is LabeledSelector ls) { ls.ItemIndex = 0; }
                else if (item is LabeledTextBox lt) { lt.Reset(); }
            }
        }

        private static LabeledSelector BuildSelector(string name, IEnumerable list)
        {
            LabeledSelector s = new() { LabelContent = name };
            s.SetItemSource(list);
            return s;
        }

        //Kind rough but should handle everything atm. Being able to pass the numeric value would be optimal.
        private static bool ValidateTextBox(LabeledTextBox textBox)
        {
            if (textBox.IsNullOrEmpty(true)) { return false; }
            if (textBox.IsNumericField)
            {
                if (double.TryParse(textBox.TextContent, out double value))
                {
                    if (value < 0) { textBox.HighLight(); return false; }
                }
                else { return false; }
            }
            return true;
        }

        private void RefreshDataGrid() => dataGrid.ItemsSource = Storage.Vehicles.Values.ToList();
        private void OnTypeChange(object obj, SelectionChangedEventArgs args) => RebuildProperties();
        private void OnResetBtnPress(object obj, RoutedEventArgs args) => ResetRegistration();
        private void OnRemoveBtnPress(object obj, RoutedEventArgs args) => RemoveVehicle();
        private void OnUnselectBtnPress(object obj, RoutedEventArgs args) => UnselectItem();
    }
}