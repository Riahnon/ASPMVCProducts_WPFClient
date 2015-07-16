using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ASPMVCProducts_WPFClient
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public Visibility TrueVisibility
        {
            get;
            set;
        }
        public Visibility FalseVisibility
        {
            get;
            set;
        }

        public BoolToVisibilityConverter()
        {
            TrueVisibility = Visibility.Visible;
            FalseVisibility = Visibility.Hidden;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
            {
                var lValue = (bool)value;
                return lValue ? TrueVisibility : FalseVisibility;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
