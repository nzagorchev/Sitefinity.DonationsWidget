using System.ComponentModel;
using System.Linq;
using Telerik.Microsoft.Practices.Unity;
using Telerik.Sitefinity;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.Ecommerce.Orders.Model;
using Telerik.Sitefinity.Modules.Ecommerce.BusinessServices.Catalog.Implementations;
using Telerik.Sitefinity.Modules.Ecommerce.Orders.Business;
using Telerik.Sitefinity.Modules.Ecommerce.Orders.Interfaces;

namespace SitefinityEcommerceDonations.EcommerceCalculators
{
    public class EcommerceOrderCalculatorCustom : EcommerceOrderCalculator
    {
        public static readonly string DonationProductSku = "Online Donation";
        protected static readonly string CartOrderDonationAmountFieldName = "DonationAmount";

        public override decimal CalculateProductPrice(Telerik.Sitefinity.Ecommerce.Orders.Model.CartDetail detail, bool useExchangeRate)
        {
            decimal productPrice = detail.Price;
            if (detail.Sku == EcommerceOrderCalculatorCustom.DonationProductSku && productPrice == 0)
            {
                CartOrder cartOrder = detail.Parent;

                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(cartOrder);
                PropertyDescriptor property = properties[EcommerceOrderCalculatorCustom.CartOrderDonationAmountFieldName];
                MetafieldPropertyDescriptor metaProperty = property as MetafieldPropertyDescriptor;
                if (metaProperty != null)
                {
                    var val = metaProperty.GetValue(cartOrder);
                    if (val != null)
                    {
                        decimal donationAmount = decimal.Parse(val.ToString());
                        if (donationAmount != 0 && detail.Sku == EcommerceOrderCalculatorCustom.DonationProductSku)
                        {
                            detail.Price = donationAmount;

                            detail.BasePrice = detail.Price;
                            productPrice = detail.Price;
                        }
                    }
                }
            }

            return Convert(productPrice, useExchangeRate);
        }

        /// <summary>
        /// Changed to use the Calculate Price method
        /// </summary>

        protected override decimal GetSubTotalWithoutTaxes(CartOrder cartOrder, bool useExchangeRate)
        {
            return cartOrder.Details.Where(cd => cd.ProductAvailable == true).Sum(cartDetail => CalculateProductPrice(cartDetail, useExchangeRate) * cartDetail.Quantity);
        }

        protected override decimal GetSubTotalWithoutTaxesNoExchangeRate(CartOrder cartOrder)
        {
            return cartOrder.Details.Where(cd => cd.ProductAvailable == true).Sum(cartDetail => CalculateProductPrice(cartDetail, false) * cartDetail.Quantity);
        }

        protected override decimal GetSubTotalTaxInclusiveNoExchangeRate(CartOrder cartOrder)
        {
            return cartOrder.Details.Where(cd => cd.ProductAvailable == true).Sum(cartDetail => CalculateProductPrice(cartDetail, false) * cartDetail.Quantity);
        }

        protected override decimal GetSubTotalTaxInclusive(CartOrder cartOrder, bool useExchangeRate)
        {
            return cartOrder.Details.Where(cd => cd.ProductAvailable == true).Sum(cartDetail => CalculateProductPrice(cartDetail, false) * cartDetail.Quantity);
        }

        internal static void Register()
        {
            ObjectFactory.Container.RegisterType<IOrderCalculator, EcommerceOrderCalculatorCustom>
                   (new TransientLifetimeManager(), new InjectionConstructor());
        }
    }
}