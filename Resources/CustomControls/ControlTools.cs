/* File: ControlTools.cs
 * Author: Michael Millar
 * Date: 16-11-2025
 * Description: 
 * A static utility class that contains control related functions.
 */

using System.Collections;
using System.Windows;
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
    }
}
