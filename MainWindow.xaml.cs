/* File: MainWindow.xaml.cs
 * Author: Michael Millar
 * Date: 16-11-2025
 * Description: 
 * This file is the root of the application and contains code for the Main Window and marjority of the UI functionality
 */

using Bongs_Vehicle_Viewer_V2.Resources;
using Bongs_Vehicle_Viewer_V2.Resources.CustomControls;
using Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem;
using Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem.Vehicles.abstracts;
using Microsoft.Win32;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace Bongs_Vehicle_Viewer_V2
{
    public partial class MainWindow : Window
    {
        public static readonly string ResourcesDir = MyFriendJson.WhereAreMyResource();
        public static readonly string ImagesDir = Path.Combine(ResourcesDir, "Images");
        public static readonly string DefaultSaveDir = Path.Combine(ResourcesDir, "SaveData");

        public static string? FileName { get; private set; }
        public static string? LastKnownPath { get; private set; }
        private static string SavePath => string.IsNullOrEmpty(LastKnownPath) ? DefaultSaveDir : LastKnownPath;

        public VehicleStorage? Storage { get; private set; }
        public Vehicle? Selected { get; private set; }

        public Dictionary<string, LabeledControl> VehicleFields { get; private set; } = [];
        public Dictionary<string, LabeledControl> ExtraFields { get; private set; } = [];

        private readonly List<int> ValidYears = VehicleFactory.GetValidYears(2026 - 50, 2026);
        private readonly List<string> classNames = VehicleFactory.GetClassNames();

        public static readonly BitmapImage bgImg = 
            ControlTools.GetImageFromURI(Path.Combine(ImagesDir, "Abstract_AI_Art.png"), UriKind.RelativeOrAbsolute);

        public static readonly SolidColorBrush GBDark = new() { Color = (Color)ColorConverter.ConvertFromString("#306230") };
        public static readonly SolidColorBrush GBDLighter = new() { Color = (Color)ColorConverter.ConvertFromString("#9BBC0F") };

        public static readonly ColorScheme standardScheme = new(){ Name="Standard", BGC = Brushes.White, FGC = Brushes.Black};
        public static readonly ColorScheme matrixScheme = new(){ Name="Matrix", BGC = Brushes.Black, FGC = Brushes.Lime };
        public static readonly ColorScheme neonScheme = new() { Name = "Neon", BGC = Brushes.Indigo, FGC = Brushes.Aqua };
        public static readonly ColorScheme gameboyScheme = new() { Name = "Gameboy", BGC = GBDark, FGC = GBDLighter };

        public MainWindow()
        {
            InitializeComponent();

            ControlTools.AddKeyBinding(this, Key.N, ModifierKeys.Control, OnNewBtnPress);
            ControlTools.AddKeyBinding(this, Key.O, ModifierKeys.Control, OnOpenBtnPress);
            ControlTools.AddKeyBinding(this, Key.S, ModifierKeys.Control, OnSaveBtnPress);
            ControlTools.AddKeyBinding(this, Key.E, ModifierKeys.Control, OnSaveAsBtnPress);
            ControlTools.AddKeyBinding(this, Key.X, ModifierKeys.Control, OnExitBtnPress);

            VehicleFields.Add("Make", makeTextBox);
            VehicleFields.Add("Model", modelTextBox);
            VehicleFields.Add("Price", priceTextBox);
            VehicleFields.Add("Condition", stateSelector);
            VehicleFields.Add("FuelType", fuelSelector);

            typeSelector.SetItemSource(classNames);
            yearSelector.SetItemSource(ValidYears);
            stateSelector.SetItemSource(Enum.GetNames(typeof(VehicleConditon)));
            fuelSelector.SetItemSource(Enum.GetNames(typeof(FuelType)));

            rootGrid.Background = new ImageBrush(bgImg);
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
                else if (type.IsEnum) { newControl = ControlTools.NewSelector(item.Name, Enum.GetValues(type)); }
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
            if (Storage != null)
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

                    if (Selected != null)
                    {
                        v.ID = Selected.ID;
                        if (!Storage.TryEditVehicle(v)) { LogSystemInfo("FAILED TO EDIT OLD VEHICLE"); return; }
                    }
                    else
                    {
                        v.ID = VehicleFactory.GetVehicleUID();
                        if (!Storage.TryAddVehicle(v)) { LogSystemInfo("FAILED TO ADD NEW VEHICLE"); return; }
                    }

                    ResetFields();
                    RefreshUI();
                }
            } else { LogSystemInfo("No Storage Currently Loaded."); }
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
            searchBar.Text = ""; // Here or down There. It's Up in the Air.
            yearSelector.ItemIndex = 0;
            submitBtn.Content = "Submit";
            ResetFieldValues(VehicleFields);
            ResetFieldValues(ExtraFields);
        }

        public void UnselectItem()
        {
            Selected = null;
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

        //Pretty limited right now and need better handling. For nnow it does the trick.
        private void OnSearchBarSubmit(object obj, KeyEventArgs args)
        {
            if (args.Key == Key.Enter )
            {
                string input = searchBar.Text.Trim();
                searchBar.Text = "";
                if (Storage != null)
                {               
                    if (!string.IsNullOrEmpty(input))
                    {
                        try
                        {
                            int value = int.Parse(input);
                            if (Storage.Vehicles.TryGetValue(value, out Vehicle? car))
                            {
                                LogSystemInfo("Vehicle Match Found");
                                dataGrid.SelectedItem = car;
                                dataGrid.ScrollIntoView(value);
                                dataGrid.Focus();
                            }                
                        }
                        catch (Exception ex) { LogSystemInfo(ex.Message); }
                    }
                } else { LogSystemInfo("No Storage Currently Loaded."); }
            } 
        }

        private void UpdateStatsPage(VehicleStorage storage)
        {
            nameInput.Text = $"{storage.Name}"; //This is linked to the textbox for storage name

            motorizedTracker.Content = $"Motorized Vehicles: {storage.MotorizedVehicles}";
            aerialTracker.Content = $"Aerial Vehicles: {storage.AerialVehicles}";
            aquaticTracker.Content = $"Aquatic Vehicles: {storage.AquaticVehicles}";
            totalTracker.Content = $"Total Vehicles: {storage.Vehicles.Count}";
            priceTracker.Content = $"Total Value: {storage.TotalPrice:C}";
            milesTracker.Content = $"Total Miles: {storage.TotalMiles}";
        }

        private void UpdateDebugPage()
        {       
            directoryTracker.Content = $"Save Directory: {SavePath ?? "Undefined"}";
            filenameTracker.Content = $"File Name: {FileName ?? "Undefined"}";
        }

        //If we want to display errors either make non-static or some way to communicate with statusbar 
        public static void PopulateFromDict(Vehicle v, Dictionary<string, LabeledControl> fieldDict)
        {
            foreach (var item in fieldDict)
            {
                PropertyInfo? prop = v.GetType().GetProperty(item.Key);
                if (prop != null) 
                {
                    var value = prop.GetValue(v);
                    if (value != null)
                    {
                        if (item.Value is LabeledSelector ls) { ls.ItemIndex = (int)value; }
                        else if (item.Value is LabeledTextBox lt) { lt.TextContent = value.ToString() ?? ""; }
                    }            
                }      
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
                    if (!ControlTools.ValidateTextBox(lt)) { log += $"{item.Key} Is Empty Or Invalid\n"; }
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

        //If we want to display errors either make non-static or some way to communicate with statusbar 
        private static void AssignVehicleValues(Vehicle v, Dictionary<string, LabeledControl> fieldDict)
        {
            foreach (var item in fieldDict)
            {
                PropertyInfo? prop = v.GetType().GetProperty(item.Key);
                if (prop != null) 
                {
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
                    nameInput.IsEnabled = true;

                    RefreshUI();
                    UpdateDebugPage();
                    storageTracker.Content = $"Storage: {Storage.Name}";
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
                    storageTracker.Content = $"Storage: {Storage.Name}";
                    fileSaveTracker.Content = $"Last Save {statusBar.TimeShowing}";
                    LogSystemInfo($"Vehicles saved to {FileName}");
                    UpdateDebugPage();
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
            storageTracker.Content = $"Storage: {Storage.Name}";
            nameInput.IsEnabled = true;
            Subscribe(Storage);

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
            if (args.Key == Key.Enter )
            {
                UpdateStorageName(nameInput.Text);
                dataGrid.Focus();
            }           
        }

        private void UpdateStorageName(string name)
        {
            if (Storage != null)
            {
                Storage.Name = name;
                storageTracker.Content = $"Storage: {name}";
            }
        }

        //Saving to prefs or just save folder doesnt hurt
        private void OnDebugThemeChange(object obj, RoutedEventArgs args)
        {         
            RadioButton rbtn = (RadioButton)obj;
            switch (rbtn.Content)
            {
                case "Standard":
                    SetTheme(standardScheme); break;
                case "Gameboy":
                    SetTheme(gameboyScheme); break;
                case "Matrix":
                    SetTheme(matrixScheme); break;
                case "Neon":
                    SetTheme(neonScheme); break;
                default: break;
            }
            if (tabControl != null) { tabControl.SelectedIndex = 2; }
        }

        private void SetTheme(ColorScheme theme)
        {
            if (debugOutput != null)
            {
                debugOutput.Background = theme.BGC;
                debugOutput.Foreground = theme.FGC;
            }
        }

        private void OnColorChange(object obj, RoutedEventArgs args)
        {
            if (dataGrid != null)
            {
                RadioButton rbtn = (RadioButton)obj;
                SolidColorBrush brush = new SolidColorBrush();
                switch (rbtn.Content) // For now till im more awake and can figure out how to get the color from enum or something
                {
                    case "White": brush = Brushes.GhostWhite; break;
                    case "Black": brush = Brushes.Black; break;
                    case "Green": brush = Brushes.Green; break;
                }
                dataGrid.Background = brush;
                tabControl.SelectedIndex = 2;
            }
        }

        private void LogSystemInfo(string message)
        {
            debugOutput.Text += $"[{statusBar.TimeShowing}] {message}\n";
            statusBar.DisplaySystemMessage(message);
        }

        private void OnTypeChange(object obj, SelectionChangedEventArgs args) => RebuildProperties();
        private void OnResetBtnPress(object obj, RoutedEventArgs args) => ResetFields();
        private void OnUnselectBtnPress(object obj, RoutedEventArgs args) => UnselectItem();
        private void OnNewBtnPress(object obj, RoutedEventArgs args) => NewStorage();
        private void OnSaveBtnPress(object obj, RoutedEventArgs args) => SaveToJson();
        private void OnSaveAsBtnPress(object obj, RoutedEventArgs args) => TrySaveAs();
        private void OnOpenBtnPress(object obj, RoutedEventArgs args) => TryOpenAndLoad();
        private void OnExitBtnPress(object obj, RoutedEventArgs args) => Close();
    }

    public struct ColorScheme()
    {
        public string Name { get; set; } = "Standard";
        public Brush BGC { get; set; } = Brushes.White;
        public Brush FGC { get; set; } = Brushes.Black;
    }
}