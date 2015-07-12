using ProductsAPI;
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
        private static readonly DependencyProperty APIClientProperty = DependencyProperty.Register("APIClient", typeof(ProductsAPIClient), typeof(MainWindow), new PropertyMetadata(null));

        private ProductsAPIClient APIClient
        {
            get { return (ProductsAPIClient)GetValue(APIClientProperty); }
            set { SetValue(APIClientProperty, value); }
        }

		public static readonly DependencyProperty IsAwaitingAPIClientProperty = DependencyProperty.Register("IsAwaitingAPIClient", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));
		public bool IsAwaitingAPIClient
		{
			get { return (bool)this.GetValue(IsAwaitingAPIClientProperty); }
			set { this.SetValue(IsAwaitingAPIClientProperty, value); }
		}


		public MainWindow()
		{
			InitializeComponent();
			this.APIClient = new ProductsAPIClient();
            this.APIClient.PropertyChanged += _OnClientPropertyChanged;
            this.Loaded += (o, e) =>
            {
                //_Login();
                //_QueryProductLists();
            };
		}
		private async Task _QueryProductLists()
		{
			this.IsAwaitingAPIClient = true;
            await this.APIClient.QueryProductLists();
			
			this.IsAwaitingAPIClient = false;
		}

		private async Task _QueryProductEntries()
		{
            if (!(mProductListsItemsControl.SelectedItem is ProductListDTO))
                return;

            var lSelectedList = (ProductListDTO)mProductListsItemsControl.SelectedItem;
            this.IsAwaitingAPIClient = true;
            await APIClient.QueryProductEntries(lSelectedList);
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
            await _Login();
		}

        private async void mRegisterBtn_Click(object sender, RoutedEventArgs e)
        {
            await _Register();
        }

        private async void mLogoutBtn_Click(object sender, RoutedEventArgs e)
        {
            await _Logout();

        }

        private async void mAddProductListBtn_Click(object sender, RoutedEventArgs e)
        {
            await _AddProductList();
        }

        private async void mDeleteProductListBtn_Click(object sender, RoutedEventArgs e)
        {
            var lSender = sender as FrameworkElement;
            if (lSender != null && lSender.DataContext is ProductListDTO)
                await _DeleteProductList((ProductListDTO)lSender.DataContext);
        }

        private async void mAddProductEntryBtn_Click(object sender, RoutedEventArgs e)
        {
            await _AddProductEntry();
        }

        private async void mDeleteProductEntry_Click(object sender, RoutedEventArgs e)
        {
            var lSender = sender as FrameworkElement;
            if (lSender == null || !(lSender.DataContext is ProductEntryDTO))
                return;
            if (!(mProductListsItemsControl.SelectedItem is ProductListDTO))
                return;

            await _DeleteProductEntry ( (ProductListDTO)mProductListsItemsControl.SelectedItem, (ProductEntryDTO)lSender.DataContext );
        }

        private async void mProductEntryNameTxtBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                await _AddProductEntry();
            }
        }

        private async void mProductListNameTxtBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                await _AddProductList();

            }
        }

        private void ProductList_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var lSender = sender as IInputElement;
            if (lSender != null)
                mProductListMouseDown = e.GetPosition(lSender);
        }

        private void ProductList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var lDownPos = mProductListMouseDown;
            mProductListMouseDown = new Point();
            var lSender = sender as FrameworkElement;
            if (lSender == null || !(lSender.DataContext is ProductListViewModel))
                return;

            var lUpPos = e.GetPosition(lSender);
            if (Math.Abs(lUpPos.X - lDownPos.X) < 3 && Math.Abs(lUpPos.Y - lDownPos.Y) < 3)
            {
                var lProductList = (ProductListViewModel)lSender.DataContext;
                if (!lProductList.IsSelected)
                {
                    foreach (ProductListViewModel lList in mProductListsItemsControl.Items)
                        lList.IsSelected = false;

                    lProductList.IsSelected = true;
                }
            }
        }

        private void ProductListDelete_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var lSender = sender as IInputElement;
            if (lSender != null)
                mProductListMouseDown = e.GetPosition(lSender);
        }

        private async void ProductListDelete_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var lDownPos = mProductListMouseDown;
            mProductListMouseDown = new Point();
            var lSender = sender as FrameworkElement;
            if (lSender == null || !(lSender.DataContext is ProductListDTO))
                return;

            var lUpPos = e.GetPosition(lSender);
            if (Math.Abs(lUpPos.X - lDownPos.X) < 3 && Math.Abs(lUpPos.Y - lDownPos.Y) < 3)
            {
                var lProductList = (ProductListDTO)lSender.DataContext;
                await _DeleteProductList(lProductList);
            }
        }

        private void ProductEntryDelete_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var lSender = sender as IInputElement;
            if (lSender != null)
                mProductListMouseDown = e.GetPosition(lSender);
        }

        private async void ProductEntryDelete_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var lDownPos = mProductListMouseDown;
            mProductListMouseDown = new Point();
            var lSender = sender as FrameworkElement;
            if (lSender == null || !(lSender.DataContext is ProductEntryDTO))
                return;

            if (!(mProductListsItemsControl.SelectedItem is ProductListDTO))
                return;

            var lUpPos = e.GetPosition(lSender);
            if (Math.Abs(lUpPos.X - lDownPos.X) < 3 && Math.Abs(lUpPos.Y - lDownPos.Y) < 3)
            {
                await _DeleteProductEntry((ProductListDTO)mProductListsItemsControl.SelectedItem, (ProductEntryDTO)lSender.DataContext);
            }
        }

        private async Task _Login()
        {
            if (String.IsNullOrEmpty(mUserNameTxtBox.Text) || string.IsNullOrEmpty(mPwdBox.Password))
                return;
            this.IsAwaitingAPIClient = true;
            if( await APIClient.Login(new RegisterUserDTO() { UserName = mUserNameTxtBox.Text, Password = mPwdBox.Password }) )
                await APIClient.QueryProductLists();
            this.IsAwaitingAPIClient = false;
        }

        private async Task _Logout()
        {
            this.IsAwaitingAPIClient = true;
            await APIClient.Logout();
            this.IsAwaitingAPIClient = false;
        }

        private async Task _Register()
        {
            if (String.IsNullOrEmpty(mUserNameTxtBox.Text) || string.IsNullOrEmpty(mPwdBox.Password))
                return;
            this.IsAwaitingAPIClient = true;
            if( await APIClient.RegisterUser(new RegisterUserDTO() { UserName = mUserNameTxtBox.Text, Password = mPwdBox.Password }) )
                await APIClient.QueryProductLists();
            this.IsAwaitingAPIClient = false;
        }

        private async Task _AddProductList()
        {
            if (String.IsNullOrEmpty(mProductListNameTxtBox.Text))
                return;

            this.IsAwaitingAPIClient = true;
            await APIClient.CreateProductList(new ProductListDTO() { Name = mProductListNameTxtBox.Text });
            this.IsAwaitingAPIClient = false;
        }

        private async Task _AddProductEntry()
        {
            if (!(mProductListsItemsControl.SelectedItem is ProductListDTO))
                return;

            if (String.IsNullOrEmpty(mProductEntryNameTxtBox.Text))
                return;

            var lSelectedList = (ProductListDTO)mProductListsItemsControl.SelectedItem;
            this.IsAwaitingAPIClient = true;
            await APIClient.CreateProductEntry(lSelectedList, new ProductEntryDTO() { ProductName = mProductEntryNameTxtBox.Text });
            this.IsAwaitingAPIClient = false;
        }

        private async Task _DeleteProductList(ProductListDTO aProductList)
        {
            this.IsAwaitingAPIClient = true;
            await APIClient.DeleteProductList(aProductList);
            this.IsAwaitingAPIClient = false;
        }

        private async Task _DeleteProductEntry(ProductListDTO aProductList, ProductEntryDTO aProductEntry)
        {
            this.IsAwaitingAPIClient = true;
            await APIClient.DeleteProductEntry(aProductList, aProductEntry);
            this.IsAwaitingAPIClient = false;
        }

		private void _OnClientPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (this.CheckAccess())
			{
				switch (e.PropertyName)
				{
				
				}
			}
			else
			{
				this.Dispatcher.BeginInvoke(new Action<object, PropertyChangedEventArgs>((_sender, _e) => _OnClientPropertyChanged(_sender, _e)), sender, e);
			}
		}

        Point mProductListMouseDown;
        

		
	}
}
