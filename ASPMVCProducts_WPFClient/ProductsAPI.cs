using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;

namespace ASPMVCProducts_WPFClient
{
	public struct UserDTO
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public static UserDTO INVALID = new UserDTO() { Id = -1, Name = string.Empty };
		public static bool operator ==(UserDTO aLhs, UserDTO aRhs)
		{
			return aLhs.Id == aRhs.Id;
		}
		public static bool operator !=(UserDTO aLhs, UserDTO aRhs)
		{
			return !(aLhs.Id == aRhs.Id);
		}
		public override int GetHashCode()
		{
			return Id;
		}
		public override string ToString()
		{
			return Name;
		}
		public override bool Equals(object obj)
		{
			if (obj is UserDTO)
				return this == (UserDTO)obj;

			return base.Equals(obj);
		}

	}

	public struct RegisterUserDTO
	{
		public string UserName { get; set; }
		public string Password { get; set; }
	}

	public struct ProductListDTO
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public static ProductListDTO INVALID = new ProductListDTO() { Id = -1, Name = string.Empty };
		public static bool operator ==(ProductListDTO aLhs, ProductListDTO aRhs)
		{
			return aLhs.Id == aRhs.Id;
		}
		public static bool operator !=(ProductListDTO aLhs, ProductListDTO aRhs)
		{
			return !(aLhs.Id == aRhs.Id);
		}
		public override int GetHashCode()
		{
			return Id;
		}
		public override string ToString()
		{
			return Name;
		}
		public override bool Equals(object obj)
		{
			if (obj is ProductListDTO)
				return this == (ProductListDTO)obj;

			return base.Equals(obj);
		}
	}

	public class CreateProductListDTO
	{
		public string Name { get; set; }

	}

	public class ProductEntryDTO
	{
		public int Id { get; set; }
		public string ProductName { get; set; }
		public bool Checked { get; set; }
		public int Ammount { get; set; }
		public string Comments { get; set; }
		public static ProductEntryDTO INVALID = new ProductEntryDTO() { Id = -1, ProductName = string.Empty };
		public static bool operator ==(ProductEntryDTO aLhs, ProductEntryDTO aRhs)
		{
			return aLhs.Id == aRhs.Id;
		}
		public static bool operator !=(ProductEntryDTO aLhs, ProductEntryDTO aRhs)
		{
			return !(aLhs.Id == aRhs.Id);
		}
		public override int GetHashCode()
		{
			return Id;
		}
		public override string ToString()
		{
			return ProductName;
		}
		public override bool Equals(object obj)
		{
			if (obj is ProductEntryDTO)
				return this == (ProductEntryDTO)obj;

			return base.Equals(obj);
		}
	}

	public class CreateProductEntryDTO
	{
		public string ProductName { get; set; }
	}

	



	public class ProductsAPIClient : INotifyPropertyChanged
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
		const string URL_DELETE_PRODUCT_ENTRY = URL_PRODUCT_ENTRIES + "/delete/{1}";

		HttpJSONRequester mJSONRequester;
		public ProductsAPIClient()
		{
			mJSONRequester = new HttpJSONRequester();
		}
		UserDTO mLoggedInUser = UserDTO.INVALID;
		public UserDTO LoggedInUser
		{
			get { return mLoggedInUser; }
			private set
			{
				if (mLoggedInUser.Id != value.Id)
				{
					mLoggedInUser = value;
					_NotifyPropertyChanged("LoggedInUser");
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private Dictionary<string, string> mRequestHeaders = new Dictionary<string, string>();

		public async Task<bool> RegisterUser(RegisterUserDTO aUser)
		{
			var lResponse = await mJSONRequester.Post<RegisterUserDTO>(URL_SERVER, URL_REGISTER_USER, aUser, mRequestHeaders);
			if (lResponse.IsSuccessStatusCode)
			{
				var lUser = await lResponse.Content.ReadAsAsync<UserDTO>();
				var lCookies = mJSONRequester.Cookies.GetCookies(new Uri(URL_SERVER));
				var lAuthCookie = lCookies.Cast<Cookie>().FirstOrDefault(aCookie => aCookie != null && aCookie.Name == FormsAuthentication.FormsCookieName);
				if (lAuthCookie != null)
				{
					mRequestHeaders[lAuthCookie.Name] = lAuthCookie.Value;
					LoggedInUser = lUser;
					return true;
				}
			}
			return false;
		}

		public async Task<bool> Login(RegisterUserDTO aUser)
		{
			var lResponse = await mJSONRequester.Post<RegisterUserDTO>(URL_SERVER, URL_LOGIN_USER, aUser, mRequestHeaders);
			if (lResponse.IsSuccessStatusCode)
			{
				var lUser = await lResponse.Content.ReadAsAsync<UserDTO>();
				var lCookies = mJSONRequester.Cookies.GetCookies(new Uri(URL_SERVER));
				var lAuthCookie = lCookies.Cast<Cookie>().FirstOrDefault(aCookie => aCookie != null && aCookie.Name == FormsAuthentication.FormsCookieName);
				if (lAuthCookie != null)
				{
					mRequestHeaders[lAuthCookie.Name] = lAuthCookie.Value;
					LoggedInUser = lUser;
					return true;
				}
			}
			return false;
		}

		public async Task Logout()
		{
			var lResponse = await mJSONRequester.Post(URL_SERVER, URL_LOGOUT_USER, mRequestHeaders);
			if (lResponse.IsSuccessStatusCode)
			{
				mRequestHeaders.Remove(FormsAuthentication.FormsCookieName);
				LoggedInUser = UserDTO.INVALID;
			}
		}

		public async Task<List<ProductListDTO>> GetProductLists()
		{
			List<ProductListDTO> lProducts = await mJSONRequester.Get<List<ProductListDTO>>(URL_SERVER, URL_PRODUCT_LISTS, mRequestHeaders);
			return lProducts;
		}

		public async Task<ProductListDTO> CreateProductList(CreateProductListDTO aProductList)
		{
			var lResponse = await mJSONRequester.Post<CreateProductListDTO>(URL_SERVER, URL_CREATE_PRODUCT_LIST, aProductList, mRequestHeaders);
			if (lResponse.IsSuccessStatusCode)
			{
				return await lResponse.Content.ReadAsAsync<ProductListDTO>();

			}
			return ProductListDTO.INVALID;
		}

		public async Task<bool> DeleteProductList(ProductListDTO aProductList)
		{
			var lResponse = await mJSONRequester.Delete(URL_SERVER, string.Format(URL_DELETE_PRODUCT_LIST, aProductList.Id), mRequestHeaders);
			return lResponse.IsSuccessStatusCode;
		}

		public async Task<List<ProductEntryDTO>> GetProductEntries(ProductListDTO aList)
		{
			List<ProductEntryDTO> lProducts = await mJSONRequester.Get<List<ProductEntryDTO>>(URL_SERVER, string.Format(URL_PRODUCT_ENTRIES, aList.Id), mRequestHeaders);
			return lProducts;
		}

		public async Task<ProductEntryDTO> CreateProductEntry(ProductListDTO aList, CreateProductEntryDTO aProductEntry)
		{
			var lResponse = await mJSONRequester.Post<CreateProductEntryDTO>(URL_SERVER, string.Format(URL_CREATE_PRODUCT_ENTRY, aList.Id), aProductEntry, mRequestHeaders);
			if (lResponse.IsSuccessStatusCode)
			{
				return await lResponse.Content.ReadAsAsync<ProductEntryDTO>();

			}
			return ProductEntryDTO.INVALID;
		}

		public async Task<bool> DeleteProductEntry(ProductListDTO aList, ProductEntryDTO aProductEntry)
		{
			var lResponse = await mJSONRequester.Delete(URL_SERVER, string.Format(URL_DELETE_PRODUCT_ENTRY, aList.Id, aProductEntry.Id), mRequestHeaders);
			return lResponse.IsSuccessStatusCode;
		}

		

		private void _NotifyPropertyChanged(string aPropertyName)
		{
			var lHandler = PropertyChanged;
			if (lHandler != null)
			{
				lHandler(this, new PropertyChangedEventArgs(aPropertyName));
			}
		}

	}
}
