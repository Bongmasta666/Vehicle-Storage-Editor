using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
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

        public bool IsNumericField
        {
            get { return (bool)GetValue(IsNumericFieldProperty); }
            set { SetValue(IsNumericFieldProperty, value); }
        }

        public static readonly DependencyProperty IsNumericFieldProperty =
            DependencyProperty.Register("IsNumericField", typeof(bool), typeof(LabeledTextBox), new PropertyMetadata(false));

        public Brush TBoxBgNormal { get; protected set; } = Brushes.White;
        public Brush TBoxBgHighlight { get; protected set; } = Brushes.MistyRose;

        public LabeledTextBox() : base()
        {
            textBox = new TextBox()
            {
                FontSize = 14,
            };

            textBox.SetBinding(TextBox.TextProperty, new Binding("TextContent") { Source = this, FallbackValue = "" });
            textBox.GotFocus += OnFocus;
            Children.Add(textBox);
        }

        private void OnFocus(object obj, RoutedEventArgs args) => textBox.Background = TBoxBgNormal;

        public void Reset() 
        {
            textBox.Background = TBoxBgNormal;
            TextContent = string.Empty;
        }

        public bool IsNullOrEmpty(bool highlight)
        {
            if (string.IsNullOrEmpty(TextContent))
            {
                if (highlight) { HighLight(); }
                return true;
            }
            return false;
        }

        public void HighLight() => textBox.Background = TBoxBgHighlight;
    }
}
