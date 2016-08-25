using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Sitefinity.Web.UI;
using Telerik.Sitefinity.Web.UI.ControlDesign;
using Telerik.Web.UI;
using Telerik.Sitefinity.Modules.Ecommerce.Catalog.Web.Definitions;
using Telerik.Sitefinity.Ecommerce.Catalog.Model;
using Telerik.Sitefinity.Modules.Pages.Web.Services;
using System.Globalization;

namespace SitefinityEcommerceDonations
{
    public class DonationsWidgetDesigner : ControlDesignerBase
    {

        protected override string LayoutTemplateName
        {
            get
            {
                return DonationsWidgetDesigner.layoutTemplateName;
            }
        }

        protected override Type ResourcesAssemblyInfo
        {
            get
            {
                return typeof(DonationsWidgetDesigner);
            }
        }

        protected virtual RadWindowManager RadWindowManager
        {
            get
            {
                return this.Container.GetControl<RadWindowManager>("windowManager", true);
            }
        }

        protected LinkButton ShowProductSelectorButton
        {
            get { return this.Container.GetControl<LinkButton>("showProductSelectorButton", true); }
        }


        protected ContentSelector ProductSelector
        {
            get { return this.Container.GetControl<ContentSelector>("productSelector", true); }
        }


        protected override void InitializeControls(GenericContainer container)
        {
            this.ProductSelector.ServiceUrl = ProductBackendDefinitions.ProductServiceUrl + "?filter=[PublishedDrafts]";
            this.ProductSelector.ItemType = typeof(Product).FullName;
        }

        public override IEnumerable<ScriptReference> GetScriptReferences()
        {
            var scripts = new List<ScriptReference>(base.GetScriptReferences());
            scripts.Add(new ScriptReference(DonationsWidgetDesigner.scriptReference, typeof(DonationsWidgetDesigner).Assembly.FullName));
            return scripts;
        }

        public override IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            IEnumerable<ScriptDescriptor> descriptors = new List<ScriptDescriptor>(base.GetScriptDescriptors());
            var descriptor = (ScriptControlDescriptor)descriptors.Last();

            descriptor.AddElementProperty("showProductSelectorButton", this.ShowProductSelectorButton.ClientID);

            descriptor.AddComponentProperty("productSelector", this.ProductSelector.ClientID);

            descriptor.AddComponentProperty("radWindowManager", this.RadWindowManager.ClientID);
            return descriptors;
        }

        private const string layoutTemplateName = "SitefinityEcommerceDonations.Resources.DonationsWidgetDesigner.ascx";
        private const string scriptReference = "SitefinityEcommerceDonations.Resources.DonationsWidgetDesigner.js";
    }

}
