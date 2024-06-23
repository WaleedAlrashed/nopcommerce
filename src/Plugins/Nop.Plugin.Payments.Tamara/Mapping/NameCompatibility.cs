using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using Nop.Plugin.Payments.Tamara.Domain;

namespace Nop.Plugin.Payments.Tamara.Mapping
{
    public partial class NameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(TamaraPaymentTransaction), "NS_TamaraPayment_Transactions" },
            
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>();
    }
}