using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;

namespace Bongs_Vehicle_Viewer_V2.Resources.CustomControls
{
    public class LabeledTextBox : LabeledControl
    {
        protected readonly TextBox textBox;

        public string TextContent
        {
            get { return (string)GetValue(TextContentProperty); }
            set { SetValue(TextContentProperty, value); }
        }

        public static readonly DependencyProperty TextContentProperty =
            DependencyProperty.Register("TextContent", typeof(string), typeof(LabeledTextBox), new PropertyMetadata(""));

        public LabeledTextBox() : base()
        {
            textBox = new TextBox()
            {
                FontSize = 14,
            };

            textBox.SetBinding(TextBox.TextProperty, new Binding("TextContent") { Source = this, FallbackValue = "" });
            Children.Add(textBox);
        }
    }
}
