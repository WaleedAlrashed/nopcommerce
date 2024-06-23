using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Microsoft.Net.Http.Headers;
using System.Threading.Tasks;
using Nop.Core;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using Nop.Plugin.Payments.Tabby.Domain.Requests;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;
using MaxMind.GeoIP2.Exceptions;

namespace Nop.Plugin.Payments.Tabby.Services
{
    public class TabbyHttpClient
    {
        #region Fields

        private readonly HttpClient _httpClient;

        #endregion

        #region Ctor

        public TabbyHttpClient(HttpClient httpClient)
        {
            //configure client
            httpClient.BaseAddress = new Uri(TabbyDefaults.CheckoutUrl);
            httpClient.Timeout = TimeSpan.FromSeconds(TabbyDefaults.RequestTimeout);
            httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, TabbyDefaults.UserAgent);
            httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, MimeTypes.ApplicationJson);

            _httpClient = httpClient;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Request services
        /// </summary>
        /// <typeparam name="TRequest">Request type</typeparam>
        /// <typeparam name="TResponse">Response type</typeparam>
        /// <param name="request">Request</param>
        /// <returns>The asynchronous task whose result contains response details</returns>
        public async Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest request, TabbySettings settings) where TRequest : Request where TResponse : class
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Remove("Authorization");
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {settings.PublicKey}");
                //prepare request parameters
                var requestString = JsonConvert.SerializeObject(request);
                var requestContent = new StringContent(requestString, Encoding.Default, MimeTypes.ApplicationJson);

                //execute request and get response
                var requestMessage = new HttpRequestMessage(new HttpMethod(request.Method), request.Path) { Content = requestContent };
                requestMessage.Headers.Remove("Authorization");
                requestMessage.Headers.Add("Authorization", $"Bearer {settings.PublicKey}");
                var httpResponse = await _httpClient.SendAsync(requestMessage);
                var responseString = await httpResponse.Content.ReadAsStringAsync();
                if (httpResponse.IsSuccessStatusCode)
                {

                    var result = JsonConvert.DeserializeObject<TResponse>(responseString);
                    if (result == null)
                        throw new NopException($"Error - the result is empty");

                    return result;
                }

                throw new HttpException(responseString, httpResponse.StatusCode, _httpClient.BaseAddress);
            }
            catch (AggregateException exception)
            {
                //rethrow actual exception
                throw exception.InnerException;
            }
        }

        #endregion

    }
}

