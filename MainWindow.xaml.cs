using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Reflection;
using System.Collections;
using System.Windows.Controls;
using System.Windows.Input;
using Bongs_Vehicle_Viewer_V2.Resources;
using Bongs_Vehicle_Viewer_V2.Resources.CustomControls;
using Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem;
using Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem.Vehicles.abstracts;
using System.Diagnostics;

namespace Bongs_Vehicle_Viewer_V2
{
    public partial class MainWindow : Window
    {
        public static string? FileName { get; private set; }
        public static string DefaultPath => MyFriendJson.WhereIsShouldISave();
        public static string? LastKnownPath { get; private set; }
        private static string SavePath => string.IsNullOrEmpty(LastKnownPath) ? DefaultPath : LastKnownPath;

        public VehicleStorage? Storage { get; private set; }
        public Vehicle? Selected { get; private set; }
        public bool IsEditing { get; private set; }

        public Dictionary<string, LabeledControl> VehicleFields { get; private set; } = [];
        public Dictionary<string, LabeledControl> ExtraFields { get; private set; } = [];

        private readonly List<int> ValidYears = VehicleFactory.GetValidYears(2026 - 50, 2026);
        private readonly List<string> classNames = VehicleFactory.GetClassNames();

        public MainWindow()
        {
            InitializeComponent();

            AddKeyBinding(this, Key.N, ModifierKeys.Control, OnNewBtnPress);
            AddKeyBinding(this, Key.O, ModifierKeys.Control, OnOpenBtnPress);
            AddKeyBinding(this, Key.S, ModifierKeys.Control, OnSaveBtnPress);
            AddKeyBinding(this, Key.E, ModifierKeys.Control, OnSaveAsBtnPress);
            AddKeyBinding(this, Key.X, ModifierKeys.Control, OnExitBtnPress);

            VehicleFields.Add("Make", makeTextBox);
            VehicleFields.Add("Model", modelTextBox);
            VehicleFields.Add("Price", priceTextBox);
            VehicleFields.Add("Condition", stateSelector);
            VehicleFields.Add("FuelType", fuelSelector);

            typeSelector.SetItemSource(classNames);
            yearSelector.SetItemSource(ValidYears);
            stateSelector.SetItemSource(Enum.GetNames(typeof(VehicleConditon)));
            fuelSelector.SetItemSource(Enum.GetNames(typeof(FuelType)));

            statusBar.StartSystemClock();
            UpdateDebugPage();

        }

        private void RebuildProperties()
        {
            ExtraFields = [];
            extendedGrid.Children.Clear();
            PropertyInfo[] subprops = VehicleFactory.GetExtendedProps(typeSelector.ItemName);

            foreach (var item in subprops)
            {
                Type type = item.PropertyType;
                LabeledControl? newControl;

                if (item.Name == "ID") { continue; }
                else if (type.IsEnum) { newControl = BuildSelector(item.Name, Enum.GetValues(type)); }
                else { newControl = new LabeledTextBox() { LabelContent = item.Name }; }

                AddToPropertyGrid(newControl);
                ExtraFields.Add(item.Name, newControl);
            }
        }

        private void AddToPropertyGrid(LabeledControl control)
        {
            Grid.SetRow(control, extendedGrid.Children.Count);
            RowDefinition r = new() { Height = GridLength.Auto };
            extendedGrid.RowDefinitions.Add(r);
            extendedGrid.Children.Add(control);
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
                    if (!Storage.TryEditVehicle(v)) { LogSystemInfo("FAILED TO EDIT OLD VEHICLE"); return; }
                }
                else //Do Something about storage warnings here
                {
                    v.ID = VehicleFactory.GetVehicleUID();
                    if (!Storage.TryAddVehicle(v)) { LogSystemInfo("FAILED TO ADD NEW VEHICLE"); return; }
                }
        
                ResetFields();
                RefreshUI();
            }
        }

        private void OnVehicleSelected(object obj, RoutedEventArgs args)
        {         
            if (dataGrid.SelectedIndex != -1)
            {
                Selected = (Vehicle)dataGrid.SelectedItem;
                PopulateAllFields(Selected);
                submitBtn.Content = "Update";
                removeBtn.IsEnabled = true;
                unselectBtn.IsEnabled = true;
                IsEditing = true;
            }
            else 
            {
                removeBtn.IsEnabled = false;
                unselectBtn.IsEnabled = false;
                Selected = null; 
            }
        }

        private void OnRemoveBtnPress(object obj, RoutedEventArgs args)
        {
            if (Storage != null && Selected != null)
            {
                if (!Storage.TryRemoveVehicle(Selected.ID)) { LogSystemInfo("FAILED TO REMOVE VEHICLE"); }
            }
        }

        private void OnVehicleAdded(object? obj, EventArgs args)
        {
            LogSystemInfo("Vehicle Added Successfully");
            RefreshUI();
        }

        private void OnVehicleUpdated(object? obj, EventArgs args)
        {
            LogSystemInfo("Vehicle Updated Successfully");
            RefreshUI();
            UnselectItem();
        }

        private void OnVehicleRemoved(object? obj, EventArgs args)
        {
            LogSystemInfo("Vehicle Removed Successfully");
            RefreshUI();
            UnselectItem();
        }

        private void RefreshUI()
        {
            if (Storage != null)
            {
                dataGrid.ItemsSource = Storage.Vehicles.Values.ToList();
                UpdateStatsPage(Storage);
            }
        }

        private void PopulateAllFields(Vehicle vehicle)
        {
            typeSelector.ItemIndex = classNames.IndexOf(vehicle.Class);
            yearSelector.ItemIndex = ValidYears.IndexOf(vehicle.Year);
            PopulateFromDict(vehicle, VehicleFields);
            PopulateFromDict(vehicle, ExtraFields);
        }

        public void ResetFields()
        {
            yearSelector.ItemIndex = 0;
            submitBtn.Content = "Submit";
            ResetFieldValues(VehicleFields);
            ResetFieldValues(ExtraFields);
        }

        public void UnselectItem()
        {
            Selected = null;
            IsEditing = false;
            dataGrid.SelectedIndex = -1;
            ResetFields();
        }

        private void OnPropScrollValueChange(object obj, RoutedPropertyChangedEventArgs<double> args)
        {
            propertyScrollView.ScrollToVerticalOffset(args.NewValue);
        }

        private void OnPropScrollChanged(object obj, ScrollChangedEventArgs args)
        {
            propertyScrollBar.Maximum = args.ExtentHeight - args.ViewportHeight;
            propertyScrollBar.ViewportSize = args.ViewportHeight;
            propertyScrollBar.Value = args.VerticalOffset;
        }

        private void LogSystemInfo(string message)
        {
            debugOutput.Text += $"[{statusBar.TimeShowing}] {message}\n";
            statusBar.DisplaySystemMessage(message);
        }

        private void UpdateStatsPage(VehicleStorage storage)
        {
            nameTracker.Text = $"{storage.Name}"; //This is linked to the textbox for storage name
            totalTracker.Content = $"Total Vehicles: {storage.Vehicles.Count}";
            priceTracker.Content = $"Total Price: {storage.TotalValue:C}";
            motorizedTracker.Content = $"Motorized Vehicles: {storage.MotorizedVehicles}";
            aerialTracker.Content = $"Aerial Vehicles: {storage.AerialVehicles}";
            aquaticTracker.Content = $"Aquatic Vehicles: {storage.AquaticVehicles}";
        }

        private void UpdateDebugPage()
        {
            //Trimming the path a bit might be nice
            directoryTracker.Content = $"Save Directory: {SavePath ?? "Undefined"}";
            filenameTracker.Content = $"File Name: {FileName ?? "Undefined"}";
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
            }return log;
        }

        private static void ResetFieldValues(Dictionary<string, LabeledControl> fieldDict)
        {
            foreach (var item in fieldDict.Values)
            {
                if (item is LabeledSelector ls) { ls.ItemIndex = 0; }
                else if (item is LabeledTextBox lt) { lt.Reset(); }
            }
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

        private static void AssignVehicleValues(Vehicle v, Dictionary<string, LabeledControl> fieldDict)
        {
            foreach (var item in fieldDict)
            {
                PropertyInfo prop = v.GetType().GetProperty(item.Key);
                Type type = prop.PropertyType;
                if (item.Value is LabeledSelector lselect) { prop.SetValue(v, lselect.ItemIndex); }
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

        private void TryOpenAndLoad()
        {
            OpenFileDialog dialog = new(){ InitialDirectory = SavePath, Filter = "Json Files (*.json)| *.json", };
            if (dialog.ShowDialog(this) == true)            
            {   
                try
                {      
                    StorageData data = (StorageData)MyFriendJson.GetThisPlease<StorageData>(dialog.FileName);
                    if (Storage == null)
                    {
                        Storage = new(data.Name);
                        Subscribe(Storage);
                    }

                    Storage.LoadFromData(data);

                    LastKnownPath = Directory.GetParent(dialog.FileName)?.FullName;
                    FileName = dialog.SafeFileName;

                    nameTracker.IsEnabled = true;
                    UpdateDebugPage();
                    RefreshUI();
                    LogSystemInfo($"{FileName} Was Loaded Successfully");
                }
                catch (Exception ex){ LogSystemInfo(ex.Message); }
            }
        }

        //This is not automatic anymore. Consider setting a flag and possibly prompting user.
        private void SaveToJson()
        {
            if (FileName == null) TrySaveAs(); //Be Wary of infinite loop.
            else if (FileName != null && Storage != null)
            {
                try
                {
                    StorageData data = Storage.GetSaveData();
                    string path = Path.Combine(SavePath, FileName);
                    MyFriendJson.SaveThisPlease(data, path);

                    UpdateDebugPage();  //Maybe do something with these
                    LogSystemInfo($"Vehicles saved to {FileName}");
                }
                catch (Exception ex) { LogSystemInfo(ex.Message); }
            }
        }

        private void TrySaveAs()
        {
            SaveFileDialog dialog = new() { FileName = FileName ?? "NewFile.json", Filter = "Json Files (*.json)| *.json" };
            if (dialog.ShowDialog(this) == true) 
            {
                FileName = dialog.SafeFileName;
                LastKnownPath = Directory.GetParent(dialog.FileName)?.FullName;
                SaveToJson();
            }
        }

        private void NewStorage()
        {
            if (Storage != null) { Unsubscribe(Storage); }

            FileName = "NewStorage.json"; // This seems weird here after adding below..
            Storage = new("New Storage"); // But than again theses have always been stinky
            Subscribe(Storage);

            nameTracker.IsEnabled = true;
            LogSystemInfo($"Created New Storage {FileName}");
            UpdateDebugPage();
            RefreshUI();
        }

        private void Subscribe(VehicleStorage storage)
        {
            storage.VehicleAdded += OnVehicleAdded;
            storage.VehicleUpdated += OnVehicleUpdated;
            storage.VehicleRemoved += OnVehicleRemoved;
        }

        private void Unsubscribe(VehicleStorage storage)
        {
            storage.VehicleAdded -= OnVehicleAdded;
            storage.VehicleUpdated -= OnVehicleUpdated;
            storage.VehicleRemoved -= OnVehicleRemoved;
        }

        private void OnStorageNameChange(object obj, KeyEventArgs args) 
        { 
            if (args.Key == Key.Enter && Storage != null)
            { 
                Storage.Name = nameTracker.Text;
                dataGrid.Focus();
            }           
        }

        private void OnTypeChange(object obj, SelectionChangedEventArgs args) => RebuildProperties();
        private void OnResetBtnPress(object obj, RoutedEventArgs args) => ResetFields();
        private void OnUnselectBtnPress(object obj, RoutedEventArgs args) => UnselectItem();
        private void OnNewBtnPress(object obj, RoutedEventArgs args) => NewStorage();
        private void OnSaveBtnPress(object obj, RoutedEventArgs args) => SaveToJson();
        private void OnSaveAsBtnPress(object obj, RoutedEventArgs args) => TrySaveAs();
        private void OnOpenBtnPress(object obj, RoutedEventArgs args) => TryOpenAndLoad();
        private void OnExitBtnPress(object obj, RoutedEventArgs args) => Close();

        private static void AddKeyBinding(UIElement control, Key key, ModifierKeys mod, ExecutedRoutedEventHandler callback)
        {
            RoutedCommand command = new();
            CommandBinding comBind = new(command, callback);
            KeyBinding keyBind = new() { Command = command, Key = key, Modifiers = mod };
            control.CommandBindings.Add(comBind);
            control.InputBindings.Add(keyBind);
        }

        private static LabeledSelector BuildSelector(string name, IEnumerable list)
        {
            LabeledSelector s = new() { LabelContent = name };
            s.SetItemSource(list);
            return s;
        }
    }
}