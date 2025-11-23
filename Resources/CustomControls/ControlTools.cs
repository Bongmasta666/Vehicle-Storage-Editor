/* File: ControlTools.cs
 * Author: Michael Millar
 * Date: 16-11-2025
 * Description: 
 * A static utility class that contains control related functions.
 */

using Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem.Vehicles.abstracts;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Bongs_Vehicle_Viewer_V2.Resources.CustomControls
{
    public static class ControlTools
    {
        public static void AddKeyBinding(UIElement control, Key key, ModifierKeys mod, ExecutedRoutedEventHandler callback)
        {
            RoutedCommand command = new();
            CommandBinding comBind = new(command, callback);
            KeyBinding keyBind = new() { Command = command, Key = key, Modifiers = mod };
            control.CommandBindings.Add(comBind);
            control.InputBindings.Add(keyBind);
        }

        public static BitmapImage GetImageFromURI(string path, UriKind pathtype)
        {
            BitmapImage btImg = new();
            btImg.BeginInit();
            btImg.UriSource = new Uri(path, pathtype);
            btImg.EndInit();
            return btImg;
        }

        public static LabeledSelector NewSelector(string name, IEnumerable list)
        {
            LabeledSelector s = new() { LabelContent = name };
            s.SetItemSource(list);
            return s;
        }

        public static Dictionary<string, LabeledControl> BuildFromPropInfo(PropertyInfo[] propArray, Grid propGrid)
        {
            Dictionary<string, LabeledControl> dict = [];
            foreach (PropertyInfo item in propArray)
            {
                LabeledControl? newControl;
                Type type = item.PropertyType;

                if (type.IsEnum) { newControl = NewSelector(item.Name, Enum.GetValues(type)); }
                else { newControl = new LabeledTextBox() { LabelContent = item.Name }; }

                Grid.SetRow(newControl, propGrid.Children.Count);
                RowDefinition r = new() { Height = GridLength.Auto };
                propGrid.RowDefinitions.Add(r);
                propGrid.Children.Add(newControl);

                dict.Add(item.Name, newControl);
            }
            return dict;
        }

        //Numeric validation is done in ValidateTextBox for now but we need to get the numeric value.
        public static string ValidateRequiredFields(List<LabeledControl> controls)
        {
            string log = "";
            foreach (LabeledControl item in controls)
            {
                if (item is LabeledTextBox lt)
                {
                    if (!ValidateTextBox(lt)) { log += $"{lt.LabelContent} Is Empty Or Invalid\n"; }
                }
            }
            return log;
        }

        //Kind rough but should handle everything atm. Being able to pass the numeric value would be optimal.
        public static bool ValidateTextBox(LabeledTextBox textBox)
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

        public static void AssignFromObject(object obj, Dictionary<string, LabeledControl> fieldDict)
        {
            foreach (var item in fieldDict)
            {
                PropertyInfo? prop = obj.GetType().GetProperty(item.Key);
                if (prop != null)
                {
                    var value = prop.GetValue(obj);
                    if (value != null)
                    {
                        if (item.Value is LabeledSelector ls) { ls.ItemIndex = (int)value; }
                        else if (item.Value is LabeledTextBox lt) { lt.TextContent = value.ToString() ?? ""; }
                    }
                }
            }
        }

        public static void AssignToObject(object obj, Dictionary<string, LabeledControl> fieldDict)
        {
            foreach (var item in fieldDict)
            {
                PropertyInfo? prop = obj.GetType().GetProperty(item.Key);
                if (prop != null)
                {
                    Type type = prop.PropertyType;
                    if (item.Value is LabeledSelector lselect) { prop.SetValue(obj, lselect.ItemIndex); }
                    else if (item.Value is LabeledTextBox ltbox)
                    {
                        if (type == typeof(int) || type == typeof(double))
                        {
                            //Kinda rough because we parse in validating ..  it works for now tho
                            if (double.TryParse(ltbox.TextContent, out double value)) { prop.SetValue(obj, value); }
                        }
                        else { prop.SetValue(obj, ltbox.TextContent); }
                    }
                }
            }
        }

        public static void ResetFieldValues(List<LabeledControl> controls)
        {
            foreach (LabeledControl item in controls)
            {
                if (item is LabeledSelector ls) { ls.ItemIndex = 0; }
                else if (item is LabeledTextBox lt) { lt.Reset(); }
            }
        }

        public static void SetRadioBtn(string name, ItemCollection collection)
        {
            foreach (var item in collection.SourceCollection)
            {
                RadioButton b = (RadioButton)item;
                if (b.Content.ToString() == name) { b.IsChecked = true; break; }
            }
        }
    }
}
