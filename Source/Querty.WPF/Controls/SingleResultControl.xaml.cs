using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Querty.WPF
{
    /// <summary>
    /// Interaction logic for SingleResultControl.xaml
    /// </summary>
    public partial class SingleResultControl : UserControl
    {

        public string DisplayValue
        {
            get { return (string)GetValue(DisplayValueProperty); }
            set { SetValue(DisplayValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisplayValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayValueProperty =
            DependencyProperty.Register(nameof(DisplayValue), typeof(string), typeof(SingleResultControl), new PropertyMetadata(""));


        public string DisplayText
        {
            get { return (string)GetValue(DisplayTextProperty); }
            set { SetValue(DisplayTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisplayText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayTextProperty =
            DependencyProperty.Register(nameof(DisplayText), typeof(string), typeof(SingleResultControl), new PropertyMetadata(""));




        public SolidColorBrush DisplayBrush
        {
            get { return (SolidColorBrush)GetValue(DisplayBrushProperty); }
            set { SetValue(DisplayBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisplayBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayBrushProperty =
            DependencyProperty.Register(nameof(DisplayBrush), typeof(SolidColorBrush), typeof(SingleResultControl), new PropertyMetadata(Brushes.Black));




        public SingleResultControl()
        {
            InitializeComponent();
        }
    }
}
