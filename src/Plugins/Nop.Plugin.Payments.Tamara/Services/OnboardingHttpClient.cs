using System.Text;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Plugin.Payments.Tamara.Domain.Onboarding;

namespace Nop.Plugin.Payments.Tamara.Services;

/// <summary>
/// Represents HTTP client to request onboarding services
/// </summary>
public class TamaraHttpClient
{
    #region Fields

    protected readonly HttpClient _httpClient;

    #endregion

    #region Ctor

    public TamaraHttpClient(HttpClient httpClient)
    {
        //configure client
        httpClient.BaseAddress = new Uri(TamaraDefaults.Onboarding.ServiceUrl);
        httpClient.Timeout = TimeSpan.FromSeconds(TamaraDefaults.Onboarding.RequestTimeout);
        httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, TamaraDefaults.UserAgent);
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
    public async Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest request) where TRequest : Request where TResponse : class
    {
        try
        {
            //prepare request parameters
            var requestString = JsonConvert.SerializeObject(request);
            var requestContent = new StringContent(requestString, Encoding.Default, MimeTypes.ApplicationJson);

            //execute request and get response
            var requestMessage = new HttpRequestMessage(new HttpMethod(request.Method), request.Path) { Content = requestContent };
            var httpResponse = await _httpClient.SendAsync(requestMessage);
            httpResponse.EnsureSuccessStatusCode();

            //return result
            var responseString = await httpResponse.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Response<TResponse>>(responseString);
            if (result.Result == ResponseResult.Error)
                throw new NopException($"Onboarding error - {result.Error}");

            return result.Data;
        }
        catch (AggregateException exception)
        {
            //rethrow actual exception
            throw exception.InnerException;
        }
    }

    #endregion
}