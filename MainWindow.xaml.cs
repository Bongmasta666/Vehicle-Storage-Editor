using System.Collections;
using System.Windows;
using System.Windows.Threading;
using Bongs_Vehicle_Viewer_V2.Resources.CustomControls;
using Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem;
using Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem.Vehicles.abstracts;
using System.Windows.Controls;
using System.Diagnostics;

namespace Bongs_Vehicle_Viewer_V2
{
    public partial class MainWindow : Window
    {
        public VehicleStorage Storage { get; private set; }
        public Vehicle? Selected { get; private set; } = null;
        public bool IsEditing { get; private set; } = false;

        private readonly List<int> ValidYears = VehicleFactory.GetValidYears(2026 - 50, 2026);
        private readonly List<string> classNames = VehicleFactory.GetClassNames();

        public MainWindow()
        {
            InitializeComponent();

            Storage = new VehicleStorage("Test");

            typeSelector.SetItemSource(classNames);
            yearSelector.SetItemSource(ValidYears);
            stateSelector.SetItemSource(Enum.GetNames(typeof(VehicleConditon)));

            DispatcherTimer timer = new() { Interval = TimeSpan.FromMilliseconds(100) };
            timer.Tick += (obj, args) => { timeLabel.Content = DateTime.Now.ToLongTimeString(); };
            timer.Start();

            Storage.LoadAllVehicles();
            RefreshData();
        }

        //Build out properties programmatically. WIP
        //Certain props will always be available, these should be pre built.
        //Then build extended props followed by concreate props.
        //This will make a dynamic form easier but we will have to do something about validating dynamic props. 
        //private void TestFoo(object obj, SelectionChangedEventArgs args)
        //{
        //    testGrid.Children.Clear();
        //    Type type = VehicleFactory.TypeDictonary[typeSelector.ItemName];
        //    var props = type.GetProperties();
        //    foreach (var item in props)
        //    {
        //        LabeledControl? p;
        //        Type t = item.PropertyType;
        //        if (item.Name == "ID") { continue; }
        //        else if (item.Name == "Class")
        //        {
        //            p = BuildSelector(item.Name, classNames);
        //        }
        //        else if (item.Name == "Year")
        //        {
        //            p = BuildSelector(item.Name, ValidYears);
        //        }
        //        else if (t.IsEnum) { p = BuildSelector(item.Name, Enum.GetValues(t)); }
        //        else
        //        {
        //            p = new LabeledTextBox() { LabelContent = item.Name };     
        //        }
        //        RowDefinition r = new() { Height = GridLength.Auto };
        //        testGrid.RowDefinitions.Add(r);
        //        Grid.SetRow(p, testGrid.Children.Count);
        //        testGrid.Children.Add(p);
           
        //    }
        //}

        private LabeledSelector BuildSelector(string name, IEnumerable list)
        {
            LabeledSelector s = new() { LabelContent = name };
            s.SetItemSource(list);
            return s;
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

            //Below is still kinda dirty and smelly, some events might help
            if (log == "")  
            {
                Vehicle? v = VehicleFactory.NewVehicle(typeSelector.ItemName);
                AssignVehicleValues(v, value);
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
                else
                {
                    v.ID = VehicleFactory.GetVehicleUID();
                    if (Storage.TryAddVehicle(v))
                    {
                        DisplaySystemMessage("Vehicle was added succesfully");
                    }
                    else { DisplaySystemMessage("FAILED TO ADD NEW VEHICLE"); return; }
                }

                RefreshData();
                Storage.SaveVehicleList();
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
                if (Storage.TryRemoveVehicle(Selected.ID))
                {
                    UnselectItem();
                    UpdateStats();
                    RefreshDataGrid();
                    DisplaySystemMessage("Vehicle removed successfully");
                    Storage.SaveVehicleList();
                }
            }
        }

        private void OnEditBtnPress(object obj, RoutedEventArgs ars)
        {
            if (Selected != null)
            {
                IsEditing = true;
                PopulateFields(Selected);
                tabControl.SelectedIndex = 0;
                submitBtn.Content = "Update";
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
        public void RefreshDataGrid() => dataGrid.ItemsSource = Storage.Vehicles.Values.ToList();
    }
}