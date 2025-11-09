using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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
                Padding = new Thickness(1,1,4,1),
                Margin = new Thickness(2, 0, 0, 0),
                FontWeight = FontWeights.SemiBold,
            };

            label.SetBinding(Label.ContentProperty, new Binding("LabelContent") { Source = this });
            Children.Add(label);
        }
    }
}
