using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HMX.HASSActron
{
	internal class Proxy
	{
		private static HttpClient _httpClient = null;
		private static int _iWaitTime = 5000; //ms

		static Proxy()
		{
			HttpClientHandler httpClientHandler = null;

			Logging.WriteDebugLog("Proxy.Proxy()");

			httpClientHandler = new HttpClientHandler();
			httpClientHandler.Proxy = null;
			httpClientHandler.UseProxy = false;

			_httpClient = new HttpClient(httpClientHandler);

			_httpClient.DefaultRequestHeaders.Connection.Add("close");

			Logging.WriteDebugLog("Proxy.Proxy() Complete");
		}

		public static async Task<IPAddress> GetTargetAddress(string strHost)
		{
			HttpResponseMessage httpResponse = null;
			CancellationTokenSource cancellationToken = null;
			dynamic dResults;
			IPAddress ipResult = null;

			Logging.WriteDebugLog("Proxy.GetTargetAddress()");

			try
			{
				cancellationToken = new CancellationTokenSource();
				cancellationToken.CancelAfter(_iWaitTime);

				httpResponse = await _httpClient.GetAsync(string.Format("https://dns.google.com/resolve?name={0}&type=A", strHost), cancellationToken.Token);

				if (httpResponse.IsSuccessStatusCode)
				{
					dResults = JsonConvert.DeserializeObject(await httpResponse.Content.ReadAsStringAsync());

					foreach (dynamic dAnswer in dResults.Answer)
					{
						if (dAnswer.type == "1")
						{
							Logging.WriteDebugLog("Proxy.GetTargetAddress() Name: {0}, IP: {1}", dAnswer.name.ToString(), dAnswer.data.ToString());
							ipResult = IPAddress.Parse(dAnswer.data.ToString());
							break;
						}
					}
				}
				else
				{
					Logging.WriteDebugLog("Proxy.GetTargetAddress() Unable to process HTTP response: {0}/{1}", httpResponse.StatusCode.ToString(), httpResponse.ReasonPhrase);

					goto Cleanup;
				}
			}
			catch (Exception eException)
			{
				Logging.WriteDebugLogError("Proxy.GetTargetAddress()", eException, "Unable to process API HTTP response.");
				goto Cleanup;
			}

		Cleanup:
			cancellationToken?.Dispose();
			httpResponse?.Dispose();

			return ipResult;
		}

		public static async void ForwardDataToOriginalWebService(IHeaderDictionary dHeaders, string strHost, string strPath, string strData)
		{
			HttpClient httpClient = null;
			HttpClientHandler httpClientHandler;
			HttpResponseMessage httpResponse = null;
			CancellationTokenSource cancellationToken = null;
			StringContent stringContent;
			IPAddress ipProxy; 
			string strContent;
			string strURL = string.Format("http://{0}{1}", strHost, strPath);

			Logging.WriteDebugLog("Proxy.ForwardDataToOriginalWebService() URL: " + strURL);

			ipProxy = await GetTargetAddress(strHost);
			if (ipProxy == null)
			{
				Logging.WriteDebugLog("Proxy.ForwardDataToOriginalWebService() Abort (no proxy)");
				return;
			}

			httpClientHandler = new HttpClientHandler();
			httpClientHandler.Proxy = new WebProxy(ipProxy.ToString(), 80);
			httpClientHandler.UseProxy = true;

			httpClient = new HttpClient(httpClientHandler);

			httpClient.DefaultRequestHeaders.Connection.Add("close");

			stringContent = new StringContent(strData);

			foreach (string strHeader in dHeaders.Keys)
			{
				try
				{
					switch (strHeader)
					{
						case "User-Agent":
							Logging.WriteDebugLogError("Proxy.ForwardDataToOriginalWebService() User-Agent: {0}", dHeaders[strHeader].ToString());
							httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(dHeaders[strHeader].ToString());
							break;

						case "Content-Type":
							Logging.WriteDebugLogError("Proxy.ForwardDataToOriginalWebService() Content-Type: {0}", dHeaders[strHeader].ToString());
							stringContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(dHeaders[strHeader].ToString());
							break;

						case "X-Ninja-Token":
							Logging.WriteDebugLogError("Proxy.ForwardDataToOriginalWebService() X-Ninja-Token: {0}", dHeaders[strHeader].ToString());
							httpClient.DefaultRequestHeaders.Add(strHeader, dHeaders[strHeader].ToString());
							break;
					}
				}
				catch (Exception eException)
				{
					Logging.WriteDebugLogError("Proxy.ForwardDataToOriginalWebService()", eException, "Unable to add request header ({0}).", strHeader);
				}
			}

			try
			{
				cancellationToken = new CancellationTokenSource();
				cancellationToken.CancelAfter(_iWaitTime);

				httpResponse = await httpClient.PostAsync(strURL, stringContent, cancellationToken.Token);

				if (httpResponse.IsSuccessStatusCode)
				{
					strContent = await httpResponse.Content.ReadAsStringAsync();
					Logging.WriteDebugLog("Response: " + strContent);
				}
				else
				{
					Logging.WriteDebugLog("Response: " + httpResponse.StatusCode);
				}
			}
			catch (Exception eException)
			{
				Logging.WriteDebugLogError("Proxy.ForwardDataToOriginalWebService()", eException, "Unable to process API HTTP response.");
			}

			cancellationToken?.Dispose();
			httpResponse?.Dispose();
			httpClient?.Dispose();
		}
	}
}
