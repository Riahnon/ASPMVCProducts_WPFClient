using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

		public static readonly DependencyProperty UserProperty = DependencyProperty.Register("User", typeof(UserDTO), typeof(MainWindow), new PropertyMetadata(UserDTO.NO_USER));
		public UserDTO User
		{
			get { return (UserDTO)this.GetValue(UserProperty); }
			set { this.SetValue(UserProperty, value); }
		}

		public static readonly DependencyProperty IsAwaitingAPIClientProperty = DependencyProperty.Register("IsAwaitingAPIClient", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));
		public bool IsAwaitingAPIClient
		{
			get { return (bool)this.GetValue(IsAwaitingAPIClientProperty); }
			set { this.SetValue(IsAwaitingAPIClientProperty, value); }
		}

		ProductsAPIClient mAPIClient; 
		public MainWindow()
		{
			InitializeComponent();
			mAPIClient = new ProductsAPIClient();
			mAPIClient.PropertyChanged += _OnClientPropertyChanged;
			//this.Loaded += (o, e) => _QueryProducts();
		}
		private async Task _QueryProducts()
		{
			var lProducts = await mAPIClient.GetProducts();
			mProductsList.ItemsSource = lProducts;
		}

		private async Task _QueryProductCategories()
		{
			var lProductCategories = await mAPIClient.GetProductCategories();
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
			this.IsAwaitingAPIClient = true;
			await mAPIClient.Login(new RegisterUserDTO() { UserName = mUserNameTxtBox.Text, Password = mPwdBox.Password });
			this.IsAwaitingAPIClient = false;
		}

		private async void mRegisterBtn_Click(object sender, RoutedEventArgs e)
		{
			if (String.IsNullOrEmpty(mUserNameTxtBox.Text) || string.IsNullOrEmpty(mPwdBox.Password))
				return;
			this.IsAwaitingAPIClient = true;
			await mAPIClient.RegisterUser(new RegisterUserDTO() { UserName = mUserNameTxtBox.Text, Password = mPwdBox.Password });
			this.IsAwaitingAPIClient = false;
		}

		private async void mLogoutBtn_Click(object sender, RoutedEventArgs e)
		{
			this.IsAwaitingAPIClient = true;
			await mAPIClient.Logout();
			this.IsAwaitingAPIClient = false;
			
		}

		private void mCreateProductBtn_Click(object sender, RoutedEventArgs e)
		{

		}

		private void mCreateProductCategoryBtn_Click(object sender, RoutedEventArgs e)
		{

		}

		private void _OnClientPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (this.CheckAccess())
			{
				switch (e.PropertyName)
				{
				case "LoggedInUser":
					if (mAPIClient.LoggedInUser != UserDTO.NO_USER)
					{
						this.IsConnected = true;
						this.User = mAPIClient.LoggedInUser;
					}
					else
					{
						this.IsConnected = false;
						this.User = UserDTO.NO_USER;
					}
					mProductsList.ItemsSource = null;
					break;
				}
			}
			else
			{
				this.Dispatcher.BeginInvoke(new Action<object, PropertyChangedEventArgs>((_sender, _e) => _OnClientPropertyChanged(_sender, _e)), sender, e);
			}
		}
	}
}
