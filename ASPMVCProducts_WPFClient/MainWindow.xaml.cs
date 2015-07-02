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

		public static readonly DependencyProperty IsConnectedProperty = DependencyProperty.Register("IsConnected", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));
		public bool IsConnected
		{
			get { return (bool)this.GetValue(IsConnectedProperty); }
			set { this.SetValue(IsConnectedProperty, value); }
		}

		public MainWindow()
		{
			InitializeComponent();
			//this.Loaded += (o, e) => _QueryProducts();
		}
		private async Task _QueryProducts()
		{
			var lProducts = await ProductsAPI.GetProducts();
			m_tProductsList.ItemsSource = lProducts;
		}

		private async Task _QueryProductCategories()
		{
			var lProductCategories = await ProductsAPI.GetProductCategories();
			m_tProductCategoriesList.ItemsSource = lProductCategories;
		}

		private async void mRefreshProductListBtn_Click(object sender, RoutedEventArgs e)
		{
			await _QueryProducts();
		}

		private async void mRefreshProductCategoriesListBtn_Click(object sender, RoutedEventArgs e)
		{
			await _QueryProductCategories();
		}

		private async void mLoginBtn_Click(object sender, RoutedEventArgs e)
		{
			if (String.IsNullOrEmpty(mUserNameTxtBox.Text) || string.IsNullOrEmpty(mPwdBox.Password))
				return;

			var lUser = await ProductsAPI.LoginUser(new RegisterUserDTO() { UserName = mUserNameTxtBox.Text, Password = mPwdBox.Password });
		}

		private async void mRegisterBtn_Click(object sender, RoutedEventArgs e)
		{
			if (String.IsNullOrEmpty(mUserNameTxtBox.Text) || string.IsNullOrEmpty(mPwdBox.Password))
				return;

			var lUser = await ProductsAPI.RegisterUser(new RegisterUserDTO() { UserName = mUserNameTxtBox.Text, Password = mPwdBox.Password });
		}

		private void mCreateProductBtn_Click(object sender, RoutedEventArgs e)
		{

		}

		private void mCreateProductCategoryBtn_Click(object sender, RoutedEventArgs e)
		{

		}
	}
}
