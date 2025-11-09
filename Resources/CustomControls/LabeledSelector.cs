using System;
using System.Windows;
using System.Collections;
using System.Windows.Data;
using System.Windows.Controls;

namespace Bongs_Vehicle_Viewer_V2.Resources.CustomControls
{
    public class LabeledSelector : LabeledControl
    {
        protected readonly ComboBox comboBox;

        //This may not need to be hooked up as DP unless planning on setting source in Xaml. 
        public int ItemIndex
        {
            get { return (int)GetValue(ItemIndexProperty); }
            set { SetValue(ItemIndexProperty, value); }
        }

        public static readonly DependencyProperty ItemIndexProperty =
            DependencyProperty.Register("ItemIndex", typeof(int), typeof(LabeledSelector), new PropertyMetadata(0));

        public string ItemName => comboBox.SelectedItem.ToString() ?? "";

        public event SelectionChangedEventHandler? SelectionChanged;

        public LabeledSelector() : base() 
        {
            comboBox = new ComboBox() 
            {
                VerticalAlignment = VerticalAlignment.Center,
            };

            comboBox.SetBinding(ComboBox.SelectedIndexProperty, new Binding("ItemIndex") { Source = this });
            comboBox.SelectionChanged += OnSelectionChange;
            Children.Add(comboBox);
        }

        public void SetItemSource(IEnumerable source, int index = 0) 
        {
            comboBox.ItemsSource = source; 
            comboBox.SelectedIndex = index;
        }

        private void OnSelectionChange(object sender, SelectionChangedEventArgs e) 
        {
            //Maybe Make or Pass Custom Args
            SelectionChanged?.Invoke(this, e); 
        }
    }
}
