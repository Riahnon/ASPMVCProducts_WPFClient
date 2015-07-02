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
    public class HttpJSONRequester
    {
        public static async Task<TResponse> Get<TResponse>(string aBaseURL, string aRequestURL)
        {
            using (var lClient = new HttpClient())
            {
                lClient.BaseAddress = new Uri(aBaseURL);
				lClient.DefaultRequestHeaders.Accept.Clear();
				lClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage lResponse = await lClient.GetAsync(aRequestURL);
                if (lResponse.IsSuccessStatusCode)
                {
									var lResult = await lResponse.Content.ReadAsAsync<TResponse>();
                    var lStr = await lResponse.Content.ReadAsAsync<string>();
                    return JsonConvert.DeserializeObject<TResponse>(lStr);
                }
                return default(TResponse);
            }

        }

        public static async Task<TResponse> Post<TRequest, TResponse>(string aBaseURL, string aRequestURL, TRequest aData)
        {
            using (var lClient = new HttpClient())
            {
                lClient.BaseAddress = new Uri(aBaseURL);
                lClient.DefaultRequestHeaders.Accept.Clear();
                lClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string lPostBody = JsonConvert.SerializeObject(aData);
                var lContent = new StringContent(lPostBody, Encoding.UTF8, "application/json");
                HttpResponseMessage lResponse = await lClient.PostAsync(aRequestURL, lContent);
                if (lResponse.IsSuccessStatusCode)
                {
                    return await lResponse.Content.ReadAsAsync<TResponse>();
                }
                return default(TResponse);
            }

        }
        /*
        private static async Task<TResponse> _ParseResponse<TResponse>(HttpResponseMessage aResponse)
        {
            Exception lException = null;
            var lStr = await aResponse.Content.ReadAsAsync<string>();
            return JsonConvert.DeserializeObject<TResponse>(lStr);
            if (typeof(TResponse) != typeof(string))
            {
                try
                {
                    return await aResponse.Content.ReadAsAsync<TResponse>();
                }
                catch (Exception ex)
                {
                    lException = ex;
                }
            }

            try
            {
                var lStr = await aResponse.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<TResponse>(lStr);
            }
            catch (Exception ex)
            {
                if (lException != null)
                    throw lException;
                else
                    throw ex;
            }
        }*/
    }
}
