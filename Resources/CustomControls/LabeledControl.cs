using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;

namespace Bongs_Vehicle_Viewer_V2.Resources.CustomControls
{
    public abstract class LabeledControl : StackPanel
    {
        protected readonly Label label;

        public string LabelContent
        {
            get { return (string)GetValue(LabelContentProperty); }
            set { SetValue(LabelContentProperty, value); }
        }

        public static readonly DependencyProperty LabelContentProperty =
            DependencyProperty.Register("LabelContent", typeof(string), typeof(LabeledControl), new PropertyMetadata("Header:"));

        public LabeledControl()
        {
            label = new Label()
            {
                FontSize = 14,
                Padding = new Thickness(1, 1, 4, 1),   
            };

            label.SetBinding(Label.ContentProperty, new Binding("LabelContent") { Source = this });
            Children.Add(label);
        }
    }
}
