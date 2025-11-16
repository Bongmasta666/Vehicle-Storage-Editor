/* File: BongsStatusBar.cs
 * Author: Michael Millar
 * Date: 16-11-2025
 * Description: 
 * A custom control that extends from <StatusBar> that can be used to display various information.
 */

using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;

namespace Bongs_Vehicle_Viewer_V2.Resources.CustomControls
{
    public class BongsStatusBar : StatusBar
    {
        private static readonly Thickness labelPadding = new(1);
        private readonly Label timeLabel = new() { Content = "00:00:00", Padding = labelPadding };
        private readonly Label outputLabel = new() { Content = "System:", Padding = labelPadding };

        public string TimeShowing => timeLabel.Content.ToString() ?? "00:00:00 AM";
        
        public BongsStatusBar() 
        {
            StatusBarItem outputContainer = new()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Content = outputLabel
            };

            StatusBarItem timeContainer = new()
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                Content = timeLabel
            };

            AddChild(outputContainer);
            AddChild(timeContainer);
        }

        //Keeping reference to the timer and such could be helpful
        public void StartSystemClock()
        {
            DispatcherTimer timer = new() { Interval = TimeSpan.FromMilliseconds(100) };
            timer.Tick += (obj, args) => { timeLabel.Content = DateTime.Now.ToLongTimeString(); };
            timer.Start();
        }

        //This may need some checks for length or some sort of scroll. 
        public void DisplaySystemMessage(string text) { outputLabel.Content = $"System [{timeLabel.Content}]: {text}"; }
    }
}
