using SitefinityEcommerceDonations.EcommerceCalculators;
using System;
using System.Linq;
using Telerik.Sitefinity;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.Data.Metadata;
using Telerik.Sitefinity.Ecommerce.Orders.Model;
using Telerik.Sitefinity.Modules.Pages;
using Telerik.Sitefinity.Modules.Pages.Configuration;

namespace SitefinityEcommerceDonations
{
    public class Installer
    {
        public static void PreApplicationStart()
        {
            // With this method we subscribe for the Sitefinity Bootstrapper_Initialized event, which is fired after initialization of the Sitefinity application
            Bootstrapper.Initialized += (new EventHandler<ExecutedEventArgs>(Installer.Bootstrapper_Initialized));
        }

        private static void Bootstrapper_Initialized(object sender, ExecutedEventArgs e)
        {
            if (e.CommandName != "Bootstrapped")
            {
                EcommerceOrderCalculatorCustom.Register();             

                Installer.CreateCustomOrderFields();
            }

            if (e.CommandName != "RegisterRoutes" || !Bootstrapper.IsDataInitialized)
            {
                return;
            }


            Installer.InstallWidget();
        }

        /// <summary>
        /// Registering the widget using the fluent API
        /// </summary>
        protected static void InstallWidget()
        {
            Installer.RegisterControl("EcommerceDonations", typeof(DonationsWidget), "PageControls", "EcommerceDonations");
        }

        protected static void CreateCustomOrderFields()
        {
            // You need an instance of the MetadataMananger in order to add the new meta field to the tables.
            MetadataManager metaManager = MetadataManager.GetManager();

            // Check if the CartOrder table has already been modified to contain meta fields
            if (metaManager.GetMetaType(typeof(CartOrder)) == null)
            {
                // Create the metatype for the CartOrder class.
                metaManager.CreateMetaType(typeof(CartOrder));

                //Save the changes.
                metaManager.SaveChanges();
            }

            // Add a new meta field to the CartOrder table
            App.WorkWith()
                .DynamicData()
                .Type(typeof(CartOrder))
                .Field()
                .TryCreateNew("DonationAmount", typeof(decimal))
                .SaveChanges(true);
        }

        public static void RegisterControl(string controlName, Type controlType, string toolboxName, string sectionName)
        {
            var configManager = ConfigManager.GetManager();
            var config = configManager.GetSection<ToolboxesConfig>();

            var controls = config.Toolboxes[toolboxName];
            var section = controls.Sections.Where<ToolboxSection>(e => e.Name == sectionName).FirstOrDefault();

            if (section == null)
            {
                section = new ToolboxSection(controls.Sections)
                {
                    Name = sectionName,
                    Title = sectionName,
                    Description = sectionName,
                    ResourceClassId = typeof(PageResources).Name
                };
                controls.Sections.Add(section);
            }

            if (!section.Tools.Any<ToolboxItem>(e => e.Name == controlName))
            {
                var tool = new ToolboxItem(section.Tools)
                {
                    Name = controlName,
                    Title = controlName,
                    Description = controlName,
                    ControlType = controlType.AssemblyQualifiedName
                };
                section.Tools.Add(tool);
            }

            configManager.SaveSection(config);
        }
    }
}
