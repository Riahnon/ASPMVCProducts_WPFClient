using ProductsAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ASPMVCProducts_WPFClient
{
    public class ProductListViewModel : DependencyObject
    {
        public static readonly DependencyProperty ProductListProperty = DependencyProperty.Register("ProductList", typeof(ProductListDTO), typeof(ProductListViewModel),
            new PropertyMetadata());

        public ProductListDTO ProductList
        {
            get { return (ProductListDTO)GetValue(ProductListProperty); }
            set { SetValue(ProductListProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(ProductListViewModel),
            new PropertyMetadata(false));

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }
    }
}
