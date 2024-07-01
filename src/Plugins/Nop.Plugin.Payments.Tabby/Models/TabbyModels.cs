namespace Nop.Plugin.Payments.Tabby.Models
{
    public class Payment
    {
        public string amount { get; set; }
        public string currency { get; set; }
        public string description { get; set; }
        public Buyer buyer { get; set; }
        public BuyerHistory buyer_history { get; set; }
        public string lang { get; set; }
        public string merchant_code { get; set; }
        public MerchantUrls merchant_urls { get; set; }
    }

    public class Buyer
    {
        public string phone { get; set; }
        public string email { get; set; }
        public string name { get; set; }
        public string dob { get; set; }
    }

    public class BuyerHistory
    {
        public string registered_since { get; set; }
        public int loyalty_level { get; set; }
        public int wishlist_count { get; set; }
        public bool is_social_networks_connected { get; set; }
        public bool is_phone_number_verified { get; set; }
        public bool is_email_verified { get; set; }
    }

    public class MerchantUrls
    {
        public string success { get; set; }
        public string cancel { get; set; }
        public string failure { get; set; }
    }

    public class RootObject
    {
        public Configuration configuration { get; set; }
    }

    public class Configuration
    {
        public AvailableProducts available_products { get; set; }
    }

    public class AvailableProducts
    {
        public List<InstallmentProduct> installments { get; set; }
    }

    public class InstallmentProduct
    {
        public string web_url { get; set; }
    }
}
