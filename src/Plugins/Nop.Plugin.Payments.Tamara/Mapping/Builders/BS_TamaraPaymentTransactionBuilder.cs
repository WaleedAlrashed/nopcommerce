using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Payments.Tamara.Domain;

namespace Nop.Plugin.Payments.Tamara.Mapping.Builders
{
    public class BS_TamaraPaymentTransactionBuilder : NopEntityBuilder<TamaraPaymentTransaction>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {

        }
    }
}
