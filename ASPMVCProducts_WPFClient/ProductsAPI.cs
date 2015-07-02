using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

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
			 if (lReponse.Headers.Contains(AUTH_TOKEN_HEADER))
			 {
				 var lToken = lReponse.Headers.GetValues(AUTH_TOKEN_HEADER).FirstOrDefault();
				 HttpJSONRequester.RequestHeaders[AUTH_TOKEN_HEADER] = lToken;
				 var lUser =  await lReponse.Content.ReadAsAsync<UserDTO>();
			 }
			return null;
		}
	}
}
