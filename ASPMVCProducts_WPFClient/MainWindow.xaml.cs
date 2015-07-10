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

		public static readonly DependencyProperty UserProperty = DependencyProperty.Register("User", typeof(UserDTO), typeof(MainWindow), new PropertyMetadata(UserDTO.INVALID));
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
		private async Task _QueryProductLists()
		{
			this.IsAwaitingAPIClient = true;
			var lProducts = await mAPIClient.GetProductLists();
			mProductsList.ItemsSource = lProducts;
			this.IsAwaitingAPIClient = false;
		}

		private async Task _QueryProductEntries()
		{
			if (!(mProductsList.SelectedItem is  ProductListDTO))
				return;

			var lSelectedList = (ProductListDTO)mProductsList.SelectedItem;
			this.IsAwaitingAPIClient = true;
			var lProductEntries = await mAPIClient.GetProductEntries(lSelectedList);
			mProductEntriesList.ItemsSource = lProductEntries;
			this.IsAwaitingAPIClient = false;
		}

		private async void mRefreshProductListBtn_Click(object sender, RoutedEventArgs e)
		{
			await _QueryProductLists();
		}

		private async void mRefreshProductEntriesBtn_Click(object sender, RoutedEventArgs e)
		{
			await _QueryProductEntries();
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

		private async void mCreateProductListBtn_Click(object sender, RoutedEventArgs e)
		{
			if (String.IsNullOrEmpty(mProductListNameTxtBox.Text))
				return;

			this.IsAwaitingAPIClient = true;
			await mAPIClient.CreateProductList(new CreateProductListDTO() { Name = mProductListNameTxtBox.Text });
			await _QueryProductLists();
			this.IsAwaitingAPIClient = false;
		}

		private async void mBtnDeleteProductListBtn_Click(object sender, RoutedEventArgs e)
		{
			var lSender = sender as FrameworkElement;
			if (lSender == null || !(lSender.Tag is ProductListDTO ))
				return;

			var lProductList = (ProductListDTO)lSender.Tag;
			this.IsAwaitingAPIClient = true;
			await mAPIClient.DeleteProductList(lProductList);
			await _QueryProductLists();
			this.IsAwaitingAPIClient = false;
		}

		private async void mProductsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			this.IsAwaitingAPIClient = true;
			await _QueryProductEntries();
			this.IsAwaitingAPIClient = false;
		}

		private async void mCreateProductEntryBtn_Click(object sender, RoutedEventArgs e)
		{
			if (!(mProductsList.SelectedItem is ProductListDTO))
				return;

			if (String.IsNullOrEmpty(mProductEntryNameTxtBox.Text))
				return;

			var lSelectedList = (ProductListDTO)mProductsList.SelectedItem;
			this.IsAwaitingAPIClient = true;
			await mAPIClient.CreateProductEntry(lSelectedList, new CreateProductEntryDTO() { ProductName = mProductEntryNameTxtBox.Text });
			await _QueryProductEntries();
			this.IsAwaitingAPIClient = false;

		}

		private async void mBtnDeleteProductEntryBtn_Click(object sender, RoutedEventArgs e)
		{
			var lSender = sender as FrameworkElement;
			if (lSender == null || !(lSender.Tag is ProductEntryDTO))
				return;
			if (!(mProductsList.SelectedItem is ProductListDTO))
				return;

			var lSelectedList = (ProductListDTO)mProductsList.SelectedItem;
			var lProductEntry = (ProductEntryDTO)lSender.Tag;
			this.IsAwaitingAPIClient = true;
			await mAPIClient.DeleteProductEntry(lSelectedList, lProductEntry);
			await _QueryProductEntries();
			this.IsAwaitingAPIClient = false;
		}

		

		private void _OnClientPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (this.CheckAccess())
			{
				switch (e.PropertyName)
				{
				case "LoggedInUser":
					if (mAPIClient.LoggedInUser != UserDTO.INVALID)
					{
						this.IsConnected = true;
						this.User = mAPIClient.LoggedInUser;
					}
					else
					{
						this.IsConnected = false;
						this.User = UserDTO.INVALID;
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
