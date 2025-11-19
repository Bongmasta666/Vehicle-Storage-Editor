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
using Newtonsoft.Json;
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
        //This should be addressed at some point too
        public static string? FileName { get; private set; }
        public static string? UserSavePath { get; private set; }

        public UserSettings settings = new();

        public VehicleStorage? Storage { get; private set; }
        public Vehicle? Selected { get; private set; }

        public Dictionary<string, LabeledControl> VehicleFields { get; private set; } = [];
        public Dictionary<string, LabeledControl> ExtendedFields { get; private set; } = [];
        public Dictionary<string, LabeledControl> ConcreteFields { get; private set; } = [];

        private readonly List<int> ValidYears = VehicleFactory.GetValidYears(2026 - 100, 2026);
        private readonly List<string> classNames = VehicleFactory.GetClassNames();

        public static readonly BitmapImage bgImg = 
            ControlTools.GetImageFromURI(Path.Combine(MyFriendJson.ImagesDir, "Abstract_AI_Art.png"), UriKind.RelativeOrAbsolute);

        public static readonly SolidColorBrush GBDark = new() { Color = (Color)ColorConverter.ConvertFromString("#306230") };
        public static readonly SolidColorBrush GBDLighter = new() { Color = (Color)ColorConverter.ConvertFromString("#9BBC0F") };

        public readonly static Dictionary<string, ColorScheme> Themes = new()
        {
            ["Standard"] = new ColorScheme(){ Name = "Standard", BGC = Brushes.GhostWhite, FGC = Brushes.Black },
            ["Gameboy"] = new ColorScheme() { Name = "Gameboy", BGC = GBDark, FGC = GBDLighter },
            ["Matrix"] = new ColorScheme() { Name = "Matrix", BGC = Brushes.Black, FGC = Brushes.LimeGreen },
            ["Neon"] = new ColorScheme() { Name = "Neon", BGC = Brushes.Indigo, FGC = Brushes.Aqua },
        };

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
            typeSelector.ItemIndex = 3; //Order Changes when adding new classes :/ Why? cause they suck.. might be based on folder order

            rootGrid.Background = new ImageBrush(bgImg);
            statusBar.StartSystemClock();
            LoadSettings();


            //Uncomment below to load sample data
            Storage = new("");
            MyFriendJson.LoadThisUpPlease(Storage, "SampleStorage.json", MyFriendJson.DefaultSaveDir);
            OnNewOrOpen("Loaded Sample Data");
        }

        private void RebuildProperties()
        {
            extendedGrid.Children.Clear();

            PropertyInfo[] extended = VehicleFactory.GetExtendedProps(typeSelector.ItemName);
            ExtendedFields = BuildFromPropInfo(extended);
            foreach (LabeledControl control in ExtendedFields.Values) { AddToPropertyGrid(control); }

            PropertyInfo[] concrete = VehicleFactory.GetConcreteProps(typeSelector.ItemName);
            ConcreteFields = BuildFromPropInfo(concrete);
            foreach (LabeledControl control in ConcreteFields.Values) { AddToPropertyGrid(control); }
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
                log += ValidateRequiredFields(ExtendedFields);

                if (log == "")
                {
                    Vehicle? v = VehicleFactory.NewVehicle(typeSelector.ItemName);
                    v.Year = ValidYears[yearSelector.ItemIndex];
                    AssignVehicleValues(v, VehicleFields);
                    AssignVehicleValues(v, ExtendedFields);

                    if (Selected != null)
                    {
                        v.ID = Selected.ID;
                        if (!Storage.TryEditVehicle(v)) { LogSystemInfo("FAILED TO EDIT OLD VEHICLE"); return; }
                    }
                    else
                    {
                        v.ID = VehicleFactory.UseVehicleUID();
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
                vehicleIDLabel.Content = $"Next ID: {VehicleFactory.VehicleUID}"; //Seems right
                dataGrid.ItemsSource = Storage.Vehicles.Values.ToList();
                UpdateStatsPage(Storage);
            }
        }

        private void PopulateAllFields(Vehicle vehicle)
        {
            typeSelector.ItemIndex = classNames.IndexOf(vehicle.Class);
            yearSelector.ItemIndex = ValidYears.IndexOf(vehicle.Year);
            AssignValuesFromDict(vehicle, VehicleFields);
            AssignValuesFromDict(vehicle, ExtendedFields);
            AssignValuesFromDict(vehicle, ConcreteFields);
        }

        public void ResetFields()
        {
            searchBar.Text = ""; // Here or down There. It's Up in the Air.
            yearSelector.ItemIndex = 0;
            submitBtn.Content = "Submit";
            ResetFieldValues(VehicleFields);
            ResetFieldValues(ExtendedFields);
        }

        public void UnselectItem()
        {
            Selected = null;
            dataGrid.SelectedIndex = -1;
            ResetFields();
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
            directoryTracker.Content = $"Save Directory: {UserSavePath ?? "Undefined"}";
            filenameTracker.Content = $"File Name: {FileName ?? "Undefined"}";
        }

        public static Dictionary<string, LabeledControl> BuildFromPropInfo(PropertyInfo[] propArray)
        {
            Dictionary<string, LabeledControl> dict = [];
            foreach (PropertyInfo item in propArray)
            {
                LabeledControl? newControl;
                Type type = item.PropertyType;

                if (item.Name == "ID") { continue; } // This here is proof something needs to be abstracted
                else if (type.IsEnum) { newControl = ControlTools.NewSelector(item.Name, Enum.GetValues(type)); }
                else { newControl = new LabeledTextBox() { LabelContent = item.Name }; }
                dict.Add(item.Name, newControl);
            }
            return dict;
        }

        public static void AssignValuesFromDict(Vehicle v, Dictionary<string, LabeledControl> fieldDict)
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


        private void NewStorage()
        {
            if (Storage != null) { Unsubscribe(Storage); }

            VehicleFactory.ResetUID();
            FileName = "NewStorage.json";
            Storage = new("New Storage");
            Subscribe(Storage);
            OnNewOrOpen($"Created New Storage"); 
        }

        //This is not automatic anymore. Consider setting a flag and possibly prompting user.
        private void SaveVehicleStorage()
        {
            if (FileName == null || UserSavePath == null) TrySaveAs();
            else if (FileName != null && UserSavePath != null && Storage != null) 
            {
                MyFriendJson.SaveThisStorage(Storage, FileName, UserSavePath);
                OnStorageSaved();
            }
        }

        private void TrySaveAs()
        {
            SaveFileDialog dialog = new() { FileName = FileName ?? "NewFile.json", Filter = "Json Files (*.json)| *.json" };
            if (dialog.ShowDialog(this) == true) 
            {
                CaptureSaveInfo(dialog.FileName);
                SaveVehicleStorage();
            }
        }

        private void TryLoad(string path)
        {
            OpenFileDialog dialog = new() { InitialDirectory = path, Filter = "Json Files (*.json)| *.json", };
            if (dialog.ShowDialog(this) == true)
            {
                if (Storage == null) { Storage = new("NewStorage"); Subscribe(Storage); }

                CaptureSaveInfo(dialog.FileName);
                MyFriendJson.LoadThisUpPlease(Storage, FileName, UserSavePath);
                OnNewOrOpen($"{Storage.Name} Was Loaded Successfully");
            }
        }

        private static void CaptureSaveInfo(string path)
        {
            UserSavePath = Directory.GetParent(path)?.FullName ?? "";
            FileName = path.Substring(UserSavePath.Length + 1);
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

        private void OnNewOrOpen(string message)
        {
            nameInput.IsEnabled = true;
            storageTracker.Content = $"Storage: {Storage?.Name}";
            LogSystemInfo(message);
            UpdateDebugPage();
            RefreshUI();
        }

        private void OnStorageSaved()
        {
            fileSaveTracker.Content = $"Last Save: {statusBar.TimeShowing}";
            LogSystemInfo($"Vehicle Storage Saved");
            UpdateDebugPage();
        }

        private void OnStorageNameChange(object obj, KeyEventArgs args)
        {
            if (args.Key == Key.Enter)
            {
                if (Storage != null)
                {
                    Storage.Name = nameInput.Text;
                    storageTracker.Content = $"Storage: {nameInput.Text}";
                    dataGrid.Focus();
                }
            }
        }

        private void OnDebugThemeChange(object obj, RoutedEventArgs args)
        {
            if (debugOutput != null)
            {
                RadioButton rbtn = (RadioButton)obj;
                string value = rbtn.Content.ToString() ?? "Standard";
                SetTheme(Themes[value]);
                settings.Theme = value;
                SaveSettings();
            }
        }

        private void SetTheme(ColorScheme theme)
        {
            debugOutput.Background = theme.BGC;
            debugOutput.Foreground = theme.FGC;
            dataGrid.Background = theme.BGC;
            dataGrid.RowBackground = (theme.Name != "Standard") ? theme.FGC : Brushes.White;

            trackPanel.Background = theme.BGC;
            storageTracker.Foreground = theme.FGC;
            milesTracker.Foreground = theme.FGC;
            priceTracker.Foreground = theme.FGC;
            motorizedTracker.Foreground = theme.FGC;
            aerialTracker.Foreground = theme.FGC;
            aquaticTracker.Foreground = theme.FGC;
            totalTracker.Foreground = theme.FGC;
        }

        public void SaveSettings()
        {
            var contents = JsonConvert.SerializeObject(settings, MyFriendJson.jsonSettings);
            var path = Path.Combine(MyFriendJson.DefaultSaveDir, "Settings.json");
            File.WriteAllText(path, contents);
        }

        public void LoadSettings()
        {
            var path = Path.Combine(MyFriendJson.DefaultSaveDir, "Settings.json");
            var contents = File.ReadAllText(path);
            settings = JsonConvert.DeserializeObject<UserSettings>(contents, MyFriendJson.jsonSettings);

            SetTheme(Themes[settings.Theme]);
            ControlTools.SetRadioBtn(settings.Theme, btnContainerTheme.Items);
        }

        private void LogSystemInfo(string message)
        {
            debugOutput.Text += $"[{statusBar.TimeShowing}] {message}\n";
            statusBar.DisplaySystemMessage(message);
        }

        private void HandleDebugInput(object obj, KeyEventArgs args)
        {
            if (args.Key == Key.Enter)
            {
                string input = debugInput.Text.Trim().ToLower();
                debugInput.Text = string.Empty;
                if (!string.IsNullOrEmpty(input))
                {
                    switch (input) // I hate switches.. do something better
                    {
                        case "cls":
                        case "clear": 
                            debugOutput.Text = string.Empty; break;
                    }
                }
            }
        }

        private void OnTypeChange(object obj, SelectionChangedEventArgs args) => RebuildProperties();
        private void OnResetBtnPress(object obj, RoutedEventArgs args) => ResetFields();
        private void OnUnselectBtnPress(object obj, RoutedEventArgs args) => UnselectItem();
        private void OnNewBtnPress(object obj, RoutedEventArgs args) => NewStorage();
        private void OnSaveBtnPress(object obj, RoutedEventArgs args) => SaveVehicleStorage();
        private void OnSaveAsBtnPress(object obj, RoutedEventArgs args) => TrySaveAs();
        private void OnOpenBtnPress(object obj, RoutedEventArgs args) => TryLoad(UserSavePath ?? "");
        private void OnExitBtnPress(object obj, RoutedEventArgs args) => Close();
    }

    public struct ColorScheme()
    {
        public string Name { get; set; } = "Standard";
        public Brush BGC { get; set; } = Brushes.White;
        public Brush FGC { get; set; } = Brushes.Black;
    }

    public class UserSettings
    {
        public string Theme { get; set; } = "Gameboy";
    }
}