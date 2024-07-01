using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;

namespace Nop.Plugin.Payments.Tamara.Domain
{
    public class TamaraPaymentTransaction : BaseEntity 
    {
        public string Status { get; set; }

        public string BillingAddressEmail { get; set; }
        public decimal Amount { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerName { get; set; }

        public string CurrencyCode { get; set; }

        public int CustomerId { get; set; }

        public bool IsMobile { get; set; }
        //public int? OrderId { get; set; }
        public int StoreId { get; set; }

        public Guid OrderReference { get; set; }
        public DateTime? CreatedOnUtc { get; set; }

        public string OrderGuidKeyRes { get; set; }

    }
}
