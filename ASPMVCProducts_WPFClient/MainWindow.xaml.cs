using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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

namespace ASPMVCProducts_WPFClient
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		
		public MainWindow()
		{
			InitializeComponent();
			//this.Loaded += (o, e) => _QueryProducts();
		}
		private async Task _QueryProducts()
		{
            var lProducts = await ProductsAPI.GetProducts();
            //m_tProductsList.ItemsSource = lProducts;
		}

        private async void mRefreshProductListBtn_Click(object sender, RoutedEventArgs e)
        {
            await _QueryProducts();
        }


        private void mLoginBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void mRegisterBtn_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(mUserNameTxtBox.Text) || string.IsNullOrEmpty(mPwdBox.Password))
                return;

            var lUser = await ProductsAPI.RegisterUser(new CreateUserDTO() { UserName = mUserNameTxtBox.Text, Password = mPwdBox.Password });
        }
	}
}
