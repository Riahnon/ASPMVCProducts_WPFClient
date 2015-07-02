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
		public static Dictionary<string, string> RequestHeaders { get; private set; }
		static HttpJSONRequester()
		{
			RequestHeaders = new Dictionary<string, string>();
		}
		public static async Task<TResponse> Get<TResponse>(string aBaseURL, string aRequestURL)
		{
			using (var lClient = new HttpClient())
			{
				lClient.BaseAddress = new Uri(aBaseURL);
				lClient.DefaultRequestHeaders.Accept.Clear();
				lClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				foreach (var lPair in RequestHeaders)
				{
					if(!lClient.DefaultRequestHeaders.Contains(lPair.Key))
						lClient.DefaultRequestHeaders.Add(lPair.Key, lPair.Value);
				}
				HttpResponseMessage lResponse = await lClient.GetAsync(aRequestURL);
				if (lResponse.IsSuccessStatusCode)
				{
					return await lResponse.Content.ReadAsAsync<TResponse>();
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
						foreach (var lPair in RequestHeaders)
						{
							if (!lClient.DefaultRequestHeaders.Contains(lPair.Key))
								lClient.DefaultRequestHeaders.Add(lPair.Key, lPair.Value);
						}
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

		public static async Task<HttpResponseMessage> PostRawResponse<TRequest>(string aBaseURL, string aRequestURL, TRequest aData)
		{
			using (var lClient = new HttpClient())
			{
				lClient.BaseAddress = new Uri(aBaseURL);
				lClient.DefaultRequestHeaders.Accept.Clear();
				lClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				foreach (var lPair in RequestHeaders)
				{
					if (!lClient.DefaultRequestHeaders.Contains(lPair.Key))
						lClient.DefaultRequestHeaders.Add(lPair.Key, lPair.Value);
				}
				string lPostBody = JsonConvert.SerializeObject(aData);
				var lContent = new StringContent(lPostBody, Encoding.UTF8, "application/json");
				return await lClient.PostAsync(aRequestURL, lContent);
			}

		}
		
	}
}
