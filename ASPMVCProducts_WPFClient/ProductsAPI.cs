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
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }


    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class CreateUserDTO
    {
        public string UserName { get; set; }
        public string Password {get; set;}
    }
    public static class ProductsAPI
    {
        public static async Task<List<Product>> GetProducts()
        {
            List<Product> lProducts = await HttpJSONRequester.Get<List<Product>>("http://localhost:51902", "api/productsapi");
            return lProducts;
        }

        public static async Task<User> RegisterUser(CreateUserDTO aUser)
        {
            var lUser = await HttpJSONRequester.Post<CreateUserDTO, User>("http://localhost:51902", "api/accountapi", aUser);
            return lUser;
        }
    }
}
