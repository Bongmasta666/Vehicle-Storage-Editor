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

        //Still W.I.P..could probably justify putting this onto Factory
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
                    if (!Storage.TryEditVehicle(v)) { statusBar.DisplaySystemMessage("FAILED TO EDIT OLD VEHICLE"); return; }
                }
                else //Do Something about storage warnings here
                {
                    v.ID = VehicleFactory.GetVehicleUID();
                    if (!Storage.TryAddVehicle(v)) { statusBar.DisplaySystemMessage("FAILED TO ADD NEW VEHICLE"); return; }
                }

                ResetFields();
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
                if (!Storage.TryRemoveVehicle(Selected.ID)) { statusBar.DisplaySystemMessage("FAILED TO REMOVE VEHICLE"); }
            }
        }

        private void OnVehicleAdded(object? obj, EventArgs args)
        {
            RefreshUI();
            statusBar.DisplaySystemMessage("Vehicle Added Succesfully");
        }

        private void OnVehicleUpdated(object? obj, EventArgs args)
        {
            RefreshUI();
            UnselectItem();
            tabControl.SelectedIndex = 1;
            statusBar.DisplaySystemMessage("Vehicle Updated Successfully");
        }

        private void OnVehicleRemoved(object? obj, EventArgs args)
        {
            RefreshUI();
            UnselectItem();
            statusBar.DisplaySystemMessage("Vehicle Removed Successfully");
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

        private void UpdateStatsPage(VehicleStorage storage)
        {
            nameTracker.Text = $"{storage.Name}"; //This could probably be seperate and called when needed
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

        //Will probably need some work but should work for now.
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
            var dialog = new OpenFileDialog
            {
                Filter = "Json Files (*.json)| *.json",
                InitialDirectory = SavePath,
            };

            if (dialog.ShowDialog(this) == true)            
            {   
                try // Might as well start trying
                {      
                    StorageData data = (StorageData)MyFriendJson.GetThisPlease<StorageData>(dialog.FileName);
                    Storage ??= new(data.Name); //Intellisense suggests: Compound assigment .. Guessing its a null assigment operator. Look into this
                    Storage.LoadFromData(data);
                    LastKnownPath = Directory.GetParent(dialog.FileName)?.FullName; //?null check operator fixed intellisense highlighting. I wonder why.
                    FileName = dialog.SafeFileName;
                    nameTracker.IsEnabled = true;
                    UpdateDebugPage();
                    RefreshUI();
                }
                catch (Exception ex){ statusBar.DisplaySystemMessage(ex.Message); }
            }
        }

        private void TrySaveAs()
        {
            SaveFileDialog dialog = new()
            {
                Filter = "Json Files (*.json)| *.json",
                FileName = FileName ?? "NewFile.json", //Default file name might have to be dynamic
            };

            if (dialog.ShowDialog(this) == true) 
            {
                FileName = dialog.SafeFileName;
                LastKnownPath = Directory.GetParent(dialog.FileName)?.FullName;
                SaveToJson();
            }
        }

        //This is not automatic anymore. Consider setting a flag and possibly prompting user. Also ERROR HANDLING!!
        //This should probably trigger SaveAs function though. Be Wary of infinite loop.
        private void SaveToJson() 
        {
            if (Storage != null && FileName != null) //These might be good for now. 
            {
                StorageData data = Storage.GetSaveData();
                string path = Path.Combine(SavePath, FileName);
                MyFriendJson.SaveThisPlease(data, path);

                UpdateDebugPage();  //Maybe do something with these
                statusBar.DisplaySystemMessage("Vehicles Saved To Json File");
            }
        }

        //If the user makes a new file .. something something.. Filename, Path, Directory.. Save
        private void NewStorage()
        {
            if (Storage != null) // I dunno, seems right.
            {
                Storage.VehicleAdded -= OnVehicleAdded;
                Storage.VehicleUpdated -= OnVehicleUpdated;
                Storage.VehicleRemoved -= OnVehicleRemoved;
            }

            FileName = "NewStorage.json";
            Storage = new("New Storage");
            Storage.VehicleAdded += OnVehicleAdded;
            Storage.VehicleUpdated += OnVehicleUpdated;
            Storage.VehicleRemoved += OnVehicleRemoved;
            nameTracker.IsEnabled = true;
            UpdateDebugPage();
            RefreshUI();
        }

        private void OnStorageNameChange(object obj, KeyEventArgs args) 
        { 
            if (args.Key == Key.Enter && Storage != null)
            { 
                Storage.Name = nameTracker.Text;
                dataGrid.Focus(); // Kinda works
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
    }
}