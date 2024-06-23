using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Tamara.Domain.RequestParams
{
    public class MerchantUrlParams
    {
        private string _baseUri;
        public MerchantUrlParams(string baseUri)
        {
            _baseUri = baseUri;
        }
        [JsonProperty(PropertyName = "success")]
        public string Success => $"{_baseUri}";

        [JsonProperty(PropertyName = "failure")]
        public string Failure => $"{_baseUri}";

        [JsonProperty(PropertyName = "cancel")]
        public string Cancel => $"{_baseUri}";

        [JsonProperty(PropertyName = "notification")]
        public string Notification => $"{_baseUri}";
    }
}
