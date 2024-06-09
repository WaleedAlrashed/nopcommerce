using System;
namespace Nop.Plugin.Payments.Tabby
{
        public class TabbyPaymentSettings : ISettings
        {
            public string PublicKey { get; set; }
            public string SecretKey { get; set; }
        }
}

