using ProductsAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ASPMVCProducts_WPFClient
{
    public class ToProductListViewModelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is IEnumerable<ProductListDTO>)
            {
                return ((IEnumerable<ProductListDTO>)value).Select(aProductList => new ProductListViewModel() { ProductList = aProductList });
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
