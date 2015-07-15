using ProductsAPI;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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

        bool mLoggingOut; //Flag to know if user logout is accidental or voluntary
        Point mProductListMouseDown; //Position to track where mouse down ocurred when clicking delete button of list items
		public MainWindow()
		{
			InitializeComponent();
			this.APIClient = new ProductsAPIClient();
            this.APIClient.PropertyChanged += _OnAPIClientPropertyChanged;
            BindingOperations.EnableCollectionSynchronization(this.APIClient.ProductLists, this.APIClient.ProductLists);
            var lProductLists = (INotifyCollectionChanged)this.APIClient.ProductLists;
            lProductLists.CollectionChanged += _OnProductListsChanged;
		}
		private async Task _QueryProductLists()
		{
            await this.APIClient.QueryProductLists();
		}

		private async Task _QueryProductEntries()
		{
            if (!(mProductListsItemsControl.SelectedItem is ProductListDTO))
                return;

            var lSelectedList = (ProductListDTO)mProductListsItemsControl.SelectedItem;
            await APIClient.QueryProductEntries(lSelectedList);
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
            if (await APIClient.Login(new RegisterUserDTO() { UserName = mUserNameTxtBox.Text, Password = mPwdBox.Password }))
            {
                await APIClient.QueryProductLists();
            }
            else
            {
                
            }
        }

        private async Task _Logout()
        {
            mLoggingOut = true;
            await APIClient.Logout();
            mLoggingOut = false;
        }

        private async Task _Register()
        {
            if (String.IsNullOrEmpty(mUserNameTxtBox.Text) || string.IsNullOrEmpty(mPwdBox.Password))
                return;
            if( await APIClient.RegisterUser(new RegisterUserDTO() { UserName = mUserNameTxtBox.Text, Password = mPwdBox.Password }) )
                await APIClient.QueryProductLists();
        }

        private async Task _AddProductList()
        {
            if (String.IsNullOrEmpty(mProductListNameTxtBox.Text))
                return;

            await APIClient.CreateProductList(new ProductListDTO() { Name = mProductListNameTxtBox.Text });
        }

        private async Task _AddProductEntry()
        {
            if (!(mProductListsItemsControl.SelectedItem is ProductListDTO))
                return;

            if (String.IsNullOrEmpty(mProductEntryNameTxtBox.Text))
                return;

            var lSelectedList = (ProductListDTO)mProductListsItemsControl.SelectedItem;
            await APIClient.CreateProductEntry(lSelectedList, new ProductEntryDTO() { ProductName = mProductEntryNameTxtBox.Text });
        }

        private async Task _DeleteProductList(ProductListDTO aProductList)
        {
            await APIClient.DeleteProductList(aProductList);
        }

        private async Task _DeleteProductEntry(ProductListDTO aProductList, ProductEntryDTO aProductEntry)
        {
            await APIClient.DeleteProductEntry(aProductList, aProductEntry);
        }

        private void _OnProductListsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var lItem in e.NewItems.Cast<ProductListDTO>())
                {
                    BindingOperations.EnableCollectionSynchronization(lItem.ProductEntries, lItem.ProductEntries);
                }
            }
            if (e.OldItems != null)
            {
                foreach (var lItem in e.OldItems.Cast<ProductListDTO>())
                {
                    BindingOperations.DisableCollectionSynchronization(lItem.ProductEntries);
                }
            }
        }
        

        private void _OnAPIClientPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //Connection is lost
            if (e.PropertyName == "LoggedInUser" && this.APIClient.LoggedInUser == null && !mLoggingOut)
            {
                MessageBox.Show("Connection with API server was lost. Try reconnecting", "Connection lost", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

		
	}
}

