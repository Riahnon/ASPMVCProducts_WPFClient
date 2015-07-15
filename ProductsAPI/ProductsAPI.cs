using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;

namespace ProductsAPI
{
	public class UserDTO
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}

	public class RegisterUserDTO
	{
		public string UserName { get; set; }
		public string Password { get; set; }
	}

	public class ProductListDTO
	{
		public int Id { get; set; }
		public string Name { get; set; }
		internal ObservableCollection<ProductEntryDTO> mProductEntries;
		public ProductListDTO()
		{
			mProductEntries = new ObservableCollection<ProductEntryDTO>();
			ProductEntries = new ReadOnlyObservableCollection<ProductEntryDTO>(mProductEntries);
		}
        public ReadOnlyObservableCollection<ProductEntryDTO> ProductEntries
        {
            get;
            private set;
        }
	}

	public class ProductEntryDTO : INotifyPropertyChanged
	{
		int mId;
		public int Id
		{
			get { return mId; }
			set
			{
				if (mId != value)
				{
					mId = value;
					_NotifyPropertyChanged("Id");
				}
			}
		}

		string mProductName;
		public string ProductName
		{
			get { return mProductName; }
			set
			{
				if (mProductName != value)
				{
					mProductName = value;
					_NotifyPropertyChanged("ProductName");
				}
			}
		}
		int mAmmount;
		public int Ammount
		{
			get { return mAmmount; }
			set
			{
				if (mAmmount != value)
				{
					mAmmount = value;
					_NotifyPropertyChanged("Ammount");
				}
			}
		}

		string mComments;
		public string Comments
		{
			get { return mComments; }
			set
			{
				if (mComments != value)
				{
					mComments = value;
					_NotifyPropertyChanged("Comments");
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void _NotifyPropertyChanged(string aPropertyName)
		{
			var lHandler = PropertyChanged;
			if (lHandler != null)
			{
				lHandler(this, new PropertyChangedEventArgs(aPropertyName));
			}
		}

		internal ProductListDTO OwnerList
		{
			get;
			set;
		}
	}

	public partial class ProductsAPIClient : INotifyPropertyChanged
	{
		const string WEB_API_PREFIX = "api";
		const string URL_SERVER = "http://localhost:51902";

		const string URL_ACCOUNTS = WEB_API_PREFIX + "/account";
		const string URL_REGISTER_USER = URL_ACCOUNTS + "/register";
		const string URL_LOGIN_USER = URL_ACCOUNTS + "/login";
		const string URL_LOGOUT_USER = URL_ACCOUNTS + "/logout";


		const string URL_PRODUCT_LISTS = WEB_API_PREFIX + "/productlists";
		const string URL_CREATE_PRODUCT_LIST = URL_PRODUCT_LISTS + "/create";
		const string URL_DELETE_PRODUCT_LIST = URL_PRODUCT_LISTS + "/delete/{0}";

		const string URL_PRODUCT_ENTRIES = WEB_API_PREFIX + "/productlists/{0}";
		const string URL_CREATE_PRODUCT_ENTRY = URL_PRODUCT_ENTRIES + "/create";
		const string URL_EDIT_PRODUCT_ENTRY = URL_PRODUCT_ENTRIES + "/edit/{1}";
		const string URL_DELETE_PRODUCT_ENTRY = URL_PRODUCT_ENTRIES + "/delete/{1}";

		const string URL_SIGNALR_HUB = URL_SERVER + "/signalr";

		private IHubProxy mHubProxy;
		private HubConnection mHubConnection;

		HttpJSONRequester mJSONRequester;
		private Dictionary<string, string> mRequestHeaders = new Dictionary<string, string>();

		public ProductsAPIClient()
		{
			mJSONRequester = new HttpJSONRequester();
			mProductLists = new ObservableCollection<ProductListDTO>();
			this.ProductLists = new ReadOnlyObservableCollection<ProductListDTO>(mProductLists);
		}
		public UserDTO LoggedInUser
		{
			get;
			private set;
		}

		ObservableCollection<ProductListDTO> mProductLists;
		public ReadOnlyObservableCollection<ProductListDTO> ProductLists
		{
			get;
			private set;
		}

		bool mIsBussy;
		public bool IsBussy
		{
			get { return mIsBussy; }
			set
			{
				if (mIsBussy != value)
				{
					mIsBussy = value;
					_NotifyPropertyChanged("IsBussy");
				}
			}
		}
		public event PropertyChangedEventHandler PropertyChanged;

		public async Task<bool> RegisterUser(RegisterUserDTO aUser)
		{
            return await _LoginOrRegister(aUser, URL_REGISTER_USER);
		}

        public async Task<bool> Login(RegisterUserDTO aUser)
        {
            return await _LoginOrRegister(aUser, URL_LOGIN_USER);
        }

		public async Task Logout()
		{
            if (this.LoggedInUser != null)
            {
                IsBussy = true;
                await mJSONRequester.Post(URL_SERVER, URL_LOGOUT_USER, mRequestHeaders);
                IsBussy = false;
                mRequestHeaders.Remove(FormsAuthentication.FormsCookieName);
                LoggedInUser = null;
                _NotifyPropertyChanged("LoggedInUser");
                lock (this.ProductLists)
                    mProductLists.Clear();

                if (mHubConnection != null)
                {
                    mHubConnection.Closed -= _OnSignalRConnectionClosed;
                    mHubConnection.Stop();
                }
            }
		}

		public async Task QueryProductLists()
		{
			IsBussy = true;
			List<ProductListDTO> lProductLists = await mJSONRequester.Get<List<ProductListDTO>>(URL_SERVER, URL_PRODUCT_LISTS, mRequestHeaders);
			IsBussy = false;
            lock (this.ProductLists)
            {
                mProductLists.Clear();
                foreach (var lProductList in lProductLists)
                    mProductLists.Add(lProductList);
            }
			foreach (var lList in this.ProductLists)
			{
				await QueryProductEntries(lList);
			}
		}

		public async Task CreateProductList(ProductListDTO aProductList)
		{
			IsBussy = true;
			await mJSONRequester.Post<ProductListDTO, ProductListDTO>(URL_SERVER, URL_CREATE_PRODUCT_LIST, aProductList, mRequestHeaders);
			IsBussy = false;
		}

		public async Task DeleteProductList(ProductListDTO aProductList)
		{
			IsBussy = true;
			await mJSONRequester.Delete(URL_SERVER, string.Format(URL_DELETE_PRODUCT_LIST, aProductList.Id), mRequestHeaders);
			IsBussy = false;
		}

		public async Task QueryProductEntries(ProductListDTO aList)
		{
			IsBussy = true;
			List<ProductEntryDTO> lProductEntries = await mJSONRequester.Get<List<ProductEntryDTO>>(URL_SERVER, string.Format(URL_PRODUCT_ENTRIES, aList.Id), mRequestHeaders);
			IsBussy = false;
			//Unsuscribe from previous event listeners
			foreach (var lProductEntry in aList.ProductEntries)
			{
				lProductEntry.PropertyChanged -= _OnProductEntryPropertyChanged;
				lProductEntry.OwnerList = null;
			}
			//Suscribe to new product entries
			foreach (var lProductEntry in lProductEntries)
			{
				lProductEntry.PropertyChanged += _OnProductEntryPropertyChanged;
				lProductEntry.OwnerList = aList;
			}

            lock( aList.ProductEntries )
            {
                aList.mProductEntries.Clear();
                foreach( var lProductEntry in lProductEntries )
                    aList.mProductEntries.Add( lProductEntry );
            }

		}

		public async Task CreateProductEntry(ProductListDTO aList, ProductEntryDTO aProductEntry)
		{
			IsBussy = true;
			await mJSONRequester.Post<ProductEntryDTO, ProductEntryDTO>(URL_SERVER, string.Format(URL_CREATE_PRODUCT_ENTRY, aList.Id), aProductEntry, mRequestHeaders);
			IsBussy = false;
		}

		public async Task DeleteProductEntry(ProductListDTO aList, ProductEntryDTO aProductEntry)
		{
			IsBussy = true;
			var lResponse = await mJSONRequester.Delete(URL_SERVER, string.Format(URL_DELETE_PRODUCT_ENTRY, aList.Id, aProductEntry.Id), mRequestHeaders);
			IsBussy = false;
		}

        private async void _OnProductEntryPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var lProductEntry = (ProductEntryDTO)sender;
			switch (e.PropertyName)
			{
			case "Ammount":
			case "Comments":
				await mJSONRequester.Put<ProductEntryDTO, ProductEntryDTO>(URL_SERVER, string.Format(URL_EDIT_PRODUCT_ENTRY, lProductEntry.OwnerList.Id, lProductEntry.Id), lProductEntry, mRequestHeaders);
				break;
			}
		}

		private void _NotifyPropertyChanged(string aPropertyName)
		{
			var lHandler = PropertyChanged;
			if (lHandler != null)
			{
				lHandler(this, new PropertyChangedEventArgs(aPropertyName));
			}
		}

        private async Task<bool> _LoginOrRegister(RegisterUserDTO aUser, string aURL)
        {
            IsBussy = true;
            var lResponse = await mJSONRequester.Post<RegisterUserDTO>(URL_SERVER, URL_LOGIN_USER, aUser, mRequestHeaders);
            IsBussy = false;
            if (lResponse.IsSuccessStatusCode)
            {
                var lUser = await lResponse.Content.ReadAsAsync<UserDTO>();
                var lCookies = mJSONRequester.Cookies.GetCookies(new Uri(URL_SERVER));
                var lAuthCookie = lCookies.Cast<Cookie>().FirstOrDefault(aCookie => aCookie != null && aCookie.Name == FormsAuthentication.FormsCookieName);
                if (lAuthCookie != null)
                {
                    this.mHubConnection = new HubConnection(URL_SIGNALR_HUB);
                    this.mHubConnection.CookieContainer = new CookieContainer();
                    this.mHubConnection.CookieContainer.Add(lAuthCookie);
                    mHubConnection.Closed += _OnSignalRConnectionClosed;
                    mHubProxy = mHubConnection.CreateHubProxy("ProductsHub");
                    mHubProxy.On<string, object>("OnServerEvent", _OnSignalREvent);
                    try
                    {
                        await mHubConnection.Start();
                        mRequestHeaders[lAuthCookie.Name] = lAuthCookie.Value;
                        LoggedInUser = lUser;
                        _NotifyPropertyChanged("LoggedInUser");
                        return true;
                    }
                    catch (HttpRequestException)
                    {
                        //False will be returned
                    }
                }
            }
            return false;
        }

		private void _OnSignalREvent(string aEventName, object aEventData)
		{
			switch (aEventName)
			{
			case "ProductListCreated":
				{
                    var lEventData = (JObject)aEventData;
					var lList = new ProductListDTO()
					{
						Id = (int)lEventData["Id"],
						Name = (string)lEventData["Name"],
					};
                    lock (this.ProductLists)
					    mProductLists.Add(lList);
				}
				break;
			case "ProductListDeleted":
				{
					var lEventData = (JObject)aEventData;
					var lList = mProductLists.FirstOrDefault ( aList => aList.Id == (int)lEventData["Id"] );
                    if (lList != null)
                    {
                        lock (this.ProductLists)
                            mProductLists.Remove(lList);
                    }
				}
				break;
			case "ProductListEntryCreated":
				{
					var lEventData = (JObject)aEventData;
					var lList = mProductLists.FirstOrDefault(aList => aList.Id == (int)lEventData["ListId"]);
					if (lList != null)
					{
						var lEntry = new ProductEntryDTO()
						{
							Id = (int)lEventData["Id"],
							ProductName = (string)lEventData["Name"],
							Ammount = (int)lEventData["Ammount"],
							Comments = (string)lEventData["Comments"],
                            OwnerList = lList
						};
                        lock(lList.ProductEntries)
						    lList.mProductEntries.Add(lEntry);
					}
				}
				break;
			case "ProductListEntryEdited":
				{
					var lEventData = (JObject)aEventData;
					var lList = mProductLists.FirstOrDefault(aList => aList.Id == (int)lEventData["ListId"]);
					if (lList != null)
					{
						var lEntry = lList.ProductEntries.FirstOrDefault(aEntry => aEntry.Id == (int)lEventData["Id"]);
						if (lEntry != null)
						{
							lEntry.Ammount = (int)lEventData["Ammount"];
							lEntry.Comments = (string)lEventData["Comments"];
						}
					}
				}
				break;
			case "ProductListEntryDeleted":
				{
					var lEventData = (JObject)aEventData;
					var lList = mProductLists.FirstOrDefault(aList => aList.Id == (int)lEventData["ListId"]);
					if (lList != null)
					{
						var lEntry = lList.ProductEntries.FirstOrDefault(aEntry => aEntry.Id == (int)lEventData["Id"]);
						if (lEntry != null)
						{
                            lock (lList.ProductEntries)
							    lList.mProductEntries.Remove(lEntry);
						}
					}
				}
				break;
			}
		}

		private async void _OnSignalRConnectionClosed()
		{
            await this.Logout();
		}
	}
}
