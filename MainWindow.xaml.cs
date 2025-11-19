/* File: MainWindow.xaml.cs
 * Author: Michael Millar
 * Date: 19-11-2025
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
        public static UserSettings Settings { get; private set; } = MyFriendJson.GetThisForMePlease<UserSettings>() ?? new();

        public static string FileName { get; private set; } = "NewStorage.json";
        public static string? UserSavePath { get; private set; }

        public VehicleStorage? Storage { get; private set; }
        public Vehicle? Selected { get; private set; }

        public Dictionary<string, LabeledControl> VehicleFields { get; private set; } = [];
        public Dictionary<string, LabeledControl> ExtendedFields { get; private set; } = [];
        public Dictionary<string, LabeledControl> ConcreteFields { get; private set; } = [];

        private readonly List<int> ValidYears = VehicleFactory.GetValidYears(2026 - 100, 2026);
        private readonly List<string> classNames = VehicleFactory.GetClassNames();
        private readonly int startType = 3; //Order Changes when adding new classes :/ This is also good to save now tho.

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

            SetTheme(Themes[Settings.Theme]);
            rootGrid.Background = new ImageBrush(bgImg);

            ControlTools.SetRadioBtn(Settings.Theme, btnContainerTheme.Items);
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
            typeSelector.ItemIndex = startType;

            statusBar.StartSystemClock(); //Doing this here prevents clock running in editor
            NewStorage();
        }

        private void RebuildProperties()
        {
            extendedGrid.Children.Clear();

            PropertyInfo[] extended = VehicleFactory.GetExtendedProps(typeSelector.ItemName);
            ExtendedFields = ControlTools.BuildFromPropInfo(extended, extendedGrid);

            PropertyInfo[] concrete = VehicleFactory.GetConcreteProps(typeSelector.ItemName);
            ConcreteFields = ControlTools.BuildFromPropInfo(concrete, extendedGrid);
        }

        private void OnSubmitBtnPress(object obj, RoutedEventArgs args)
        {
            if (Storage != null)
            {
                string log = ""; //Were still doing nothing with this.
                log += ControlTools.ValidateRequiredFields([.. VehicleFields.Values]);
                log += ControlTools.ValidateRequiredFields([.. ExtendedFields.Values]);
                log += ControlTools.ValidateRequiredFields([.. ConcreteFields.Values]);

                if (log == "")
                {
                    Vehicle? v = VehicleFactory.NewVehicle(typeSelector.ItemName);
                    if (v != null) 
                    {
                        ControlTools.AssignToObject(v, VehicleFields);
                        ControlTools.AssignToObject(v, ExtendedFields);
                        ControlTools.AssignToObject(v, ConcreteFields);
                        v.Year = ValidYears[yearSelector.ItemIndex];

                        if (Selected != null)
                        {
                            v.ID = Selected.ID;
                            if (!Storage.TryEditVehicle(v)) { LogSystemInfo("FAILED TO EDIT OLD VEHICLE"); return; }
                        }
                        else //Doing something better with logging will futrther improve this
                        {
                            v.ID = VehicleFactory.UseVehicleUID();
                            if (!Storage.TryAddVehicle(v)) { LogSystemInfo("FAILED TO ADD NEW VEHICLE"); return; }
                        } 
                    }            
                }
            } else { LogSystemInfo("No Storage Currently Loaded."); }
        }

        //Pretty limited right now and need better handling. For nnow it does the trick.
        private void OnSearchBarSubmit(object obj, KeyEventArgs args)
        {
            if (args.Key == Key.Enter)
            {
                if (Storage != null)
                {
                    string input = searchBar.Text.Trim();
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
                            else
                            {
                                searchBar.Text = "";
                                LogSystemInfo("No Match Found");
                            }
                        }
                        catch (Exception ex) { LogSystemInfo(ex.Message); }
                    }
                } else { LogSystemInfo("No Storage Currently Loaded."); }
            }
        }

        private void OnVehicleSelected(object obj, RoutedEventArgs args)
        {         
            if (dataGrid.SelectedIndex != -1)
            {
                Selected = (Vehicle)dataGrid.SelectedItem;
                ControlTools.AssignFromObject(Selected, VehicleFields);
                ControlTools.AssignFromObject(Selected, ExtendedFields);
                ControlTools.AssignFromObject(Selected, ConcreteFields);

                typeSelector.ItemIndex = classNames.IndexOf(Selected.Class);
                yearSelector.ItemIndex = ValidYears.IndexOf(Selected.Year);

                submitBtn.Content = "Update";
                searchBar.Text = Selected.ID.ToString();
                removeBtn.IsEnabled = unselectBtn.IsEnabled = true;
            }
            else { removeBtn.IsEnabled = unselectBtn.IsEnabled = false; }
        }


        private void OnVehicleAdded(object? obj, EventArgs args)
        {
            LogSystemInfo("Vehicle Added Successfully");
            ResetFields();
            RefreshUI();
        }

        private void OnVehicleUpdated(object? obj, EventArgs args)
        {
            LogSystemInfo("Vehicle Updated Successfully");
            UnselectItem();
            RefreshUI();
        }

        private void OnVehicleRemoved(object? obj, EventArgs args)
        {
            LogSystemInfo("Vehicle Removed Successfully");
            UnselectItem();
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (Storage != null)
            {
                vehicleIDLabel.Content = $"Next ID: {VehicleFactory.VehicleUID}";
                dataGrid.ItemsSource = Storage.Vehicles.Values.ToList();
                UpdateStatsPage(Storage);
            }
        }

        public void ResetFields()
        {
            ControlTools.ResetFieldValues([.. VehicleFields.Values]);
            ControlTools.ResetFieldValues([.. ExtendedFields.Values]);
            ControlTools.ResetFieldValues([.. ConcreteFields.Values]);

            yearSelector.ItemIndex = 0;
            typeSelector.ItemIndex = startType;

            if (Selected != null) { dataGrid.Focus(); }
        }

        public void UnselectItem()
        {
            Selected = null;
            dataGrid.SelectedIndex = -1;
            submitBtn.Content = "Submit";
            searchBar.Text = ""; 
            ResetFields();
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
            filenameTracker.Content = $"File Name: {FileName}";
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
            if (UserSavePath == null) TrySaveAs();
            else if (UserSavePath != null && Storage != null) 
            {
                MyFriendJson.SaveThisStorage(Storage, FileName, UserSavePath);
                OnStorageSaved();
            }
        }

        private void TrySaveAs()
        {
            SaveFileDialog dialog = new() { FileName = FileName , Filter = "Json Files (*.json)| *.json" };
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
                Settings.Theme = value;
                SetTheme(Themes[value]);
                MyFriendJson.SaveTheseSettings(Settings);
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

        private void OnRemoveBtnPress(object obj, RoutedEventArgs args)
        {
            if (Storage != null && Selected != null)
            {
                if (!Storage.TryRemoveVehicle(Selected.ID)) { LogSystemInfo("FAILED TO REMOVE VEHICLE"); }
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