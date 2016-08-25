using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Telerik.Sitefinity.Modules.Ecommerce.Catalog;
using Telerik.Sitefinity.Modules.Ecommerce.Configuration;
using Telerik.Sitefinity.Modules.Ecommerce.Orders;
using Telerik.Sitefinity.Modules.Ecommerce.Orders.Business;
using Telerik.Sitefinity.Modules.Ecommerce.Orders.Web.UI;
using Telerik.Sitefinity.Web.UI;
using Telerik.Sitefinity.Web.UI.ControlDesign;
using Telerik.Sitefinity.Web.UI.Fields;
using Telerik.Sitefinity.Ecommerce.Catalog.Model;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.GenericContent.Model;
using System.Web;
using Telerik.Sitefinity.Modules.Ecommerce;
using Telerik.Sitefinity.Ecommerce.Orders.Model;
using System.ComponentModel;
using Telerik.Sitefinity;
using Telerik.Sitefinity.Services;

namespace SitefinityEcommerceDonations
{
    [ControlDesigner(typeof(DonationsWidgetDesigner))]
    public class DonationsWidget : SimpleScriptView, IOrdersControl
    {
        protected override string LayoutTemplateName
        {
            get
            {
                return DonationsWidget.layoutTemplateName;
            }
        }

        protected override Type ResourcesAssemblyInfo
        {
            get
            {
                return typeof(DonationsWidget);
            }
        }

        protected OrdersManager OrdersManager
        {
            get
            {
                if (this.ordersManager == null)
                    this.ordersManager = OrdersManager.GetManager();
                return this.ordersManager;
            }
        }

        protected CatalogManager CatalogManager
        {
            get
            {
                if (this.catalogManager == null)
                    this.catalogManager = CatalogManager.GetManager();
                return this.catalogManager;
            }
        }


        public Guid ProductId
        {
            get
            {
                return this.productId;
            }
            set
            {
                this.productId = value;
            }
        }

        public string ProductName
        {
            get { return Product == null ? null : Product.Title.Value; }
            set { /* required for WCF */}
        }

        public Product Product
        {
            get
            {
                if (this.product == null && this.ProductId != null && this.ProductId != Guid.Empty)
                {
                    Product product = this.CatalogManager.GetProduct(this.ProductId);

                    // We use the live product object
                    if (product.Status == ContentLifecycleStatus.Master)
                    {
                        product = this.CatalogManager.GetProducts().First(p => p.OriginalContentId == product.Id && p.Status == ContentLifecycleStatus.Live);
                    }

                    this.product = product;
                }
                return this.product;
            }
        }

        public Guid CheckoutPageId
        {
            get
            {
                return this.checkoutPageId;
            }
            set
            {
                this.checkoutPageId = value;
            }
        }

        protected virtual ChoiceField DonationAmountDropDown
        {
            get
            {
                return this.Container.GetControl<ChoiceField>("donationAmountDropDown", true);
            }
        }


        protected virtual IButtonControl AddToCartButton
        {
            get
            {
                return this.Container.GetControl<IButtonControl>("addToCartButton", true);
            }
        }

        protected virtual TextField OtherAmountControl
        {
            get
            {
                return this.Container.GetControl<TextField>("otherAmount", true);
            }
        }

        protected virtual HtmlGenericControl WidgetStatus
        {
            get
            {
                return this.Container.GetControl<HtmlGenericControl>("widgetStatus", false);
            }
        }

        protected virtual HtmlGenericControl DonationsWidgetWrapper
        {
            get
            {
                return this.Container.GetControl<HtmlGenericControl>("donationsWidgetWrapper", false);
            }
        }


        protected virtual Label WidgetStatusMessage
        {
            get
            {
                return this.Container.GetControl<Label>("widgetStatusMessage", true);
            }
        }

        protected virtual Message AddedToCartMessage
        {
            get
            {
                return this.Container.GetControl<Message>("addedToCartMessage", true);
            }
        }


        protected override void InitializeControls(GenericContainer container)
        {
            if (!this.IsDesignMode())
            {
                if (this.ProductId == Guid.Empty)
                {
                    this.WidgetStatus.Visible = true;
                    this.WidgetStatusMessage.Text = "This widget is not configured, please choose a product by editing the widget.";

                    this.DonationsWidgetWrapper.Visible = false;
                }
                else
                {
                    this.WidgetStatus.Visible = false;
                    this.WidgetStatusMessage.Text = "";

                    this.DonationsWidgetWrapper.Visible = true;

                    AddToCartButton.Click += new EventHandler(AddToCartButton_Command);
                }
            }
        }

        protected void AddToCartButton_Command(object sender, EventArgs e)
        {
            try
            {
                int quantity = 1;

                IShoppingCartAdder shoppingCartAdder = new ShoppingCartAdder();
                string defaultCurrencyName = Config.Get<EcommerceConfig>().DefaultCurrency;

                OptionsDetails optionsDetails = new OptionsDetails();

                decimal price = 0;
                if (!decimal.TryParse(DonationAmountDropDown.Value.ToString(), out price))
                {
                    price = Convert.ToDecimal(OtherAmountControl.Value);
                }

                this.Product.Price = price;
                shoppingCartAdder.AddItemToShoppingCart(this, this.OrdersManager, this.Product, optionsDetails, quantity, defaultCurrencyName);

                // Save the donation amount in custom field
                OrdersManager ordersManager = OrdersManager.GetManager();
                string cookieKey = EcommerceConstants.OrdersConstants.ShoppingCartIdCookieName;
                if (SystemManager.CurrentContext.IsMultisiteMode)
                {
                    cookieKey += SystemManager.CurrentContext.CurrentSite.Id;
                }

                HttpCookie shoppingCartCookie = HttpContext.Current.Request.Cookies[cookieKey];

                CartOrder cartOrder = null;
                if (shoppingCartCookie == null || !shoppingCartCookie.Value.IsGuid())
                {
                    // throw new ArgumentException("The shopping cart cookie does not exist or its value is not a valid string.");
                    cartOrder = ordersManager.CreateCartOrder();
                }
                else
                {
                    Guid cartOrderId = new Guid(shoppingCartCookie.Value);
                    cartOrder = ordersManager.GetCartOrder(cartOrderId);
                }

                if (cartOrder != null)
                {
                    PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(cartOrder);
                    PropertyDescriptor property = properties["DonationAmount"];

                    MetafieldPropertyDescriptor metaProperty = property as MetafieldPropertyDescriptor;

                    if (metaProperty != null)
                    {
                        metaProperty.SetValue(cartOrder, price);
                        ordersManager.SaveChanges();
                    }
                }

                this.AddedToCartMessage.ShowPositiveMessage("Donation added to cart");
            }
            catch (Exception ex)
            {
                this.AddedToCartMessage.ShowNegativeMessage("Failed to add donation to cart, try again");
            }
        }

        public override IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            var descriptors = new List<ScriptDescriptor>();
            var descriptor = new ScriptControlDescriptor(typeof(DonationsWidget).FullName, this.ClientID);

            descriptor.AddComponentProperty("donationAmountDropDown", this.DonationAmountDropDown.ClientID);
            descriptor.AddComponentProperty("otherAmount", this.OtherAmountControl.ClientID);

            descriptors.Add(descriptor);

            return new[] { descriptor };
        }

        public override IEnumerable<ScriptReference> GetScriptReferences()
        {
            var scripts = new List<ScriptReference>()
            {
                new ScriptReference(DonationsWidget.donationsWidgetScript, typeof(DonationsWidget).Assembly.FullName),
            };
            return scripts;
        }

        private static readonly string layoutTemplateName = "SitefinityEcommerceDonations.Resources.DonationsWidget.ascx";
        private static readonly string donationsWidgetScript = "SitefinityEcommerceDonations.Resources.DonationsWidget.js";

        private CatalogManager catalogManager;
        private OrdersManager ordersManager;
        private Product product;
        private Guid productId;
        private Guid checkoutPageId;
    }

}
