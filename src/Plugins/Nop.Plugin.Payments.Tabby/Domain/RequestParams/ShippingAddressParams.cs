using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Tabby.Domain.RequestParams
{
    public class ShippingAddressParams
    {
        public ShippingAddressParams(string firstName, string lastName, string line1, string city, string countryCode)
        {
            FirstName = firstName;
            LastName = lastName;
            Line1 = line1;
            City = city;
            CountryCode = countryCode;
        }

        /// <summary>
        /// Gets or sets first name
        /// </summary>
        [JsonProperty(PropertyName = "first_name")]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets last name
        /// </summary>
        [JsonProperty(PropertyName = "last_name")]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets line1
        /// </summary>
        [JsonProperty(PropertyName = "line1")]
        public string Line1 { get; set; }

        /// <summary>
        /// Gets or sets city
        /// </summary>
        [JsonProperty(PropertyName = "city")]
        public string City { get; set; }

        /// <summary>
        /// Gets or sets country_code
        /// </summary>
        [JsonProperty(PropertyName = "country_code")]
        public string CountryCode { get; set; }
    }
}
