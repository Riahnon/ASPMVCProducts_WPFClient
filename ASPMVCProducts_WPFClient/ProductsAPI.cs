using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;

namespace ASPMVCProducts_WPFClient
{
	public class ProductDTO
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
	}

	public class ProductCategory
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
	}


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
	public static class ProductsAPI
	{
		const string AUTH_TOKEN_HEADER = "Authorization-Token";
		static string mAuthToken; 
		public static async Task<List<ProductDTO>> GetProducts()
		{
			List<ProductDTO> lProducts = await HttpJSONRequester.Get<List<ProductDTO>>("http://localhost:51902", "api/productsapi");
			return lProducts;
		}

		public static async Task<List<ProductCategory>> GetProductCategories()
		{
			List<ProductCategory> lProductCategories = await HttpJSONRequester.Get<List<ProductCategory>>("http://localhost:51902", "api/productsapi");
			return lProductCategories;
		}

		public static async Task<UserDTO> RegisterUser(RegisterUserDTO aUser)
		{
			var lUser = await HttpJSONRequester.Post<RegisterUserDTO, UserDTO>("http://localhost:51902", "api/accountapi/register/", aUser);
			return lUser;
		}

		public static async Task<UserDTO> LoginUser(RegisterUserDTO aUser)
		{
			 var lReponse = await HttpJSONRequester.PostRawResponse<RegisterUserDTO>("http://localhost:51902", "api/accountapi/login/", aUser);
			 var lCookies = HttpJSONRequester.Cookies.GetCookies(new Uri("http://localhost:51902"));
			 var lAuthCookie = lCookies.Cast<Cookie>().FirstOrDefault(aCookie => aCookie != null && aCookie.Name == FormsAuthentication.FormsCookieName);
			 if (lAuthCookie != null)
			 {
				 HttpJSONRequester.RequestHeaders[lAuthCookie.Name] = lAuthCookie.Value;
			 }
			return null;
		}
	}
}
