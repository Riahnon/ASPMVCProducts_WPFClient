using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ProductsAPI
{
    public class HttpJSONRequester
    {
        public CookieContainer Cookies { get; private set; }
        HttpClientHandler mClientHandler;
        public HttpJSONRequester()
        {
            Cookies = new CookieContainer();
            mClientHandler = new HttpClientHandler()
            {
                CookieContainer = Cookies,
                UseCookies = true,
                UseDefaultCredentials = false
            };
        }


        public async Task<TResponse> Get<TResponse>(string aBaseURL, string aRequestURL, IEnumerable<KeyValuePair<string, string>> aRequestHeaders = null)
        {
            using (var lClient = new HttpClient(mClientHandler, false))
            {
                _InitClient(lClient, aBaseURL, aRequestHeaders);
                HttpResponseMessage lResponse = await lClient.GetAsync(aRequestURL);
                if (lResponse.IsSuccessStatusCode)
                {
                    return await lResponse.Content.ReadAsAsync<TResponse>();
                }
                return default(TResponse);
            }

        }

        public async Task<TResponse> Post<TRequest, TResponse>(string aBaseURL, string aRequestURL, TRequest aData, IEnumerable<KeyValuePair<string, string>> aRequestHeaders = null)
        {
            var lResponse = await Post<TRequest>(aBaseURL, aRequestURL, aData, aRequestHeaders);
            if (lResponse.IsSuccessStatusCode)
            {
                return await lResponse.Content.ReadAsAsync<TResponse>();
            }
            return default(TResponse);
        }

        public async Task<HttpResponseMessage> Post<TRequest>(string aBaseURL, string aRequestURL, TRequest aData, IEnumerable<KeyValuePair<string, string>> aRequestHeaders = null)
        {
            using (var lClient = new HttpClient(mClientHandler, false))
            {
                _InitClient(lClient, aBaseURL, aRequestHeaders);
                string lPostBody = JsonConvert.SerializeObject(aData);
                var lContent = new StringContent(lPostBody, Encoding.UTF8, "application/json");
                return await lClient.PostAsync(aRequestURL, lContent);
            }

        }

        public async Task<TResponse> Put<TRequest, TResponse>(string aBaseURL, string aRequestURL, TRequest aData, IEnumerable<KeyValuePair<string, string>> aRequestHeaders = null)
        {
            var lResponse = await Put<TRequest>(aBaseURL, aRequestURL, aData, aRequestHeaders);
            if (lResponse.IsSuccessStatusCode)
            {
                return await lResponse.Content.ReadAsAsync<TResponse>();
            }
            return default(TResponse);
        }

        public async Task<HttpResponseMessage> Put<TRequest>(string aBaseURL, string aRequestURL, TRequest aData, IEnumerable<KeyValuePair<string, string>> aRequestHeaders = null)
        {
            using (var lClient = new HttpClient(mClientHandler, false))
            {
                _InitClient(lClient, aBaseURL, aRequestHeaders);
                string lPostBody = JsonConvert.SerializeObject(aData);
                var lContent = new StringContent(lPostBody, Encoding.UTF8, "application/json");
                return await lClient.PutAsync(aRequestURL, lContent);
            }

        }

        public async Task<HttpResponseMessage> Delete(string aBaseURL, string aRequestURL, IEnumerable<KeyValuePair<string, string>> aRequestHeaders = null)
        {
            using (var lClient = new HttpClient(mClientHandler, false))
            {
                _InitClient(lClient, aBaseURL, aRequestHeaders);
                return await lClient.DeleteAsync(aRequestURL);
            }
        }

        private static void _InitClient(HttpClient aClient, string aBaseURL, IEnumerable<KeyValuePair<string, string>> aRequestHeaders)
        {
            aClient.BaseAddress = new Uri(aBaseURL);
            aClient.DefaultRequestHeaders.Accept.Clear();
            aClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (aRequestHeaders != null)
            {
                foreach (var lPair in aRequestHeaders)
                {
                    if (!aClient.DefaultRequestHeaders.Contains(lPair.Key))
                        aClient.DefaultRequestHeaders.Add(lPair.Key, lPair.Value);
                }
            }
        }
    }
}
