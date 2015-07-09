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
	public struct ProductListDTO
	{
		public int Id { get; set; }
		public string Name { get; set; }
        public static ProductListDTO NO_LIST = new ProductListDTO() { Id = -1, Name = string.Empty };
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

	public struct ProductCategoryDTO
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
	}


	public struct UserDTO
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public static UserDTO NO_USER = new UserDTO() { Id = -1, Name = string.Empty };
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

	public class ProductsAPIClient : INotifyPropertyChanged
	{
		HttpJSONRequester mJSONRequester;
		public ProductsAPIClient()
		{
			mJSONRequester = new HttpJSONRequester();
		}
		UserDTO mLoggedInUser = UserDTO.NO_USER;
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

		private Dictionary<string, string> mRequestHeaders = new Dictionary<string,string>();

		public async Task<List<ProductListDTO>> GetProductLists()
		{
			List<ProductListDTO> lProducts = await mJSONRequester.Get<List<ProductListDTO>>("http://localhost:51902", "api/productlistsapi");
			return lProducts;
		}

        public async Task<ProductListDTO> CreateProductList(CreateProductListDTO aProductList)
        {
            var lResponse = await mJSONRequester.Post<CreateProductListDTO>("http://localhost:51902", "api/productlistsapi/create", aProductList, mRequestHeaders);
            if (lResponse.IsSuccessStatusCode)
            {
                return await lResponse.Content.ReadAsAsync<ProductListDTO>();
                
            }
            return ProductListDTO.NO_LIST;
        }

		public async Task<List<ProductCategoryDTO>> GetProductCategories()
		{
			List<ProductCategoryDTO> lProductCategories = await mJSONRequester.Get<List<ProductCategoryDTO>>("http://localhost:51902", "api/productsapi", mRequestHeaders);
			return lProductCategories;
		}

		public async Task<bool> RegisterUser(RegisterUserDTO aUser)
		{
			var lResponse = await mJSONRequester.Post<RegisterUserDTO>("http://localhost:51902", "api/accountapi/register/", aUser, mRequestHeaders);
			if (lResponse.IsSuccessStatusCode)
			{
				var lUser = await lResponse.Content.ReadAsAsync<UserDTO>();
				var lCookies = mJSONRequester.Cookies.GetCookies(new Uri("http://localhost:51902"));
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
			var lResponse = await mJSONRequester.Post<RegisterUserDTO>("http://localhost:51902", "api/accountapi/login/", aUser, mRequestHeaders);
			if (lResponse.IsSuccessStatusCode)
			{
				var lUser = await lResponse.Content.ReadAsAsync<UserDTO>();
				var lCookies = mJSONRequester.Cookies.GetCookies(new Uri("http://localhost:51902"));
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

		public async Task Logout( )
		{
			var lResponse = await mJSONRequester.Post("http://localhost:51902", "api/accountapi/logout/", mRequestHeaders);
			if (lResponse.IsSuccessStatusCode)
			{
				mRequestHeaders.Remove(FormsAuthentication.FormsCookieName);
				LoggedInUser = UserDTO.NO_USER;
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
		
	}
}
