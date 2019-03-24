using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Instagram_Checker
{
    /// <summary>
    /// Interaction logic for Numeric_UpDown.xaml
    /// </summary>
    public partial class Numeric_UpDown : UserControl
    {
        private int _valueChanging;

        public Numeric_UpDown()
        {
            InitializeComponent();
            tbCur_value.Text = Value.ToString();
            Value = 10;
            _valueChanging = 1;
        }

        public static DependencyProperty ValueProprerty;

        static Numeric_UpDown()
        {
            FrameworkPropertyMetadata propMetadata = new FrameworkPropertyMetadata();
            propMetadata.DefaultValue = 0;
            propMetadata.PropertyChangedCallback = ValueChangedCallback;

            ValueProprerty = DependencyProperty.Register
                         (
                             "Value",
                             typeof(int),
                             typeof(Numeric_UpDown),
                             propMetadata
                         );
        }

        private static void ValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (Numeric_UpDown)d;
            obj.tbCur_value.Text = obj.Value.ToString();
        }

        #region int PLACES
        public int int_Places
        {
            get { return (int)GetValue(int_PlacesProperty); }
            set
            {
                SetValue(int_PlacesProperty, value);
            }
        }

        public static readonly DependencyProperty int_PlacesProperty =
            DependencyProperty.Register("int_Places", typeof(int), typeof(Numeric_UpDown), new PropertyMetadata(0.0)
            {
                DefaultValue = 0,
                PropertyChangedCallback = PlacesChangedCallback
            });

        private static void PlacesChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (Numeric_UpDown)d;
            String.Format($"N{obj.int_Places.ToString()}", obj.tbCur_value);

        }
        #endregion

        #region VALUE
        public int Value
        {
            get { return (int)GetValue(ValueProprerty); }
            set
            {
                SetValue(ValueProprerty, value);
            }
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            Value += _valueChanging;
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            Value -= _valueChanging;
        }
        #endregion

        private void tbCur_value_TextChanged(object sender, TextChangedEventArgs e)
        {
            int temp = 0;
            if (Int32.TryParse(tbCur_value.Text, out temp))
            {
                Value = temp;
            }
        }
    }
}
